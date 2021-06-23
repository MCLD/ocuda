using System;
using System.Text.Json.Serialization;

namespace Ocuda.Ops.Service.Models.Screenly.v11
{
    public class AssetModel
    {
        [JsonPropertyName("asset_id")]
        public string AssetId { get; set; }

        [JsonPropertyName("duration")]
        public object Duration { get; set; }

        [JsonPropertyName("end_date")]
        public DateTime EndDate { get; set; }

        [JsonPropertyName("is_active")]
        public int IsActive { get; set; }

        [JsonPropertyName("is_enabled")]
        public int IsEnabled { get; set; }

        [JsonPropertyName("is_processing")]
        public int IsProcessing { get; set; }

        [JsonPropertyName("mimetype")]
        public string MimeType { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("no_cache")]
        public int NoCache { get; set; }

        [JsonPropertyName("play_order")]
        public int PlayOrder { get; set; }

        [JsonPropertyName("skip_asset_check")]
        public int SkipAssetCheck { get; set; }

        [JsonPropertyName("start_date")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("uri")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
            "CA1056:URI-like properties should not be strings",
            Justification = "This matches the Screenly API")]
        public string Uri { get; set; }
    }
}