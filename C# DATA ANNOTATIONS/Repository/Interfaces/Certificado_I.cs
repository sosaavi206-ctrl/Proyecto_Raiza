using System.Collections.Generic;
using System.Threading.Tasks;
using RAIZA.Models;

namespace RAIZA.Interfaces
{
    public interface CertificadoI
    {
        Task<List<Certificado>> GetCertificados();
        Task<Certificado?> GetCertificadoById(int id);
        Task<bool> CreateCertificado(Certificado certificado);
        Task<bool> UpdateCertificado(Certificado certificado);
        Task<bool> DeleteCertificado(int id);
    }
}