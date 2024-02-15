using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ocuda.Promenade.Models.Entities
{
    public class NavBannerLinkText
    {
        [Key]
        [Required]
        public int LanguageId { get; set; }
        public Language Language { get; set; }

        [Key]
        [Required]
        public int NavBannerLinkId { get; set; }
        public NavBannerLink NavBannerLink { get; set; }

        [Required]
        [MaxLength(255)]
        public string Text { get; set; }
    }
}