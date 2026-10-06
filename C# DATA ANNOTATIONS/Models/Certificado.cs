using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RAIZA.Models
{
    [Table("certificado")]
    public class Certificado
    {
        [Key]
        [Column("id")]
        public int idcertificado { get; set; }

        [Required(ErrorMessage = "La fecha de emisión es obligatoria.")]
        [Column("fecha_emision")]
        public DateTime FechaEmision { get; set; }

        [Required(ErrorMessage = "La URL del PDF es obligatoria.")]
        [StringLength(255, ErrorMessage = "La URL no puede superar los 255 caracteres.")]
        [Column("url_pdf")]
        public string UrlPdf { get; set; } = string.Empty;

        [Required(ErrorMessage = "El código de verificación es obligatorio.")]
        [StringLength(100, ErrorMessage = "El código de verificación no puede superar los 100 caracteres.")]
        [Column("codigo_verificacion")]
        public string CodigoVerificacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar un estudiante.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un estudiante válido.")]
        [Column("id_estudiante")]
        public int idestudiante { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un módulo.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un módulo válido.")]
        [Column("id_modulo")]
        public int idmodulo { get; set; }
    }
}