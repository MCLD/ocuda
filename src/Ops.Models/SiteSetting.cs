using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Ocuda.Ops.Models.Abstract;

namespace Ocuda.Ops.Models
{
    public class SiteSetting : BaseEntity
    {
        [Required]
        [MaxLength(255)]
        public string Key { get; set; }
        [MaxLength(8)]
        public string Type { get; set; }

        // shown to user:
        [MaxLength(255)]
        public string Name { get; set; }
        public string Description { get; set; }
        [MaxLength(255)]
        public string Value { get; set; }
        [MaxLength(255)]
        public string Category { get; set; }
    }

}
