using System.Text.Json.Serialization;

namespace Ocuda.Utility.Models.Google
{
    public class GoogleMultiResponse<T>
    {
        [JsonPropertyName("info_messages")]
        public string[] InfoMessages { get; set; }

        [JsonPropertyName("results")]
        public T[] Results { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}