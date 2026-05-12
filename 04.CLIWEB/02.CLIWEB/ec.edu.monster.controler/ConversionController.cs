using _04.CLIWEB.ec.edu.monster.modelo;
using _04.CLIWEB.ec.edu.monster.servicio;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace _04.CLIWEB.ec.edu.monster.controler
{
    public class ConversionController : Controller
    {
        private readonly ConUniService _service;

        // Mapeo de los identificadores de la vista (clave) al tipo usado en el servicio REST
        private static readonly Dictionary<string, string> TipoVistaAServicio = new()
        {
            // Longitud
            ["MetroAPie"] = "MetroAPie",
            ["KilometroAMilla"] = "KilometroAMilla",
            ["CentimetroAPulgada"] = "CentimetroAPulgada",
            ["PulgadaACentimetro"] = "PulgadaACentimetro",
            ["PieAMetro"] = "PieAMetro",
            // Peso
            ["KilogramoALibra"] = "KilogramoALibra",
            ["GramoAOnza"] = "GramoAOnza",
            ["ToneladaAKilogramo"] = "ToneladaAKilogramo",
            ["LibraAKilogramo"] = "LibraAKilogramo",
            ["OnzaAGramo"] = "OnzaAGramo",
            // Temperatura
            ["CelsiusAFahrenheit"] = "CelsiusAFahrenheit",
            ["CelsiusAKelvin"] = "CelsiusAKelvin",
            ["CelsiusARankine"] = "CelsiusARankine",
            ["CelsiusAReaumur"] = "CelsiusAReaumur",
            ["FahrenheitACelsius"] = "FahrenheitACelsius"
        };

        public ConversionController(ConUniService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("usuario") == null)
                return RedirectToAction("Index", "Login");

            return View(new Resultado());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string tipo, double? valor)
        {
            if (HttpContext.Session.GetString("usuario") == null)
                return RedirectToAction("Index", "Login");

            if (string.IsNullOrWhiteSpace(tipo) || !valor.HasValue)
            {
                ViewBag.Error = "Debe seleccionar un tipo y un valor válido.";
                return View(new Resultado());
            }

            // Obtener el nombre de tipo para el servicio (debe estar en el diccionario)
            if (!TipoVistaAServicio.TryGetValue(tipo, out var tipoServicio))
            {
                ViewBag.Error = "Tipo de conversión no válido.";
                return View(new Resultado());
            }

            try
            {
                Resultado resultado = await _service.ConvertirAsync(tipoServicio, valor.Value);
                ViewData["viewTipo"] = tipo;
                ViewData["valor"] = valor.Value;
                return View(resultado);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error al procesar la conversión: {ex.Message}";
                ViewData["viewTipo"] = tipo;
                ViewData["valor"] = valor.Value;
                return View(new Resultado());
            }
        }
    }
}