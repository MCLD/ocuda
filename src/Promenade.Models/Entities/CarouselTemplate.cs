using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class CarouselTemplate
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(50)]
        public string ButtonUrlLabel { get; set; }

        [MaxLength(255)]
        public string ButtonUrlInfo { get; set; }

        [MaxLength(255)]
        public string ButtonUrlTemplate { get; set; }
    }
}
