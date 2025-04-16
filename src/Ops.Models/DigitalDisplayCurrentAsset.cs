using System;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Models
{
    public class DigitalDisplayCurrentAsset
    {
        public DigitalDisplayAsset Asset { get; set; }
        public string AssetLink { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsEnabled { get; set; }
        public string SetName { get; set; }
        public DateTime StartDate { get; set; }
    }
}