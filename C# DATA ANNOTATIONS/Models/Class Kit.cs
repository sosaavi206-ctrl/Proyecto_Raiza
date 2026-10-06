using System.ComponentModel.DataAnnotations;

namespace RAIZA.Models
{
    public class Class_Kit
    {
        [Key]
        public int idclass_kit { get; set; }

        [Required(ErrorMessage = "El nombre del kit es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string name { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
        public string description { get; set; } = string.Empty;

        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(0.01, 999999.99, ErrorMessage = "El precio debe ser mayor que cero.")]
        public decimal precio { get; set; }

        [Required(ErrorMessage = "El stock disponible es obligatorio.")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        public int stockdisponible { get; set; }

        [Required(ErrorMessage = "El tipo de kit es obligatorio.")]
        [StringLength(50, ErrorMessage = "El tipo no puede superar los 50 caracteres.")]
        public string tipo { get; set; } = string.Empty;

        public int? idmodulo { get; set; }
    }
}