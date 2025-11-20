using System;
using System.Collections.Generic;
using System.Text;

namespace Kurtis.Common.DTOs
{
    public class UpdateReviewDTO
    {
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}
