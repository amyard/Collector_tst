namespace Collector.Models
{
    public class EmployeeData
    {
        public string ExternalLocationId { get; set; }
        public EmployeeInfo EmployeeInfo { get; set; }
        public JobInfo[] JobInfo { get; set; }
    }
    
    public class EmployeeInfo
    {
        public string EmployeeId { get; set; }   
        public string FirstName { get; set; }   
        public string LastName { get; set; }   
        public string BirthDate { get; set; }   
        public bool BlockInstantPay { get; set; }   
        public Terminated Terminated { get; set; }   
        public bool PaidInstant { get; set; }   
    }
    public class JobInfo
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public bool IsHourly { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal AnnualSalary { get; set; }
        public bool IsDefault { get; set; }
    }
    
    public class Terminated
    {
        public bool IsTerminated { get; set; }
        public string TerminatedDate { get; set; }
    }
}