
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RAIZA.Interfaces;
using RAIZA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RAIZA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TareaController : ControllerBase
    {
        private readonly ITarea_I _tareaRepository;
        private readonly IEntregaTareaI _entregaTareaRepository;

        public TareaController(ITarea_I tareaRepository, IEntregaTareaI entregaTareaRepository)
        {
            _tareaRepository = tareaRepository;
            _entregaTareaRepository = entregaTareaRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetTareas()
        {
            try
            {
                var tareas = await _tareaRepository.GetTareas();
                return Ok(tareas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al consultar las tareas.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTareaById([FromRoute] int id)
        {
            try
            {
                var tarea = await _tareaRepository.GetTareaById(id);

                if (tarea == null)
                {
                    return NotFound(new { mensaje = "No se encontró la tarea solicitada." });
                }

                return Ok(tarea);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al consultar la tarea.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpGet("modulo/{idModulo}")]
        public async Task<IActionResult> GetTareasByModuloId([FromRoute] int idModulo)
        {
            try
            {
                var tareas = await _tareaRepository.GetTareasByModuloId(idModulo);
                return Ok(tareas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al consultar las tareas por módulo.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        // ==========================================
        // NUEVOS ENDPOINTS PARA EL PORTAL DEL ESTUDIANTE
        // ==========================================

        [HttpGet("mis-tareas")]
        [Authorize] // Exige que el estudiante haya iniciado sesión con su token JWT
        public async Task<IActionResult> GetMisTareas()
        {
            try
            {
                // El claim se llama "IdUsuario" (lo emite AuthController al crear el JWT).
                // Antes se leía "IdEstudiante"/NameIdentifier, que no existen en el token:
                // el endpoint devolvía 401 para cualquier sesión válida.
                if (!int.TryParse(User.FindFirst("IdUsuario")?.Value, out int estudianteId))
                {
                    return Unauthorized(new { mensaje = "No se pudo identificar al estudiante autenticado." });
                }

                var tareas = await _tareaRepository.GetTareas();

                return Ok(tareas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al consultar las tareas del estudiante.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpPost("entregar")]
        [Authorize]
        public async Task<IActionResult> EntregarTarea([FromBody] EntregaDto modelo)
        {
            if (modelo == null || modelo.IdTarea <= 0)
            {
                return BadRequest(new { mensaje = "Los datos de la entrega son inválidos." });
            }

            try
            {
                if (!int.TryParse(User.FindFirst("IdUsuario")?.Value, out int idEstudiante))
                {
                    return Unauthorized(new { mensaje = "Usuario no autorizado." });
                }

                var tareaExistente = await _tareaRepository.GetTareaById(modelo.IdTarea);
                if (tareaExistente == null)
                {
                    return NotFound(new { mensaje = "La tarea especificada no existe." });
                }

                // Upsert real: si este estudiante ya entregó la tarea se actualiza su comentario;
                // si no, se crea la entrega. (Antes la línea estaba comentada y el endpoint
                // devolvía 200 "registrada" sin guardar NADA.)
                var comentario = modelo.Comentario ?? string.Empty;
                if (comentario.Length > 500) comentario = comentario[..500];

                var entregas = await _entregaTareaRepository.GetEntregaTareas();
                var previa = entregas.FirstOrDefault(e => e.idtarea == modelo.IdTarea && e.idestudiante == idEstudiante);

                bool resultado;
                if (previa != null)
                {
                    previa.Comentario = comentario;
                    resultado = await _entregaTareaRepository.UpdateEntregaTarea(previa);
                }
                else
                {
                    resultado = await _entregaTareaRepository.CreateEntregaTarea(new EntregaTarea
                    {
                        idtarea = modelo.IdTarea,
                        idestudiante = idEstudiante,
                        UrlArchivo = string.Empty, // sin archivo: la subida hace POST /EntregaTarea/subir-archivo
                        FechaEntrega = DateTime.UtcNow,
                        Comentario = comentario
                    });
                }

                if (!resultado)
                {
                    return BadRequest(new { mensaje = "No fue posible registrar la entrega." });
                }

                return Ok(new { mensaje = "Tarea entregada y registrada correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al procesar la entrega de la tarea.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        // ==========================================

        // La gestión de tareas es exclusiva del instructor y del administrador.
        [Authorize(Roles = "Administrador,Instructor")]
        [HttpPost]
        public async Task<IActionResult> CreateTarea([FromBody] Tarea tarea)
        {
            if (tarea == null)
            {
                return BadRequest(new { mensaje = "Los datos de la tarea son obligatorios." });
            }

            try
            {
                var resultado = await _tareaRepository.CreateTarea(tarea);

                if (!resultado)
                {
                    return BadRequest(new { mensaje = "No fue posible registrar la tarea." });
                }

                return CreatedAtAction(
                    nameof(GetTareaById),
                    new { id = tarea.idtarea },
                    tarea
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al registrar la tarea.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [Authorize(Roles = "Administrador,Instructor")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTarea([FromRoute] int id, [FromBody] Tarea tarea)
        {
            if (tarea == null)
            {
                return BadRequest(new { mensaje = "Los datos de la tarea son obligatorios." });
            }

            if (id != tarea.idtarea)
            {
                return BadRequest(new { mensaje = "El ID de la URL no coincide con el ID de la tarea enviada." });
            }

            try
            {
                var tareaExistente = await _tareaRepository.GetTareaById(id);

                if (tareaExistente == null)
                {
                    return NotFound(new { mensaje = "No se encontró la tarea a actualizar." });
                }

                var resultado = await _tareaRepository.UpdateTarea(tarea);

                if (!resultado)
                {
                    return BadRequest(new { mensaje = "No fue posible actualizar la tarea." });
                }

                return Ok(new { mensaje = "Tarea actualizada correctamente.", tarea });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al actualizar la tarea.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [Authorize(Roles = "Administrador,Instructor")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTarea([FromRoute] int id)
        {
            try
            {
                var tareaExistente = await _tareaRepository.GetTareaById(id);

                if (tareaExistente == null)
                {
                    return NotFound(new { mensaje = "No se encontró la tarea a eliminar." });
                }

                var resultado = await _tareaRepository.DeleteTarea(id);

                if (!resultado)
                {
                    return BadRequest(new { mensaje = "No fue posible eliminar la tarea." });
                }

                return Ok(new { mensaje = "Tarea eliminada correctamente." });
            }
            catch (DbUpdateException)
            {
                return Conflict(new { mensaje = "No se puede eliminar la tarea porque tiene entregas asociadas. Elimina o reasigna esas entregas primero." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al eliminar la tarea.", detalle = UtilidadesError.Registrar(ex) });
            }
        }
    }

    // Modelo auxiliar para recibir los datos de entrega desde el frontend
    public class EntregaDto
    {
        public int IdTarea { get; set; }
        public string Comentario { get; set; } = string.Empty;
    }
}


