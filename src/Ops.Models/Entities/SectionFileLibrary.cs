using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ocuda.Ops.Models.Entities
{
    public class SectionFileLibrary : Abstract.BaseEntity
    {
        [Required]
        public int SectionId { get; set; }

        [Required]
        public int FileLibraryId { get; set; }
    }
}
