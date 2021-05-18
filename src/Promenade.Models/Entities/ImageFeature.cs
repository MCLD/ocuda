using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class ImageFeature
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        public ICollection<ImageFeatureItem> Items { get; set; }
    }
}