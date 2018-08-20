using System;
using System.Collections.Generic;
using System.Text;

namespace Ocuda.Ops.Models
{
    public class CategoryFileType : Abstract.BaseEntity
    {
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public int FileTypeId { get; set; }
        public FileType FileType { get; set; }
    }
}
