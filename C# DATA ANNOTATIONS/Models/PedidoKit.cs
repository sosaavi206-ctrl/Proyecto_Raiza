using System.ComponentModel.DataAnnotations;

namespace RAIZA.Models
{
    public class PedidoKit
    {
        [Key]
        public int idPedidoKit { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que cero.")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio.")]
        [StringLength(30, ErrorMessage = "El estado no puede superar los 30 caracteres.")]
        public string Estado { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección de envío es obligatoria.")]
        [StringLength(255, ErrorMessage = "La dirección no puede superar los 255 caracteres.")]
        public string Direccionenvio { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha del pedido es obligatoria.")]
        public DateTime Fechapedido { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un estudiante.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un estudiante válido.")]
        public int Idestudiante { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un kit.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un kit válido.")]
        public int idclasskit { get; set; }

        public int? idcompra { get; set; }
    }
}