using BloomFilter;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Bitbucket.HealthCheck
{
    public class BloomFilterHealthCheck : IHealthCheck
    {
        private readonly IBloomFilter _filter;

        public BloomFilterHealthCheck(IBloomFilter filter)
        {
            _filter = filter;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                if (_filter is null)
                    return HealthCheckResult.Unhealthy();
                return HealthCheckResult.Healthy();
            });
        }
    }
}