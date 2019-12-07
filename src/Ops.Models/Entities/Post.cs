using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Ocuda.Ops.Models.Entities
{
    public class Post : Abstract.BaseEntity
    {
        public int SectionId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Stub { get; set; }

        [Required]
        public string Content { get; set; }

        public bool ShowOnHomePage { get; set; }

        public DateTime PublishedAt { get; set; }

        [NotMapped]
        public List<Category> Categories { get; set; }
    }
}
