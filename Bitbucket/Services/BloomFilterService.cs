using Bitbucket.Data;
using Bitbucket.Exceptions;
using Bitbucket.Models;
using Bitbucket.Models.Interfaces;
using Microsoft.EntityFrameworkCore;
using BloomFilter;

namespace Bitbucket.Services
{
    public class BloomFilterService : IRepository<Shipment>
    {
        private readonly AppDbContext _context;
        private readonly BarCodeGenerator _generator;
        private readonly IBloomFilter _bloomFilter;

        public BloomFilterService(AppDbContext context,
            BarCodeGenerator barCodeGenerator,
            IBloomFilter filter)
        {
            _context = context;
            _generator = barCodeGenerator;
            _bloomFilter = filter;
        }

        public async Task Add(Shipment value, CancellationToken cancellationToken)
        {
            if (_bloomFilter is null)
            {
                throw new DomainException("Bloom Filter haven`t instance");
            }
            try
            {
                var barcode = _generator.GenerateBarCode(2, 2);
                value.Barcode = barcode;

                _bloomFilter.Add(value.Barcode);

                await _context.AddAsync(value, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException e)
            {
                throw new DomainException($"Problem with adding shipments: {e?.InnerException?.Message}");
            }
        }

        public async Task<bool> Contains(string barcode, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(barcode) || barcode.Length < 13 || barcode.Length > 25)
            {
                throw new DomainException("Barcode must be 13-25 length", 400);
            }
            if (_bloomFilter is null)
                return false;

            if (!_bloomFilter.Contains(barcode))
                return false;


            var shipment = await _context.Shipments.FirstOrDefaultAsync(x => x.Barcode == barcode, cancellationToken);
            if (shipment == null)
                return false;

            return true;
        }
    }
}