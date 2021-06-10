using System.Text.Json.Serialization;

namespace Ocuda.Utility.Models.Google
{
    public class TextSearchResult
    {
        [JsonPropertyName("formatted_address")]
        public string FormattedAddress { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("place_id")]
        public string PlaceId { get; set; }
    }
}