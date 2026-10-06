using System.ComponentModel.DataAnnotations;

namespace RAIZA.Models
{
    public class Compra
    {
        [Key]
        public int idcompra { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio.")]
        [Range(0.01, 99999999.99, ErrorMessage = "El monto debe ser mayor que cero.")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "El método de pago es obligatorio.")]
        [StringLength(50, ErrorMessage = "El método de pago no puede superar los 50 caracteres.")]
        public string MetodoPago { get; set; } = string.Empty;

        [Required(ErrorMessage = "El estado es obligatorio.")]
        [StringLength(30, ErrorMessage = "El estado no puede superar los 30 caracteres.")]
        public string Estado { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "La referencia de Wompi no puede superar los 100 caracteres.")]
        public string ReferenciaWompi { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de compra es obligatoria.")]
        public DateTime FechaCompra { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un estudiante.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un estudiante válido.")]
        public int idestudiante { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un módulo.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un módulo válido.")]
        public int idmodulo { get; set; }
    }
}