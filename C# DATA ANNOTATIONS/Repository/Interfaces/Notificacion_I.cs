using System.Collections.Generic;
using System.Threading.Tasks;
using RAIZA.Models;

namespace RAIZA.Interfaces
{
    public interface INotificacionI
    {
        Task<List<Notificacion>> GetNotificaciones();
        Task<Notificacion?> GetNotificacionById(int id);
        Task<bool> CreateNotificacion(Notificacion notificacion);
        Task<bool> UpdateNotificacion(Notificacion notificacion);
        Task<bool> DeleteNotificacion(int id);
    }
}