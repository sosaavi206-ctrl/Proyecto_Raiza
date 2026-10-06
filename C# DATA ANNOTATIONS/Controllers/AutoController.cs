using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RAIZA.Data;
using RAIZA.Models;

namespace RAIZA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private const int PasswordLongitudMinima = 8;
        private const string MensajeCredencialesInvalidas = "Correo o contraseña incorrectos.";

        private static readonly HttpClient HttpGoogle = new() { Timeout = TimeSpan.FromSeconds(10) };

        private readonly IConfiguration _configuration;
        private readonly DatabaseService _context;
        private readonly ILogger<AuthController> _logger;
        private readonly IHostEnvironment _env;

        public AuthController(IConfiguration configuration, DatabaseService context,
                              ILogger<AuthController> logger, IHostEnvironment env)
        {
            _configuration = configuration;
            _context = context;
            _logger = logger;
            _env = env;
        }

        // --- DIAGNÓSTICO ---
        [HttpGet("Diagnostico")]
        public async Task<IActionResult> Diagnostico()
        {
            if (!_env.IsDevelopment()) return NotFound();

            string baseDeDatos;
            try
            {
                await _context.Database.OpenConnectionAsync();
                await _context.Database.CloseConnectionAsync();
                baseDeDatos = "OK";
            }
            catch (Exception ex)
            {
                baseDeDatos = ex.GetBaseException().Message;
            }

            return Ok(new
            {
                baseDeDatos,
                jwtKeyConfigurada = !string.IsNullOrWhiteSpace(_configuration["Jwt:Key"]),
                recaptchaSecretConfigurada = !string.IsNullOrWhiteSpace(_configuration["Recaptcha:SecretKey"])
            });
        }

        // --- 1. LOGIN DIRECTO ---
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            if (login is null || string.IsNullOrWhiteSpace(login.Email) || string.IsNullOrEmpty(login.Password))
            {
                return BadRequest(new { mensaje = "Credenciales incompletas." });
            }

            try
            {
                if (!await ValidarGoogleRecaptcha(login.RecaptchaToken))
                {
                    return BadRequest(new { mensaje = "Verificación de reCAPTCHA fallida. Por favor, inténtalo de nuevo." });
                }

                var correo = NormalizarCorreo(login.Email);
                var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Correo == correo);

                if (usuario is null)
                {
                    _logger.LogWarning("Login rechazado: no existe un usuario con el correo {Correo}", correo);
                    return Unauthorized(new { mensaje = MensajeCredencialesInvalidas });
                }

                var esBcrypt = EsHashBcrypt(usuario.ContrasenaHash);
                bool passwordValida;

                if (esBcrypt)
                {
                    passwordValida = VerificarPassword(login.Password, usuario.ContrasenaHash);
                }
                else
                {
                    passwordValida = CompararCodigos(usuario.ContrasenaHash ?? string.Empty, login.Password);

                    if (passwordValida)
                    {
                        try
                        {
                            usuario.ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(login.Password);
                            await _context.SaveChangesAsync();
                            _logger.LogWarning("La cuenta {Correo} se migró de texto plano a BCrypt.", correo);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "No se pudo guardar el hash BCrypt de {Correo}", correo);
                        }
                    }
                }

                if (!passwordValida)
                {
                    return Unauthorized(new { mensaje = MensajeCredencialesInvalidas });
                }

                if (usuario.Estado != "Activo")
                {
                    return Unauthorized(new { mensaje = "Tu cuenta se encuentra inactiva." });
                }

                return Ok(new
                {
                    Token = GenerarJwt(usuario),
                    Rol = usuario.Rol,
                    Nombre = usuario.Nombre,
                    IdUsuario = usuario.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en Login");
                return StatusCode(500, new
                {
                    mensaje = "Error interno en el servidor.",
                    detalle = _env.IsDevelopment() ? ex.GetBaseException().Message : null
                });
            }
        }

        // --- 2. REGISTRO ---
        [HttpPost("Registro")]
        [HttpPost("Register")]
        public async Task<IActionResult> Registrar([FromBody] RegistroDto datos)
        {
            if (datos is null || string.IsNullOrWhiteSpace(datos.Nombre) ||
                string.IsNullOrWhiteSpace(datos.Correo) || string.IsNullOrEmpty(datos.ContrasenaHash))
            {
                return BadRequest(new { mensaje = "Los datos de registro son obligatorios." });
            }

            if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(datos.Correo))
            {
                return BadRequest(new { mensaje = "El correo electrónico no es válido." });
            }

            if (datos.ContrasenaHash.Length < PasswordLongitudMinima)
            {
                return BadRequest(new { mensaje = $"La contraseña debe tener al menos {PasswordLongitudMinima} caracteres." });
            }

            try
            {
                var correo = NormalizarCorreo(datos.Correo);

                if (await _context.Usuario.AnyAsync(u => u.Correo == correo))
                {
                    return Conflict(new { mensaje = "Ya existe una cuenta registrada con este correo electrónico." });
                }

                var nuevoUsuario = new Usuario
                {
                    Nombre = datos.Nombre.Trim(),
                    Correo = correo,
                    ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(datos.ContrasenaHash),
                    Rol = "Estudiante",
                    Estado = "Activo"
                };

                var estrategia = _context.Database.CreateExecutionStrategy();
                await estrategia.ExecuteInTransactionAsync(
                    async () =>
                    {
                        _context.Usuario.Add(nuevoUsuario);
                        await _context.SaveChangesAsync();

                        _context.Estudiante.Add(new Estudiante
                        {
                            idestudiante = nuevoUsuario.Id,
                            Espremium = false,
                            FechaAcceso = DateTime.UtcNow
                        });
                        await _context.SaveChangesAsync();
                    },
                    async () => await _context.Usuario.AnyAsync(u => u.Id == nuevoUsuario.Id));

                return Ok(new { mensaje = "Usuario registrado correctamente. Ya puedes iniciar sesión." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar usuario");
                return StatusCode(500, new { mensaje = "Error al registrar el usuario." });
            }
        }

        // --- 3. ACTUALIZAR CONTRASEÑA DIRECTO (Sin código de verificación) ---
        [HttpPost("Actualizar-Password-Directo")]
        public async Task<IActionResult> ActualizarPasswordDirecto([FromBody] ActualizarPasswordDirectoDto modelo)
        {
            if (modelo is null || string.IsNullOrWhiteSpace(modelo.Email) || string.IsNullOrEmpty(modelo.NuevaContrasena))
            {
                return BadRequest(new { mensaje = "El correo y la nueva contraseña son obligatorios." });
            }

            if (modelo.NuevaContrasena.Length < PasswordLongitudMinima)
            {
                return BadRequest(new { mensaje = $"La contraseña debe tener al menos {PasswordLongitudMinima} caracteres." });
            }

            try
            {
                var correo = NormalizarCorreo(modelo.Email);
                var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Correo == correo);

                if (usuario is null)
                {
                    return NotFound(new { mensaje = "No se encontró una cuenta registrada con este correo electrónico." });
                }

                usuario.ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(modelo.NuevaContrasena);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Contraseña actualizada directamente para el usuario {Correo}", correo);

                return Ok(new { mensaje = "Contraseña actualizada correctamente. Ya puedes iniciar sesión." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la contraseña directamente");
                return StatusCode(500, new { mensaje = "Error al actualizar la contraseña." });
            }
        }

        // ---------------- Métodos auxiliares ----------------

        private async Task<bool> ValidarGoogleRecaptcha(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return false;

            var secretKey = _configuration["Recaptcha:SecretKey"];
            if (string.IsNullOrWhiteSpace(secretKey)) return false;

            try
            {
                using var contenido = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["secret"] = secretKey,
                    ["response"] = token
                });

                using var respuesta = await HttpGoogle.PostAsync("https://www.google.com/recaptcha/api/siteverify", contenido);
                if (!respuesta.IsSuccessStatusCode) return false;

                using var json = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
                var raiz = json.RootElement;

                return raiz.TryGetProperty("success", out var ok) && ok.GetBoolean();
            }
            catch
            {
                return false;
            }
        }

        private static bool VerificarPassword(string passwordPlano, string hashGuardado)
        {
            if (string.IsNullOrEmpty(hashGuardado)) return false;
            try
            {
                return BCrypt.Net.BCrypt.Verify(passwordPlano, hashGuardado);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                return false;
            }
        }

        private static bool EsHashBcrypt(string? valor) =>
            !string.IsNullOrEmpty(valor) && valor.Length == 60 && valor.StartsWith("$2");

        private static bool CompararCodigos(string esperado, string recibido)
        {
            var a = Encoding.UTF8.GetBytes(esperado);
            var b = Encoding.UTF8.GetBytes(recibido);
            return CryptographicOperations.FixedTimeEquals(a, b);
        }

        private static string NormalizarCorreo(string correo) => correo.Trim().ToLowerInvariant();

        private string GenerarJwt(Usuario usuario)
        {
            var clave = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Falta la configuración Jwt:Key");

            var credenciales = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clave)),
                SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, usuario.Correo),
                new(ClaimTypes.Role, usuario.Rol),
                new("role", usuario.Rol),
                new("IdUsuario", usuario.Id.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credenciales);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string RecaptchaToken { get; set; } = string.Empty;
    }

    public class RegistroDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string ContrasenaHash { get; set; } = string.Empty;
    }

    public class ActualizarPasswordDirectoDto
    {
        public string Email { get; set; } = string.Empty;
        public string NuevaContrasena { get; set; } = string.Empty;
    }
}