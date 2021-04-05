using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    public class DigitalDisplayDisplaySet
    {
        public DigitalDisplay DigitalDisplay { get; set; }

        [Required]
        public int DigitalDisplayId { get; set; }

        public DigitalDisplaySet DigitalDisplaySet { get; set; }

        [Required]
        public int DigitalDisplaySetId { get; set; }
    }
}