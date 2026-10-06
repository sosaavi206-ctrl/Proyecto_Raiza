using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RAIZA.Interfaces;
using RAIZA.Models;
using RAIZA.Repositories;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace RAIZA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EntregaTareaController : ControllerBase
    {
        private readonly IEntregaTareaI _entregaTareaRepository;

        public EntregaTareaController(IEntregaTareaI entregaTareaRepository)
        {
            _entregaTareaRepository = entregaTareaRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetEntregaTareas()
        {
            try
            {
                var entregas = await _entregaTareaRepository.GetEntregaTareas();

                // Un estudiante solo ve SUS entregas; los gestores (admin/instructor), todas.
                if (!Autorizacion.EsGestor(User))
                {
                    var idPropio = Autorizacion.IdUsuario(User);
                    if (idPropio == null) return Unauthorized(new { mensaje = "No se pudo identificar al usuario de la sesión." });
                    entregas = entregas.Where(e => e.idestudiante == idPropio.Value).ToList();
                }

                return Ok(entregas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al consultar las entregas de tareas.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEntregaTareaById([FromRoute] int id)
        {
            try
            {
                var entrega = await _entregaTareaRepository.GetEntregaTareaById(id);

                if (entrega == null)
                {
                    return NotFound(new { mensaje = "No se encontró la entrega de tarea solicitada." });
                }

                // Solo el dueño o un gestor pueden consultar la entrega.
                if (!Autorizacion.EsGestor(User))
                {
                    var idPropio = Autorizacion.IdUsuario(User);
                    if (idPropio == null || entrega.idestudiante != idPropio.Value)
                    {
                        return StatusCode(403, new { mensaje = "No tienes permiso para consultar esta entrega." });
                    }
                }

                return Ok(entrega);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al consultar la entrega de tarea.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateEntregaTarea([FromBody] EntregaTarea entregaTarea)
        {
            if (entregaTarea == null)
            {
                return BadRequest(new { mensaje = "Los datos de la entrega son obligatorios." });
            }

            try
            {
                // Para un estudiante, la identidad SIEMPRE sale del token (nunca del body):
                // no puede entregar a nombre de otro ni auto-calificarse. Calificación e
                // instructor solo los asigna un gestor (por PUT).
                var esGestor = User.IsInRole("Administrador") || User.IsInRole("Instructor");
                if (!esGestor)
                {
                    if (!int.TryParse(User.FindFirst("IdUsuario")?.Value, out int idPropio))
                    {
                        return Unauthorized(new { mensaje = "No se pudo identificar al usuario de la sesión." });
                    }

                    entregaTarea.idestudiante = idPropio;
                    entregaTarea.Calificacion = null;
                    entregaTarea.idinstructorcalifica = null;
                    entregaTarea.FechaEntrega = DateTime.UtcNow; // sin fecha inventada por el cliente
                }

                var resultado = await _entregaTareaRepository.CreateEntregaTarea(entregaTarea);

                if (!resultado)
                {
                    return BadRequest(new { mensaje = "No fue posible guardar la entrega de tarea." });
                }

                return CreatedAtAction(
                    nameof(GetEntregaTareaById),
                    new { id = entregaTarea.Identregatarea },
                    entregaTarea
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al registrar la entrega de tarea.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        // Solo el instructor o el administrador puede calificar/modificar una entrega.
        // Los estudiantes siguen pudiendo CREAR (POST) y ELIMINAR (DELETE) sus propias entregas.
        [Authorize(Roles = "Administrador,Instructor")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEntregaTarea([FromRoute] int id, [FromBody] EntregaTarea entregaTarea)
        {
            if (entregaTarea == null)
            {
                return BadRequest(new { mensaje = "Los datos de la entrega son obligatorios." });
            }

            if (id != entregaTarea.Identregatarea)
            {
                return BadRequest(new { mensaje = "El ID de la URL no coincide con el ID de la entrega enviada." });
            }

            try
            {
                var entregaExistente = await _entregaTareaRepository.GetEntregaTareaById(id);

                if (entregaExistente == null)
                {
                    return NotFound(new { mensaje = "No se encontró la entrega de tarea a actualizar." });
                }

                var resultado = await _entregaTareaRepository.UpdateEntregaTarea(entregaTarea);

                if (!resultado)
                {
                    return BadRequest(new { mensaje = "No fue posible actualizar la entrega de tarea." });
                }

                return Ok(new { mensaje = "Entrega de tarea actualizada correctamente.", entregaTarea });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al actualizar la entrega de tarea.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEntregaTarea([FromRoute] int id)
        {
            try
            {
                var entregaExistente = await _entregaTareaRepository.GetEntregaTareaById(id);

                if (entregaExistente == null)
                {
                    return NotFound(new { mensaje = "No se encontró la entrega de tarea a eliminar." });
                }

                // Solo el dueño de la entrega (claim IdUsuario) o un gestor pueden eliminarla.
                // Antes cualquier autenticado podía borrar entregas ajenas: el comentario del
                // código ("sus propias entregas") no se verificaba en ningún lado.
                var esGestor = User.IsInRole("Administrador") || User.IsInRole("Instructor");
                if (!esGestor)
                {
                    if (!int.TryParse(User.FindFirst("IdUsuario")?.Value, out int idPropio)
                        || entregaExistente.idestudiante != idPropio)
                    {
                        return StatusCode(403, new { mensaje = "Solo puedes eliminar tus propias entregas." });
                    }
                }

                var resultado = await _entregaTareaRepository.DeleteEntregaTarea(id);

                if (!resultado)
                {
                    return BadRequest(new { mensaje = "No fue posible eliminar la entrega de tarea." });
                }

                return Ok(new { mensaje = "Entrega de tarea eliminada correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al eliminar la entrega de tarea.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        // ==========================================
        // SUBIDA DE ARCHIVO (multipart/form-data)
        // Cualquier usuario autenticado puede subir; la URL resultante se guarda en UrlArchivo.
        // ==========================================
        [HttpPost("subir-archivo")]
        [RequestSizeLimit(10 * 1024 * 1024)] // Máximo 10 MB por archivo
        public async Task<IActionResult> SubirArchivo(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
            {
                return BadRequest(new { mensaje = "Selecciona un archivo para subir." });
            }

            // Solo extensiones permitidas (protege contra archivos maliciosos o nombres con rutas)
            string[] permitidas = { ".pdf", ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".docx", ".doc", ".zip" };
            string ext = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (!permitidas.Contains(ext))
            {
                return BadRequest(new { mensaje = "Tipo de archivo no permitido. Usa: PDF, JPG, PNG, GIF, MP4, DOCX, DOC o ZIP." });
            }

            try
            {
                string carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "entregas");
                Directory.CreateDirectory(carpeta);

                // Nombre único en servidor: evita colisiones y nombres de archivo maliciosos
                string nombreServidor = Guid.NewGuid().ToString("N") + ext;
                string rutaCompleta = Path.Combine(carpeta, nombreServidor);

                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await archivo.CopyToAsync(stream);
                }

                string url = $"{Request.Scheme}://{Request.Host}/entregas/{nombreServidor}";

                return Ok(new
                {
                    url,
                    nombreOriginal = archivo.FileName,
                    tamaño = archivo.Length
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al guardar el archivo.", detalle = UtilidadesError.Registrar(ex) });
            }
        }
    }
}