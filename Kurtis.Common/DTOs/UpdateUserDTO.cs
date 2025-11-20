using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Kurtis.Common.DTOs
{
    public class UpdateUserDTO
    {
        [Required(ErrorMessage = "First name is required.")]
        [MaxLength(50, ErrorMessage = "First name must be less than or equal to 50 characters.")]
        public string Firstname { get; set; } = null!;

        [Required(ErrorMessage = "UserName is Required.")]
        [MaxLength(50, ErrorMessage = "UserName name must be less than or equal to 50 characters.")]
        public string Username { get; set; } = null!;

        [MaxLength(50, ErrorMessage = "Last name must be less than or equal to 50 characters.")]
        public string? Lastname { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string ConfirmPassword { get; set; } = null!;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; } = null!;
        [MaxLength(10, ErrorMessage = "Phone number must be 10 digits.")]
        [MinLength(10, ErrorMessage = "Phone number must be 10 digits.")]
        public string? PhoneNumber { get; set; }
    }
}
