using System.Collections.Generic;
using System.Threading.Tasks;
using RAIZA.Models;

namespace RAIZA.Interfaces
{
    public interface IClassKitI
    {
        Task<List<Class_Kit>> GetClassKits();
        Task<Class_Kit?> GetClassKitById(int id);
        Task<bool> CreateClassKit(Class_Kit classKit);
        Task<bool> UpdateClassKit(Class_Kit classKit);
        Task<bool> DeleteClassKit(int id);
    }
}