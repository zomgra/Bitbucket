using Bitbucket.Data;
using Bitbucket.Exceptions;
using Bitbucket.Models;
using Bitbucket.Models.Interfaces;
using Bitbucket.Responces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Bitbucket.Services
{
    public class ShipmentService : IRepository<Shipment>
    {
        private readonly AppDbContext _context;
        private readonly BarCodeGenerator _generator;

        public ShipmentService(AppDbContext context, 
            BarCodeGenerator generator)
        {
            _context = context;
            _generator = generator;
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
                return false;
            }
            var shipment = await _context.Shipments.FirstOrDefaultAsync(x => x.Barcode == barcode, cancellationToken);

            return shipment != null;
        }
    }
}