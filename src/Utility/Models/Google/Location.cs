using System.Text.Json.Serialization;

namespace Ocuda.Utility.Models.Google
{
    public class Location
    {
        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        [JsonPropertyName("lng")]
        public double Lng { get; set; }
    }
}