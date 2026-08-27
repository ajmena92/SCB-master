# Requisitos del portal web de comedor

## Objetivo

Permitir que cada estudiante decida su asistencia al comedor para el día actual, mostrando antes el menú. La confirmación alimenta los reportes existentes sin sustituir la aplicación de escritorio.

## Roles y operaciones

- **Estudiante:** inicia con `Cedula`/carné y PIN numérico de seis dígitos; debe cambiar el PIN inicial. Ve el menú de hoy, confirma asistencia o cancela su propia confirmación antes del cierre.
- **Operador y Administrador:** usan los usuarios administrativos existentes para publicar menús, asignar o reiniciar PINes, consultar el tablero y ajustar los parámetros exclusivos del portal.
- **Administrador:** además puede corregir una asistencia después del cierre; el motivo es obligatorio y queda auditado.

## Reglas de asistencia

- El sistema solo permite actuar sobre la fecha actual y usa la hora del servidor.
- El cierre corresponde a la hora límite efectiva configurada en `ComedorPortal` para el horario del estudiante; se inicializa desde `dbo.Horario.HoraLimite` pero no modifica `dbo.Horario`. Después del cierre la pantalla queda en solo lectura.
- El aviso previo al cierre (`MinutosAvisoPrevio`) es un parámetro global del portal (de 1 a 120 minutos). Los cambios quedan auditados como evento `ParametrosPortal`, con usuario administrativo, IP y valores anteriores/nuevos.
- Los cambios de hora límite se aplican en caliente, incluso después del cierre: ampliar el límite puede reabrir de inmediato la confirmación del día actual. La siguiente consulta o acción vuelve a calcular la ventana con la hora de SQL Server y el valor auditado.
- “Sí” crea una fila en `dbo.RegistroTransporte` con `IdUsuario`, `IdRuta`, `IdHorario` y `Fecha`.
- “No” no crea una fila. Cancelar antes del cierre elimina únicamente la marca creada por el portal y conserva la auditoría.
- Debe existir una única confirmación por estudiante y fecha; no se deben modificar marcas históricas o creadas por otro flujo.

## Experiencia del estudiante

- Mientras no haya confirmado y el período esté abierto, se muestran el reloj basado en la hora de SQL Server, el tiempo restante y el aviso configurado al acercarse el cierre. Ambos se actualizan en pantalla y se reconcilian periódicamente con el servidor.
- Cuando el estudiante confirma, la interfaz muestra la hora registrada por el servidor, detiene el reloj y oculta el aviso de tiempo. **Confirmar almuerzo** permanece visible pero deshabilitado; **No asistiré** se vuelve rojo y permite retirar la marca antes del cierre.
- Al cancelar, el estado vuelve a no asistir y, si el período continúa abierto, se puede confirmar nuevamente.
- Desde la hora límite exacta, confirmar y cancelar quedan bloqueados tanto en API como en interfaz. Se conserva el menú y se muestra únicamente el resultado final: “Marcó asistencia al comedor” o “No marcó asistencia al comedor”.
- La apertura, el cierre y una extensión dinámica de la hora límite se confirman con la respuesta de SQL Server; el cliente no autoriza acciones por su cuenta.

## Menú y tablero

El menú base se define por semana del mes (1–5) y día laboral (lunes a viernes), con título, componentes ordenados y observaciones. Puede existir una sustitución para una fecha específica. El tablero muestra total confirmado y desglose por horario, sección y beca, además de la lista nominal autorizada.

## Datos complementarios propuestos

- `ComedorPortal.CredencialEstudiante`: `IdUsuario`, hash/salt/iteraciones del PIN, `DebeCambiarPin`, intentos, bloqueo y fechas.
- `ComedorPortal.MenuPlantilla` y `MenuComponente`: semana, día, título, observaciones, orden y tipo de componente.
- `ComedorPortal.MenuSustitucion`: fecha, título, observaciones y usuario que modificó.
- `ComedorPortal.ConfirmacionAsistencia`: estudiante, fecha, `IdRegistroTransporte`, estado, fechas de confirmación/cancelación, administrador y motivo de corrección.
- `ComedorPortal.AuditoriaConfirmacion`: evento, detalle, fecha, IP y actores involucrados, incluido `ParametrosPortal`.
- `ComedorPortal.ConfiguracionPortal` y `ConfiguracionHorario`: aviso previo al cierre y hora límite por horario, exclusivos del portal.
