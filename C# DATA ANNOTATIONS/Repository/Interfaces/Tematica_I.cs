using System.Collections.Generic;
using System.Threading.Tasks;
using RAIZA.Models;

namespace RAIZA.Interfaces
{
    public interface ITematicaI
    {
        Task<List<Tematica>> GetTematicas();
        Task<Tematica?> GetTematicaById(int id);
        Task<bool> CreateTematica(Tematica tematica);
        Task<bool> UpdateTematica(Tematica tematica);
        Task<bool> DeleteTematica(int id);
    }
}