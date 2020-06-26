using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class Language
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }
    }
}
