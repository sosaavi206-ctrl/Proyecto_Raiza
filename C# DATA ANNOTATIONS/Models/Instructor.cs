using System.ComponentModel.DataAnnotations;

namespace RAIZA.Models
{
    public class Instructor
    {
        [Key]
        public int idinstructor { get; set; }

        [Required(ErrorMessage = "La especialidad es obligatoria.")]
        [StringLength(100, ErrorMessage = "La especialidad no puede superar los 100 caracteres.")]
        public string Especialidad { get; set; } = string.Empty;

        [Required(ErrorMessage = "La biografía es obligatoria.")]
        [StringLength(1000, ErrorMessage = "La biografía no puede superar los 1000 caracteres.")]
        public string Biografia { get; set; } = string.Empty;
    }
}