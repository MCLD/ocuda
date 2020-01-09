using System;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class SiteAlert
    {
        [Key]
        [Required]
        public int Id { get; set; }

        public bool IsActive { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }

        [Required]
        [MaxLength(500)]
        public string Message { get; set; }
    }
}
