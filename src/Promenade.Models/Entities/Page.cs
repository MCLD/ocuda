using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Ocuda.Promenade.Models.Abstract;

namespace Ocuda.Promenade.Models.Entities
{
    public class Page : BaseEntity
    {
        [Required]
        [MaxLength(255)]
        public string Title { get; set; }

        public string Content { get; set; }

        [DisplayName("Publish this page")]
        public bool IsPublished { get; set; }

        [DisplayName("Social Card")]
        public int? SocialCardId { get; set; }

        public SocialCard SocialCard { get; set; }

        [Required]
        public int LanguageId { get; set; }

        public Language Language { get; set; }

        [Required]
        public int PageHeaderId { get; set; }

        public PageHeader PageHeader { get; set; }
    }
}
