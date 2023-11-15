using Bitbucket.Data;
using Bitbucket.Exceptions;
using Bitbucket.Models;
using Microsoft.EntityFrameworkCore;
using BloomFilter;
using Prometheus;

namespace Bitbucket.Services
{
    public class BloomFilterService
    {
        private readonly AppDbContext _context;
        private readonly BarCodeGenerator _generator;
        private readonly IBloomFilter _bloomFilter;
        private readonly PrometheusService _prometheusService;

        public BloomFilterService(AppDbContext context,
            BarCodeGenerator barCodeGenerator,
            IBloomFilter filter,
            PrometheusService prometheusService)
        {
            _context = context;
            _generator = barCodeGenerator;
            _bloomFilter = filter;
            this._prometheusService = prometheusService;
        }

        public async Task Add(Shipment value, CancellationToken cancellationToken)
        {
            HealthCheckBloomFilter();

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

        public async Task<Shipment> Create(CancellationToken cancellationToken = default)
        {

            HealthCheckBloomFilter();

            var barcode = _generator.GenerateBarCode(2, 2);
            var shipment = new Shipment(barcode);

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
            return shipment;

        }

        public async Task InjectFromDB()
        {
            using (_prometheusService.CreateDurationOperation().NewTimer())
            {
                _bloomFilter.Clear();
                try
                {
                    foreach (var item in _context.Shipments.Select(x => x.Barcode).ToList())
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

        private void HealthCheckBloomFilter()
        {
            if (_bloomFilter is null)
            {
                throw new DomainException("Bloom Filter haven`t instance");
            }
        }
    }
}