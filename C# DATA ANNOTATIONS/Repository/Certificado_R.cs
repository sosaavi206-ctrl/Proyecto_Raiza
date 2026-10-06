using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RAIZA.Interfaces;
using RAIZA.Models;
using RAIZA.Data;

namespace RAIZA.Repositories
{
    public class Certificado_R : CertificadoI
    {
        private readonly DatabaseService _context;

        public Certificado_R(DatabaseService context)
        {
            this._context = context;
        }

        public async Task<List<Certificado>> GetCertificados() =>
            await _context.Certificado.ToListAsync();

        public async Task<Certificado?> GetCertificadoById(int id) =>
            await _context.Certificado.AsNoTracking().FirstOrDefaultAsync(c => c.idcertificado == id);

        public async Task<bool> CreateCertificado(Certificado certificado)
        {
            await _context.Certificado.AddAsync(certificado);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateCertificado(Certificado certificado)
        {
            _context.Certificado.Update(certificado);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteCertificado(int id)
        {
            var certificado = await _context.Certificado.FirstOrDefaultAsync(c => c.idcertificado == id);
            if (certificado == null) return false;

            _context.Certificado.Remove(certificado);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}