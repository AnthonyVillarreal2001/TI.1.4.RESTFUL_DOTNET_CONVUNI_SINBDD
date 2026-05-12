using Microsoft.AspNetCore.Mvc;
using WS_ConUni_REST_DOTNET.Models;
using WS_ConUni_REST_DOTNET.Services;

namespace WS_ConUni_REST_DOTNET.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConversionController : ControllerBase
    {
        private readonly ConversionService _service = new();

        [HttpPost("convert")]
        public IActionResult Convert([FromBody] ConversionRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Type))
                return BadRequest(new { error = "Debe enviar 'type' y 'value'" });

            string type = request.Type.Trim();
            double value = request.Value;

            // Diccionario de conversiones -> devuelve (resultado, unidad origen, unidad destino)
            var conversions = new Dictionary<string, (Func<double, double> func, string from, string to)>
            {
                // Longitud
                ["MetroAPie"] = (_service.MetroAPie, "m", "ft"),
                ["KilometroAMilla"] = (_service.KilometroAMilla, "km", "mi"),
                ["CentimetroAPulgada"] = (_service.CentimetroAPulgada, "cm", "in"),
                ["PulgadaACentimetro"] = (_service.PulgadaACentimetro, "in", "cm"),
                ["PieAMetro"] = (_service.PieAMetro, "ft", "m"),

                // Masa
                ["KilogramoALibra"] = (_service.KilogramoALibra, "kg", "lb"),
                ["GramoAOnza"] = (_service.GramoAOnza, "g", "oz"),
                ["ToneladaAKilogramo"] = (_service.ToneladaAKilogramo, "ton", "kg"),
                ["LibraAKilogramo"] = (_service.LibraAKilogramo, "lb", "kg"),
                ["OnzaAGramo"] = (_service.OnzaAGramo, "oz", "g"),

                // Temperatura
                ["CelsiusAFahrenheit"] = (_service.CelsiusAFahrenheit, "°C", "°F"),
                ["CelsiusAKelvin"] = (_service.CelsiusAKelvin, "°C", "K"),
                ["CelsiusARankine"] = (_service.CelsiusARankine, "°C", "°Ra"),
                ["CelsiusAReaumur"] = (_service.CelsiusAReaumur, "°C", "°Re"),
                ["FahrenheitACelsius"] = (_service.FahrenheitACelsius, "°F", "°C"),
            };

            if (!conversions.TryGetValue(type, out var conv))
            {
                return BadRequest(new
                {
                    error = $"Tipo de conversión '{type}' no soportado.",
                    supported = conversions.Keys.ToArray()
                });
            }

            double result = conv.func(value);
            return Ok(new
            {
                type,
                input = $"{value:F4} {conv.from}",
                output = $"{result:F4} {conv.to}",
                value = result,
                fromUnit = conv.from,
                toUnit = conv.to
            });
        }
    }
}