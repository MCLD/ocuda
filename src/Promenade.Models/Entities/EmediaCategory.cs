using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class EmediaCategory
    {
        [Key]
        [Required]
        public int EmediaId { get; set; }

        public Emedia Emedia { get; set; }

        [Key]
        [Required]
        public int CategoryId { get; set; }

        public Category Category { get; set; }
    }
}
