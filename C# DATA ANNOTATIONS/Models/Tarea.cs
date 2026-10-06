using System.ComponentModel.DataAnnotations;

namespace RAIZA.Models
{
    public class Tarea
    {
        [Key]
        public int idtarea { get; set; }

        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(150, ErrorMessage = "El título no puede superar los 150 caracteres.")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(1000, ErrorMessage = "La descripción no puede superar los 1000 caracteres.")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de entrega es obligatoria.")]
        public DateTime FechaEntrega { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un módulo.")]
        public int idmodulo { get; set; }
    }
}