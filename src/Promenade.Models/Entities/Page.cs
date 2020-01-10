using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class Page
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

        [Key]
        [Required]
        public int LanguageId { get; set; }

        public Language Language { get; set; }

        [Key]
        [Required]
        public int PageHeaderId { get; set; }

        public PageHeader PageHeader { get; set; }
    }
}
