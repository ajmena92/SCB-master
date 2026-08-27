# Contrato de integración del frontend

El frontend usa `/api` en el mismo origen. Estas condiciones son obligatorias para el backend final.

## Sesión y CSRF

- `GET /api/auth/csrf` inicia la protección previa al login y establece `scsc_csrf`: cookie legible por JavaScript, `Secure`, `SameSite=Lax`, `Path=/`.
- `POST /api/v1/identidad/autenticacion` recibe `{ nombreUsuario, contrasena }` y devuelve el contrato de autenticación canónico.
- `GET /api/v1/identidad/sesion` consulta la sesión activa y `POST /api/v1/identidad/sesion/cerrar` la revoca; las mutaciones requieren CSRF.
- Toda solicitud mutante requiere `X-CSRF-Token` con el valor actual de `scsc_csrf`; el cliente lo lee solo para enviar el encabezado, nunca guarda una sesión en almacenamiento web.

## Rutas funcionales

Las vistas modulares nuevas consumen exclusivamente `/api/v1/...` y cookies, sin `Authorization: Bearer`. Las rutas administrativas antiguas solo permanecen en componentes históricos mientras se migra su dominio correspondiente; no deben utilizarse para nuevas funcionalidades.

`GET /api/student/attendance/today` devuelve `estado` (minúscula; `Confirmada`, `Cancelada`, `Corregida` o `null`), `horaLimite` en formato `HH:mm`, `horaServidor`, `fechaHoraConfirmacionServidor`, `periodoAbierto`, `periodoCerrado`, `segundosParaApertura`, `segundosParaCierre` y `minutosAvisoPrevio`. Los nombres son parte del contrato público y no se deben cambiar de mayúsculas/minúsculas. Las respuestas de confirmar o cancelar incluyen también `estado`, `horaLimite` y `minutosAvisoPrevio`; SQL Server conserva la autoridad de tiempo.

La ventana está abierta en `[horaInicio, horaLimite)`: desde la hora límite exacta, `periodoCerrado` es verdadero y los POST de confirmación/cancelación responden conflicto. Si se amplía la hora límite desde Parámetros, la siguiente consulta devuelve nuevamente la ventana abierta cuando corresponda.

`GET` y `PUT /api/v1/administracion/parametros` son para usuarios autorizados: usan `{ minutosAvisoPrevio, horarios: [{ idHorario, horaLimite }] }`, donde `minutosAvisoPrevio` es entero entre 1 y 120 y `horaLimite` es `HH:mm`.

`GET /api/v1/estudiantes` es paginado: acepta `pagina` (desde 1), `tamano` (1–100, predeterminado 50) y `buscar`; devuelve `{ elementos, pagina, tamano, total }`.

`GET /api/v1/transporte/rutas` devuelve el catálogo completo con `idRuta`, `codigo`, `descripcion`, `activo`, `colorCarnetHex` y `estudiantesAsignados`. `GET /api/v1/transporte/rutas/paleta` devuelve los colores oficiales sugeridos. `POST /api/v1/transporte/rutas` y `PUT /api/v1/transporte/rutas/{idRuta}` reciben `{ codigo, descripcion, colorHex, activo }`; `colorHex` acepta cualquier color hexadecimal válido y la interfaz también ofrece la paleta oficial como selección rápida. La ruta `0` no puede modificarse y las rutas usadas se desactivan lógicamente con `activo=false`. Estas operaciones requieren el permiso `rutas.administrar`.

Las respuestas de error usan `{ "detail": "mensaje seguro" }` y los estados 401, 403, 409, 422 y 429 según corresponda. Una respuesta 401 a una ruta protegida obliga al cliente a cerrar su estado local.

## Despliegue

El proxy HTTPS institucional debe servir la SPA para rutas del navegador como `/estudiante` y `/admin/panel`, y enrutar `/api/` a FastAPI. SQL Server nunca se expone al navegador.
