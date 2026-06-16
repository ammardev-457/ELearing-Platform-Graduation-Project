using ELProject.Domain.Enums;

namespace ELProject.Shared.DTOs.Student
{
    public class EditStudentProfileDto
    {
        public string? Name { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public Gender? Gender { get; set; }
        public IFormFile? Image { get; set; }
    }
}
