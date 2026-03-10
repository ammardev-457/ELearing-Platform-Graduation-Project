namespace ELProject.Shared.DTOs.Instructor
{
    public class InstructorCourseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public int StudentsCount { get; set; }
        public double Rate { get; set; }
        public decimal Revenue { get; set; }
        public DateTime CreatedDate { get; set; }
    }

}
