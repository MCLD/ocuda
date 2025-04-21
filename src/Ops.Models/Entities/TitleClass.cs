using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    public class TitleClass : Abstract.BaseEntity
    {
        [MaxLength(255)]
        public string Name { get; set; }

        public ICollection<TitleClassMapping> TitleClassMappings { get; set; }
    }
}
