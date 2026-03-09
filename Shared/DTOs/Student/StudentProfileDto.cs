namespace ELProject.Shared.DTOs.Student
{
    public class StudentProfileDto
    {

        public required string Email { get; set; }
        public required string Username { get; set; }
        public string? Bio { get; set; }
        public int CoursesCount { get; set; }
    }
}