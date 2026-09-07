using System.Text.Json.Serialization;

namespace WebClient.DTOs.Response
{
    public class ODataResponse<T>
    {
        [JsonPropertyName("value")]
        public List<T> Value { get; set; }
    }
}
