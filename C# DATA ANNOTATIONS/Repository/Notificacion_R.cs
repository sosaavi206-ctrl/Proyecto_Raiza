using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RAIZA.Interfaces;
using RAIZA.Models;
using RAIZA.Data;

namespace RAIZA.Repositories
{
    public class Notificacion_R(DatabaseService context) : INotificacionI
    {
        private readonly DatabaseService _context = context;

        public async Task<List<Notificacion>> GetNotificaciones() =>
            await _context.Notificacion.ToListAsync();

        public async Task<Notificacion?> GetNotificacionById(int id)
        {
            return await _context.Notificacion
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Idnotificacion == id);
        }

        public async Task<bool> CreateNotificacion(Notificacion notificacion)
        {
            await _context.Notificacion.AddAsync(notificacion);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateNotificacion(Notificacion notificacion)
        {
            _context.Notificacion.Update(notificacion);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteNotificacion(int id)
        {
            var notificacion = await _context.Notificacion.FirstOrDefaultAsync(n => n.Idnotificacion == id);
            if (notificacion == null) return false;

            _context.Notificacion.Remove(notificacion);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}