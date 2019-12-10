using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ocuda.Promenade.Models.Abstract;

namespace Ocuda.Promenade.Models.Entities
{
    public class PageHeader : BaseEntity
    {
        [Required]
        [MaxLength(255)]
        [DisplayName("Page Name")]
        public string PageName { get; set; }

        [Required]
        [MaxLength(255)]
        public string Stub { get; set; }

        public PageType Type { get; set; }

        [NotMapped]
        public ICollection<string> PageLanguages { get; set; }
    }

    public enum PageType
    {
        About,
        Home,
        News,
        Subject
    }
}
