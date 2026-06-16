using ELProject.Domain.Enums;

namespace ELProject.Shared.DTOs.Student
{
    public class StudentProfileDto
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Username { get; set; }
        public string? Bio { get; set; }
        public string? ProfilePicture { get; set; }
        public Gender? Gender { get; set; }
        public DateTime JoinDate { get; set; }
        public int CoursesCount { get; set; }
    }
}