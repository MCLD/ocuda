using System;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class Language
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }
    }
}
