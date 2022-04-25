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
    
    public class EmployeeInfo
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string BirthDate { get; set; }
        public bool BlockInstantPay { get; set; }
        public string Terminated { get; set; }
        public bool PaidOnInstant { get; set; }
    }
    
    public class JobInfo
    {
        public string Name { get; set; }
        public decimal? HourlyRate { get; set; }
        public decimal? AnnualSalary { get; set; }
    }
}