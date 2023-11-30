using Bitbucket.HealthCheck.Options;
using Bitbucket.Models;
using Bitbucket.Models.Interfaces;
using Bitbucket.Services;
using Microsoft.Extensions.Options;

namespace Bitbucket.Workers
{
    public class BloomFilterInitializerWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public BloomFilterInitializerWorker(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var prometheusService = scope.ServiceProvider.GetRequiredService<PrometheusService>();
                prometheusService.SendBloomFilterServiceAvaible(0);
                try
                {
                    var bloomFilterRepository = scope.ServiceProvider.GetService<IBloomFilterRepository<Shipment>>();
                    var optionsMonitor = scope.ServiceProvider.GetService<IOptionsMonitor<BloomFilterHealthOptions>>();
                    await bloomFilterRepository.InjectFromDB();
                    await Task.Delay(10000);
                    optionsMonitor.CurrentValue.IsInjected = true;
                    prometheusService.SendBloomFilterServiceAvaible(1);
                }
                catch
                {

                }
            }
        }
    }
}