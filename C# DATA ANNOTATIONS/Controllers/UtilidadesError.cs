namespace RAIZA.Controllers
{
    /// <summary>
    /// M2 — manejo seguro de errores internos (respuestas 500).
    /// Antes los controladores devolvían "detalle = ex.Message" al cliente: eso filtra
    /// información interna de EF/SQL (nombres de tablas y columnas, mensajes del motor,
    /// rutas). Ahora la excepción COMPLETA se registra en el log del servidor (consola
    /// de `dotnet run` / logs del host) con una referencia, y el cliente recibe solo
    /// esa referencia corta ("ref 1A2B3C4D") para poder reportarla y localizar el
    /// error exacto en los logs.
    /// </summary>
    public static class UtilidadesError
    {
        public static string Registrar(Exception ex)
        {
            string referencia = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
            Console.Error.WriteLine($"[RAIZA-ERROR {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC] ref={referencia} {ex}");
            return $"ref {referencia}";
        }
    }
}
