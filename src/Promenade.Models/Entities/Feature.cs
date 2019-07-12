using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class Feature
    {
        [Required]
        public int Id { get; set; }

        [MaxLength(255)]
        [Required]
        public string Name { get; set; }

        [MaxLength(48)]
        public string FontAwesome { get; set; }

        [MaxLength(255)]
        public string ImagePath { get; set; }

        [MaxLength(80)]
        [Required]
        public string Stub { get; set; }

        [Required]
        public string BodyText { get; set; }
    }
}
