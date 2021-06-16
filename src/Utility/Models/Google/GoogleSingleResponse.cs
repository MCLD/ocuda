using System.Text.Json.Serialization;

namespace Ocuda.Utility.Models.Google
{
    public class GoogleSingleResponse<T>
    {
        [JsonPropertyName("info_messages")]
        public string[] InfoMessages { get; set; }

        [JsonPropertyName("result")]
        public T Result { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}