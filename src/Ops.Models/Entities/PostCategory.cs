using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ocuda.Ops.Models.Entities
{
    public class PostCategory : Abstract.BaseEntity
    {
        public int SectionId { get; set; }
        public Section Section { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        public ICollection<Post> Posts { get; set; }
    }
}
