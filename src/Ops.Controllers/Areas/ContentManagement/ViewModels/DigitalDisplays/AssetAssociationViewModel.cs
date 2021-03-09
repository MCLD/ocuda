using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.DigitalDisplays
{
    public class AssetAssociationViewModel
    {
        public DigitalDisplayAsset DigitalDisplayAsset { get; set; }
        public ICollection<DigitalDisplayAssetSet> DigitalDisplayAssetSets { get; set; }
        public ICollection<DigitalDisplaySet> DigitalDisplaySets { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
            "CA1056:URI-like properties should not be strings",
            Justification = "URL is coming back from the UI as a string")]
        public string ImageUrl { get; set; }
    }
}