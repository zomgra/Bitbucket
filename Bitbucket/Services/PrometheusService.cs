using Prometheus;
using Serilog;

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
        public void SendBloomFilterServiceAvaible(int isAvaible) 
        {
            Log.Warning("Using {name}", nameof(SendBloomFilterServiceAvaible));
            var counter = Metrics.CreateCounter("is_bloom_filter_avaible", "View Bloom Filter is avaible now", new CounterConfiguration
            {
                LabelNames = new[] { "is_bloom_filter_avaible" }
            });
            counter.Inc(isAvaible);
        }
        public Counter CreateDurationOperation(string name, string desc = "") =>
            Metrics.CreateCounter(name, desc, new CounterConfiguration()
            {
                LabelNames = new[] { name }
            });
    }
}