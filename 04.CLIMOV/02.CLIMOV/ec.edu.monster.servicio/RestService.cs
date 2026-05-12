using _02.CLIMOV.Utils;
using System.Text;
using System.Text.Json;

namespace _02.CLIMOV.Servicio
{
    public class RestService : IRestService
    {
        private readonly HttpClient _httpClient;

        public RestService()
        {
            var handler = new HttpClientHandler
            {
                // Acepta certificados de desarrollo
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };
            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(15)
            };
        }

        public async Task<double> ConvertirAsync(string type, double value)
        {
            var payload = new { type, value };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(ApiConfig.BaseUrl, content);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error del servidor: {response.StatusCode} - {body}");

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            // El servidor devuelve { "value": <double>, ... }
            if (root.TryGetProperty("value", out var valProp) && valProp.TryGetDouble(out var result))
                return Math.Round(result, 2, MidpointRounding.AwayFromZero);

            // Fallback: intentar extraer del campo "output" (si existe)
            if (root.TryGetProperty("output", out var outputProp))
            {
                var text = outputProp.GetString() ?? "";
                var numberPart = text.Split(' ')[0].Replace(',', '.');
                if (double.TryParse(numberPart,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var fallbackResult))
                    return Math.Round(fallbackResult, 2, MidpointRounding.AwayFromZero);
            }

            throw new Exception("No se pudo leer el valor convertido de la respuesta.");
        }

        public async Task<bool> ValidarConexionAsync()
        {
            try
            {
                await ConvertirAsync("MetroAPie", 1.0);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}