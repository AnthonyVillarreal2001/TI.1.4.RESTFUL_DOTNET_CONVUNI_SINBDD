using System.Text.Json.Serialization;

namespace CLIESC_CONVUNI_DOTNET_GR69.Models
{
    public sealed class ConversionResponse
    {
        public string? Type { get; set; }
        public string? Input { get; set; }
        public string? Output { get; set; }
        public double Value { get; set; }
        public string? FromUnit { get; set; }
        public string? ToUnit { get; set; }
        public string? Error { get; set; }
        [JsonIgnore] public string? Raw { get; set; }
    }
}
