using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RAIZA.Interfaces;
using RAIZA.Models;
using RAIZA.Data;

namespace RAIZA.Repositories
{
    public class Compra_R : CompraI
    {
        private readonly DatabaseService _context;

        public Compra_R(DatabaseService context)
        {
            this._context = context;
        }

        public async Task<List<Compra>> GetCompras() =>
            await _context.Compra.ToListAsync();

        public async Task<Compra?> GetCompraById(int id) =>
            await _context.Compra.AsNoTracking().FirstOrDefaultAsync(c => c.idcompra == id);

        public async Task<bool> CreateCompra(Compra compra)
        {
            await _context.Compra.AddAsync(compra);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateCompra(Compra compra)
        {
            _context.Compra.Update(compra);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteCompra(int id)
        {
            var compra = await _context.Compra.FirstOrDefaultAsync(c => c.idcompra == id);
            if (compra == null) return false;

            _context.Compra.Remove(compra);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}