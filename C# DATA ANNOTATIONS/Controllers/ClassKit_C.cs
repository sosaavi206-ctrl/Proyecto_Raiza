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
    public class ClassKitController : ControllerBase
    {
        private readonly IClassKitI _classKitRepository;

        public ClassKitController(IClassKitI classKitRepository)
        {
            _classKitRepository = classKitRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetClassKits()
        {
            try
            {
                var kits = await _classKitRepository.GetClassKits();
                return Ok(kits);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al obtener ClassKits.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetClassKitById(int id)
        {
            try
            {
                var kit = await _classKitRepository.GetClassKitById(id);

                if (kit == null)
                {
                    return NotFound(new { mensaje = "No se encontró el elemento ClassKit." });
                }

                return Ok(kit);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al buscar el registro ClassKit.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Instructor")] // precios/stock: no para estudiantes
        public async Task<IActionResult> CreateClassKit([FromBody] Class_Kit classKit)
        {
            if (classKit == null)
            {
                return BadRequest(new { mensaje = "Los datos de ClassKit son obligatorios." });
            }

            try
            {
                var resultado = await _classKitRepository.CreateClassKit(classKit);

                if (!resultado)
                {
                    return BadRequest(new { mensaje = "No fue posible crear el registro ClassKit." });
                }

                return CreatedAtAction(
                    nameof(GetClassKitById),
                    new { id = classKit.idclass_kit },
                    classKit
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al guardar ClassKit.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Instructor")]
        public async Task<IActionResult> UpdateClassKit(int id, [FromBody] Class_Kit classKit)
        {
            if (classKit == null)
            {
                return BadRequest(new { mensaje = "Los datos de ClassKit son obligatorios." });
            }

            if (id != classKit.idclass_kit)
            {
                return BadRequest(new { mensaje = "El ID de la URL no coincide con el ID del objeto ClassKit." });
            }

            try
            {
                var kitExistente = await _classKitRepository.GetClassKitById(id);

                if (kitExistente == null)
                {
                    return NotFound(new { mensaje = "No se encontró el elemento ClassKit." });
                }

                var resultado = await _classKitRepository.UpdateClassKit(classKit);

                if (!resultado)
                {
                    return BadRequest(new { mensaje = "No fue posible actualizar el registro ClassKit." });
                }

                return Ok(new { mensaje = "ClassKit actualizado correctamente.", classKit });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al actualizar ClassKit.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador,Instructor")]
        public async Task<IActionResult> DeleteClassKit(int id)
        {
            try
            {
                var kit = await _classKitRepository.GetClassKitById(id);

                if (kit == null)
                {
                    return NotFound(new { mensaje = "No se encontró el elemento ClassKit." });
                }

                var resultado = await _classKitRepository.DeleteClassKit(id);

                if (!resultado)
                {
                    return BadRequest(new { mensaje = "No fue posible eliminar el registro ClassKit." });
                }

                return Ok(new { mensaje = "ClassKit eliminado correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al eliminar ClassKit.", detalle = UtilidadesError.Registrar(ex) });
            }
        }
    }
}