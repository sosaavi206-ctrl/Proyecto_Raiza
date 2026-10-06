using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RAIZA.Interfaces;
using RAIZA.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RAIZA.Controllers
{
    [ApiController]
    [Authorize] // Por defecto se exige un token JWT válido (consistente con la FallbackPolicy global)
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioI _usuarioRepository;

        public UsuarioController(IUsuarioI usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        // ---------------- Helpers de autorización ----------------

        private bool EsAdministrador() => User.IsInRole("Administrador");

        private int? IdUsuarioDeSesion()
        {
            var valor = User.FindFirst("IdUsuario")?.Value;
            return int.TryParse(valor, out var id) ? id : null;
        }

        private bool PuedeAccederAlUsuario(int id)
        {
            if (EsAdministrador()) return true;
            return IdUsuarioDeSesion() == id;
        }

        // Nunca se devuelve el hash de la contraseña al cliente (se entrega vacío).
        private static void OcultarContrasena(Usuario usuario) => usuario.ContrasenaHash = string.Empty;

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> GetUsuarios()
        {
            try
            {
                var usuarios = await _usuarioRepository.GetUsuarios();
                foreach (var usuario in usuarios) OcultarContrasena(usuario);
                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al consultar los usuarios.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetUsuarioById([FromRoute] int id)
        {
            try
            {
                // Solo el propio usuario o un administrador pueden consultar el perfil.
                if (!PuedeAccederAlUsuario(id))
                {
                    return StatusCode(403, new { mensaje = "No tienes permiso para consultar este usuario." });
                }

                var usuario = await _usuarioRepository.GetUsuarioById(id);

                if (usuario == null)
                {
                    return NotFound(new { mensaje = "No se encontró el usuario solicitado." });
                }

                OcultarContrasena(usuario);
                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al consultar el usuario.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpGet("correo/{correo}")]
        public async Task<IActionResult> GetUsuarioByCorreo([FromRoute] string correo)
        {
            try
            {
                // Solo el propio usuario (su correo viaja en el claim "name") o un administrador.
                var esPropio = User.Identity?.Name?.Equals(correo, System.StringComparison.OrdinalIgnoreCase) == true;
                if (!EsAdministrador() && !esPropio)
                {
                    return StatusCode(403, new { mensaje = "No tienes permiso para consultar este correo." });
                }

                var usuario = await _usuarioRepository.GetUsuarioByCorreo(correo);

                if (usuario == null)
                {
                    return NotFound(new { mensaje = "No se encontró un usuario con ese correo electrónico." });
                }

                OcultarContrasena(usuario);
                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al consultar el usuario por correo.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpGet("rol/{rol}")]
        [Authorize(Roles = "Administrador,Instructor")]
        public async Task<IActionResult> GetUsuariosByRol([FromRoute] string rol)
        {
            try
            {
                var usuarios = await _usuarioRepository.GetUsuariosByRol(rol);
                foreach (var usuario in usuarios) OcultarContrasena(usuario);
                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al consultar usuarios por rol.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CreateUsuario([FromBody] Usuario usuario)
        {
            if (usuario == null)
            {
                return BadRequest(new { mensaje = "Los datos del usuario son obligatorios." });
            }

            // Validación explícita: el modelo ya no trae [Required] (ver Models/Usuario.cs),
            // así que el alta valida aquí. Los valores de rol/estado coinciden con los CHECK
            // de la BD (usuario.rol, usuario.estado).
            if (string.IsNullOrWhiteSpace(usuario.Correo) || !MailAddress.TryCreate(usuario.Correo.Trim(), out _))
            {
                return BadRequest(new { mensaje = "El correo electrónico no es válido." });
            }

            if (string.IsNullOrWhiteSpace(usuario.ContrasenaHash))
            {
                return BadRequest(new { mensaje = "La contraseña es obligatoria." });
            }

            // Misma regla que el registro público (AutoController: PasswordLongitudMinima = 8).
            if (usuario.ContrasenaHash.Length < 8)
            {
                return BadRequest(new { mensaje = "La contraseña debe tener al menos 8 caracteres." });
            }

            if (string.IsNullOrWhiteSpace(usuario.Nombre))
            {
                return BadRequest(new { mensaje = "El nombre es obligatorio." });
            }

            var rolValido = new[] { "Administrador", "Instructor", "Estudiante" };
            if (string.IsNullOrWhiteSpace(usuario.Rol) || !rolValido.Contains(usuario.Rol.Trim()))
            {
                return BadRequest(new { mensaje = "El rol debe ser Administrador, Instructor o Estudiante." });
            }

            var estadoValido = new[] { "Activo", "Inactivo" };
            if (!string.IsNullOrWhiteSpace(usuario.Estado) && !estadoValido.Contains(usuario.Estado.Trim()))
            {
                return BadRequest(new { mensaje = "El estado debe ser Activo o Inactivo." });
            }

            usuario.Correo = usuario.Correo.Trim();

            // Mismo control que el registro público: correo duplicado = 409 (sin esto, el
            // índice único de la BD revienta y el cliente veía un 500 genérico).
            if (await _usuarioRepository.GetUsuarioByCorreo(usuario.Correo) != null)
            {
                return Conflict(new { mensaje = "Ya existe una cuenta con este correo electrónico." });
            }

            try
            {
                var resultado = await _usuarioRepository.CreateUsuario(usuario);

                if (!resultado)
                {
                    return BadRequest(new { mensaje = "No fue posible registrar el usuario." });
                }

                OcultarContrasena(usuario);
                return CreatedAtAction(
                    nameof(GetUsuarioById),
                    new { id = usuario.Id },
                    usuario
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al registrar el usuario.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateUsuario([FromRoute] int id, [FromBody] Usuario usuario)
        {
            if (usuario == null)
            {
                return BadRequest(new { mensaje = "Los datos del usuario son obligatorios." });
            }

            if (id != usuario.Id)
            {
                return BadRequest(new { mensaje = "El ID de la URL no coincide con el ID del usuario enviado." });
            }

            try
            {
                // Solo el propio usuario o un administrador pueden editar el perfil.
                if (!PuedeAccederAlUsuario(id))
                {
                    return StatusCode(403, new { mensaje = "No tienes permiso para editar este usuario." });
                }

                var usuarioExistente = await _usuarioRepository.GetUsuarioById(id);

                if (usuarioExistente == null)
                {
                    return NotFound(new { mensaje = "No se encontró el usuario a actualizar." });
                }

                // Un usuario regular nunca puede cambiarse el rol ni el estado; solo un administrador.
                if (!EsAdministrador())
                {
                    usuario.Rol = usuarioExistente.Rol;
                    usuario.Estado = usuarioExistente.Estado;
                }

                var resultado = await _usuarioRepository.UpdateUsuario(usuario);

                if (!resultado)
                {
                    return BadRequest(new { mensaje = "No fue posible actualizar el usuario." });
                }

                var actualizado = await _usuarioRepository.GetUsuarioById(id);
                if (actualizado != null) OcultarContrasena(actualizado);

                return Ok(new { mensaje = "Usuario actualizado correctamente.", usuario = actualizado });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al actualizar el usuario.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpPatch("{id:int}/estado")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CambiarEstadoUsuario([FromRoute] int id, [FromBody] string nuevoEstado)
        {
            if (string.IsNullOrWhiteSpace(nuevoEstado))
            {
                return BadRequest(new { mensaje = "El nuevo estado es requerido." });
            }

            // Solo los estados admitidos por la BD (varchar(20) sin CHECK pero con este dominio
            // usado por todo el sistema). Evita estados basura y el error de truncamiento >20.
            var estadosValidos = new[] { "Activo", "Inactivo" };
            var estadoNormalizado = estadosValidos
                .FirstOrDefault(e => e.Equals(nuevoEstado.Trim(), System.StringComparison.OrdinalIgnoreCase));
            if (estadoNormalizado == null)
            {
                return BadRequest(new { mensaje = "El estado debe ser Activo o Inactivo." });
            }
            nuevoEstado = estadoNormalizado;

            try
            {
                var resultado = await _usuarioRepository.CambiarEstadoUsuario(id, nuevoEstado);

                if (!resultado)
                {
                    return NotFound(new { mensaje = "No fue posible actualizar el estado. Verifica si el usuario existe." });
                }

                return Ok(new { mensaje = "Estado del usuario actualizado correctamente.", estado = nuevoEstado });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al cambiar el estado del usuario.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteUsuario([FromRoute] int id)
        {
            try
            {
                var usuarioExistente = await _usuarioRepository.GetUsuarioById(id);

                if (usuarioExistente == null)
                {
                    return NotFound(new { mensaje = "No se encontró el usuario a eliminar." });
                }

                var resultado = await _usuarioRepository.DeleteUsuario(id);

                if (!resultado)
                {
                    return BadRequest(new { mensaje = "No fue posible eliminar el usuario." });
                }

                return Ok(new { mensaje = "Usuario eliminado correctamente." });
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    mensaje = "No se puede eliminar el usuario porque tiene registros asociados (por ejemplo, notificaciones u otros datos vinculados). Elimina o reasigna esos registros primero."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al eliminar el usuario.", detalle = UtilidadesError.Registrar(ex) });
            }
        }
    }
}