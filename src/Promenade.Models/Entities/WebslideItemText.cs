﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class WebslideItemText
    {
        public int WebslideItemId { get; set; }
        public WebslideItem WebslideItem { get; set; }

        public int LanguageId { get; set; }
        public Language Language { get; set; }

        [MaxLength(255)]
        public string Filename { get; set; }

        [MaxLength(255)]
        [Required]
        public string Url { get; set; }

        [DisplayName("Alt Text")]
        [MaxLength(255)]
        public string AltText { get; set; }
    }
}
