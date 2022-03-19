namespace Collector.Models
{
    public class JobInfo
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public bool IsHourly { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal AnnualSalary { get; set; }
        public bool IsDefault { get; set; }
    }
}