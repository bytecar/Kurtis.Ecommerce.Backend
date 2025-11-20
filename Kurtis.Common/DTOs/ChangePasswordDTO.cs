using System;
using System.Collections.Generic;
using System.Text;

namespace Kurtis.Common.DTOs
{
    public class ChangePasswordDTO
    {
        public required string CurrentPassword { get; set; }
        public required string NewPassword { get; set; }
    }
}
