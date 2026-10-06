using System.ComponentModel.DataAnnotations;

namespace RAIZA.Models
{
    public class Tematica
    {
        [Key]
        public int idtematica { get; set; }

        [Required(ErrorMessage = "El nombre de la temática es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La imagen de portada es obligatoria.")]
        [StringLength(255, ErrorMessage = "La ruta de la imagen no puede superar los 255 caracteres.")]
        public string ImagenPortada { get; set; } = string.Empty;
    }
}