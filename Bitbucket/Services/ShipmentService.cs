using Bitbucket.Data;
using Bitbucket.Exceptions;
using Bitbucket.Models;
using Microsoft.EntityFrameworkCore;
using Bitbucket.Models.Interfaces;

namespace Bitbucket.Services
{
    public class ShipmentService : IShipmentRepository<Shipment>
    {
        private readonly AppDbContext _context;
        private readonly BarCodeGenerator _generator;

        public ShipmentService(AppDbContext context,
            BarCodeGenerator barCodeGenerator)
        {
            _context = context;
            _generator = barCodeGenerator;
        }

        public async Task Add(Shipment value, CancellationToken cancellationToken)
        {
            try
            {
                var barcode = _generator.GenerateBarCode(2, 2);
                value.Barcode = barcode;

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

            var shipment = await _context.Shipments.FirstOrDefaultAsync(x => x.Barcode == barcode, cancellationToken);
            if (shipment == null)
                return false;

            return true;
        }

        public async Task<Shipment> Create(CancellationToken cancellationToken = default)
        {
            var barcode = _generator.GenerateBarCode(2, 2);
            var shipment = new Shipment(barcode);

            try
            {
                await _context.AddAsync(shipment, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                throw new DomainException("Error with saving data");
            }
            return shipment;
        }

        public Task InjectFromDB()
        {
            return Task.CompletedTask;
        }
    }
}