using System;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class EmediaAccess
    {
        [Required]
        public DateTime AccessDate { get; set; }

        public Emedia Emedia { get; set; }

        [Required]
        public int EmediaId { get; set; }

        [Required]
        [Key]
        public int Id { get; set; }
    }
}