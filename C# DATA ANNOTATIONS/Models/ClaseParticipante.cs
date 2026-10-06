using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RAIZA.Models
{
    [Table("clase_participante")]
    public class ClaseParticipante
    {
        [Required(ErrorMessage = "La clase es obligatoria.")]
        [Column("id_clase")]
        public int idclase { get; set; }

        [Required(ErrorMessage = "El estudiante es obligatorio.")]
        [Column("id_estudiante")]
        public int idestudiante { get; set; }

        [Column("fecha_ingreso")]
        public DateTime? FechaIngreso { get; set; }
    }
}