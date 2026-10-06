using System.Collections.Generic;
using System.Threading.Tasks;
using RAIZA.Models;

namespace RAIZA.Interfaces
{
    public interface AdministradorI
    {
        Task<List<Administrador>> GetAdministradores();
        Task<Administrador?> GetAdministradorById(int id);
        Task<bool> CreateAdministrador(Administrador administrador);
        Task<bool> UpdateAdministrador(Administrador administrador);
        Task<bool> DeleteAdministrador(int id);
    }
}