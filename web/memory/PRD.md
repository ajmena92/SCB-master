# PRD — Portal Web de Comedor SCSC

## Problema original
Portal web (en español, móvil-primero) para que estudiantes confirmen/cancelen su asistencia al comedor del día actual antes de la hora límite de su horario. Las confirmaciones alimentan `dbo.RegistroTransporte` (compartida con el reporte WinForms existente). Usuarios administrativos (Operador, Administrador) gestionan menús, sustituciones, PIN de estudiantes, dashboard en tiempo real y correcciones auditadas.

## Arquitectura
- **Frontend**: React (CRA + craco), TailwindCSS, shadcn/ui, recharts, framer-motion. Tipografía Chivo/Karla. Rutas: `/` (login estudiante), `/cambiar-pin`, `/estudiante`, `/admin`, `/admin/panel`.
- **Backend**: FastAPI + pyodbc contra SQL Server institucional. Todas las rutas con prefijo `/api`; no se usa MongoDB ni datos semilla.
- **Auth**: sesiones opacas revocables en cookie `HttpOnly` y CSRF de doble envío; no JWT ni `localStorage`. PBKDF2-HMAC-SHA256 con salt+iteraciones para PIN y compatibilidad con hashes administrativos de `Seguridad.Usuario`. Bloqueo tras 5 intentos fallidos.
- **Zona horaria servidor**: America/Costa_Rica. Cierres: Diurno 09:40, Nocturno 18:40.
- **Modelo de datos**: tablas existentes `dbo.Usuario`, `dbo.Horario`, `dbo.RegistroTransporte`, `Seguridad.Usuario`/roles, y tablas nuevas en `ComedorPortal`. No se modifica ninguna tabla existente. `ConfirmacionAsistencia` relaciona una confirmación con su marca y `MarcaCreadaPorPortal` evita borrar o reinterpretar marcas originadas por escritorio.

## Personas
- Estudiante: consulta menú, confirma/cancela asistencia, cambia PIN.
- Operador: gestiona menú, sustituciones, PIN; consulta dashboard/nominal.
- Administrador: todo lo de Operador + correcciones post-cierre con motivo obligatorio + auditoría.

## Requisitos core (estáticos)
- RF-01 Acceso estudiantil (carné+PIN 6 dígitos, cambio obligatorio primer ingreso, hash+salt). ✅
- RF-02 Menú del día (5 semanas × L-V, sustitución por fecha prevalece, publicación inmediata). ✅
- RF-03 Confirmación asistencia (solo día actual, hora servidor, cierres por horario, "Sí" crea marca en RegistroTransporte, cancelar antes del cierre, consulta tras cierre, sin duplicados). ✅
- RF-04 Dashboard (total en tiempo real, desglose horario/sección/beca, lista nominal, corrección solo Administrador con motivo+auditoría). ✅

## Cobertura web actual (pendiente de validación en staging y sin paridad total)
- Backend con todos los endpoints SQL, sin cuentas/datos de demostración ni migraciones automáticas. Las migraciones se ejecutan manualmente por el DBA.
- Frontend con login estudiante/admin, cambio de PIN, portal estudiante con estados (carga/error/menú/cerrado/confirmado) y panel administrativo con 10 vistas web actuales: Dashboard, Menú, Calendario, Sustituciones, Estudiantes/PIN, Rutas, Correcciones [solo Admin], Parámetros, Auditoría y Registro de transporte.
- El objetivo de producto es la migración progresiva del sistema local a web. Las vistas existentes son cobertura inicial y no implican paridad funcional, operativa o visual con WinForms; cada módulo requiere validación y ficha de paridad antes de retirar su equivalente local.
- Testing histórico del prototipo no sustituye las pruebas de staging contra SQL Server.

## Backlog / pendientes (P1/P2)
- P1: Exportar reporte/nominal a CSV/Excel.
- P1: Historial de menús por mes con vista de calendario.
- P2: Feriados y cierres extraordinarios (excluidos en v1 por requerimiento).
- P1: Validación integral contra staging, antes de cualquier operación en producción.
- P2: Notificaciones/recordatorios antes del cierre.
