namespace Collector.Models
{
    public class ConfigData
    {
        public string CollectorName { get; set; }
        public string EmailFromAddress { get; set; }
        public string NotifyEmailAddress { get; set; }
        public string SearchDrive { get; set; }
        public string InstantPayCardDataFolder { get; set; }
        public string InstantPayCardArchiveFolder { get; set; }
        public string InstantAPIBaseUrl { get; set; }
        public string InstantAPICensusFileUrl { get; set; }
        public string InstantPaycardAPIKey { get; set; }
        public string InternalLocationId { get; set; }
        public long FileSizeLimitInBytes { get; set; }
    }
}