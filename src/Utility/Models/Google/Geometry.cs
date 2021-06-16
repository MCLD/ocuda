using System.Text.Json.Serialization;

namespace Ocuda.Utility.Models.Google
{
    public class Geometry
    {
        [JsonPropertyName("location")]
        public Location Location { get; set; }
    }
}