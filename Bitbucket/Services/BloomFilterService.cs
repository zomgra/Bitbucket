using Bitbucket.Data;
using Bitbucket.Exceptions;
using Bitbucket.Models;
using Microsoft.EntityFrameworkCore;
using BloomFilter;
using Bitbucket.Models.Interfaces;

namespace Bitbucket.Services
{
    public class BloomFilterService : IBloomFilterRepository<Shipment>
    {
        private readonly AppDbContext _context;
        private readonly IBloomFilter _bloomFilter;

        public BloomFilterService(AppDbContext context,
            IBloomFilter filter)
        {
            _context = context;
            _bloomFilter = filter;
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

        public async Task Add(Shipment shipment, CancellationToken cancellationToken = default)
        {
            await _bloomFilter.AddAsync(shipment.Barcode);
            try
            {
                await _context.AddAsync(shipment, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                throw new DomainException("Error with saving data");
            }

        }

        public async Task InjectFromDB()
        {
            try
            {
                await foreach (var item in _context.Shipments.Select(x => x.Barcode).AsAsyncEnumerable())
                {
                    await _bloomFilter.AddAsync(item);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}