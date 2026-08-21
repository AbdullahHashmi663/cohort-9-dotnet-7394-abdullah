using System.ComponentModel.DataAnnotations;

namespace TaskManager.API.DTOs
{
    public class UserCreateAdminDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^(Admin|User)$", ErrorMessage = "Role must be either 'Admin' or 'User'.")]
        public string Role { get; set; } = "User";
    }
}
