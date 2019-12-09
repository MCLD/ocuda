using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ocuda.Promenade.Models.Abstract;

namespace Ocuda.Promenade.Models.Entities
{
    public class SocialCard : BaseEntity
    {
        [Required]
        [MaxLength(70)]
        public string Title { get; set; }

        [Required]
        [MaxLength(200)]
        public string Description { get; set; }

        [Required]
        [MaxLength(255)]
        public string Image { get; set; }

        [DisplayName("Image Alt")]
        [MaxLength(420)]
        public string ImageAlt { get; set; }

        [NotMapped]
        public string Url { get; set; }

        [NotMapped]
        public string TwitterSite { get; set; }
    }
}
