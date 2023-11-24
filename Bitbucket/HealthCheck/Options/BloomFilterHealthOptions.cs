namespace Bitbucket.HealthCheck.Options
{
    public class BloomFilterHealthOptions
    {
        public bool IsInjected { get; set; } = false;
        public bool IsHealthy { get; set; } = false;
        public bool UseBloomFilterService
        {
            get
            {
                return IsInjected || IsHealthy;
            }
            set => UseBloomFilterService = value;
        }
    }
}