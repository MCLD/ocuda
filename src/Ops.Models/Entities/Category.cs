using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Ops.Models.Entities
{
    public class Category : Abstract.BaseEntity
    {
        public Category()
        {
            UsedInSections = [];
        }

        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Slug { get; set; }

        [NotMapped]
        public IList<Section> UsedInSections { get; }
    }
}
