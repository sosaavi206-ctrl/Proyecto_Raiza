using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RAIZA.Interfaces;
using RAIZA.Models;
using RAIZA.Data;

namespace RAIZA.Repositories
{
    public class Modulo_R : IModuloI
    {
        private readonly DatabaseService _context;

        public Modulo_R(DatabaseService context)
        {
            this._context = context;
        }

        public async Task<List<Modulo>> GetModulos() =>
            await _context.Modulo.ToListAsync();

        public async Task<Modulo?> GetModuloById(int id) =>
            await _context.Modulo.AsNoTracking().FirstOrDefaultAsync(m => m.idmodulo == id);

        public async Task<bool> CreateModulo(Modulo modulo)
        {
            await _context.Modulo.AddAsync(modulo);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateModulo(Modulo modulo)
        {
            _context.Modulo.Update(modulo);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteModulo(int id)
        {
            var modulo = await _context.Modulo.FirstOrDefaultAsync(m => m.idmodulo == id);
            if (modulo == null) return false;

            _context.Modulo.Remove(modulo);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}