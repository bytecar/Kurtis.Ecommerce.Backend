using System;
using System.Collections.Generic;
using System.Text;

namespace Kurtis.Common.DTOs
{
    public class BulkFeatureDTO
    {
        public int[]? ProductIds { get; set; }
        public bool Featured { get; set;  }
    }
}
