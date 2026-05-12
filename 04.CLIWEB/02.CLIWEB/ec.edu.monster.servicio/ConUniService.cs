using _04.CLIWEB.ec.edu.monster.modelo;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace _04.CLIWEB.ec.edu.monster.servicio
{
    public class ConUniService
    {
        private readonly HttpClient _httpClient;

        public ConUniService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Login
        public async Task<LoginResponse> LoginAsync(string usuario, string clave)
        {
            var payload = new { username = usuario, password = clave };
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", payload);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<LoginResponse>();
        }

        // Conversión (las 15 operaciones, el tipo debe ser el nombre exacto, ej: "MetroAPie")
        public async Task<Resultado> ConvertirAsync(string tipo, double valor)
        {
            var payload = new { type = tipo, value = valor };
            var response = await _httpClient.PostAsJsonAsync("api/conversion/convert", payload);
            response.EnsureSuccessStatusCode();
            var resultado = await response.Content.ReadFromJsonAsync<Resultado>();
            return resultado ?? new Resultado();
        }
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string Token { get; set; } = "";
    }
}