using System.Collections.Generic;
using System.Threading.Tasks;
using RAIZA.Models;

namespace RAIZA.Interfaces
{
    public interface IProgresoLeccionI
    {
        Task<List<ProgresoLeccion>> GetProgresoLecciones();
        Task<ProgresoLeccion?> GetProgresoLeccionById(int id);
        Task<bool> CreateProgresoLeccion(ProgresoLeccion progresoLeccion);
        Task<bool> UpdateProgresoLeccion(ProgresoLeccion progresoLeccion);
        Task<bool> DeleteProgresoLeccion(int id);
    }
}