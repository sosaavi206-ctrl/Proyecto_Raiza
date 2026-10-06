using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RAIZA.Interfaces;
using RAIZA.Models;
using RAIZA.Data;

namespace RAIZA.Repositories
{
    public class EntregaTarea_R : IEntregaTareaI
    {
        private readonly DatabaseService _context;

        public EntregaTarea_R(DatabaseService context)
        {
            this._context = context;
        }

        public async Task<List<EntregaTarea>> GetEntregaTareas() =>
            await _context.EntregaTarea.ToListAsync();

        public async Task<EntregaTarea?> GetEntregaTareaById(int id) =>
            await _context.EntregaTarea.AsNoTracking().FirstOrDefaultAsync(e => e.Identregatarea == id);

        public async Task<bool> CreateEntregaTarea(EntregaTarea entregaTarea)
        {
            await _context.EntregaTarea.AddAsync(entregaTarea);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateEntregaTarea(EntregaTarea entregaTarea)
        {
            var existente = await _context.EntregaTarea.FirstOrDefaultAsync(e => e.Identregatarea == entregaTarea.Identregatarea);
            if (existente == null) return false;

            // Actualización PARCIAL: solo se reemplazan los campos que vienen con información.
            // Esto evita que una calificación (que solo manda nota + instructor) borre url/archivo/comentario.
            if (!string.IsNullOrWhiteSpace(entregaTarea.UrlArchivo))
            {
                existente.UrlArchivo = entregaTarea.UrlArchivo.Trim();
            }

            if (entregaTarea.FechaEntrega != default)
            {
                existente.FechaEntrega = entregaTarea.FechaEntrega;
            }

            if (entregaTarea.Calificacion.HasValue)
            {
                existente.Calificacion = entregaTarea.Calificacion;
            }

            if (!string.IsNullOrWhiteSpace(entregaTarea.Comentario))
            {
                existente.Comentario = entregaTarea.Comentario.Trim();
            }

            if (entregaTarea.idtarea > 0)
            {
                existente.idtarea = entregaTarea.idtarea;
            }

            if (entregaTarea.idestudiante > 0)
            {
                existente.idestudiante = entregaTarea.idestudiante;
            }

            if (entregaTarea.idinstructorcalifica.HasValue)
            {
                existente.idinstructorcalifica = entregaTarea.idinstructorcalifica;
            }

            _context.EntregaTarea.Update(existente);
            // 0 filas = la entrega ya tenía exactamente esos valores (p. ej. re-guardar la misma
            // calificación): no es un error (antes devolvía false → 400 en el modal del instructor).
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteEntregaTarea(int id)
        {
            var entregaTarea = await _context.EntregaTarea.FirstOrDefaultAsync(e => e.Identregatarea == id);
            if (entregaTarea == null) return false;

            _context.EntregaTarea.Remove(entregaTarea);
            return await _context.SaveChangesAsync() > 0;
        }

        public Task GetEntregasTareas()
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<EntregaTarea>> IEntregaTareaI.GetEntregasTareas()
        {
            throw new NotImplementedException();
        }
    }
}