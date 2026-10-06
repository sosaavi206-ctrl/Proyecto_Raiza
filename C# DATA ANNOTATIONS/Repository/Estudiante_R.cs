using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RAIZA.Interfaces;
using RAIZA.Models;
using RAIZA.Data;

namespace RAIZA.Repositories
{
    public class Estudiante_R : IEstudiante_I
    {
        private readonly DatabaseService _context;

        public Estudiante_R(DatabaseService context)
        {
            this._context = context;
        }

        public async Task<List<Estudiante>> GetEstudiantes() =>
            await _context.Estudiante.ToListAsync();

        public async Task<Estudiante?> GetEstudianteById(int id) =>
            await _context.Estudiante.AsNoTracking().FirstOrDefaultAsync(e => e.idestudiante == id);

        public async Task<bool> CreateEstudiante(Estudiante estudiante)
        {
            await _context.Estudiante.AddAsync(estudiante);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateEstudiante(Estudiante estudiante)
        {
            _context.Estudiante.Update(estudiante);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteEstudiante(int id)
        {
            var estudiante = await _context.Estudiante.FirstOrDefaultAsync(e => e.idestudiante == id);
            if (estudiante == null) return false;

            _context.Estudiante.Remove(estudiante);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}