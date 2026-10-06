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
    public class ClaseParticipanteController : ControllerBase
    {
        private readonly ClaseParticipanteI _claseParticipanteRepository;

        public ClaseParticipanteController(ClaseParticipanteI claseParticipanteRepository)
        {
            _claseParticipanteRepository = claseParticipanteRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetClaseParticipantes()
        {
            try
            {
                var participantes = await _claseParticipanteRepository.GetClaseParticipantes();
                return Ok(participantes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al obtener participantes.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpGet("{idclase}/{idestudiante}")]
        public async Task<IActionResult> GetClaseParticipanteById(int idclase, int idestudiante)
        {
            try
            {
                var participante = await _claseParticipanteRepository.GetClaseParticipanteById(idclase, idestudiante);

                if (participante == null)
                {
                    return NotFound(new { mensaje = "No se encontró la inscripción del estudiante en la clase." });
                }

                return Ok(participante);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al buscar el participante.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateClaseParticipante([FromBody] ClaseParticipante claseParticipante)
        {
            if (claseParticipante == null)
            {
                return BadRequest(new { mensaje = "Los datos del participante de la clase son obligatorios." });
            }

            // Un estudiante solo puede inscribirse a sí mismo; un gestor (admin/instructor), a cualquiera.
            if (!Autorizacion.EsGestor(User))
            {
                var idPropio = Autorizacion.IdUsuario(User);
                if (idPropio == null)
                {
                    return Unauthorized(new { mensaje = "No se pudo identificar al usuario de la sesión." });
                }
                if (claseParticipante.idestudiante != idPropio.Value)
                {
                    return StatusCode(403, new { mensaje = "Solo puedes inscribirte a ti mismo en la clase." });
                }
            }

            try
            {
                var resultado = await _claseParticipanteRepository.CreateClaseParticipante(claseParticipante);

                if (!resultado)
                {
                    return BadRequest(new { mensaje = "No fue posible registrar al estudiante en la clase." });
                }

                return CreatedAtAction(
                    nameof(GetClaseParticipanteById),
                    new { idclase = claseParticipante.idclase, idestudiante = claseParticipante.idestudiante },
                    claseParticipante
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al registrar el participante.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpPut("{idclase}/{idestudiante}")]
        public async Task<IActionResult> UpdateClaseParticipante(int idclase, int idestudiante, [FromBody] ClaseParticipante claseParticipante)
        {
            if (claseParticipante == null)
            {
                return BadRequest(new { mensaje = "Los datos del participante son obligatorios." });
            }

            if (idclase != claseParticipante.idclase || idestudiante != claseParticipante.idestudiante)
            {
                return BadRequest(new { mensaje = "Los IDs de la URL no coinciden con las claves de la entidad." });
            }

            try
            {
                var participanteExistente = await _claseParticipanteRepository.GetClaseParticipanteById(idclase, idestudiante);

                if (participanteExistente == null)
                {
                    return NotFound(new { mensaje = "No se encontró el registro del participante en la clase." });
                }

                if (!Autorizacion.EsGestor(User))
                {
                    var idPropio = Autorizacion.IdUsuario(User);
                    if (idPropio == null)
                    {
                        return Unauthorized(new { mensaje = "No se pudo identificar al usuario de la sesión." });
                    }
                    if (participanteExistente.idestudiante != idPropio.Value)
                    {
                        return StatusCode(403, new { mensaje = "Solo puedes actualizar tu propia inscripción." });
                    }
                }

                participanteExistente.FechaIngreso = claseParticipante.FechaIngreso;

                var resultado = await _claseParticipanteRepository.UpdateClaseParticipante(participanteExistente);

                if (!resultado)
                {
                    return BadRequest(new { mensaje = "No fue posible actualizar el registro del participante." });
                }

                return Ok(new { mensaje = "Registro de participante actualizado correctamente.", claseParticipante = participanteExistente });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al actualizar el participante.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpDelete("{idclase}/{idestudiante}")]
        public async Task<IActionResult> DeleteClaseParticipante(int idclase, int idestudiante)
        {
            try
            {
                var participante = await _claseParticipanteRepository.GetClaseParticipanteById(idclase, idestudiante);

                if (participante == null)
                {
                    return NotFound(new { mensaje = "No se encontró la inscripción a la clase." });
                }

                if (!Autorizacion.EsGestor(User))
                {
                    var idPropio = Autorizacion.IdUsuario(User);
                    if (idPropio == null)
                    {
                        return Unauthorized(new { mensaje = "No se pudo identificar al usuario de la sesión." });
                    }
                    if (participante.idestudiante != idPropio.Value)
                    {
                        return StatusCode(403, new { mensaje = "Solo puedes eliminar tu propia inscripción." });
                    }
                }

                var resultado = await _claseParticipanteRepository.DeleteClaseParticipante(idclase, idestudiante);

                if (!resultado)
                {
                    return BadRequest(new { mensaje = "No fue posible eliminar la inscripción del estudiante." });
                }

                return Ok(new { mensaje = "Inscripción eliminada correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al eliminar la inscripción.", detalle = UtilidadesError.Registrar(ex) });
            }
        }
    }
}