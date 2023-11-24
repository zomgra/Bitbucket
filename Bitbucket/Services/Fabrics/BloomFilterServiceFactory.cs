using Bitbucket.HealthCheck.Options;
using Bitbucket.Models;
using Bitbucket.Models.Interfaces;
using Microsoft.Extensions.Options;

namespace Bitbucket.Services.Fabrics
{
    public class BloomFilterServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptionsMonitor<BloomFilterHealthOptions> _optionsMonitor;

        public BloomFilterServiceFactory(IServiceProvider serviceProvider, 
            IOptionsMonitor<BloomFilterHealthOptions> optionsMonitor)
        {
            _serviceProvider = serviceProvider;
            _optionsMonitor = optionsMonitor;
        }

        public IRepository<Shipment> CreateRepository()
        {
            var isBloomFilterHealthy = CheckBloomFilterHealth();
            var scope = _serviceProvider.CreateScope();
            var bloom = scope.ServiceProvider.GetRequiredService<IBloomFilterRepository<Shipment>>();
            var shipment = scope.ServiceProvider.GetRequiredService<IShipmentRepository<Shipment>>();

            return isBloomFilterHealthy
            ? bloom
            : shipment;
        }
        private bool CheckBloomFilterHealth()
        {          
            return _optionsMonitor.CurrentValue.UseBloomFilterService;
        }
    }
}