using ELProject.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

public class RegisterDto
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    [RegularExpression(@"^[a-zA-Z0-9_]+$",
        ErrorMessage = "Name can only contain letters, numbers, and underscores.")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [StringLength(254, ErrorMessage = "Email cannot exceed 254 characters.")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8,
        ErrorMessage = "Password must be at least 8 characters.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$",
        ErrorMessage = "Password must contain uppercase, lowercase, number and special character.")]
    public string Password { get; set; } = string.Empty;

    public string? Bio { get; set; }

    [Required]
    [EnumDataType(typeof(UserRole), ErrorMessage = "Invalid role selected.")]
    public UserRole Role { get; set; }

    [EnumDataType(typeof(Gender), ErrorMessage = "Invalid gender selected.")]
    public Gender? Gender { get; set; }
}