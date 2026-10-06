using System.ComponentModel.DataAnnotations;

namespace RAIZA.Models
{
    public class Modulo
    {
        [Key]
        public int idmodulo { get; set; }

        [Required(ErrorMessage = "El nivel es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nivel no puede superar los 50 caracteres.")]
        public string Nivel { get; set; } = string.Empty;

        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(0.01, 99999999.99, ErrorMessage = "El precio debe ser mayor que cero.")]
        public decimal Precio { get; set; }

        public bool IncluyeKit { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una temática.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una temática válida.")]
        public int idtematica { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un instructor.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un instructor válido.")]
        public int idinstructor { get; set; }
    }
}