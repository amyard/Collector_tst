namespace Collector.Models
{
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
}