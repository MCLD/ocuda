using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Ocuda.Promenade.Models.Abstract;

namespace Ocuda.Promenade.Models.Entities
{
    public class Page : BaseEntity
    {
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
