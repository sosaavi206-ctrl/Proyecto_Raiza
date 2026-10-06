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
    public class ModuloController : ControllerBase
    {
        private readonly IModuloI _moduloRepository;

        public ModuloController(IModuloI moduloRepository)
        {
            _moduloRepository = moduloRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetModulos()
        {
            try
            {
                var modulos = await _moduloRepository.GetModulos();
                return Ok(modulos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al consultar los módulos.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetModuloById([FromRoute] int id)
        {
            try
            {
                var modulo = await _moduloRepository.GetModuloById(id);

                if (modulo == null)
                {
                    return NotFound(new { mensaje = "No se encontró el módulo solicitado." });
                }

                return Ok(modulo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al consultar el módulo.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Instructor")] // escritura de catálogo: no para estudiantes
        public async Task<IActionResult> CreateModulo([FromBody] Modulo modulo)
        {
            if (modulo == null)
            {
                return BadRequest(new { mensaje = "Los datos del módulo son obligatorios." });
            }

            try
            {
                var resultado = await _moduloRepository.CreateModulo(modulo);

                if (!resultado)
                {
                    return BadRequest(new { mensaje = "No fue posible registrar el módulo." });
                }

                return CreatedAtAction(
                    nameof(GetModuloById),
                    new { id = modulo.idmodulo },
                    modulo
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al registrar el módulo.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Instructor")]
        public async Task<IActionResult> UpdateModulo([FromRoute] int id, [FromBody] Modulo modulo)
        {
            if (modulo == null)
            {
                return BadRequest(new { mensaje = "Los datos del módulo son obligatorios." });
            }

            if (id != modulo.idmodulo)
            {
                return BadRequest(new { mensaje = "El ID de la URL no coincide con el ID del módulo enviado." });
            }

            try
            {
                var moduloExistente = await _moduloRepository.GetModuloById(id);

                if (moduloExistente == null)
                {
                    return NotFound(new { mensaje = "No se encontró el módulo a actualizar." });
                }

                var resultado = await _moduloRepository.UpdateModulo(modulo);

                if (!resultado)
                {
                    return BadRequest(new { mensaje = "No fue posible actualizar el módulo." });
                }

                return Ok(new { mensaje = "Módulo actualizado correctamente.", modulo });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al actualizar el módulo.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador,Instructor")]
        public async Task<IActionResult> DeleteModulo([FromRoute] int id)
        {
            try
            {
                var moduloExistente = await _moduloRepository.GetModuloById(id);

                if (moduloExistente == null)
                {
                    return NotFound(new { mensaje = "No se encontró el módulo a eliminar." });
                }

                var resultado = await _moduloRepository.DeleteModulo(id);

                if (!resultado)
                {
                    return BadRequest(new { mensaje = "No fue posible eliminar el módulo." });
                }

                return Ok(new { mensaje = "Módulo eliminado correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al eliminar el módulo.", detalle = UtilidadesError.Registrar(ex) });
            }
        }
    }
}