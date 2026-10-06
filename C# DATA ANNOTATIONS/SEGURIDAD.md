# SEGURIDAD - Proyecto RAIZA (Backend)

Resumen de las medidas aplicadas y pendientes para el despliegue seguro de la API.

## 1. Contraseñas con hash (BCrypt)

- `AuthController` (login/registro) ya hasheaba con BCrypt.
- **Nuevo:** `Repository/Usuario_R.cs` ahora también hashea al **crear o actualizar** usuarios desde
  el panel de administración (`POST/PUT /api/Usuario`). Antes se guardaban en texto plano.
  - Se detecta si el valor ya es un hash BCrypt (`$2a$/$2b$/$2y$`) para no re-hashearlo.
  - `UpdateUsuario` ahora es **parcial**: solo reemplaza los campos que llegan con valor.
    Esto evita que la actualización de perfil borre correo, rol o contraseña (bug existente:
    el portal del estudiante solo envía `nombre/telefono/direccion`).

## 2. Secrets fuera del código

- Los valores reales ya NO están en `appsettings.json` (quedaron en blanco).
- Se movieron a **User-Secrets** (solo en tu máquina, en `%APPDATA%\Microsoft\UserSecrets\`):
  - `ConnectionStrings:CadenaConexion`
  - `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`
- Para verlos o modificarlos dentro de la carpeta del proyecto:

```bash
dotnet user-secrets list
dotnet user-secrets set "Jwt:Key" "NUEVA_LLAVE_DE_AL_MENOS_32_CARACTERES"
```

> Los user-secrets solo se cargan con `ASPNETCORE_ENVIRONMENT=Development` (el perfil "https"
> de `launchSettings.json` ya lo activa). **En cualquier otro entorno** define las variables de
> entorno: `ConnectionStrings__CadenaConexion`, `Jwt__Key`, `Jwt__Issuer`, `Jwt__Audience`.

## 3. Autorización por rol implementada en `UsuarioController`

- Clase completa: `[Authorize]` (todo exige JWT, coherente con la FallbackPolicy).
- `GET /api/Usuario` y `POST/PUT/DELETE /api/Usuario` u `/estado`: **solo Administrador**.
- `GET /api/Usuario/rol/{rol}`: **Administrador o Instructor**.
- `GET /api/Usuario/{id}` y `correo/{correo}` y `PUT /api/Usuario/{id}`: cualquier usuario
  autenticado pero **solo sobre su propio perfil** (o si es administrador).
- Un usuario regular **no puede cambiarse el rol ni el estado** (se conservan los del registro).
- El JWT incluye un claim `role` explícito para que `[Authorize(Roles = ...)]` funcione siempre.

## 3.1 Autorización por rol en `TareaController` y `EntregaTareaController`

- `Tarea`: **crear, editar y eliminar** tareas (`POST`, `PUT`, `DELETE /api/Tarea`)
  → solo **Administrador, Instructor**. La **lectura** (GET, GET por id, por módulo,
  mis-tareas) y la entrega (`POST /api/Tarea/entregar`) quedan abiertas a cualquier usuario autenticado.
- `EntregaTarea`: **calificar/modificar** (`PUT /api/EntregaTarea/{id}`) → solo **Administrador, Instructor**.
  Los estudiantes **siguen pudiendo** crear su entrega (`POST`) y eliminarla (`DELETE`) para reenviarla.

## 3.2 Subida de archivos de entrega (`EntregaTarea/subir-archivo`)

- Nuevo endpoint `POST /api/EntregaTarea/subir-archivo` (multipart/form-data, máximo **10 MB**).
- Solo se aceptan extensiones seguras: PDF, JPG, JPEG, PNG, GIF, MP4, DOCX, DOC, ZIP.
- El archivo se guarda en `wwwroot/entregas/` con **nombre aleatorio** (`Guid`) — nunca se usa el
  nombre original como ruta (evita path traversal/inyección).
- La respuesta devuelve la URL pública (`https://host/entregas/{uuid}.ext`) para guardarla en `UrlArchivo`.
- Los archivos subidos son **públicos** (sin token) para que el instructor pueda verlos sin sesión.
- **Nota técnica importante:** la API pasó de usar `FallbackPolicy` (que protegía TAMBIÉN los
  archivos estáticos) a `app.MapControllers().RequireAuthorization()`. Esto exige token en todos los
  endpoints pero **deja públicos los estáticos** de `wwwroot`. El `[AllowAnonymous]` del AuthController
  sigue funcionando igual.

## 3.3 Pedidos de kits (`PedidoKitController`) y actualización parcial de entregas

A partir de esta sesión:

- `GET /api/PedidoKit` (ver todos) y `PUT/DELETE /api/PedidoKit/{id}` (cambiar estado/eliminar)
  → solo **Administrador, Instructor** (la gestión de pedidos es de personal RAIZA).
- `POST /api/PedidoKit` → cualquier usuario autenticado (los estudiantes registran su pedido,
  siempre con estado inicial `Pendiente` y **sin pasarela de pago real**).
- **Nuevo:** `GET /api/PedidoKit/mis-pedidos` → devuelve **únicamente los pedidos del estudiante
  de la sesión** (se lee el claim `IdUsuario` del JWT). El portal del estudiante usa este endpoint
  para que nadie vea los pedidos ajenos ni desbloquee el módulo avanzado con pedidos de otros.
- `EntregaTarea_R.UpdateEntregaTarea` ahora es una **actualización parcial**: solo se reescriben los
  campos que llegan con información (`urlArchivo`, `fechaEntrega`, `calificacion`, `comentario`,
  `idtarea`, `idestudiante`, `idinstructorcalifica`). Así, calificar una entrega solo con
  calificación/instructor no borra el archivo ni el comentario del estudiante. El comentario se
  reescribe únicamente si viene con texto (vacío = no tocar).
- Frontend: el "Editar" del estudiante ya **no usa PUT** (ruta reservada al instructor).
  Reenviar = `DELETE` de la entrega anterior + `POST` de la nueva.
- **Checkout de kits (`POST /api/PedidoKit`):** el API crea internamente la `Compra` (registro de
  pago en `Pendiente`) con el método de pago elegido en el modal (PSE/Tarjeta), el monto =
  `precio del kit × cantidad`, el `idmodulo` del kit, y enlaza su `idcompra` al pedido (la columna
  `pedido_kit.id_compra` es NOT NULL). El `idestudiante` **siempre sale del claim `IdUsuario` del
  JWT** (se ignora el que venga en el cuerpo): así nadie registra pedidos a nombre de otro. Si el
  usuario no tiene fila en la tabla `estudiante` se devuelve 400 con mensaje claro.
  El portal del estudiante usa el catálogo **real** de `class_kit` (nombres, precios en COP e ids
  válidos) en lugar de tarjetas inventadas.
- **Estados admitidos por la BD:** la tabla `pedido_kit` tiene un `CHECK` constraint que solo
  permite `Pendiente`, `Enviado`, `Entregado` y `Cancelado`. El select del portal admin se alineó
  a esos valores (antes ofrecía `Pagado`/`En camino`/`Rechazado`, que la BD rechazaba con 500).
  El **desbloqueo del módulo avanzado** del estudiante se activa cuando uno de sus pedidos de un
  kit avanzado (nombre „Avanzado") queda en `Enviado` o `Entregado` (confirmación de pago y
  despacho); `Cancelado` o `Pendiente` no desbloquean.
- `PedidoKit_R.UpdatePedidoKit` se corrigió para evitar el **conflicto de doble tracking de EF**
  (actualiza la instancia ya rastreada en lugar de adjuntar la del cuerpo; `idcompra` jamás se
  modifica).
- **CHECK de `compra.metodo_pago`:** la BD solo admite `Otro | Nequi | PSE | Tarjeta`. `POST
  /api/PedidoKit` normaliza el método (tolera tildes/espacios y alias del frontend como
  "Tarjeta de Crédito/Débito" → `Tarjeta`) y devuelve 400 con mensaje claro si no reconoce el
  valor (antes la compra fallaba con 500 por violación del CHECK). El select del portal del
  estudiante ahora envía `value="Tarjeta"` en lugar de "Tarjeta de Crédito".
- **Perfil de estudiante en el registro (`POST /api/Auth/Registro`):** el registro ahora es
  **atómico** y crea el `Usuario` + su fila en `estudiante` (`es_premium=false`,
  `fecha_acceso=UTC`). Se usa `Database.CreateExecutionStrategy().ExecuteInTransactionAsync`
  porque el contexto usa `SqlServerRetryingExecutionStrategy` (EnableRetryOnFailure), que
  **no admite `BeginTransactionAsync` manual** (lanzaba `InvalidOperationException`).
- **`estudiante.id` NO es identity en la BD** (solo `usuario.id` lo es). El mapeo EF cambió de
  `ValueGeneratedOnAdd()` a `ValueGeneratedNever()` y el registro inserta el perfil con el
  **mismo id del usuario** (`estudiante.id == usuario.Id`), preservando el invariante que
  asumen el frontend y el claim `IdUsuario` del JWT. Verificado en vivo: estudiante recién
  registrado (id 28) pidió su kit → 201 con `idcompra`.
- `POST /api/Estudiante` se restringió a **Administrador/Instructor** (antes cualquier usuario
  autenticado podía crear filas en `estudiante`, incluidos perfiles fantasma ajenos).

## 3.4 Restricción de escritura por rol en TODOS los controladores (sesión completa)

- `Compra` (`POST/PUT/DELETE /api/Compra`): solo **Administrador, Instructor** (antes cualquier
  autenticado podía crear o reescribir montos/estados de pagos).
- `Estudiante` (`PUT/DELETE`): también solo **Administrador, Instructor** (además del `POST`).
- Catálogos `Modulo`, `Leccion`, `Tematica`, `ClassKit`, `ClasesEnVivo` (`POST/PUT/DELETE`)
  → **Administrador, Instructor**; `Instructor` y `Administrador` (`POST/PUT/DELETE`) → solo
  **Administrador**. El frontend no escribe en estos endpoints, así que nada se rompe.
- `Notificacion` y `Certificado` (`POST/PUT/DELETE`): **Administrador, Instructor** (antes cualquier
  estudiante podía emitirse certificados o notificar a usuarios ajenos).
- **Control de pertenencia (dueño-o-gestor)** en `Progreso`, `ProgresoLeccion` y
  `ClaseParticipante`: un estudiante solo crea/actualiza/borra sus propios registros (claim
  `IdUsuario` del JWT); los gestores operan sobre cualquiera. En el `PUT` el `idestudiante` del
  registro nunca cambia: se conserva el que ya tenía.
- `EntregaTarea`: el `DELETE` exige ser el dueño (claim) o gestor; `POST /api/Tarea/entregar`
  fuerza `idestudiante` y `fechaEntrega` desde el JWT y anula `calificacion`/
  `idinstructorcalifica` si quien entrega no es gestor. `POST /api/EntregaTarea` hace lo mismo
  con el id y la fecha.

## 3.5 Lectura filtrada por usuario (datos personales de terceros)

- Los `GET` de listas con datos personales devuelven **solo los registros propios** cuando quien
  consulta no es gestor: `EntregaTarea`, `Certificado`, `Compra`, `Progreso`, `ProgresoLeccion`,
  `Notificacion` (por `idusuario`) y `Estudiante`. Los gestores (Administrador/Instructor) siguen
  viendo la lista completa.
- Los `GET /{id}` de esas mismas rutas comprueban **dueño-o-gestor** y responden 403 en caso contrario.
- `GET /api/Usuario` (listado) y `rol/{rol}` ya estaban restringidos (ver §3).

## 3.6 Validaciones de valores (evitaban 500 por violación de CHECKs)

- `Controllers/UtilidadesPago.cs` centraliza `NormalizarMetodoPago` (admitidos:
  `Otro|Nequi|PSE|Tarjeta`) y `NormalizarEstado(..., EstadosCompra | EstadosPedidoKit)`
  (`Pendiente|Aprobado|Rechazado` para compras; `Pendiente|Enviado|Entregado|Cancelado` para
  pedidos). Los usan `POST/PUT /api/Compra` y `POST/PUT /api/PedidoKit` → 400 con mensaje claro
  en lugar de 500 por CHECK de la BD.
- `POST /api/Estudiante` valida `idestudiante > 0` (antes aceptaba `id=0` y reventaba en BD).
- `PATCH /api/Usuario/{id}/estado` valida `Activo|Inactivo` (antes pasaba cualquier texto hasta
  que el CHECK de la BD respondía 500).

## 3.7 Perfil de estudiante: teléfono y dirección (columnas nuevas)

- La BD tiene las columnas `usuario.telefono varchar(30) NULL` y `usuario.direccion varchar(150) NULL`
  (ALTER aplicado). El modelo `Usuario` las expone como `string?` y `UpdateUsuario` las mezcla en el
  PUT parcial: `null` en el cuerpo = conservar lo que había, `""` = limpiar el campo.
- `Models/Usuario.cs` ya **no** lleva `[Required]` ni `[EmailAddress]` en `Correo`/`ContrasenaHash`/
  `Rol`/`Estado`: con `[ApiController]` esos atributos rechazaban con 400 ANTES de llegar al
  controlador el cuerpo parcial `{ id, nombre, telefono, direccion }` que manda el portal del
  estudiante (el perfil nunca se guardaba). La validación completa —formato de correo con
  `MailAddress.TryCreate`, contraseña, rol ∈ CHECK, estado ∈ `{Activo,Inactivo}`— ahora se hace
  manualmente en `UsuarioController.CreateUsuario`.
- `UpdateUsuario`, `UpdateProgreso`, `UpdateProgresoLeccion`, `UpdateTematica`, `UpdatePedidoKit`,
  `UpdateEntregaTarea` y `CambiarEstadoUsuario` devuelven `true` aunque `SaveChanges` afecte 0
  filas (valores idénticos = estado final correcto, no un error): antes un guardado sin cambios
  respondía 400 "No fue posible actualizar...".

## 3.8 Errores internos sin filtrar (M2) e invariantes de perfil — cierre backend

- **M2 HECHO:** ninguna respuesta 500 devuelve ya `ex.Message`. Los 98 `catch` de los 18
  controladores usan `Controllers/UtilidadesError.Registrar(ex)`: la excepción COMPLETA se
  imprime en el log del servidor con una referencia (`[RAIZA-ERROR ...] ref=...`) y el cliente
  recibe solo `detalle: "ref XXXXXXXX"` (el frontend lo muestra entre paréntesis y sirve para
  localizar el error exacto en los logs). `AuthController` ya actuaba así.
- **Invariantes usuario ↔ perfil (alta y baja atómicas):**
  - `POST /api/Usuario` crea en UNA transacción el usuario Y su fila de perfil según el rol
    (`estudiante`; `instructor` con especialidad/biografía "Por definir" —se completan después
    con `PUT /api/Instructor/{id}`—; `administrador` con `nivel_acceso = 1`), con la convención
    `perfil.id == usuario.Id`. Antes un usuario creado desde el panel quedaba huérfano (fue el
    origen de los perfiles reparados en la BD).
  - `DELETE /api/Usuario` borra solo la fila `usuario`: las FKs de las tablas de perfil son
    `CASCADE` en la BD, así que la fila de perfil se elimina automáticamente (sin huérfanos).
    Si el perfil tiene registros hijos (entregas, progresos, pedidos...), la FK revierte el
    borrado y el controlador responde 409 con mensaje claro: nada se borra a medias.
- **POST de perfiles validados** (`/api/Estudiante`, `/api/Instructor`, `/api/Administrador`):
  el `id` debe ser > 0, pertenecer a un usuario existente con el MISMO rol, y no tener ya una
  fila propia (409). El DbContext dejó de marcar `instructor.id` y `administrador.id` como
  `ValueGeneratedOnAdd` (NO son identity): antes EF omitía el `id` en el INSERT y el motor
  respondía 500 (columna `id` sin valor).
- **`POST /api/Usuario`:** correo duplicado → 409 (antes 500 por índice único) y contraseña
  mínima de 8 caracteres — la misma regla del registro público y de `app.js`.
- Se eliminó el `WeatherForecastController` de plantilla (era cruft sin relación con el negocio).

## 4. El hash nunca se expone

- Las respuestas de `/api/Usuario*` devuelven `contrasenaHash = null` (OcultarContrasena).

## 5. Pendientes / recomendaciones

- **reCAPTCHA:** hoy se usan las claves de prueba de Google. Antes de producción, reemplaza
  `Recaptcha:SecretKey` (y la site key del frontend) por claves reales.
- **Correo OTP:** configurar `EmailSettings:Remitente` y `EmailSettings:Password` reales
  (los actuales son de ejemplo; el envío del OTP fallará hasta cambiarlos).
- **En Azure:** además de variables de entorno, valorar Azure Key Vault para los secretos.
- Los controladores ya exigen token por la política global (`MapControllers().RequireAuthorization()`),
  y TODA la escritura revisada quedó restringida por rol y/o por pertenencia (ver §3.1–§3.6).
- **M2 — mensajes con `detalle: ex.Message` — HECHO (§3.8):** ya ningún 500 devuelve el texto de
  la excepción; el cliente recibe `detalle: "ref XXXXXXXX"` y la excepción completa queda en el
  log del servidor vía `UtilidadesError.Registrar`.
- En los smoke tests se crean `PedidoKit`+`Compra` reales; al eliminar un pedido de prueba,
  conviene borrar también su `Compra` (`DELETE /api/Compra/{idcompra}`) porque el API conserva
  las compras a propósito (registro de pago auditable).
- **Habilitación de estudiantes nuevos — HECHO:** el registro (`POST /api/Auth/Registro`) ya
  crea la fila en `estudiante` con el mismo id del usuario (`estudiante.id == usuario.Id`;
  `estudiante.id` no es identity, por eso es posible). Un estudiante recién registrado ya puede
  pedir kits y entregar tareas sin intervención del administrador. **PERFILES NO ESTUDIANTES —
  HECHO:** `POST /api/Usuario` crea en la misma transacción la fila de `instructor` o
  `administrador` con la convención `perfil.id == usuario.Id` (ver §3.8).