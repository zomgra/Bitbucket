using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Bitbucket.HealthCheck
{
    public class BloomFilterHealthCheck : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {   
                return HealthCheckResult.Healthy();
            });
        }
    }
}