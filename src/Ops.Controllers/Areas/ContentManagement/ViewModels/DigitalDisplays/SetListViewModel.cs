using System.Collections.Generic;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.DigitalDisplays
{
    public class SetListViewModel
    {
        public IDictionary<int, int> DigitalDisplaySetAssets { get; set; }
        public IDictionary<int, int> DigitalDisplaySetAssetsActive { get; set; }
        public IDictionary<int, int> DigitalDisplaySetDisplays { get; set; }
        public ICollection<Models.Entities.DigitalDisplaySet> DigitalDisplaySets { get; set; }
        public bool HasDigitalDisplayPermissions { get; set; }

        public bool HasPermissions
        {
            get
            {
                return IsSiteManager || HasDigitalDisplayPermissions;
            }
        }

        public bool IsSiteManager { get; set; }

        public int ActiveAssetCount(int digitalDisplaySetId)
        {
            return DigitalDisplaySetAssetsActive?.ContainsKey(digitalDisplaySetId) == true
                ? DigitalDisplaySetAssetsActive[digitalDisplaySetId]
                : 0;
        }

        public int AssetCount(int digitalDisplaySetId)
        {
            return DigitalDisplaySetAssets?.ContainsKey(digitalDisplaySetId) == true
                ? DigitalDisplaySetAssets[digitalDisplaySetId]
                : 0;
        }

        public int DisplayCount(int digitalDisplaySetId)
        {
            if (DigitalDisplaySetDisplays == null)
            {
                return 0;
            }
            return DigitalDisplaySetDisplays.ContainsKey(digitalDisplaySetId)
                ? DigitalDisplaySetDisplays[digitalDisplaySetId]
                : 0;
        }
    }
}