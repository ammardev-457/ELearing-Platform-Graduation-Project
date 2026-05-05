namespace ELProject.Shared.DTOs.Instructor
{
    public class EditInstructorProfileDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? AboutMe { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public IFormFile? Image { get; set; }
    }
}
