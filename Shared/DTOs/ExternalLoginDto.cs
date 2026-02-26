using ELProject.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ELProject.Shared.DTOs
{
    public class ExternalLoginDto
    {
        public string IdToken { get; set; } = null!;

        [Required]
        [EnumDataType(typeof(UserRole), ErrorMessage = "Invalid role selected.")]
        public string Role { get; set; } = string.Empty;
    }
}
