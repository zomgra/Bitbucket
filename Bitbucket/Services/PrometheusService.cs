using Prometheus;

namespace Bitbucket.Services
{
    public class PrometheusService
    {
        public void PushSummary(string name, string description, long data)
        {
            var summary = Metrics.CreateSummary(name, description);
            summary.Observe(data);
        }
        public Gauge CreateDurationOperation() =>
            Metrics.CreateGauge("duartion_executing_operation", "How long time executing action");
    }
}