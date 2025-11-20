using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
namespace Kurtis.Common.Models
{
    public class User : IdentityUser<int>
    {
                
        public override string? UserName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public override string? Email { get; set; } = null!;
        public override string? PasswordHash { get; set; } = null!;
        public override int Id { get; set; }
        public int RoleId { get; set; } = 2; // Default role ID for 'customer'
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }    

}
