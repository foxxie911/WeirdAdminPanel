using System.ComponentModel.DataAnnotations;

namespace WeirdAdminPanel.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        public required string Name {get; set;}
        [Required]
        public required string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        public bool? IsBlocked { get; set; } = false;

        [DataType(DataType.DateTime)]
        public DateTime? LastLogin { get; set; }
    }

    public class RegitrationViewModel
    {
        [Required]
        public required string Name {get; set;}
        [Required]
        public required string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public required string ConfirmPassword { get; set; }
    }
}