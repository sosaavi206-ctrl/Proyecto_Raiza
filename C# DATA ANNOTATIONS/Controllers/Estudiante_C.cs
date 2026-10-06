using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RAIZA.Interfaces;
using RAIZA.Models;
using Microsoft.EntityFrameworkCore;



namespace RAIZA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EstudianteController : ControllerBase
    {
        private readonly IEstudiante_I _estudianteRepository;
        private readonly IUsuarioI _usuarioRepository;

        public EstudianteController(IEstudiante_I estudianteRepository, IUsuarioI usuarioRepository)
        {
            _estudianteRepository = estudianteRepository;
            _usuarioRepository = usuarioRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetEstudiantes()
        {
            try
            {
                var estudiantes = await _estudianteRepository.GetEstudiantes();

                // Un estudiante solo ve SU perfil; los gestores (admin/instructor), todos.
                if (!Autorizacion.EsGestor(User))
                {
                    var idPropio = Autorizacion.IdUsuario(User);
                    if (idPropio == null) return Unauthorized(new { mensaje = "No se pudo identificar al usuario de la sesión." });
                    estudiantes = estudiantes.Where(e => e.idestudiante == idPropio.Value).ToList();
                }

                return Ok(estudiantes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al consultar estudiantes.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEstudianteById(int id)
        {
            try
            {
                var estudiante = await _estudianteRepository.GetEstudianteById(id);

                if (estudiante == null)
                {
                    return NotFound(new { mensaje = "No se encontró el estudiante." });
                }

                // Solo el propio estudiante o un gestor pueden consultar el perfil.
                if (!Autorizacion.EsGestor(User))
                {
                    var idPropio = Autorizacion.IdUsuario(User);
                    if (idPropio == null || estudiante.idestudiante != idPropio.Value)
                    {
                        return StatusCode(403, new { mensaje = "No tienes permiso para consultar este estudiante." });
                    }
                }

                return Ok(estudiante);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al consultar el estudiante.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Instructor")]
        public async Task<IActionResult> CreateEstudiante([FromBody] Estudiante estudiante)
        {
            if (estudiante == null)
            {
                return BadRequest(new { mensaje = "Los datos del estudiante son obligatorios." });
            }

            // estudiante.id NO es identity: debe ser el id de un usuario existente. Sin esta
            // validación, un cuerpo sin id insertaba una fila con id=0 (perfil huérfano).
            if (estudiante.idestudiante <= 0)
            {
                return BadRequest(new { mensaje = "Debes indicar el id de usuario (idestudiante); debe coincidir con un usuario existente." });
            }

            try
            {
                // El id debe pertenecer a un usuario real con rol Estudiante (invariante
                // estudiante.id == usuario.Id); si no, quedaría un perfil huérfano.
                var usuario = await _usuarioRepository.GetUsuarioById(estudiante.idestudiante);
                if (usuario == null || !string.Equals(usuario.Rol, "Estudiante", System.StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { mensaje = "No existe un usuario con ese id y rol Estudiante. Crea primero la cuenta de usuario (POST /api/Usuario)." });
                }

                if (await _estudianteRepository.GetEstudianteById(estudiante.idestudiante) != null)
                {
                    return Conflict(new { mensaje = "Ya existe el perfil de estudiante con ese id." });
                }

                var resultado = await _estudianteRepository.CreateEstudiante(estudiante);

                if (!resultado)
                {
                    return BadRequest(new { mensaje = "No fue posible crear el estudiante." });
                }

                return CreatedAtAction(
                    nameof(GetEstudianteById),
                    new { id = estudiante.idestudiante },
                    estudiante
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al registrar el estudiante.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Instructor")] // antes cualquier autenticado: podía marcarse premium/ajeno (IDOR)
        public async Task<IActionResult> UpdateEstudiante(int id, [FromBody] Estudiante estudiante)
        {
            if (estudiante == null)
            {
                return BadRequest(new { mensaje = "Los datos del estudiante son obligatorios." });
            }

            if (id != estudiante.idestudiante)
            {
                return BadRequest(new { mensaje = "El ID de la URL no coincide con el ID del estudiante." });
            }

            try
            {
                var estudianteExistente = await _estudianteRepository.GetEstudianteById(id);

                if (estudianteExistente == null)
                {
                    return NotFound(new { mensaje = "No se encontró el estudiante a actualizar." });
                }

                var resultado = await _estudianteRepository.UpdateEstudiante(estudiante);

                if (!resultado)
                {
                    return BadRequest(new { mensaje = "No fue posible actualizar el estudiante." });
                }

                return Ok(new { mensaje = "Estudiante actualizado correctamente.", estudiante });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al actualizar el estudiante.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador,Instructor")] // antes cualquier autenticado podía borrar perfiles de estudiantes (IDOR)
        public async Task<IActionResult> DeleteEstudiante(int id)
        {
            try
            {
                var estudiante = await _estudianteRepository.GetEstudianteById(id);

                if (estudiante == null)
                {
                    return NotFound(new { mensaje = "No se encontró el estudiante a eliminar." });
                }

                var resultado = await _estudianteRepository.DeleteEstudiante(id);

                if (!resultado)
                {
                    return BadRequest(new { mensaje = "No fue posible eliminar el estudiante." });
                }

                return Ok(new { mensaje = "Estudiante eliminado correctamente." });
            }
            catch (DbUpdateException) 
            {
                return Conflict(new
                {
                    mensaje = "No se puede eliminar el estudiante porque tiene registros asociados (certificados, compras, entregas de tareas, progreso, pedidos de kits o clases en vivo). Elimina o reasigna esos registros primero."
                });
            }
            catch (Exception ex) 
            {
                return StatusCode(500, new { mensaje = "Error interno al eliminar el estudiante.", detalle = UtilidadesError.Registrar(ex) });
            }
        }
    }
    }
