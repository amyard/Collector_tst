namespace Collector.Models
{
    public class InstantPayCardImportData
    {
        public string ExternalLocationId { get; set; }
        public EmployeeInfo EmployeeInfo { get; set; }
        public JobInfo[] JobInfo { get; set; }
    }
}