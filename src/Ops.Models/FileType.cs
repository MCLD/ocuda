using System;
using System.Collections.Generic;
using System.Text;

namespace Ocuda.Ops.Models
{
    public class FileType : Abstract.BaseEntity
    {
        public string Extension { get; set; }
        public string Icon { get; set; }
        public bool IsDefault { get; set; }
    }
}
