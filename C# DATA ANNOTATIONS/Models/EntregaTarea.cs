using System.ComponentModel.DataAnnotations;

namespace RAIZA.Models
{
    public class EntregaTarea
    {
        [Key]
        public int Identregatarea { get; set; }

        [Required(ErrorMessage = "La URL del archivo es obligatoria.")]
        [StringLength(255, ErrorMessage = "La URL no puede superar los 255 caracteres.")]
        public string UrlArchivo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de entrega es obligatoria.")]
        public DateTime FechaEntrega { get; set; }

        [Range(0, 5, ErrorMessage = "La calificación debe estar entre 0 y 5.")]
        public decimal? Calificacion { get; set; }

        [StringLength(500, ErrorMessage = "El comentario no puede superar los 500 caracteres.")]
        public string Comentario { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar una tarea.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una tarea válida.")]
        public int idtarea { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un estudiante.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un estudiante válido.")]
        public int idestudiante { get; set; }

        public int? idinstructorcalifica { get; set; }
    }
}