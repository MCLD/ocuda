using System.Text.Json.Serialization;

namespace Ocuda.Utility.Models.Google
{
    public class PlaceDetails
    {
        [JsonPropertyName("url")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
            "CA1056:URI-like properties should not be strings",
            Justification = "This object matches the API")]
        public string Url { get; set; }
    }
}