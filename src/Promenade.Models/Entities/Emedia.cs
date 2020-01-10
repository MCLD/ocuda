using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class Emedia
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [MaxLength(255)]
        [Required]
        public string Name { get; set; }

        [MaxLength(255)]
        [Required]
        public string Description { get; set; }

        public string Details { get; set; }

        [MaxLength(255)]
        [Required]
        public string RedirectUrl { get; set; }

        [NotMapped]
        public List<Category> Categories { get; set; }
    }
}
