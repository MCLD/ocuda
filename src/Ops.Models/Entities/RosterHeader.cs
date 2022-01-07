using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Ops.Models.Entities
{
    public class RosterHeader : Abstract.BaseEntity
    {
        [NotMapped]
        public int DetailCount { get; set; }
    }
}
