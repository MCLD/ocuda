using System;
using System.ComponentModel.DataAnnotations;
using Ocuda.Promenade.Models.Abstract;

namespace Ocuda.Promenade.Models.Entities
{
    public class SiteAlert : BaseEntity
    {
        public bool IsActive { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }

        [Required]
        [MaxLength(500)]
        public string Message { get; set; }
    }
}
