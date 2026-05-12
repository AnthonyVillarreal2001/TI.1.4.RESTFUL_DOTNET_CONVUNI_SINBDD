using Microsoft.AspNetCore.Mvc;
using WS_ConUni_REST_DOTNET.Models;

namespace WS_ConUni_REST_DOTNET.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (request.Username == "MONSTER" && request.Password == "MONSTER9")
            {
                // En un sistema real devolverías un token JWT.
                return Ok(new { success = true, message = "Autenticación exitosa", token = "fake-jwt-token" });
            }
            return Unauthorized(new { success = false, message = "Usuario o contraseña incorrectos" });
        }
    }
}