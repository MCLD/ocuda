using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ocuda.Promenade.Models.Entities
{
    public class Page
    {
        public int Id { get; set; }

        [MaxLength(255)]
        public string Stub { get; set; }

        public string Content { get; set; }
        public PageType Type { get; set; }
        public bool IsPublished { get; set; }

        [DisplayName("Social Card")]
        public int? SocialCardId { get; set; }

        public SocialCard SocialCard { get; set; }
    }

    public enum PageType
    {
        About
    }
}
