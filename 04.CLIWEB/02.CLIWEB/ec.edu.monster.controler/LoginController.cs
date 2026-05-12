using Microsoft.AspNetCore.Mvc;
using _04.CLIWEB.ec.edu.monster.servicio;
using System.Threading.Tasks;

namespace _04.CLIWEB.ec.edu.monster.controler
{
    public class LoginController : Controller
    {
        private readonly ConUniService _service;

        public LoginController(ConUniService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string usuario, string clave)
        {
            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(clave))
            {
                ViewBag.Error = "Todos los campos son obligatorios.";
                return View();
            }

            try
            {
                var login = await _service.LoginAsync(usuario, clave);
                if (login.Success)
                {
                    HttpContext.Session.SetString("usuario", usuario);
                    return RedirectToAction("Index", "Conversion");
                }
                else
                {
                    ViewBag.Error = login.Message ?? "Credenciales incorrectas.";
                }
            }
            catch
            {
                ViewBag.Error = "No se pudo conectar con el servidor. Revisa que el servicio REST esté activo.";
            }

            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}