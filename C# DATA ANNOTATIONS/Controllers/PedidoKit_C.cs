using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using RAIZA.Interfaces;
using RAIZA.Models;

namespace RAIZA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PedidoKitController : ControllerBase
    {
        private readonly IPedidoKit_I _pedidoKitRepository;
        private readonly IClassKitI _classKitRepository;
        private readonly IEstudiante_I _estudianteRepository;
        private readonly CompraI _compraRepository;

        public PedidoKitController(
            IPedidoKit_I pedidoKitRepository,
            IClassKitI classKitRepository,
            IEstudiante_I estudianteRepository,
            CompraI compraRepository)
        {
            _pedidoKitRepository = pedidoKitRepository;
            _classKitRepository = classKitRepository;
            _estudianteRepository = estudianteRepository;
            _compraRepository = compraRepository;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador,Instructor")]
        public async Task<IActionResult> GetPedidoKits()
        {
            try
            {
                var pedidos = await _pedidoKitRepository.GetPedidoKits();
                return Ok(pedidos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al consultar los pedidos de kits.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpGet("mis-pedidos")]
        public async Task<IActionResult> GetMisPedidos()
        {
            try
            {
                // Cada estudiante solo debe ver SUS pedidos (no los del resto de la plataforma).
                var idUsuario = User.FindFirst("IdUsuario")?.Value;
                if (!int.TryParse(idUsuario, out int idEstudiante))
                {
                    return Unauthorized(new { mensaje = "No se pudo identificar al usuario de la sesión." });
                }

                var pedidos = await _pedidoKitRepository.GetPedidoKits();
                var misPedidos = pedidos.Where(p => p.Idestudiante == idEstudiante).ToList();

                return Ok(misPedidos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al consultar tus pedidos.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPedidoKitById([FromRoute] int id)
        {
            try
            {
                var pedido = await _pedidoKitRepository.GetPedidoKitById(id);

                if (pedido == null)
                {
                    return NotFound(new { mensaje = "No se encontró el pedido de kit solicitado." });
                }

                return Ok(pedido);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al consultar el pedido de kit.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePedidoKit([FromBody] RegistrarPedidoKitRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { mensaje = "Los datos del pedido son obligatorios." });
            }

            try
            {
                // La identidad del estudiante sale del token (nunca del cuerpo): así nadie puede
                // registrar un pedido a nombre de otra persona.
                var claimId = User.FindFirst("IdUsuario")?.Value;
                if (!int.TryParse(claimId, out int idEstudiante))
                {
                    return Unauthorized(new { mensaje = "No se pudo identificar al usuario de la sesión." });
                }

                if (request.idClassKit <= 0)
                {
                    return BadRequest(new { mensaje = "Debes elegir un kit del catálogo." });
                }

                var kit = await _classKitRepository.GetClassKitById(request.idClassKit);
                if (kit == null)
                {
                    return BadRequest(new { mensaje = "El kit seleccionado no existe en el catálogo." });
                }

                // El pedido queda ligado al perfil de estudiante real (tabla estudiante).
                var estudiante = await _estudianteRepository.GetEstudianteById(idEstudiante);
                if (estudiante == null)
                {
                    return BadRequest(new { mensaje = "Tu cuenta no tiene un perfil de estudiante registrado. Contacta al administrador para activar tus pedidos de kits." });
                }

                var cantidad = request.Cantidad > 0 ? request.Cantidad : 1;

                // Normaliza el método de pago: la BD (compra.metodo_pago) solo acepta
                // Otro | Nequi | PSE | Tarjeta (CHECK constraint). Los alias del frontend
                // ("Tarjeta de Crédito", etc.) se mapean al valor canónico.
                var metodoPago = string.IsNullOrWhiteSpace(request.MetodoPago)
                    ? "PSE"
                    : UtilidadesPago.NormalizarMetodoPago(request.MetodoPago);
                if (metodoPago == null)
                {
                    return BadRequest(new { mensaje = "El método de pago no es válido. Elige uno de: PSE, Nequi, Tarjeta u Otro." });
                }

                // 1) Se registra la compra (registro de pago en estado Pendiente; la pasarela de
                //    pago real se conectará después, con Wompi/Nequi).
                var compra = new Compra
                {
                    Monto = kit.precio * cantidad,
                    MetodoPago = metodoPago,
                    Estado = "Pendiente",
                    FechaCompra = request.Fechapedido ?? DateTime.UtcNow,
                    idestudiante = idEstudiante,
                    idmodulo = kit.idmodulo ?? 1
                };

                var compraOk = await _compraRepository.CreateCompra(compra);
                if (!compraOk)
                {
                    return StatusCode(500, new { mensaje = "No fue posible registrar la compra del kit." });
                }

                // 2) Se registra el pedido ligado a la compra recién creada (id_compra es NOT NULL).
                var pedidoKit = new PedidoKit
                {
                    idclasskit = request.idClassKit,
                    Cantidad = cantidad,
                    Estado = "Pendiente",
                    Direccionenvio = request.Direccionenvio ?? string.Empty,
                    Fechapedido = request.Fechapedido ?? DateTime.UtcNow,
                    Idestudiante = idEstudiante,
                    idcompra = compra.idcompra
                };

                var resultado = await _pedidoKitRepository.CreatePedidoKit(pedidoKit);
                if (!resultado)
                {
                    // Evitar compras huérfanas si el pedido no se pudo crear.
                    await _compraRepository.DeleteCompra(compra.idcompra);
                    return BadRequest(new { mensaje = "No fue posible registrar el pedido de kit." });
                }

                return CreatedAtAction(
                    nameof(GetPedidoKitById),
                    new { id = pedidoKit.idPedidoKit },
                    pedidoKit
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al registrar el pedido de kit.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Instructor")]
        public async Task<IActionResult> UpdatePedidoKit([FromRoute] int id, [FromBody] PedidoKit pedidoKit)
        {
            if (pedidoKit == null)
            {
                return BadRequest(new { mensaje = "Los datos del pedido son obligatorios." });
            }

            if (id != pedidoKit.idPedidoKit)
            {
                return BadRequest(new { mensaje = "El ID de la URL no coincide con el ID del pedido enviado." });
            }

            // Estado canónico del CHECK de pedido_kit: evita un 500 por violación de constraint
            // si llega un valor fuera de Pendiente|Enviado|Entregado|Cancelado.
            var estadoValido = UtilidadesPago.NormalizarEstado(pedidoKit.Estado, UtilidadesPago.EstadosPedidoKit);
            if (estadoValido == null)
            {
                return BadRequest(new { mensaje = "El estado debe ser Pendiente, Enviado, Entregado o Cancelado." });
            }
            pedidoKit.Estado = estadoValido;

            try
            {
                var pedidoExistente = await _pedidoKitRepository.GetPedidoKitById(id);

                if (pedidoExistente == null)
                {
                    return NotFound(new { mensaje = "No se encontró el pedido de kit a actualizar." });
                }

                var resultado = await _pedidoKitRepository.UpdatePedidoKit(pedidoKit);

                if (!resultado)
                {
                    return BadRequest(new { mensaje = "No fue posible actualizar el pedido de kit." });
                }

                return Ok(new { mensaje = "Pedido de kit actualizado correctamente.", pedidoKit });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al actualizar el pedido de kit.", detalle = UtilidadesError.Registrar(ex) });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador,Instructor")]
        public async Task<IActionResult> DeletePedidoKit([FromRoute] int id)
        {
            try
            {
                var pedidoExistente = await _pedidoKitRepository.GetPedidoKitById(id);

                if (pedidoExistente == null)
                {
                    return NotFound(new { mensaje = "No se encontró el pedido de kit a eliminar." });
                }

                var resultado = await _pedidoKitRepository.DeletePedidoKit(id);

                if (!resultado)
                {
                    return BadRequest(new { mensaje = "No fue posible eliminar el pedido de kit." });
                }

                return Ok(new { mensaje = "Pedido de kit eliminado correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al eliminar el pedido de kit.", detalle = UtilidadesError.Registrar(ex) });
            }
        }
    }

    // Contrato del checkout del estudiante: además de los datos del pedido lleva el método de
    // pago elegido (PSE/Tarjeta) que alimenta la Compra que el API crea internamente.
    public class RegistrarPedidoKitRequest
    {
        public int idClassKit { get; set; }
        public int Cantidad { get; set; } = 1;
        public string? Direccionenvio { get; set; }
        public DateTime? Fechapedido { get; set; }
        public string? MetodoPago { get; set; }
    }
}