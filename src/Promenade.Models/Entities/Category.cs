using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class Category
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [MaxLength(255)]
        [Required]
        public string Name { get; set; }

        [MaxLength(255)]
        [Required]
        public string Class { get; set; }

        [NotMapped]
        public CategoryText CategoryText { get; set; }

        [NotMapped]
        public ICollection<string> CategoryLanguages { get; set; }
    }
}
