namespace RAPIDDTechnologiesAssignment.Models
{
    public class Employee
    {
        public Guid Id { get; set; }
        public string? EmployeeName { get; set; }
        public DateTime StarTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }
        public string? EntryNotes { get; set; }
        public DateTime? DeletedOn { get; set; }
    }
    public class EmployeeDTO
    {
        public int SrNo { get; set; }
        public string? EmployeeName { get; set; }
        public string? TotalTimeInHours { get; set; }
        public string? TotalTimeWorked { get; set; }
    }
}
