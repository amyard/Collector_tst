namespace Collector.Models.Instant
{
    public class InstantPyCardExportData
    {
        public string EmployeeId { get; set; }
        public string ExternalLocationId { get; set; }
        public string JobCode { get; set; }
        public EmployeeInfo EmployeeInfo { get; set; }
        public JobInfo JobInfo { get; set; }
    }
}