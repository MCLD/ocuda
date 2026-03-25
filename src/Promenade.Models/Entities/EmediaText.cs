using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class EmediaText
    {
        [Key]
        [Required]
        public int EmediaId { get; set; }

        public Emedia Emedia { get; set; }

        [Key]
        [Required]
        public int LanguageId { get; set; }

        public Language Language { get; set; }

        [MaxLength(1000)]
        [Required]
        public string Description { get; set; }

        [DisplayName("Info icon pop-up text")]
        [MaxLength(1000)]
        public string Details { get; set; }
    }
}
