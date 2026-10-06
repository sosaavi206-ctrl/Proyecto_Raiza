using System.Collections.Generic;
using System.Threading.Tasks;
using RAIZA.Models;

namespace RAIZA.Interfaces
{
    public interface ITarea_I
    {
        Task<IEnumerable<Tarea>> GetTareas();
        Task<Tarea?> GetTareaById(int id);
        Task<IEnumerable<Tarea>> GetTareasByModuloId(int idModulo);
        Task<bool> CreateTarea(Tarea tarea);
        Task<bool> UpdateTarea(Tarea tarea);
        Task<bool> DeleteTarea(int id);
    }
}