using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ocuda.Ops.Models
{
    public class ExternalResource : Abstract.BaseEntity
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
        [Required]
        public string Url { get; set; }
        public int SortOrder { get; set; }
        public ExternalResourceType Type { get; set; }
    }

    public enum ExternalResourceType
    {
        CSS,
        JS
    }
}
