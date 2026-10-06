using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RAIZA.Interfaces;
using RAIZA.Models;
using RAIZA.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


    namespace RAIZA.Controllers
    {
        [ApiController]
        [Route("api/[controller]")]
        public class CompraController : ControllerBase
        {
            private readonly CompraI _compraRepository;

            public CompraController(CompraI compraRepository)
            {
                _compraRepository = compraRepository;
            }

            [HttpGet]
            public async Task<IActionResult> GetCompras()
            {
                try
                {
                    var compras = await _compraRepository.GetCompras();

                    // Un estudiante solo ve SUS compras; los gestores (admin/instructor), todas.
                    if (!Autorizacion.EsGestor(User))
                    {
                        var idPropio = Autorizacion.IdUsuario(User);
                        if (idPropio == null) return Unauthorized(new { mensaje = "No se pudo identificar al usuario de la sesión." });
                        compras = compras.Where(c => c.idestudiante == idPropio.Value).ToList();
                    }

                    return Ok(compras);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { mensaje = "Error interno al obtener compras.", detalle = UtilidadesError.Registrar(ex) });
                }
            }

            [HttpGet("{id}")]
            public async Task<IActionResult> GetCompraById(int id)
            {
                try
                {
                    var compra = await _compraRepository.GetCompraById(id);

                    if (compra == null)
                    {
                        return NotFound(new { mensaje = "No se encontró la compra." });
                    }

                    // Solo el dueño o un gestor pueden consultar la compra.
                    if (!Autorizacion.EsGestor(User))
                    {
                        var idPropio = Autorizacion.IdUsuario(User);
                        if (idPropio == null || compra.idestudiante != idPropio.Value)
                        {
                            return StatusCode(403, new { mensaje = "No tienes permiso para consultar esta compra." });
                        }
                    }

                    return Ok(compra);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { mensaje = "Error interno al consultar la compra.", detalle = UtilidadesError.Registrar(ex) });
                }
            }

            [HttpPost]
            [Authorize(Roles = "Administrador,Instructor")] // antes cualquier autenticado podía crear/reescribir pagos
            public async Task<IActionResult> CreateCompra([FromBody] Compra compra)
            {
                if (compra == null)
                {
                    return BadRequest(new { mensaje = "Los datos de la compra son obligatorios." });
                }

                var errorValidacion = ValidarValoresCompra(compra);
                if (errorValidacion != null)
                {
                    return BadRequest(new { mensaje = errorValidacion });
                }

                try
                {
                    var resultado = await _compraRepository.CreateCompra(compra);

                    if (!resultado)
                    {
                        return BadRequest(new { mensaje = "No fue posible registrar la compra." });
                    }

                    return CreatedAtAction(
                        nameof(GetCompraById),
                        new { id = compra.idcompra },
                        compra
                    );
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { mensaje = "Error interno al crear la compra.", detalle = UtilidadesError.Registrar(ex) });
                }
            }

            [HttpPut("{id}")]
            [Authorize(Roles = "Administrador,Instructor")] // antes cualquier autenticado podía reescribir montos/estados de pagos
            public async Task<IActionResult> UpdateCompra(int id, [FromBody] Compra compra)
            {
                if (compra == null)
                {
                    return BadRequest(new { mensaje = "Los datos de la compra son obligatorios." });
                }

                if (id != compra.idcompra)
                {
                    return BadRequest(new { mensaje = "El ID de la URL no coincide con el ID de la compra." });
                }

                var errorValidacion = ValidarValoresCompra(compra);
                if (errorValidacion != null)
                {
                    return BadRequest(new { mensaje = errorValidacion });
                }

                try
                {
                    var compraExistente = await _compraRepository.GetCompraById(id);

                    if (compraExistente == null)
                    {
                        return NotFound(new { mensaje = "No se encontró la compra." });
                    }

                    var resultado = await _compraRepository.UpdateCompra(compra);

                    if (!resultado)
                    {
                        return BadRequest(new { mensaje = "No fue posible actualizar la compra." });
                    }

                    return Ok(new { mensaje = "Compra actualizada correctamente.", compra });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { mensaje = "Error interno al actualizar la compra.", detalle = UtilidadesError.Registrar(ex) });
                }
            }

            [HttpDelete("{id}")]
            [Authorize(Roles = "Administrador,Instructor")] // registro de pagos: solo gestión
            public async Task<IActionResult> DeleteCompra(int id)
            {
                try
                {
                    var compra = await _compraRepository.GetCompraById(id);

                    if (compra == null)
                    {
                        return NotFound(new { mensaje = "No se encontró la compra." });
                    }

                    var resultado = await _compraRepository.DeleteCompra(id);

                    if (!resultado)
                    {
                        return BadRequest(new { mensaje = "No fue posible eliminar la compra." });
                    }

                    return Ok(new { mensaje = "Compra eliminada correctamente." });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { mensaje = "Error interno al eliminar la compra.", detalle = UtilidadesError.Registrar(ex) });
                }
            }

            /// <summary>
            /// Normaliza metodo_pago y estado a los valores canónicos de los CHECK de la BD
            /// (metodo_pago: Otro|Nequi|PSE|Tarjeta; estado: Pendiente|Aprobado|Rechazado).
            /// Devuelve un mensaje de error o null si todo está válido. Evitaba un 500 por
            /// violación de constraint cuando llegaba, p. ej., "Efectivo" o "Tarjeta de Crédito".
            /// </summary>
            private static string? ValidarValoresCompra(Compra compra)
            {
                var metodoPago = UtilidadesPago.NormalizarMetodoPago(compra.MetodoPago);
                if (metodoPago == null)
                {
                    return "El método de pago debe ser PSE, Nequi, Tarjeta u Otro.";
                }
                compra.MetodoPago = metodoPago;

                if (string.IsNullOrWhiteSpace(compra.Estado))
                {
                    compra.Estado = "Pendiente"; // valor por defecto del checkout
                }
                else
                {
                    var estado = UtilidadesPago.NormalizarEstado(compra.Estado, UtilidadesPago.EstadosCompra);
                    if (estado == null)
                    {
                        return "El estado de la compra debe ser Pendiente, Aprobado o Rechazado.";
                    }
                    compra.Estado = estado;
                }

                return null;
            }
        }
    }