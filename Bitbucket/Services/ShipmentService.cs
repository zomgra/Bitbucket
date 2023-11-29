using Bitbucket.Data;
using Bitbucket.Exceptions;
using Bitbucket.Models;
using Microsoft.EntityFrameworkCore;
using Bitbucket.Models.Interfaces;
using BloomFilter;

namespace Bitbucket.Services
{
    public class ShipmentService : IShipmentRepository<Shipment>
    {
        private readonly AppDbContext _context;
        private readonly IBloomFilter _bloomFilter;

        public ShipmentService(AppDbContext context,
            IBloomFilter bloomFilter)
        {
            _context = context;
            _bloomFilter = bloomFilter;
        }
        public async Task<bool> Contains(string barcode, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(barcode) || barcode.Length < 13 || barcode.Length > 25)
            {
                throw new DomainException("Barcode must be 13-25 length", 400);
            }

            var shipment = await _context.Shipments.FirstOrDefaultAsync(x => x.Barcode == barcode, cancellationToken);
            if (shipment == null)
                return false;

            return true;
        }

        public async Task Add(Shipment shipment, CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.AddAsync(shipment, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                await _bloomFilter.AddAsync(shipment.Barcode);
            }
            catch
            {
                throw new DomainException("Error with saving data");
            }
        }

        public Task InjectFromDB()
        {
            throw new NotImplementedException("Injecting from DB in Shipment Service not should be use");
        }
    }
}