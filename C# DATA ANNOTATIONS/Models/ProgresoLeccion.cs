using System.ComponentModel.DataAnnotations;

namespace RAIZA.Models
{
    public class ProgresoLeccion
    {
        [Key]
        public int Idprogresoleccion { get; set; }

        [Required(ErrorMessage = "Debe indicar si la lección fue completada.")]
        public bool completado { get; set; }

        public DateTime? fecha { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un estudiante.")]
        public int Idestudiante { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una lección.")]
        public int Idleccion { get; set; }
    }
}