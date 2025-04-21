using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    public class Category : Abstract.BaseEntity
    {
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Stub { get; set; }
    }
}
