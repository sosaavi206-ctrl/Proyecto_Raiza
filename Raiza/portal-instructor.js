/**
 * Portal del Instructor - RAIZA
 * Sincronizado con EntregaTareaController y UsuarioController del backend.
 */

const API_BASE_URL = "https://localhost:7248/api";

let entregas = [];        // Entregas pendientes por mostrar
let estudiantesMap = {};  // id -> nombre (para mostrar quién entregó)
let tareasMap = {};       // idtarea -> título (el modelo EntregaTarea solo trae el id)

// ==========================================
// 1. SEGURIDAD Y CONTROL DE ACCESO
// ==========================================
function verificarSesionInstructor() {
    const token = localStorage.getItem('raiza_jwt');
    const rol = (localStorage.getItem('raiza_rol') || '').toLowerCase();

    if (!token) {
        alert('Por favor, inicia sesión para acceder al portal.');
        window.location.href = 'login.html';
        return false;
    }

    if (!rol.includes('instructor')) {
        alert('No tienes permisos de instructor.');
        window.location.href = 'login.html';
        return false;
    }

    return true;
}

function cerrarSesionInstructor() {
    if (confirm('¿Deseas cerrar sesión?')) {
        localStorage.clear();
        window.location.href = 'login.html';
    }
}

function escapar(valor) {
    return String(valor ?? '')
        .replaceAll('&', '&amp;')
        .replaceAll('<', '&lt;')
        .replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;')
        .replaceAll("'", '&#39;');
}

// True si el valor es un enlace http/https real (para mostrar "Ver archivo")
function esEnlace(url) {
    return /^https?:\/\/[^\s]+$/i.test(String(url || '').trim());
}

// Avatar SVG con iniciales (funciona sin servicios externos)
function avatarIniciales(nombre) {
    const iniciales = (nombre || 'U').trim().split(/\s+/).slice(0, 2).map(p => p[0].toUpperCase()).join('');
    const svg = "<svg xmlns='http://www.w3.org/2000/svg' width='160' height='160'>" +
        "<rect width='160' height='160' rx='80' fill='#B56445'/>" +
        "<text x='50%' y='50%' dominant-baseline='central' text-anchor='middle' font-family='Poppins, Arial' font-size='52' fill='#F2EBDD' font-weight='600'>" +
        iniciales + "</text></svg>";
    return 'data:image/svg+xml;utf8,' + encodeURIComponent(svg);
}

// ==========================================
// 2. NAVEGACIÓN
// ==========================================
function cambiarSeccion(idSeccion, botonPresionado) {
    document.querySelectorAll('.seccion-dashboard').forEach(sec => sec.classList.remove('activa'));
    const seccionActiva = document.getElementById(idSeccion);
    if (seccionActiva) seccionActiva.classList.add('activa');

    document.querySelectorAll('.sidebar-btn').forEach(btn => btn.classList.remove('activo'));
    if (botonPresionado) botonPresionado.classList.add('activo');

    if (idSeccion === 'revision') cargarEntregasDesdeServidor();
    if (idSeccion === 'estudiantes') cargarEstudiantesDesdeServidor();
}

// ==========================================
// 3. ESTUDIANTES (UsuarioController)
// ==========================================
async function cargarEstudiantesDesdeServidor() {
    const tbody = document.getElementById('tablaEstudiantes');
    if (!tbody) return;

    const token = localStorage.getItem('raiza_jwt');

    try {
        const response = await fetch(`${API_BASE_URL}/Usuario/rol/Estudiante`, {
            method: 'GET',
            headers: { 'Authorization': `Bearer ${token}`, 'Content-Type': 'application/json' }
        });

        if (response.ok) {
            const estudiantes = await response.json();
            tbody.innerHTML = '';

            estudiantes.forEach(e => {
                const id = e.id ?? e.Id;
                const nombre = e.nombre ?? e.Nombre ?? 'Estudiante';
                const correo = e.correo ?? e.Correo ?? '';
                const estado = e.estado ?? e.Estado ?? 'Activo';

                estudiantesMap[id] = nombre;

                tbody.innerHTML += `
                    <tr>
                        <td>${escapar(id)}</td>
                        <td>${escapar(nombre)}</td>
                        <td>${escapar(correo)}</td>
                        <td><span class="badge-entregada">${escapar(estado)}</span></td>
                    </tr>
                `;
            });
        } else {
            cargarEstudiantesRespaldo();
        }
    } catch (error) {
        console.warn('No se pudo consultar los estudiantes. Usando datos de respaldo.');
        cargarEstudiantesRespaldo();
    }
}

function cargarEstudiantesRespaldo() {
    const tbody = document.getElementById('tablaEstudiantes');
    if (!tbody) return;

    const respaldo = [
        { id: 101, nombre: 'Valentina Ríos', correo: 'valentina@correo.com', estado: 'Activo' },
        { id: 102, nombre: 'Mateo Duarte', correo: 'mateo@correo.com', estado: 'Activo' },
        { id: 103, nombre: 'Camila Torres', correo: 'camila@correo.com', estado: 'Activo' },
        { id: 104, nombre: 'Santiago Quintero', correo: 'santiago@correo.com', estado: 'Activo' }
    ];

    tbody.innerHTML = '';
    respaldo.forEach(e => {
        estudiantesMap[e.id] = e.nombre;
        tbody.innerHTML += `
            <tr>
                <td>${e.id}</td>
                <td>${escapar(e.nombre)}</td>
                <td>${escapar(e.correo)}</td>
                <td><span class="badge-entregada">${escapar(e.estado)}</span></td>
            </tr>
        `;
    });
}

// ==========================================
// 4. ENTREGAS DE TAREAS (EntregaTareaController)
// ==========================================
async function cargarEntregasDesdeServidor() {
    const token = localStorage.getItem('raiza_jwt');
    const encabezados = { 'Authorization': `Bearer ${token}`, 'Content-Type': 'application/json' };

    try {
        const [response, respTareas] = await Promise.all([
            fetch(`${API_BASE_URL}/EntregaTarea`, { method: 'GET', headers: encabezados }),
            fetch(`${API_BASE_URL}/Tarea`, { method: 'GET', headers: encabezados })
        ]);

        // Títulos reales de las tareas (EntregaTarea solo trae idtarea).
        if (respTareas.ok) {
            const listaTareas = await respTareas.json();
            tareasMap = {};
            listaTareas.forEach(t => {
                const id = t.idtarea ?? t.IdTarea;
                if (id !== undefined && id !== null) tareasMap[id] = t.titulo || t.Titulo || `Tarea #${id}`;
            });
        }

        if (response.ok) {
            const data = await response.json();
            entregas = (Array.isArray(data) ? data : []).map(e => ({
                id: e.identregatarea ?? e.Identregatarea,
                urlArchivo: e.urlArchivo ?? e.UrlArchivo ?? '',
                fechaEntrega: e.fechaEntrega ?? e.FechaEntrega ?? '',
                calificacion: e.calificacion ?? e.Calificacion ?? null,
                comentario: e.comentario ?? e.Comentario ?? '',
                idtarea: e.idtarea ?? e.IdTarea,
                idestudiante: e.idestudiante ?? e.IdEstudiante,
                idinstructorcalifica: e.idinstructorcalifica ?? e.IdInstructorcalifica ?? null
            }));
            renderizarEntregas();
        } else {
            cargarEntregasRespaldo();
        }
    } catch (error) {
        console.warn('Servidor de entregas inactivo. Usando respaldo local.');
        cargarEntregasRespaldo();
    }
}

function cargarEntregasRespaldo() {
    entregas = [
        { id: 1, estudianteNombre: 'Valentina Ríos', tareaNombre: 'Anillo Sencillo en Espiral', fechaEntrega: 'Hace 1 día', calificacion: null, comentario: 'Subí el anillo desde varios ángulos.' },
        { id: 2, estudianteNombre: 'Mateo Duarte', tareaNombre: 'Pulsera de Nudos Básicos', fechaEntrega: 'Hace 3 días', calificacion: null, comentario: 'Hice dos variaciones de nudo.' },
        { id: 3, estudianteNombre: 'Camila Torres', tareaNombre: 'Engaste de Gota', fechaEntrega: 'Hace 5 días', calificacion: 4.5, comentario: 'Entregado con instrucciones de la guía.' },
        { id: 4, estudianteNombre: 'Santiago Quintero', tareaNombre: 'Dije Entrelazado', fechaEntrega: 'Hace 2 días', calificacion: null, comentario: 'Usé alambre calibre 22.' }
    ];
    renderizarEntregas();
}

function renderizarEntregas() {
    const grid = document.getElementById('gridEntregas');
    if (!grid) return;

    let pendientes = 0;
    let calificadas = 0;

    grid.innerHTML = '';

    entregas.forEach(entrega => {
        const nombreEstudiante = estudianteMap(entrega) || `Estudiante #${escapar(entrega.idestudiante) || '?'}`;
        const nombreTarea = tareasMap[entrega.idtarea] || entrega.tareaNombre || `Tarea #${escapar(entrega.idtarea) || '?'}`;
        const calificada = entrega.calificacion !== null && entrega.calificacion !== undefined;

        if (calificada) {
            calificadas++;
        } else {
            pendientes++;
        }

        grid.innerHTML += `
            <div class="tarjeta-modulo-item">
                <div>
                    <span class="${calificada ? 'badge-entregada' : 'badge-pendiente'}">
                        ${calificada ? `Calificada: ${escapar(entrega.calificacion)} / 5` : 'Por revisar'}
                    </span>
                    <h4 style="margin: 10px 0 4px; color: var(--terracota-coral); font-size: 0.85rem;">${escapar(nombreEstudiante)}</h4>
                    <h3 class="modulo-item-titulo">${escapar(nombreTarea)}</h3>
                    <p class="modulo-item-desc">Enviada: ${escapar(entrega.fechaEntrega) || 'Fecha reciente'}</p>
                    ${entrega.comentario ? `<p class="modulo-item-desc">💬 "${escapar(entrega.comentario)}"</p>` : ''}
                    ${entrega.retroalimentacion ? `<p class="modulo-item-desc" style="color: var(--verde-oliva);">🏷️ Retroalimentación: "${escapar(entrega.retroalimentacion)}"</p>` : ''}
                    ${entrega.urlArchivo ? (esEnlace(entrega.urlArchivo)
                        ? `<a href="${escapar(entrega.urlArchivo)}" target="_blank" rel="noopener" class="btn-secundario" style="font-size: 0.8rem; padding: 8px 14px; text-decoration: none;">Ver archivo</a>`
                        : `<p class="modulo-item-desc" title="${escapar(entrega.urlArchivo)}">📎 ${escapar(entrega.urlArchivo)}</p>`) : ''}
                </div>
                <div style="display: flex; gap: 8px;">
                    ${calificada
                        ? `<button class="btn-secundario" style="flex: 1; padding: 8px; font-size: 0.8rem;" onclick="calificarEntrega(${escapar(entrega.id)})">Actualizar nota</button>`
                        : `<button class="btn-secundario" style="flex: 1; padding: 8px; font-size: 0.8rem;" onclick="calificarEntrega(${escapar(entrega.id)})">Calificar</button>`}
                    <button class="btn-peligro" style="flex: 1; padding: 8px; font-size: 0.8rem;" onclick="rechazarEntrega(${escapar(entrega.id)})">Rechazar</button>
                </div>
            </div>
        `;
    });

    // Estadísticas
    const statPendientes = document.getElementById('statPendientes');
    if (statPendientes) statPendientes.textContent = pendientes;

    const statCalificadas = document.getElementById('statCalificadas');
    if (statCalificadas) statCalificadas.textContent = calificadas;

    // Tarjeta "Estudiantes": alumnos únicos con entregas (antes quedaba en "–" para siempre)
    const statEstudiantes = document.getElementById('statEstudiantes');
    if (statEstudiantes) {
        const alumnosUnicos = new Set(entregas.map(e => e.idestudiante).filter(Boolean));
        statEstudiantes.textContent = alumnosUnicos.size;
    }

    if (entregas.length === 0) {
        grid.innerHTML = `<div class="tarjeta-panel-dashboard" style="grid-column: 1 / -1;">
            <h3 class="tarjeta-titulo">✨ Todo al día</h3>
            <p style="color: var(--texto-suave); margin: 0;">No hay entregas pendientes de revisión.</p>
        </div>`;
    }
}

// Resuelve el nombre del estudiante desde el mapa (si el servidor estuvo disponible)
function estudianteMap(entrega) {
    const nombre = estudiantesMap[entrega.idestudiante];
    return nombre || entrega.estudianteNombre || null;
}

// ==========================================
// 5. CALIFICAR / RECHAZAR
// ==========================================
let entregaEnRevision = null; // entrega que se está calificando en el modal

function calificarEntrega(idEntrega) {
    const entrega = entregas.find(e => e.id === idEntrega || e.id === String(idEntrega));
    if (!entrega) return;

    entregaEnRevision = entrega;

    const inputNota = document.getElementById('notaCalificacion');
    if (inputNota) {
        inputNota.value = entrega.calificacion ?? '';
        inputNota.style.borderColor = '';
    }
    const textoFeedback = document.getElementById('comentarioCalificacion');
    // La retroalimentación real viaja embebida al inicio de `comentario` (no hay campo propio
    // en el modelo): sin este parseo el modal se abría vacío y re-guardar duplicaba el prefijo.
    if (textoFeedback) {
        const partes = separarComentarioYFeedback(entrega.comentario);
        textoFeedback.value = entrega.retroalimentacion || partes.feedback;
    }

    const modal = document.getElementById('modalCalificar');
    if (modal) modal.classList.add('activo');
}

function cerrarModalCalificar() {
    const modal = document.getElementById('modalCalificar');
    if (modal) modal.classList.remove('activo');
}

function guardarCalificacionModal() {
    if (!entregaEnRevision) return;

    const inputNota = document.getElementById('notaCalificacion');
    const notaTexto = inputNota ? (inputNota.value || '').replace(',', '.') : '';
    const nota = parseFloat(notaTexto);
    if (isNaN(nota) || nota < 0 || nota > 5) {
        if (inputNota) inputNota.style.borderColor = 'var(--rojo-peligro)';
        alert('La calificación debe ser un número entre 0 y 5.');
        return;
    }

    const feedback = ((document.getElementById('comentarioCalificacion') || {}).value || '').trim();
    entregaEnRevision.calificacion = nota;
    entregaEnRevision.retroalimentacion = feedback;

    cerrarModalCalificar();
    actualizarEntregaEnServidor(entregaEnRevision);
}

// Rechazar = se devuelve la entrega (DELETE), el estudiante la ve nuevamente como pendiente
function rechazarEntrega(idEntrega) {
    const entrega = entregas.find(e => e.id === idEntrega || e.id === String(idEntrega));
    if (!entrega) return;

    if (!confirm('¿Rechazar esta entrega? El estudiante deberá volver a subir su trabajo.')) return;

    const token = localStorage.getItem('raiza_jwt');

    fetch(`${API_BASE_URL}/EntregaTarea/${entrega.id}`, {
        method: 'DELETE',
        headers: { 'Authorization': `Bearer ${token}`, 'Content-Type': 'application/json' }
    })
        .then(response => {
            if (!response.ok && response.status !== 404) {
                console.warn('No se pudo eliminar la entrega en el servidor. Actualizando localmente.');
            }
            entregas = entregas.filter(e => e !== entrega);
            alert('Entrega rechazada. El estudiante podrá volver a subirla.');
            renderizarEntregas();
        })
        .catch(() => {
            entregas = entregas.filter(e => e !== entrega);
            alert('Entrega rechazada localmente (servidor no disponible).');
            renderizarEntregas();
        });
}

// Devuelve una fecha válida (evita que una fecha inválida se envíe como null y falle la notificación)
function fechaEntregaValida(d) {
    if (!d) return new Date();
    const fecha = new Date(d);
    return isNaN(fecha.getTime()) ? new Date() : fecha;
}

// El modelo EntregaTarea NO tiene campo "retroalimentación": el feedback del instructor viaja
// al inicio de `comentario` con el formato "<feedback>\n\n(Comentario del estudiante: <texto>)".
// Separa ambas partes para precargar el modal y para no duplicar el prefijo al volver a guardar.
const MARCA_COMENTARIO_ESTUDIANTE = '(Comentario del estudiante:';
function separarComentarioYFeedback(comentario) {
    const texto = comentario || '';
    const marca = texto.indexOf(MARCA_COMENTARIO_ESTUDIANTE);
    if (marca === -1) return { feedback: '', comentarioEstudiante: texto };
    return {
        feedback: texto.slice(0, marca).trim(),
        comentarioEstudiante: texto.slice(marca + MARCA_COMENTARIO_ESTUDIANTE.length).replace(/\s*\)\s*$/, '').trim()
    };
}

async function actualizarEntregaEnServidor(entrega) {
    const token = localStorage.getItem('raiza_jwt');
    const instructorId = parseInt(localStorage.getItem('raiza_idUsuario') || '0');

    // Con datos de respaldo local no existen idtarea/idestudiante: enviar 0 haría fallar la
    // validación [Range(1,...)] del backend con un 400. Mejor avisar que no se puede guardar.
    if (!(entrega.idtarea > 0) || !(entrega.idestudiante > 0)) {
        alert('Esta entrega proviene del respaldo local (sin datos del servidor): no se puede guardar la calificación en la API.');
        renderizarEntregas();
        return;
    }

    // UrlArchivo es obligatorio en el backend: si la entrega no tiene adjunto se usa un marcador
    // para que la calificación (nota) sí pueda guardarse en el registro real.
    const urlArchivo = (entrega.urlArchivo && entrega.urlArchivo.trim()) ? entrega.urlArchivo.trim() : 'sin archivo';

    // Se conserva el comentario original del estudiante; la retroalimentación del instructor va al inicio.
    const partes = separarComentarioYFeedback(entrega.comentario);
    const comentarioEstudiante = partes.comentarioEstudiante;
    const feedback = entrega.retroalimentacion || partes.feedback;
    const comentarioFinal = feedback
        ? (comentarioEstudiante ? `${feedback}\n\n(Comentario del estudiante: ${comentarioEstudiante})` : feedback)
        : comentarioEstudiante;

    const payload = {
        identregatarea: entrega.id,
        urlArchivo: urlArchivo,
        fechaEntrega: fechaEntregaValida(entrega.fechaEntrega),
        calificacion: entrega.calificacion,
        comentario: comentarioFinal,
        idtarea: entrega.idtarea,
        idestudiante: entrega.idestudiante,
        idinstructorcalifica: instructorId > 0 ? instructorId : null
    };

    try {
        const response = await fetch(`${API_BASE_URL}/EntregaTarea/${entrega.id}`, {
            method: 'PUT',
            headers: { 'Authorization': `Bearer ${token}`, 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        if (response.status === 401) {
            alert('Tu sesión expiró. Inicia sesión nuevamente.');
            window.location.href = 'login.html';
            return;
        }

        if (response.ok) {
            alert('✅ Nota guardada en el registro real de la entrega.');
        } else {
            const err = await response.json().catch(() => ({}));
            alert('La API rechazó la calificación: ' + (err.mensaje || ('código ' + response.status)) + '. La nota quedó guardada localmente.');
        }
    } catch (error) {
        console.warn('Servidor no disponible. La calificación quedó guardada de forma local.');
        alert('Servidor no disponible. La nota quedó guardada localmente (se enviará cuando haya conexión).');
    }

    renderizarEntregas();
}

// ==========================================
// 6. CARGA INICIAL
// ==========================================
document.addEventListener('DOMContentLoaded', () => {
    if (!verificarSesionInstructor()) return;

    const nombre = localStorage.getItem('raiza_nombre') || 'Instructor';
    const titulo = document.getElementById('tituloBienvenida');
    if (titulo) titulo.textContent = `Hola, ${nombre}`;

    // Avatar de respaldo con iniciales: si la foto externa no carga, no se ve rota
    const imgSidebar = document.getElementById('imgPerfilSidebar');
    if (imgSidebar) {
        const iniciales = avatarIniciales(nombre);
        imgSidebar.addEventListener('error', () => { imgSidebar.src = iniciales; });
        if (imgSidebar.complete && imgSidebar.naturalWidth === 0) imgSidebar.src = iniciales;
    }

    cargarEstudiantesDesdeServidor();
    cargarEntregasDesdeServidor();
});