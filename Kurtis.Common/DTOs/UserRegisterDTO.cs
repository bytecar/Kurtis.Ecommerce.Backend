using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Kurtis.Common.DTOs
{
    public class UserRegisterDTO
    {
        [Required(ErrorMessage = "First name is required.")]
        [MaxLength(50, ErrorMessage = "First name must be less than or equal to 50 characters.")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "UserName is Required.")]
        [MaxLength(50, ErrorMessage = "UserName name must be less than or equal to 50 characters.")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; set; } = null!;       

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; } = null!;
/*
        [Required(ErrorMessage = "Role is Required.")]
        [MaxLength(50, ErrorMessage = "Role name must be less than or equal to 50 characters.")]
        public string Role { get; set; } = "customer";*/
        [Required(ErrorMessage = "Status is Required.")]
        [MaxLength(50, ErrorMessage = "Status name must be less than or equal to 50 characters.")]
        public string Status { get; set; } = "active";
    }
}
