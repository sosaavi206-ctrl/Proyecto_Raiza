using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RAIZA.Models
{
    [Table("administrador")]
    public class Administrador
    {
        [Key]
        [Column("id")]
        public int idadministrador { get; set; }

        [Required(ErrorMessage = "El nivel de acceso es obligatorio.")]
        [Range(1, 5, ErrorMessage = "El nivel de acceso debe estar entre 1 y 5.")]
        [Column("nivel_acceso")]
        public int NivelAcceso { get; set; }
    }
}