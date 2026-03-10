namespace ELProject.Shared.DTOs.Instructor
{
    public class RecentActivityDto
    {
        public string ActivityType { get; set; } = null!; // "Order", "Enrollment", "Review"
        public string Description { get; set; } = null!;
        public DateTime Date { get; set; }
    }
}
