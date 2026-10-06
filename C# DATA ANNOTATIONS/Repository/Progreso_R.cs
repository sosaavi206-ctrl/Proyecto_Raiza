using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RAIZA.Interfaces;
using RAIZA.Models;
using RAIZA.Data;

namespace RAIZA.Repositories
{
    public class Progreso_R : IProgresoI
    {
        private readonly DatabaseService _context;

        public Progreso_R(DatabaseService context)
        {
            this._context = context;
        }

        public async Task<List<Progreso>> GetProgresos() =>
            await _context.Progreso.ToListAsync();

        public async Task<Progreso?> GetProgresoById(int id) =>
            await _context.Progreso.FirstOrDefaultAsync(p => p.Idprogreso == id);

        public async Task<bool> CreateProgreso(Progreso progreso)
        {
            await _context.Progreso.AddAsync(progreso);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateProgreso(Progreso progreso)
        {
            // Evita el doble tracking de EF: el controlador ya rastrea esta instancia ( GetById
            // sin AsNoTracking ); adjuntar la del body con la misma clave lanza InvalidOperationException
            // y el PUT devolvía 500 siempre. Se copian los campos sobre la instancia rastreada.
            var existente = await _context.Progreso.FirstOrDefaultAsync(p => p.Idprogreso == progreso.Idprogreso);
            if (existente == null) return false;

            existente.Completado = progreso.Completado;
            existente.Porcentaje = progreso.Porcentaje;
            existente.FechaCompletado = progreso.FechaCompletado;
            existente.idestudiante = progreso.idestudiante;
            existente.Idmodulo = progreso.Idmodulo;

            // 0 filas afectadas = los valores ya eran iguales: el estado final ES el pedido,
            // no es un error (antes devolvía false y el controlador respondía 400).
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteProgreso(int id)
        {
            var progreso = await _context.Progreso.FirstOrDefaultAsync(p => p.Idprogreso == id);
            if (progreso == null) return false;

            _context.Progreso.Remove(progreso);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}