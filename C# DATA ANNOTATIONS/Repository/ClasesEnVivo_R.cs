using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RAIZA.Interfaces;
using RAIZA.Models;
using RAIZA.Data;

namespace RAIZA.Repositories
{
    public class ClasesEnVivo_R : IClasesEnVivoI
    {
        private readonly DatabaseService _context;

        public ClasesEnVivo_R(DatabaseService context)
        {
            this._context = context;
        }

        public async Task<List<ClasesEnVivo>> GetClasesEnVivo() =>
            await _context.ClasesEnVivo.ToListAsync();

        public async Task<ClasesEnVivo?> GetClasesEnVivoById(int id) =>
            await _context.ClasesEnVivo.AsNoTracking().FirstOrDefaultAsync(c => c.idclaasesenvivo == id);

        public async Task<bool> CreateClasesEnVivo(ClasesEnVivo clasesEnVivo)
        {
            await _context.ClasesEnVivo.AddAsync(clasesEnVivo);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateClasesEnVivo(ClasesEnVivo clasesEnVivo)
        {
            _context.ClasesEnVivo.Update(clasesEnVivo);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteClasesEnVivo(int id)
        {
            var clasesEnVivo = await _context.ClasesEnVivo.FirstOrDefaultAsync(c => c.idclaasesenvivo == id);
            if (clasesEnVivo == null) return false;

            _context.ClasesEnVivo.Remove(clasesEnVivo);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}