using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    public class DigitalDisplayItem
    {
        [Required]
        [MaxLength(255)]
        public string AssetId { get; set; }

        public DigitalDisplay DigitalDisplay { get; set; }

        public DigitalDisplayAsset DigitalDisplayAsset { get; set; }

        [Required]
        public int DigitalDisplayAssetId { get; set; }

        [Required]
        public int DigitalDisplayId { get; set; }
    }
}