using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Kurtis.Common.DTOs
{
    public class RefreshTokenRequestDTO
    {
        [Required(ErrorMessage = "Refresh Token is required.")]
        public string RefreshToken { get; set; } = null!;
        [Required(ErrorMessage = "Client Id is required.")]
        public string ClientId { get; set; } = null!;
        [Required(ErrorMessage = "Access Token is required.")]
        public string AccessToken { get; set; } = null!;


    }
}
