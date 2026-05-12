using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CLIESC_CONVUNI_DOTNET_GR69.Models;

namespace CLIESC_CONVUNI_DOTNET_GR69.Controllers
{
    public class ConversionController
    {
        // Ajusta la URL base a tu servidor REST (sin ruta al final)
        private const string BASE_URL = "https://localhost:7118/api/";

        private readonly HttpClient _http;
        private static readonly JsonSerializerOptions SerOpts = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public ConversionController()
        {
            var handler = new HttpClientHandler
            {
                // Acepta certificado de desarrollo autofirmado
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };
            _http = new HttpClient(handler)
            {
                BaseAddress = new Uri(BASE_URL)
            };
        }

        // Autenticación
        public async Task<LoginResponse> LoginAsync(string user, string pass, CancellationToken ct = default)
        {
            var payload = new { username = user, password = pass };
            var response = await _http.PostAsJsonAsync("auth/login", payload, SerOpts, ct);
            var content = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error de autenticación: {response.StatusCode} - {content}");
            var result = JsonSerializer.Deserialize<LoginResponse>(content, SerOpts);
            return result!;
        }

        // Conversión
        public async Task<ConversionResponse> ConvertAsync(ConversionRequest req, CancellationToken ct = default)
        {
            var response = await _http.PostAsJsonAsync("conversion/convert", req, SerOpts, ct);
            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
                return new ConversionResponse { Error = $"HTTP {(int)response.StatusCode}: {body}" };

            var ok = JsonSerializer.Deserialize<ConversionResponse>(body, SerOpts)
                     ?? new ConversionResponse { Error = "Respuesta vacía" };
            ok.Raw = body; // para depurar
            return ok;
        }

        // Método síncrono wrapper (opcional)
        public ConversionResponse Convert(ConversionRequest req)
        {
            return Task.Run(() => ConvertAsync(req)).Result;
        }
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string Token { get; set; } = "";
    }
}