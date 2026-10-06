using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RAIZA.Interfaces;
using RAIZA.Models;
using RAIZA.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace RAIZA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClasesEnVivoController : ControllerBase
    {
        private readonly IClasesEnVivoI _clasesEnVivoRepository;

        public ClasesEnVivoController(IClasesEnVivoI clasesEnVivoRepository)
        {
            _clasesEnVivoRepository = clasesEnVivoRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetClasesEnVivo()
        {
            try
            {
                var clases = await _clasesEnVivoRepository.GetClasesEnVivo();
                return Ok(clases);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al listar clases en vivo.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetClasesEnVivoById(int id)
        {
            try
            {
                var clase = await _clasesEnVivoRepository.GetClasesEnVivoById(id);

                if (clase == null)
                {
                    return NotFound(new { mensaje = "No se encontró la clase en vivo." });
                }

                return Ok(clase);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al buscar la clase en vivo.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Instructor")] // programación de clases: no para estudiantes
        public async Task<IActionResult> CreateClasesEnVivo([FromBody] ClasesEnVivo clasesEnVivo)
        {
            if (clasesEnVivo == null)
            {
                return BadRequest(new { mensaje = "Los datos de la clase en vivo son obligatorios." });
            }

            try
            {
                var resultado = await _clasesEnVivoRepository.CreateClasesEnVivo(clasesEnVivo);

                if (!resultado)
                {
                    return BadRequest(new { mensaje = "No fue posible programar la clase en vivo." });
                }

                return CreatedAtAction(
                    nameof(GetClasesEnVivoById),
                    new { id = clasesEnVivo.idclaasesenvivo },
                    clasesEnVivo
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al guardar la clase en vivo.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Instructor")]
        public async Task<IActionResult> UpdateClasesEnVivo(int id, [FromBody] ClasesEnVivo clasesEnVivo)
        {
            if (clasesEnVivo == null)
            {
                return BadRequest(new { mensaje = "Los datos de la clase en vivo son obligatorios." });
            }

            if (id != clasesEnVivo.idclaasesenvivo)
            {
                return BadRequest(new { mensaje = "El ID de la URL no coincide con el ID de la clase en vivo." });
            }

            try
            {
                var claseExistente = await _clasesEnVivoRepository.GetClasesEnVivoById(id);

                if (claseExistente == null)
                {
                    return NotFound(new { mensaje = "No se encontró la clase en vivo." });
                }

                var resultado = await _clasesEnVivoRepository.UpdateClasesEnVivo(clasesEnVivo);

                if (!resultado)
                {
                    return BadRequest(new { mensaje = "No fue posible actualizar la clase en vivo." });
                }

                return Ok(new { mensaje = "Clase en vivo actualizada correctamente.", clasesEnVivo });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al actualizar la clase en vivo.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador,Instructor")]
        public async Task<IActionResult> DeleteClasesEnVivo(int id)
        {
            try
            {
                var clase = await _clasesEnVivoRepository.GetClasesEnVivoById(id);

                if (clase == null)
                {
                    return NotFound(new { mensaje = "No se encontró la clase en vivo." });
                }

                var resultado = await _clasesEnVivoRepository.DeleteClasesEnVivo(id);

                if (!resultado)
                {
                    return BadRequest(new { mensaje = "No fue posible eliminar la clase en vivo." });
                }

                return Ok(new { mensaje = "Clase en vivo eliminada correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al eliminar la clase en vivo.", detalle = UtilidadesError.Registrar(ex) });
            }
        }
    }
}