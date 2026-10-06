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
    public class LeccionController : ControllerBase
    {
        private readonly ILeccionI _leccionRepository;

        public LeccionController(ILeccionI leccionRepository)
        {
            _leccionRepository = leccionRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetLecciones()
        {
            try
            {
                var lecciones = await _leccionRepository.GetLecciones();
                return Ok(lecciones);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al consultar las lecciones.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLeccionById([FromRoute] int id)
        {
            try
            {
                var leccion = await _leccionRepository.GetLeccionById(id);

                if (leccion == null)
                {
                    return NotFound(new { mensaje = "No se encontró la lección solicitada." });
                }

                return Ok(leccion);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al consultar la lección.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Instructor")] // escritura de catálogo: no para estudiantes
        public async Task<IActionResult> CreateLeccion([FromBody] Leccion leccion)
        {
            if (leccion == null)
            {
                return BadRequest(new { mensaje = "Los datos de la lección son obligatorios." });
            }

            try
            {
                var resultado = await _leccionRepository.CreateLeccion(leccion);

                if (!resultado)
                {
                    return BadRequest(new { mensaje = "No fue posible registrar la lección." });
                }

                return CreatedAtAction(
                    nameof(GetLeccionById),
                    new { id = leccion.idleccion },
                    leccion
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al registrar la lección.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Instructor")]
        public async Task<IActionResult> UpdateLeccion([FromRoute] int id, [FromBody] Leccion leccion)
        {
            if (leccion == null)
            {
                return BadRequest(new { mensaje = "Los datos de la lección son obligatorios." });
            }

            if (id != leccion.idleccion)
            {
                return BadRequest(new { mensaje = "El ID de la URL no coincide con el ID de la lección enviada." });
            }

            try
            {
                var leccionExistente = await _leccionRepository.GetLeccionById(id);

                if (leccionExistente == null)
                {
                    return NotFound(new { mensaje = "No se encontró la lección a actualizar." });
                }

                var resultado = await _leccionRepository.UpdateLeccion(leccion);

                if (!resultado)
                {
                    return BadRequest(new { mensaje = "No fue posible actualizar la lección." });
                }

                return Ok(new { mensaje = "Lección actualizada correctamente.", leccion });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al actualizar la lección.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador,Instructor")]
        public async Task<IActionResult> DeleteLeccion([FromRoute] int id)
        {
            try
            {
                var leccionExistente = await _leccionRepository.GetLeccionById(id);

                if (leccionExistente == null)
                {
                    return NotFound(new { mensaje = "No se encontró la lección a eliminar." });
                }

                var resultado = await _leccionRepository.DeleteLeccion(id);

                if (!resultado)
                {
                    return BadRequest(new { mensaje = "No fue posible eliminar la lección." });
                }

                return Ok(new { mensaje = "Lección eliminada correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al eliminar la lección.", detalle = UtilidadesError.Registrar(ex) });
            }
        }
    }
}