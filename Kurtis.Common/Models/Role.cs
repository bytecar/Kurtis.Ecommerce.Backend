using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Kurtis.Common.Models
{
    public class Role : IdentityRole<int>
    {
        public Role() : base() { }

        [Required(ErrorMessage = "Role name is required.")]
        [MaxLength(20, ErrorMessage = "Role name cannot exceed 20 characters.")]
        public override string? Name { get; set; }
        [Key]
        public override int Id {  get; set; }        
        public Role(string roleName) : base(roleName)
        {
            Name = roleName;
        }
    }
}
