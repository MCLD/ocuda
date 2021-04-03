using System;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.DigitalDisplays
{
    public class UpdatedAssetAssociationsModel
    {
        public int AssetId { get; set; }
        public bool Enabled { get; set; }
        public DateTime EndDateTimeUTC { get; set; }
        public string Message { get; set; }
        public int SetId { get; set; }
        public DateTime StartDateTimeUTC { get; set; }
        public bool Success { get; set; }
        public int TimeZoneOffsetMinutes { get; set; }
    }
}