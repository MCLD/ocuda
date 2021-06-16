using System.Text.Json.Serialization;

namespace Ocuda.Utility.Models.Google
{
    public class AddressComponent
    {
        [JsonPropertyName("short_name")]
        public string ShortName { get; set; }
    }
}