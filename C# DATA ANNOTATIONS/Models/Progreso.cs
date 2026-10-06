using System.ComponentModel.DataAnnotations;

namespace RAIZA.Models
{
    public class Progreso
    {
        [Key]
        public int Idprogreso { get; set; }

        [Required(ErrorMessage = "Debe indicar si el módulo está completado.")]
        public bool Completado { get; set; }

        [Required(ErrorMessage = "El porcentaje de progreso es obligatorio.")]
        [Range(0, 100, ErrorMessage = "El porcentaje debe estar entre 0 y 100.")]
        public decimal Porcentaje { get; set; }

        public DateTime? FechaCompletado { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un estudiante.")]
        public int idestudiante { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un módulo.")]
        public int Idmodulo { get; set; }
    }
}