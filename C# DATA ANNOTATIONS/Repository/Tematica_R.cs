using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RAIZA.Interfaces;
using RAIZA.Models;
using RAIZA.Data;

namespace RAIZA.Repositories
{
    public class Tematica_R : ITematicaI
    {
        private readonly DatabaseService _context;

        public Tematica_R(DatabaseService context)
        {
            _context = context;
        }

        public async Task<List<Tematica>> GetTematicas() =>
            await _context.Tematica.ToListAsync();

        public async Task<Tematica?> GetTematicaById(int id) =>
            await _context.Tematica.FirstOrDefaultAsync(t => t.idtematica == id);

        public async Task<bool> CreateTematica(Tematica tematica)
        {
            await _context.Tematica.AddAsync(tematica);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateTematica(Tematica tematica)
        {
            // Mismo fix de doble tracking que Progreso_R: muta la instancia ya rastreada por el
            // GetById del controlador (FirstOrDefault sin AsNoTracking) en vez de adjuntar la del
            // body con la misma clave, lo que lanzaba InvalidOperationException (PUT devolvía 500).
            var existente = await _context.Tematica.FirstOrDefaultAsync(t => t.idtematica == tematica.idtematica);
            if (existente == null) return false;

            existente.Nombre = tematica.Nombre;
            existente.ImagenPortada = tematica.ImagenPortada;

            // 0 filas = valores idénticos: no es un error (antes respondía 400).
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTematica(int id)
        {
            var tematica = await _context.Tematica.FirstOrDefaultAsync(t => t.idtematica == id);
            if (tematica == null) return false;

            _context.Tematica.Remove(tematica);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}