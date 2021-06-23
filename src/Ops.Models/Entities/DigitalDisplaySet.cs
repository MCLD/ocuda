using System;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    public class DigitalDisplaySet : Abstract.BaseEntity
    {
        public DateTime LastContentUpdate { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
    }
}