using System.ComponentModel.DataAnnotations;

namespace TaskManager.API.DTOs
{
    public class UserUpdateAdminDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        public string? Password { get; set; }

        [Required]
        [RegularExpression("^(Admin|User)$", ErrorMessage = "Role must be either 'Admin' or 'User'.")]
        public string Role { get; set; } = "User";
    }
}
