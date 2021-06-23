using System.Text.Json.Serialization;

namespace Ocuda.Utility.Models.Google
{
    public class GeocodeResult
    {
        [JsonPropertyName("address_components")]
        public AddressComponent[] AddressComponents { get; set; }

        [JsonPropertyName("geometry")]
        public Geometry Geometry { get; set; }

        [JsonPropertyName("types")]
        public string[] Types { get; set; }
    }
}