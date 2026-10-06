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
    public class TematicaController : ControllerBase
    {
        private readonly ITematicaI _tematicaRepository;

        public TematicaController(ITematicaI tematicaRepository)
        {
            _tematicaRepository = tematicaRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetTematicas()
        {
            try
            {
                var tematicas = await _tematicaRepository.GetTematicas();
                return Ok(tematicas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al consultar las temáticas.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTematicaById([FromRoute] int id)
        {
            try
            {
                var tematica = await _tematicaRepository.GetTematicaById(id);

                if (tematica == null)
                {
                    return NotFound(new { mensaje = "No se encontró la temática solicitada." });
                }

                return Ok(tematica);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al consultar la temática.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Instructor")] // escritura de catálogo: no para estudiantes
        public async Task<IActionResult> CreateTematica([FromBody] Tematica tematica)
        {
            if (tematica == null)
            {
                return BadRequest(new { mensaje = "Los datos de la temática son obligatorios." });
            }

            try
            {
                var resultado = await _tematicaRepository.CreateTematica(tematica);

                if (!resultado)
                {
                    return BadRequest(new { mensaje = "No fue posible registrar la temática." });
                }

                return CreatedAtAction(
                    nameof(GetTematicaById),
                    new { id = tematica.idtematica },
                    tematica
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al registrar la temática.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Instructor")]
        public async Task<IActionResult> UpdateTematica([FromRoute] int id, [FromBody] Tematica tematica)
        {
            if (tematica == null)
            {
                return BadRequest(new { mensaje = "Los datos de la temática son obligatorios." });
            }

            if (id != tematica.idtematica)
            {
                return BadRequest(new { mensaje = "El ID de la URL no coincide con el ID de la temática enviada." });
            }

            try
            {
                var tematicaExistente = await _tematicaRepository.GetTematicaById(id);

                if (tematicaExistente == null)
                {
                    return NotFound(new { mensaje = "No se encontró la temática a actualizar." });
                }

                var resultado = await _tematicaRepository.UpdateTematica(tematica);

                if (!resultado)
                {
                    return BadRequest(new { mensaje = "No fue posible actualizar la temática." });
                }

                return Ok(new { mensaje = "Temática actualizada correctamente.", tematica });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al actualizar la temática.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador,Instructor")]
        public async Task<IActionResult> DeleteTematica([FromRoute] int id)
        {
            try
            {
                var tematicaExistente = await _tematicaRepository.GetTematicaById(id);

                if (tematicaExistente == null)
                {
                    return NotFound(new { mensaje = "No se encontró la temática a eliminar." });
                }

                var resultado = await _tematicaRepository.DeleteTematica(id);

                if (!resultado)
                {
                    return BadRequest(new { mensaje = "No fue posible eliminar la temática." });
                }

                return Ok(new { mensaje = "Temática eliminada correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al eliminar la temática.", detalle = UtilidadesError.Registrar(ex) });
            }
        }
    }
}