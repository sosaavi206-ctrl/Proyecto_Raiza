using System.Globalization;
using System.Text;

namespace RAIZA.Controllers
{
    /// <summary>
    /// Valores canónicos de los CHECK constraints de la BD y normalizadores para que el código
    /// jamás escriba un valor que la BD rechace (provocando un 500 por violación de constraint).
    /// </summary>
    internal static class UtilidadesPago
    {
        // CHECK compra.estado
        public static readonly string[] EstadosCompra = { "Pendiente", "Aprobado", "Rechazado" };

        // CHECK pedido_kit.estado
        public static readonly string[] EstadosPedidoKit = { "Pendiente", "Enviado", "Entregado", "Cancelado" };

        // CHECK compra.metodo_pago
        public static string? NormalizarMetodoPago(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return null;

            var clave = QuitarDiacriticos(valor.Trim())
                .ToLowerInvariant()
                .Replace(" ", string.Empty)
                .Replace("-", string.Empty)
                .Replace("_", string.Empty);

            if (clave.StartsWith("tarjeta")) return "Tarjeta";      // Tarjeta, Tarjeta de Crédito/Débito, tarjeta credito...
            if (clave.StartsWith("nequi")) return "Nequi";
            if (clave.StartsWith("pse")) return "PSE";
            if (clave.StartsWith("otro")) return "Otro";

            return null;
        }

        /// <summary>Devuelve el valor canónico si pertenece a la lista; null si no es válido.</summary>
        public static string? NormalizarEstado(string? valor, string[] permitidos)
        {
            if (string.IsNullOrWhiteSpace(valor)) return null;

            var limpio = valor.Trim();
            foreach (var permitido in permitidos)
            {
                if (limpio.Equals(permitido, StringComparison.OrdinalIgnoreCase)) return permitido;
            }

            return null;
        }

        private static string QuitarDiacriticos(string texto)
        {
            var formD = texto.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in formD)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}
