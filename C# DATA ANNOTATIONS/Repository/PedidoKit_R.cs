using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RAIZA.Interfaces;
using RAIZA.Models;
using RAIZA.Data;

namespace RAIZA.Repositories
{
    public class PedidoKit_R : IPedidoKit_I
    {
        private readonly DatabaseService _context;

        public PedidoKit_R(DatabaseService context)
        {
            this._context = context;
        }

        public async Task<IEnumerable<PedidoKit>> GetPedidoKits() =>
            await _context.PedidoKit.ToListAsync();

        public async Task<PedidoKit?> GetPedidoKitById(int id) =>
            await _context.PedidoKit.FirstOrDefaultAsync(p => p.idPedidoKit == id);

        public async Task<bool> CreatePedidoKit(PedidoKit pedidoKit)
        {
            await _context.PedidoKit.AddAsync(pedidoKit);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdatePedidoKit(PedidoKit pedidoKit)
        {
            // Se actualiza la instancia ya rastreada (evita el conflicto de doble tracking de EF).
            // idcompra nunca se modifica: el pedido conserva siempre el vínculo a su compra.
            var existente = await _context.PedidoKit.FirstOrDefaultAsync(p => p.idPedidoKit == pedidoKit.idPedidoKit);
            if (existente == null) return false;

            existente.Cantidad = pedidoKit.Cantidad;
            existente.Estado = pedidoKit.Estado;
            existente.Direccionenvio = pedidoKit.Direccionenvio;
            existente.Fechapedido = pedidoKit.Fechapedido;
            existente.Idestudiante = pedidoKit.Idestudiante;
            existente.idclasskit = pedidoKit.idclasskit;

            // 0 filas = valores idénticos: no es un error (antes respondía 400).
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePedidoKit(int id)
        {
            var pedidoKit = await _context.PedidoKit.FirstOrDefaultAsync(p => p.idPedidoKit == id);
            if (pedidoKit == null) return false;

            _context.PedidoKit.Remove(pedidoKit);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}