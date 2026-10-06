using System.ComponentModel.DataAnnotations;

namespace RAIZA.Models
{
    public class ClasesEnVivo
    {
        [Key]
        public int idclaasesenvivo { get; set; }

        [Required(ErrorMessage = "La fecha y hora son obligatorias.")]
        public DateTime FechaHora { get; set; }

        [Required(ErrorMessage = "La URL de la sala es obligatoria.")]
        [StringLength(255, ErrorMessage = "La URL no puede superar los 255 caracteres.")]
        public string UrlSala { get; set; } = string.Empty;

        [Required(ErrorMessage = "El estado es obligatorio.")]
        [StringLength(20, ErrorMessage = "El estado no puede superar los 20 caracteres.")]
        public string Estado { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar un módulo.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un módulo válido.")]
        public int idmodulo { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un instructor.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un instructor válido.")]
        public int idinstructor { get; set; }
    }
}