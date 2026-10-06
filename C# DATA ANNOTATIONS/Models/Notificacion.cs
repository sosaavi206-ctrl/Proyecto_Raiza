using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RAIZA.Models

    {
    [Table("notificacion")]
    public class Notificacion
        {
            [Key]
            [Column("id")]
            public int Idnotificacion { get; set; }

            [Column("tipo_notificacion")]
            public string tiponotificacion { get; set; } = string.Empty;

            [Column("mensaje")]
            public string mensaje { get; set; } = string.Empty;

            [Column("estado_leido")]
            public bool estadoleido { get; set; }

            [Column("fecha_envio")]
            public DateTime fechaenvivo { get; set; }

            [Column("id_usuario")]
            public int idusuario { get; set; }
        }
    }

    