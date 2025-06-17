using System.ComponentModel.DataAnnotations;

namespace GoogleOAuthDemo.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Email { get; set; }

        // Role will be "Admin" or "User"
        [Required]
        public string Role { get; set; } = "User"; // Default is User
    }
}
