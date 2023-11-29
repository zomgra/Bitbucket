using Prometheus;

namespace Bitbucket.Services
{
    public class PrometheusService
    {
        public readonly Counter ErrorCounter = Metrics.CreateCounter("error_counter", "How many errors count");
        public void PushSummary(string name, string description, long data)
        {
            var summary = Metrics.CreateSummary(name, description);
            summary.Observe(data);
        }
        public Counter CreateDurationOperation(string name, string desc = "") =>
            Metrics.CreateCounter(name, desc, new CounterConfiguration()
            {
                LabelNames = new[] { name }
            });
    }
}