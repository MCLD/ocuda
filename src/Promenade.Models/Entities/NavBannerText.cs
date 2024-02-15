using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ocuda.Promenade.Models.Entities
{
    public class NavBannerText
    {
        [Key]
        [Required]
        public int NavBannerId { get; set; }
        public NavBanner NavBanner { get; set; }

        [Key]
        [Required]
        public int LanguageId { get; set; }
        public Language Language { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; }
    }
}
