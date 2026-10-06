using System.Collections.Generic;
using System.Threading.Tasks;
using RAIZA.Models;

namespace RAIZA.Interfaces
{
    public interface IEstudiante_I
    {
        Task<List<Estudiante>> GetEstudiantes();
        Task<Estudiante?> GetEstudianteById(int id);
        Task<bool> CreateEstudiante(Estudiante estudiante);
        Task<bool> UpdateEstudiante(Estudiante estudiante);
        Task<bool> DeleteEstudiante(int id);
    }
}