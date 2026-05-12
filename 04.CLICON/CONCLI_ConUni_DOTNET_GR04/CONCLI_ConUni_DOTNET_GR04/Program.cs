using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConUniClient
{
    internal class Program
    {
        // Configuración del servidor REST
        private const string BASE_URL = "https://localhost:7118/api";
        private const int MAX_LOGIN_TRIES = 3;

        private static readonly HttpClient HTTP = new(new HttpClientHandler
        {
            // Acepta certificado de desarrollo autofirmado
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        })
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        private static string _token = string.Empty;

        // Mapa de las 15 conversiones con su nombre de tipo para el servidor
        private static readonly List<(int Num, string Descripcion, string TypeKey, string From, string To)> Conversiones = new()
        {
            (1,  "Metros → Pies",              "MetroAPie",          "m",  "ft"),
            (2,  "Kilómetros → Millas",        "KilometroAMilla",    "km", "mi"),
            (3,  "Centímetros → Pulgadas",     "CentimetroAPulgada", "cm", "in"),
            (4,  "Pulgadas → Centímetros",     "PulgadaACentimetro", "in", "cm"),
            (5,  "Pies → Metros",              "PieAMetro",          "ft", "m"),
            (6,  "Kilogramos → Libras",        "KilogramoALibra",    "kg", "lb"),
            (7,  "Gramos → Onzas",             "GramoAOnza",         "g",  "oz"),
            (8,  "Toneladas → Kilogramos",     "ToneladaAKilogramo", "ton","kg"),
            (9,  "Libras → Kilogramos",        "LibraAKilogramo",    "lb", "kg"),
            (10, "Onzas → Gramos",             "OnzaAGramo",         "oz", "g"),
            (11, "Celsius → Fahrenheit",       "CelsiusAFahrenheit", "°C", "°F"),
            (12, "Celsius → Kelvin",           "CelsiusAKelvin",     "°C", "K"),
            (13, "Celsius → Rankine",          "CelsiusARankine",    "°C", "°Ra"),
            (14, "Celsius → Réaumur",          "CelsiusAReaumur",    "°C", "°Re"),
            (15, "Fahrenheit → Celsius",       "FahrenheitACelsius", "°F", "°C"),
        };

        static async Task Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "Conversor Sullivan – Cliente REST";

            MostrarBanner();

            if (!await LoginAsync())
            {
                Console.WriteLine("\n❌ Demasiados intentos fallidos. Saliendo...");
                return;
            }

            Console.WriteLine("\n✅ Autenticación exitosa.\n");

            bool salir = false;
            while (!salir)
            {
                MostrarMenu();
                Console.Write("👉 Elige opción: ");
                string? op = Console.ReadLine()?.Trim();

                if (op == "0")
                {
                    salir = true;
                    Console.WriteLine("\n👋 ¡Hasta luego!\n");
                    break;
                }

                if (!int.TryParse(op, out int idx) || idx < 1 || idx > 15)
                {
                    Console.WriteLine("⚠️  Opción no válida. Intenta de nuevo.\n");
                    continue;
                }

                var conv = Conversiones[idx - 1];
                double valor = LeerDouble($"🔢 {conv.Descripcion} – Ingresa el valor: ");

                try
                {
                    Console.Write("⏳ Consultando...");
                    var resultado = await ConvertirAsync(conv.TypeKey, valor);
                    Console.WriteLine("\r✅ Conversión realizada:\n");
                    Console.WriteLine($"   {valor:F4} {conv.From} → {resultado.Value:F4} {conv.To}");
                    Console.WriteLine($"   📋 Detalle: {resultado.Input} → {resultado.Output}\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\r❌ Error: {ex.Message}\n");
                }
            }
        }

        // --- Autenticación contra API ---
        private static async Task<bool> LoginAsync()
        {
            for (int i = 1; i <= MAX_LOGIN_TRIES; i++)
            {
                Console.Write("\n👤 Usuario: ");
                string? user = Console.ReadLine()?.Trim();
                Console.Write("🔒 Contraseña: ");
                string? pass = LeerPassword();

                if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
                {
                    Console.WriteLine("⚠️  Campos obligatorios.\n");
                    continue;
                }

                try
                {
                    var payload = new { username = user, password = pass };
                    var response = await HTTP.PostAsJsonAsync($"{BASE_URL}/auth/login", payload);

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                        _token = result?.Token ?? "";
                        return true;
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        Console.WriteLine($"❌ Credenciales incorrectas ({i}/{MAX_LOGIN_TRIES})");
                    }
                    else
                    {
                        Console.WriteLine($"🌐 Error del servidor: {response.StatusCode}");
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"🌐 No se pudo conectar al servidor: {ex.Message}");
                    return false;
                }
            }
            return false;
        }

        // --- Conversión contra API ---
        private static async Task<ConversionResultResponse> ConvertirAsync(string typeKey, double value)
        {
            var payload = new { type = typeKey, value };
            var request = new HttpRequestMessage(HttpMethod.Post, $"{BASE_URL}/conversion/convert")
            {
                Content = JsonContent.Create(payload)
            };
            // Agregar token si existiera
            if (!string.IsNullOrEmpty(_token))
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

            var response = await HTTP.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error HTTP {(int)response.StatusCode}: {body}");

            var result = JsonSerializer.Deserialize<ConversionResultResponse>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result == null)
                throw new Exception("Respuesta inválida del servidor.");

            return result;
        }

        // --- Utilidades de UI ---
        private static void MostrarBanner()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(@"
  ╔═══════════════════════════════════╗
  ║   CONVERSOR SULLIVAN ELEGANCE   ║
  ║   Cliente REST · 15 conversiones ║
  ╚═══════════════════════════════════╝
            ");
            Console.ResetColor();
        }

        private static void MostrarMenu()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("═══════════════ MENÚ DE CONVERSIONES ═══════════════");
            Console.ResetColor();

            // Agrupar por categoría
            Console.WriteLine("📏 **Longitud**");
            for (int i = 0; i < 5; i++)
                Console.WriteLine($"  {Conversiones[i].Num,2}) {Conversiones[i].Descripcion}");

            Console.WriteLine("\n⚖️  **Peso**");
            for (int i = 5; i < 10; i++)
                Console.WriteLine($"  {Conversiones[i].Num,2}) {Conversiones[i].Descripcion}");

            Console.WriteLine("\n🌡️  **Temperatura**");
            for (int i = 10; i < 15; i++)
                Console.WriteLine($"  {Conversiones[i].Num,2}) {Conversiones[i].Descripcion}");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n  0) Salir\n");
            Console.ResetColor();
        }

        private static double LeerDouble(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine()?.Trim();
                if (double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
                    return v;
                Console.WriteLine("⚠️  Número inválido. Intenta de nuevo.");
            }
        }

        private static string LeerPassword()
        {
            var pass = new StringBuilder();
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                {
                    pass.Remove(pass.Length - 1, 1);
                    Console.Write("\b \b");
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    pass.Append(key.KeyChar);
                    Console.Write("*");
                }
            } while (key.Key != ConsoleKey.Enter);
            Console.WriteLine();
            return pass.ToString();
        }
    }

    // Modelos para deserializar respuestas
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string Token { get; set; } = "";
    }

    public class ConversionResultResponse
    {
        public string Type { get; set; } = "";
        public string Input { get; set; } = "";
        public string Output { get; set; } = "";
        public double Value { get; set; }
        public string FromUnit { get; set; } = "";
        public string ToUnit { get; set; } = "";
    }
}