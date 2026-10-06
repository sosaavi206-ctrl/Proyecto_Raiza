using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using RAIZA.Interfaces;
using RAIZA.Models;



namespace RAIZA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InstructorController : ControllerBase
    {
        private readonly IInstructor_I _instructorRepository;
        private readonly IUsuarioI _usuarioRepository;

        public InstructorController(IInstructor_I instructorRepository, IUsuarioI usuarioRepository)
        {
            _instructorRepository = instructorRepository;
            _usuarioRepository = usuarioRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetInstructores()
        {
            try
            {
                var instructores = await _instructorRepository.GetInstructores();
                return Ok(instructores);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al consultar los instructores.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetInstructorById([FromRoute] int id)
        {
            try
            {
                var instructor = await _instructorRepository.GetInstructorById(id);

                if (instructor == null)
                {
                    return NotFound(new { mensaje = "No se encontró el instructor solicitado." });
                }

                return Ok(instructor);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al consultar el instructor.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")] // crear cuentas de instructores: solo administrador
        public async Task<IActionResult> CreateInstructor([FromBody] Instructor instructor)
        {
            if (instructor == null)
            {
                return BadRequest(new { mensaje = "Los datos del instructor son obligatorios." });
            }

            // idinstructor NO es identity (columna id): sigue la convención id = usuario.Id.
            if (instructor.idinstructor <= 0)
            {
                return BadRequest(new { mensaje = "Debes indicar el id de usuario (idinstructor); debe coincidir con un usuario existente." });
            }

            try
            {
                // El id debe pertenecer a un usuario real con rol Instructor (invariante
                // perfil.id == usuario.Id); si no, quedaría un perfil huérfano.
                var usuario = await _usuarioRepository.GetUsuarioById(instructor.idinstructor);
                if (usuario == null || !string.Equals(usuario.Rol, "Instructor", System.StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { mensaje = "No existe un usuario con ese id y rol Instructor. Crea primero la cuenta de usuario (POST /api/Usuario)." });
                }

                if (await _instructorRepository.GetInstructorById(instructor.idinstructor) != null)
                {
                    return Conflict(new { mensaje = "Ya existe el perfil de instructor con ese id." });
                }

                var resultado = await _instructorRepository.CreateInstructor(instructor);

                if (!resultado)
                {
                    return BadRequest(new { mensaje = "No fue posible registrar el instructor." });
                }

                return CreatedAtAction(
                    nameof(GetInstructorById),
                    new { id = instructor.idinstructor },
                    instructor
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al guardar el instructor.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> UpdateInstructor([FromRoute] int id, [FromBody] Instructor instructor)
        {
            if (instructor == null)
            {
                return BadRequest(new { mensaje = "Los datos del instructor son obligatorios." });
            }

            if (id != instructor.idinstructor)
            {
                return BadRequest(new { mensaje = "El ID de la URL no coincide con el ID del instructor." });
            }

            try
            {
                var instructorExistente = await _instructorRepository.GetInstructorById(id);

                if (instructorExistente == null)
                {
                    return NotFound(new { mensaje = "No se encontró el instructor a actualizar." });
                }

                var resultado = await _instructorRepository.UpdateInstructor(instructor);

                if (!resultado)
                {
                    return BadRequest(new { mensaje = "No fue posible actualizar el instructor." });
                }

                return Ok(new { mensaje = "Instructor actualizado correctamente.", instructor });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al actualizar el instructor.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteInstructor([FromRoute] int id)
        {
            try
            {
                var instructorExistente = await _instructorRepository.GetInstructorById(id);

                if (instructorExistente == null)
                {
                    return NotFound(new { mensaje = "No se encontró el instructor a eliminar." });
                }

                var resultado = await _instructorRepository.DeleteInstructor(id);

                if (!resultado)
                {
                    return BadRequest(new { mensaje = "No fue posible eliminar el instructor." });
                }

                return Ok(new { mensaje = "Instructor eliminado correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al eliminar el instructor.", detalle = UtilidadesError.Registrar(ex) });
            }
        }
    }
}