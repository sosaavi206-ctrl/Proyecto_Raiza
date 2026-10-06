using System.ComponentModel.DataAnnotations;

namespace RAIZA.Models
{
    public class Leccion
    {
        [Key]
        public int idleccion { get; set; }

        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(150, ErrorMessage = "El título no puede superar los 150 caracteres.")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de lección es obligatorio.")]
        [StringLength(50, ErrorMessage = "El tipo no puede superar los 50 caracteres.")]
        public string Tipo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El orden es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El orden debe ser mayor que cero.")]
        public int Orden { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un módulo.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un módulo válido.")]
        public int idmodulo { get; set; }
    }
}