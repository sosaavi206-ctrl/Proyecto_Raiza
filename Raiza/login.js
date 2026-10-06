const API_BASE_URL = "https://localhost:7248/api";

// ---------------- Utilidades ----------------

function mostrarAlerta(mensaje) {
    const box = document.getElementById('alertaBox');
    if (box) {
        box.textContent = mensaje;
        box.hidden = false;
        box.style.display = 'block';
    } else {
        alert(mensaje);
    }
}

function ocultarAlerta() {
    const box = document.getElementById('alertaBox');
    if (box) {
        box.textContent = '';
        box.style.display = 'none';
        box.hidden = true;
    }
}

async function leerJson(response) {
    try {
        return await response.json();
    } catch {
        return {};
    }
}

function mensajePorEstado(status) {
    switch (status) {
        case 400: return "Los datos enviados no son válidos.";
        case 401: return "No autorizado. Revisa tus credenciales.";
        case 403: return "No tienes permiso para realizar esta acción.";
        case 404: return "Servicio no encontrado.";
        case 429: return "Demasiados intentos. Espera un momento.";
        default:
            return status >= 500 ? `Error del servidor (código ${status}).` : `Error inesperado (código ${status}).`;
    }
}

function textoDeError(data, response, fallback) {
    const base = data.mensaje || fallback || mensajePorEstado(response.status);
    return data.detalle ? `${base} (${data.detalle})` : base;
}

function bloquearBoton(boton, bloquear, textoCargando) {
    if (!boton) return;
    if (bloquear) {
        boton.dataset.textoOriginal = boton.textContent;
        boton.textContent = textoCargando;
        boton.disabled = true;
    } else {
        boton.textContent = boton.dataset.textoOriginal || boton.textContent;
        boton.disabled = false;
    }
}

function reiniciarCaptcha() {
    if (typeof grecaptcha !== 'undefined') {
        grecaptcha.reset();
    }
}

// ---------------- Interfaz: Pestañas y Contraseña ----------------

function togglePassword(inputId, btn) {
    const input = document.getElementById(inputId);
    if (input.type === 'password') {
        input.type = 'text';
        btn.textContent = '🙈';
    } else {
        input.type = 'password';
        btn.textContent = '👁️';
    }
}

function cambiarVista(vista) {
    const formLogin = document.getElementById('formLogin');
    const formRegistro = document.getElementById('formRegistro');
    const tabLogin = document.getElementById('tabLogin');
    const tabRegistro = document.getElementById('tabRegistro');
    
    ocultarAlerta();

    if (vista === 'login') {
        formLogin.style.display = 'block';
        formRegistro.style.display = 'none';
        tabLogin.classList.add('activo');
        tabRegistro.classList.remove('activo');
    } else {
        formLogin.style.display = 'none';
        formRegistro.style.display = 'block';
        tabLogin.classList.remove('activo');
        tabRegistro.classList.add('activo');
    }
}

// ---------------- Paso 1: Login Directo ----------------

async function enviarLogin(e) {
    e.preventDefault();
    ocultarAlerta();

    const email = document.getElementById('email').value.trim();
    const password = document.getElementById('password').value;

    if (typeof grecaptcha === 'undefined') {
        mostrarAlerta("No se pudo cargar reCAPTCHA. Revisa tu conexión y recarga la página.");
        return;
    }

    const recaptchaToken = grecaptcha.getResponse();
    if (!recaptchaToken) {
        mostrarAlerta("Por favor, completa la verificación 'No soy un robot'.");
        return;
    }

    const boton = e.target.querySelector('button[type="submit"]');
    bloquearBoton(boton, true, 'Iniciando sesión...');

    try {
        const response = await fetch(`${API_BASE_URL}/Auth/Login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                Email: email,
                Password: password,
                RecaptchaToken: recaptchaToken
            })
        });

        const data = await leerJson(response);

        if (!response.ok) {
            mostrarAlerta(textoDeError(data, response, "Correo o contraseña incorrectos."));
            reiniciarCaptcha();
            return;
        }

        const token = data.token ?? data.Token;
        const rolCrudo = data.rol ?? data.Rol ?? "";

        if (!token) {
            mostrarAlerta("Respuesta inesperada del servidor. Inténtalo de nuevo.");
            reiniciarCaptcha();
            return;
        }

        localStorage.setItem('raiza_jwt', token);
        localStorage.setItem('raiza_rol', rolCrudo);
        localStorage.setItem('raiza_nombre', data.nombre ?? data.Nombre ?? "");
        localStorage.setItem('raiza_idUsuario', data.idUsuario ?? data.IdUsuario ?? "");

        const rol = rolCrudo.trim().toLowerCase();

        if (rol.includes('estudiante')) {
            window.location.href = 'portal-estudiante.html';
        } else if (rol.includes('instructor')) {
            window.location.href = 'portal-instructor.html';
        } else {
            window.location.href = 'portal-admin.html';
        }
    } catch (error) {
        console.error("Error de conexión:", error);
        mostrarAlerta("No se pudo conectar con el servidor. Verifica que la API esté en ejecución.");
        reiniciarCaptcha();
    } finally {
        bloquearBoton(boton, false);
    }
}

// ---------------- Paso 2: Registro ----------------

async function enviarRegistro(e) {
    e.preventDefault();
    ocultarAlerta();

    const nombre = document.getElementById('regNombre').value.trim();
    const email = document.getElementById('regEmail').value.trim();
    const password = document.getElementById('regPassword').value;

    const cumpleRequisitos = /^(?=.*[A-Za-z])(?=.*\d).{8,}$/.test(password);

    if (!cumpleRequisitos) {
        mostrarAlerta('La contraseña debe tener al menos 8 caracteres e incluir al menos una letra y un número.');
        return;
    }

    const boton = e.target.querySelector('button[type="submit"]');
    bloquearBoton(boton, true, 'Creando cuenta...');

    try {
        const response = await fetch(`${API_BASE_URL}/Auth/Registro`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ 
                Nombre: nombre, 
                Correo: email, 
                ContrasenaHash: password 
            })
        });

        const data = await leerJson(response);

        if (response.ok) {
            mostrarAlerta("¡Cuenta creada con éxito! Ahora puedes iniciar sesión.");
            cambiarVista('login');
            document.getElementById('email').value = email;
        } else {
            mostrarAlerta(textoDeError(data, response, "Error al registrar la cuenta."));
        }
    } catch (error) {
        console.error("Error de conexión:", error);
        mostrarAlerta("No se pudo conectar con el servidor.");
    } finally {
        bloquearBoton(boton, false);
    }
}

// ---------------- Actualización Directa de Contraseña ----------------

function mostrarVistaRecuperar(e) {
    e.preventDefault();
    const card = document.querySelector('.login-card');

    card.innerHTML = `
        <h2>Actualizar Contraseña</h2>
        <div id="alertaBox" class="mensaje-alerta" role="alert" aria-live="polite" hidden></div>
        <p class="otp-descripcion">Ingresa tu correo electrónico y tu nueva contraseña para cambiarla de inmediato.</p>

        <form onsubmit="ejecutarActualizacionDirecta(event)">
            <div class="form-group">
                <label for="emailRecuperar">Correo Electrónico</label>
                <input type="email" id="emailRecuperar" required placeholder="tu@correo.com">
            </div>
            <div class="form-group">
                <label for="nuevaPassword">Nueva Contraseña</label>
                <div class="input-wrapper">
                    <input type="password" id="nuevaPassword" required placeholder="Mínimo 8 caracteres">
                    <button type="button" class="btn-mostrar-pass" onclick="togglePassword('nuevaPassword', this)">👁️</button>
                </div>
            </div>
            <button type="submit" class="btn-submit">Actualizar Contraseña</button>
        </form>

        <div class="extra-links" style="margin-top: 15px;">
            <p><a href="login.html">Volver al inicio de sesión</a></p>
        </div>
    `;
}

async function ejecutarActualizacionDirecta(e) {
    e.preventDefault();
    ocultarAlerta();

    const email = document.getElementById('emailRecuperar').value.trim();
    const nuevaContrasena = document.getElementById('nuevaPassword').value;

    const cumpleRequisitos = /^(?=.*[A-Za-z])(?=.*\d).{8,}$/.test(nuevaContrasena);
    if (!cumpleRequisitos) {
        mostrarAlerta('La contraseña debe tener al menos 8 caracteres e incluir al menos una letra y un número.');
        return;
    }

    const boton = e.target.querySelector('button[type="submit"]');
    bloquearBoton(boton, true, 'Actualizando...');

    try {
        const response = await fetch(`${API_BASE_URL}/Auth/Actualizar-Password-Directo`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                Email: email,
                NuevaContrasena: nuevaContrasena
            })
        });

        const data = await leerJson(response);

        if (response.ok) {
            mostrarAlerta("¡Contraseña actualizada con éxito! Ya puedes iniciar sesión.");
            setTimeout(() => {
                window.location.href = 'login.html';
            }, 2500);
        } else {
            mostrarAlerta(textoDeError(data, response, "Error al actualizar la contraseña."));
        }
    } catch (error) {
        console.error("Error de conexión:", error);
        mostrarAlerta("No se pudo conectar con el servidor.");
    } finally {
        bloquearBoton(boton, false);
    }
}

// ---------------- Banner de cookies e inicialización ----------------

document.addEventListener("DOMContentLoaded", () => {
    const accion = new URLSearchParams(window.location.search).get('action');
    if (accion === 'register' || accion === 'registro') {
        cambiarVista('registro');
    }

    const cookieBanner = document.getElementById('cookieBanner');
    const btnAceptar = document.getElementById('btnAceptarCookies');

    if (cookieBanner && btnAceptar) {
        if (!localStorage.getItem('raiza_cookies_aceptadas')) {
            setTimeout(() => {
                cookieBanner.classList.add('mostrar');
            }, 500);
        }

        btnAceptar.addEventListener('click', () => {
            localStorage.setItem('raiza_cookies_aceptadas', 'true');
            cookieBanner.classList.remove('mostrar');
        });
    }
});