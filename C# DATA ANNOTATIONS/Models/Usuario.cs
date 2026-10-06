using System.ComponentModel.DataAnnotations;

namespace RAIZA.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        // IMPORTANTE: Correo/ContrasenaHash/Rol/Estado NO llevan [Required] ni [EmailAddress].
        // El portal del estudiante actualiza el perfil con un cuerpo parcial ({ id, nombre,
        // telefono, direccion }) y con [ApiController] esos atributos rechazaban la petición con
        // 400 ANTES de llegar al controlador (el correo vacío disparaba "Debe ingresar un correo
        // electrónico válido." y el perfil nunca se guardaba). La validación completa —formato
        // con MailAddress, contraseña, rol y estado— ahora se hace en UsuarioController.CreateUsuario
        // y el PUT solo aplica los campos enviados.
        [StringLength(150, ErrorMessage = "El correo no puede superar los 150 caracteres.")]
        public string Correo { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "La contraseña no puede superar los 255 caracteres.")]
        public string ContrasenaHash { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "El rol no puede superar los 50 caracteres.")]
        public string Rol { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "El estado no puede superar los 20 caracteres.")]
        public string Estado { get; set; } = string.Empty;

        // Datos del perfil (antes solo existían en localStorage; hoy persisten en la BD).
        [StringLength(30, ErrorMessage = "El teléfono no puede superar los 30 caracteres.")]
        public string? Telefono { get; set; }

        [StringLength(150, ErrorMessage = "La dirección no puede superar los 150 caracteres.")]
        public string? Direccion { get; set; }
    }
}