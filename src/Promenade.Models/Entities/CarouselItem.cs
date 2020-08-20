using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class CarouselItem
    {
        [Key]
        [Required]
        public int Id { get; set; }

        public int Order { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
    }
}
