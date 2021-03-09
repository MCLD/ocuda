using System.Collections.Generic;
using System.Globalization;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.DigitalDisplays
{
    public class AssetListViewModel
    {
        public ICollection<DigitalDisplayAsset> DigitalDisplayAssets { get; set; }
        public IDictionary<int, int> DigitalDisplaySetAssets { get; set; }
        public IDictionary<int, string> ImageUrls { get; set; }
        public PaginateModel PaginateModel { get; set; }

        public string GetSetCount(int assetId)
        {
            return DigitalDisplaySetAssets.ContainsKey(assetId)
                ? DigitalDisplaySetAssets[assetId].ToString(CultureInfo.InvariantCulture)
                : "0";
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
            "CA1055:URI-like return values should not be strings",
            Justification = "URL is coming back from the UI as a string")]
        public string GetUrlPath(int assetId)
        {
            return ImageUrls.ContainsKey(assetId) ? ImageUrls[assetId] : null;
        }
    }
}