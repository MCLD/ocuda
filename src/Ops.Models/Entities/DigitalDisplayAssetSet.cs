using System;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    public class DigitalDisplayAssetSet
    {
        public DigitalDisplayAsset DigitalDisplayAsset { get; set; }

        [Required]
        public int DigitalDisplayAssetId { get; set; }

        public DigitalDisplaySet DigitalDisplaySet { get; set; }

        [Required]
        public int DigitalDisplaySetId { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public bool IsEnabled { get; set; }

        [Required]
        public DateTime StartDate { get; set; }
    }
}