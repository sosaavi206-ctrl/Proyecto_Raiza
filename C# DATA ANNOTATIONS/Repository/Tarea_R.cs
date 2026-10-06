using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RAIZA.Interfaces;
using RAIZA.Models;
using RAIZA.Data;

namespace RAIZA.Repositories
{
    public class Tarea_R : ITarea_I
    {
        private readonly DatabaseService _context;

        public Tarea_R(DatabaseService context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Tarea>> GetTareas() =>
            await _context.Tarea.AsNoTracking().ToListAsync();

        public async Task<Tarea?> GetTareaById(int id) =>
            await _context.Tarea.AsNoTracking().FirstOrDefaultAsync(t => t.idtarea == id);

        public async Task<IEnumerable<Tarea>> GetTareasByModuloId(int idModulo) =>
            await _context.Tarea
                .AsNoTracking()
                .Where(t => t.idmodulo == idModulo)
                .ToListAsync();

        public async Task<bool> CreateTarea(Tarea tarea)
        {
            await _context.Tarea.AddAsync(tarea);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateTarea(Tarea tarea)
        {
            _context.Tarea.Update(tarea);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteTarea(int id)
        {
            var tarea = await _context.Tarea.FirstOrDefaultAsync(t => t.idtarea == id);
            if (tarea == null) return false;

            _context.Tarea.Remove(tarea);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}