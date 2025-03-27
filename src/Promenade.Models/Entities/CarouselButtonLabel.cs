using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class CarouselButtonLabel
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public bool IsDisabled { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
    }
}