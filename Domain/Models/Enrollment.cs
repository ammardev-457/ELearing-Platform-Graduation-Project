namespace ELProject.Domain.Models
{
    public class Enrollment
    {
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;
        public DateTime EnrollDate { get; set; }
        public bool IsCompleted { get; set; }
    }

    // GET api?iscompleted=true&enrollmentdate=""
    public class EnrollmentFilter
    {
        public bool? IsCompleted{get;set;}
        public DateTime? EnrollDate { get; set; }
    }
}