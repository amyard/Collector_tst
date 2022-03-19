namespace Collector.Models.Instant
{
    public class EmployeeInfo
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string BirthDate { get; set; }
        public bool BlockInstantPay { get; set; }
        public string Terminated { get; set; }
        public bool PaidOnInstant { get; set; }
    }
}