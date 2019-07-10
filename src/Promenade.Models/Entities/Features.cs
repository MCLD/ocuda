using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    class Features
    {
        [Required]
        public int Id { get; set; }

        [MaxLength(255)]
        [Required]
        public string Name { get; set; }

        [MaxLength(255)]
        public string FontAwesome { get; set; }

        [MaxLength(255)]
        public string ImagePath { get; set; }

        [MaxLength(255)]
        [Required]
        public string Stub { get; set; }

        [MaxLength(255)]
        [Required]
        public string BodyText { get; set; }
    }
}
