using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RAIZA.Interfaces;
using RAIZA.Models;
using RAIZA.Data;

namespace RAIZA.Repositories
{
    public class Leccion_R : ILeccionI
    {
        private readonly DatabaseService _context;

        public Leccion_R(DatabaseService context)
        {
            this._context = context;
        }

        public async Task<List<Leccion>> GetLecciones() =>
            await _context.Leccion.ToListAsync();

        public async Task<Leccion?> GetLeccionById(int id) =>
            await _context.Leccion.AsNoTracking().FirstOrDefaultAsync(l => l.idleccion == id);

        public async Task<bool> CreateLeccion(Leccion leccion)
        {
            await _context.Leccion.AddAsync(leccion);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateLeccion(Leccion leccion)
        {
            _context.Leccion.Update(leccion);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteLeccion(int id)
        {
            var leccion = await _context.Leccion.FirstOrDefaultAsync(l => l.idleccion == id);
            if (leccion == null) return false;

            _context.Leccion.Remove(leccion);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}