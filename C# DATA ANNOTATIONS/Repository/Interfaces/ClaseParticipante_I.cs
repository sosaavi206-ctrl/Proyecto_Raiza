using System.Collections.Generic;
using System.Threading.Tasks;
using RAIZA.Models;

namespace RAIZA.Interfaces
{
    public interface ClaseParticipanteI
    {
        Task<List<ClaseParticipante>> GetClaseParticipantes();
        Task<ClaseParticipante?> GetClaseParticipanteById(int idclase, int idestudiante);
        Task<bool> CreateClaseParticipante(ClaseParticipante claseParticipante);
        Task<bool> UpdateClaseParticipante(ClaseParticipante claseParticipante);
        Task<bool> DeleteClaseParticipante(int idclase, int idestudiante);
    }
}