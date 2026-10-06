using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RAIZA.Interfaces;
using RAIZA.Models;

namespace RAIZA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificacionController : ControllerBase
    {
        private readonly INotificacionI _notificacionRepository;

        public NotificacionController(INotificacionI notificacionRepository)
        {
            _notificacionRepository = notificacionRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotificaciones()
        {
            try
            {
                var notificaciones = await _notificacionRepository.GetNotificaciones();

                // Un usuario solo ve SUS notificaciones; los gestores, todas.
                if (!Autorizacion.EsGestor(User))
                {
                    var idPropio = Autorizacion.IdUsuario(User);
                    if (idPropio == null) return Unauthorized(new { mensaje = "No se pudo identificar al usuario de la sesión." });
                    notificaciones = notificaciones.Where(n => n.idusuario == idPropio.Value).ToList();
                }

                return Ok(notificaciones);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al consultar las notificaciones.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetNotificacionById([FromRoute] int id)
        {
            try
            {
                var notificacion = await _notificacionRepository.GetNotificacionById(id);

                if (notificacion == null)
                {
                    return NotFound(new { mensaje = "No se encontró la notificación solicitada." });
                }

                // Solo el destinatario o un gestor pueden consultar la notificación.
                if (!Autorizacion.EsGestor(User))
                {
                    var idPropio = Autorizacion.IdUsuario(User);
                    if (idPropio == null || notificacion.idusuario != idPropio.Value)
                    {
                        return StatusCode(403, new { mensaje = "No tienes permiso para consultar esta notificación." });
                    }
                }

                return Ok(notificacion);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al consultar la notificación.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Instructor")] // antes cualquier autenticado podía notificar a usuarios ajenos
        public async Task<IActionResult> CreateNotificacion([FromBody] Notificacion notificacion)
        {
            if (notificacion == null)
            {
                return BadRequest(new { mensaje = "Los datos de la notificación son obligatorios." });
            }

            try
            {
                var resultado = await _notificacionRepository.CreateNotificacion(notificacion);

                if (!resultado)
                {
                    return BadRequest(new { mensaje = "No fue posible registrar la notificación." });
                }

                return CreatedAtAction(
                    nameof(GetNotificacionById),
                    new { id = notificacion.Idnotificacion },
                    notificacion
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al registrar la notificación.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Instructor")]
        public async Task<IActionResult> UpdateNotificacion([FromRoute] int id, [FromBody] Notificacion notificacion)
        {
            if (notificacion == null)
            {
                return BadRequest(new { mensaje = "Los datos de la notificación son obligatorios." });
            }

            if (id != notificacion.Idnotificacion)
            {
                return BadRequest(new { mensaje = "El ID de la URL no coincide con el ID de la notificación enviada." });
            }

            try
            {
                var notificacionExistente = await _notificacionRepository.GetNotificacionById(id);

                if (notificacionExistente == null)
                {
                    return NotFound(new { mensaje = "No se encontró la notificación a actualizar." });
                }

                var resultado = await _notificacionRepository.UpdateNotificacion(notificacion);

                if (!resultado)
                {
                    return BadRequest(new { mensaje = "No fue posible actualizar la notificación." });
                }

                return Ok(new { mensaje = "Notificación actualizada correctamente.", notificacion });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al actualizar la notificación.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador,Instructor")]
        public async Task<IActionResult> DeleteNotificacion([FromRoute] int id)
        {
            try
            {
                var notificacionExistente = await _notificacionRepository.GetNotificacionById(id);

                if (notificacionExistente == null)
                {
                    return NotFound(new { mensaje = "No se encontró la notificación a eliminar." });
                }

                var resultado = await _notificacionRepository.DeleteNotificacion(id);

                if (!resultado)
                {
                    return BadRequest(new { mensaje = "No fue posible eliminar la notificación." });
                }

                return Ok(new { mensaje = "Notificación eliminada correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al eliminar la notificación.", detalle = UtilidadesError.Registrar(ex) });
            }
        }
    }
}