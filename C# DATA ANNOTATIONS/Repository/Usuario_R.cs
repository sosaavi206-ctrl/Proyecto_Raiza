using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RAIZA.Interfaces;
using RAIZA.Models;
using RAIZA.Data;

namespace RAIZA.Repositories
{
    public class Usuario_R(DatabaseService context) : IUsuarioI
    {
        private readonly DatabaseService _context = context;

        /// <summary>
        /// BCrypt genera hashes de ~60 caracteres que siempre empiezan con "$2a$", "$2b$" o "$2y$".
        /// Permite detectar si el valor ya es un hash (no se re-hashea) o es una contraseña en texto plano.
        /// </summary>
        private static bool EsHashBcrypt(string password) =>
            password.StartsWith("$2a$", StringComparison.Ordinal)
            || password.StartsWith("$2b$", StringComparison.Ordinal)
            || password.StartsWith("$2y$", StringComparison.Ordinal);

        public async Task<IEnumerable<Usuario>> GetUsuarios() =>
            await _context.Usuario.AsNoTracking().ToListAsync();

        public async Task<Usuario?> GetUsuarioById(int id) =>
            await _context.Usuario.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);

        public async Task<Usuario?> GetUsuarioByCorreo(string correo) =>
            await _context.Usuario.AsNoTracking().FirstOrDefaultAsync(u => u.Correo == correo);

        public async Task<IEnumerable<Usuario>> GetUsuariosByRol(string rol) =>
            await _context.Usuario
                .AsNoTracking()
                .Where(u => u.Rol.ToLower() == rol.ToLower())
                .ToListAsync();

        public async Task<bool> CreateUsuario(Usuario usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario.Estado))
            {
                usuario.Estado = "Activo";
            }

            // Seguridad: nunca se guarda la contraseña en texto plano; siempre su hash BCrypt.
            if (!string.IsNullOrWhiteSpace(usuario.ContrasenaHash) && !EsHashBcrypt(usuario.ContrasenaHash))
            {
                usuario.ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(usuario.ContrasenaHash);
            }

            // Alta ATÓMICA: usuario + su fila de perfil según el rol, con la misma convención
            // del registro público (perfil.id == usuario.Id; ninguna de esas columnas es
            // identity). Sin esto, un usuario creado desde el panel quedaba "huérfano" (sin
            // fila en estudiante/instructor/administrador): las consultas con JOIN no lo
            // encontraban y un estudiante no podía entregar tareas ni pedir kits (fue el
            // origen de los perfiles reparados en la BD).
            // CreateExecutionStrategy porque el contexto usa SqlServerRetryingExecutionStrategy
            // (EnableRetryOnFailure), que NO admite BeginTransactionAsync manual.
            var estrategia = _context.Database.CreateExecutionStrategy();
            await estrategia.ExecuteInTransactionAsync(
                async () =>
                {
                    _context.Usuario.Add(usuario);
                    await _context.SaveChangesAsync();

                    switch ((usuario.Rol ?? string.Empty).Trim())
                    {
                        case "Estudiante":
                            _context.Estudiante.Add(new Estudiante
                            {
                                idestudiante = usuario.Id,
                                Espremium = false,
                                FechaAcceso = DateTime.UtcNow
                            });
                            break;
                        case "Instructor":
                            // Valores por defecto: el administrador completa especialidad y
                            // biografía después con PUT /api/Instructor/{id}.
                            _context.Instructor.Add(new Instructor
                            {
                                idinstructor = usuario.Id,
                                Especialidad = "Por definir",
                                Biografia = "Por definir"
                            });
                            break;
                        case "Administrador":
                            _context.Administrador.Add(new Administrador
                            {
                                idadministrador = usuario.Id,
                                NivelAcceso = 1
                            });
                            break;
                    }

                    await _context.SaveChangesAsync();
                },
                async () => await _context.Usuario.AnyAsync(u => u.Id == usuario.Id));

            return true;
        }

        /// <summary>
        /// Actualización PARCIAL: solo reemplaza los campos que llegan con valor, de modo que
        /// actualizar el perfil no borre datos que no se enviaron (correo, rol, contraseña, etc.).
        /// Si la contraseña llega en texto plano se hashea; si ya es un hash (no cambió) se conserva.
        /// </summary>
        public async Task<bool> UpdateUsuario(Usuario usuario)
        {
            var existente = await _context.Usuario.FirstOrDefaultAsync(u => u.Id == usuario.Id);
            if (existente == null) return false;

            if (!string.IsNullOrWhiteSpace(usuario.Nombre)) existente.Nombre = usuario.Nombre;
            if (!string.IsNullOrWhiteSpace(usuario.Correo)) existente.Correo = usuario.Correo;
            if (!string.IsNullOrWhiteSpace(usuario.Rol)) existente.Rol = usuario.Rol;
            if (!string.IsNullOrWhiteSpace(usuario.Estado)) existente.Estado = usuario.Estado;

            // Teléfono/dirección: "" limpia el campo (el cliente lo envió vacío a propósito),
            // null significa que no vino en el cuerpo y se conserva lo que ya había.
            if (usuario.Telefono != null) existente.Telefono = usuario.Telefono;
            if (usuario.Direccion != null) existente.Direccion = usuario.Direccion;

            if (!string.IsNullOrWhiteSpace(usuario.ContrasenaHash))
            {
                existente.ContrasenaHash = EsHashBcrypt(usuario.ContrasenaHash)
                    ? usuario.ContrasenaHash
                    : BCrypt.Net.BCrypt.HashPassword(usuario.ContrasenaHash);
            }

            // 0 filas = los campos enviados ya coincidían con lo guardado (perfil re-sin cambios):
            // no es un error (antes respondía 400 y el portal mostraba "No se pudo actualizar").
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CambiarEstadoUsuario(int id, string nuevoEstado)
        {
            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null) return false;

            usuario.Estado = nuevoEstado;
            // 0 filas = el estado ya era ese: no es un error.
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null) return false;

            // Se borra SOLO la fila usuario: las FKs estudiante/instructor/administrador →
            // usuario son CASCADE en la BD, así que la fila de perfil se elimina sola y no
            // queda huérfana. (Borrar además el perfil de forma explícita hacía fallar con
            // DbUpdateConcurrencyException: el DELETE del hijo afectaba 0 filas porque el
            // cascade ya lo había borrado.) Si el perfil tiene registros hijos (entregas,
            // progresos, pedidos...), la FK lo bloquea y el controlador responde 409:
            // no se borra nada a medias.
            _context.Usuario.Remove(usuario);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}