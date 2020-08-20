using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class PageDetail
    {
        [Key]
        [Required]
        public int Id { get; set; }

        public int PageHeaderId { get; set; }
        public PageHeader PageHeader { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        public int? SocialCardId { get; set; }
        public SocialCard SocialCard { get; set; }

        public DateTime? StartDate { get; set; }

        public ICollection<PageItem> Items { get; set; }
    }
}
