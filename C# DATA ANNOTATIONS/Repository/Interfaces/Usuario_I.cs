using System.Collections.Generic;
using System.Threading.Tasks;
using RAIZA.Models;

namespace RAIZA.Interfaces
{
    public interface IUsuarioI
    {
        Task<IEnumerable<Usuario>> GetUsuarios();
        Task<Usuario?> GetUsuarioById(int id);
        Task<Usuario?> GetUsuarioByCorreo(string correo);
        Task<IEnumerable<Usuario>> GetUsuariosByRol(string rol);
        Task<bool> CreateUsuario(Usuario usuario);
        Task<bool> UpdateUsuario(Usuario usuario);
        Task<bool> CambiarEstadoUsuario(int id, string nuevoEstado);
        Task<bool> DeleteUsuario(int id);
    }
}