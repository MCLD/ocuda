using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Ops.Models.Entities
{
    public class RosterHeader : Abstract.BaseEntity
    {
        [NotMapped]
        public int DetailCount { get; set; }

        public bool IsDisabled { get; set; }
        public bool IsImported { get; set; }

        [NotMapped]
        public string Status
        {
            get
            {
                return IsImported ? "Imported" : IsDisabled ? "Disabled" : "Ready for import";
            }
        }
    }
}