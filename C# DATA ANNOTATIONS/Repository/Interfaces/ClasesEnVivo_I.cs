using System.Collections.Generic;
using System.Threading.Tasks;
using RAIZA.Models;

namespace RAIZA.Interfaces
{
    public interface IClasesEnVivoI
    {
        Task<List<ClasesEnVivo>> GetClasesEnVivo();
        Task<ClasesEnVivo?> GetClasesEnVivoById(int id);
        Task<bool> CreateClasesEnVivo(ClasesEnVivo clasesEnVivo);
        Task<bool> UpdateClasesEnVivo(ClasesEnVivo clasesEnVivo);
        Task<bool> DeleteClasesEnVivo(int id);
    }
}