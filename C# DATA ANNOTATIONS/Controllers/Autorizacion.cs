using System.Security.Claims;

namespace RAIZA.Controllers
{
    /// <summary>
    /// Utilidades para leer el rol y el id del usuario desde el JWT.
    /// El claim del id de usuario se llama "IdUsuario" (lo emite AuthController).
    /// </summary>
    internal static class Autorizacion
    {
        /// <summary>Administradores e instructores pueden gestionar recursos de terceros.</summary>
        public static bool EsGestor(ClaimsPrincipal usuario) =>
            usuario.IsInRole("Administrador") || usuario.IsInRole("Instructor");

        /// <summary>Id del usuario de la sesión según el token; null si no es legible.</summary>
        public static int? IdUsuario(ClaimsPrincipal usuario)
        {
            var valor = usuario.FindFirst("IdUsuario")?.Value;
            return int.TryParse(valor, out var id) ? id : null;
        }
    }
}
