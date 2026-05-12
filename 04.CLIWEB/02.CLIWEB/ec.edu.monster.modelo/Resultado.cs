using System.Text.Json.Serialization;

namespace _04.CLIWEB.ec.edu.monster.modelo
{
    public class Resultado
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("input")]
        public string Input { get; set; } = string.Empty;

        [JsonPropertyName("output")]
        public string Output { get; set; } = string.Empty;
    }
}
