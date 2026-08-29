# Requisitos del portal web de comedor

## Objetivo

Permitir que cada estudiante decida su asistencia al comedor para el día actual, mostrando antes el menú. La confirmación alimenta los reportes existentes sin sustituir la aplicación de escritorio.

## Roles y operaciones

- **Estudiante:** inicia con `Cedula`/carné y PIN numérico de seis dígitos; debe cambiar el PIN inicial. Ve el menú de hoy, confirma asistencia o cancela su propia confirmación antes del cierre.
- **Operador y Administrador:** usan los usuarios administrativos existentes para publicar menús, asignar o reiniciar PINes, consultar el tablero y ajustar los parámetros exclusivos del portal.
- **Administrador:** además puede corregir una asistencia después del cierre; el motivo es obligatorio y queda auditado.

## Estado de comedor y tiquetes

El estado operativo de un estudiante para comedor es exclusivamente uno de estos valores:

- `becado_comedor`: no compra tiquetes; su ingreso se registra con modalidad `beca`.
- `no_becado_comedor`: requiere un tiquete válido para reservar asistencia y entrar al comedor.

La autorización debe consultar únicamente el estado canónico y la disponibilidad del
tiquete. Los profesores también requieren tiquete, pero no son estudiantes becados ni
no becados.

La reserva se realiza al confirmar asistencia y el consumo al registrar el ingreso.
Cancelar antes del cierre libera la reserva. Reserva y consumo deben ser atómicos e
impedir que un mismo tiquete se utilice dos veces.

> Estado actual: el contrato canónico ya define personas, estados, cuentas, reservas e
> ingresos. La migración `0023` corresponde al registro histórico de modalidad y no
> debe confundirse con un endpoint vigente. La compra, la reserva y el ingreso deben
> validarse contra el contrato canónico y sus pruebas de persistencia.

## Reglas de asistencia

- El sistema solo permite actuar sobre la fecha actual y usa la hora del servidor.
- El reloj operativo oficial es `GETDATE()` de SQL Server, configurado con la zona local del centro (`America/Costa_Rica`). Los timestamps técnicos con `SYSUTCDATETIME()` se almacenan en UTC; no se usan para decidir el cierre operativo.
- El cierre corresponde a la hora límite efectiva configurada en `comedor.horario_operacion` para el horario del estudiante. La configuración se administra desde el módulo `parametros` y no depende del sistema local.
- El aviso previo al cierre (`minutosAvisoPrevio`) es un parámetro global web (de 1 a 120 minutos). Los cambios se auditan con usuario administrativo y valores anteriores/nuevos.
- Los cambios de hora límite se aplican en caliente, incluso después del cierre: ampliar el límite puede reabrir de inmediato la confirmación del día actual. La siguiente consulta o acción vuelve a calcular la ventana con la hora de SQL Server y el valor auditado.
- Transporte solo expone la existencia de la marca diaria importada en `transporte.uso_diario`; no aporta la hora para calcular tardanzas y el comedor no crea ni modifica esa marca.
- La operación de comedor registra su propia fecha/hora de servidor, horario aplicado y si existía uso diario de transporte.
- La operación permite o rechaza tardanzas y ausencia de transporte mediante políticas independientes (`permitirMarcaTardia` y `permitirSinMarcaTransporte`).
- La API de kiosco está bajo `/api/v1/comedor/operacion`; cada ingreso conserva la hora límite, resultado, advertencias y políticas aplicadas.
- Un segundo ingreso de la misma persona en la misma fecha responde `409` con código `ingreso_duplicado`.
- Debe existir una única confirmación por estudiante y fecha; no se deben modificar marcas históricas o creadas por otro flujo.

## Profesores y estadísticas

Los profesores pueden confirmar asistencia y consumir tiquetes, pero se identifican
como personas de tipo `profesor`. Las estadísticas estudiantiles excluyen siempre a
los profesores: no suman padrón, asistencia, consumo, becas, rutas ni secciones.
Una vista o filtro explícito de profesores puede mostrar sus propias confirmaciones,
ingresos y consumo. Los estudiantes inactivos no aparecen en el padrón activo; sus
marcas e ingresos históricos se conservan para consulta histórica.

## Experiencia del estudiante

- Mientras no haya confirmado y el período esté abierto, se muestran el reloj basado en la hora de SQL Server, el tiempo restante y el aviso configurado al acercarse el cierre. Ambos se actualizan en pantalla y se reconcilian periódicamente con el servidor.
- Cuando el estudiante confirma, la interfaz muestra la hora registrada por el servidor, detiene el reloj y oculta el aviso de tiempo. **Confirmar almuerzo** permanece visible pero deshabilitado; **No asistiré** se vuelve rojo y permite retirar la marca antes del cierre.
- Al cancelar, el estado vuelve a no asistir y, si el período continúa abierto, se puede confirmar nuevamente.
- Desde la hora límite exacta, confirmar y cancelar quedan bloqueados tanto en API como en interfaz. Se conserva el menú y se muestra únicamente el resultado final: “Marcó asistencia al comedor” o “No marcó asistencia al comedor”.
- La apertura, el cierre y una extensión dinámica de la hora límite se confirman con la respuesta de SQL Server; el cliente no autoriza acciones por su cuenta.

## Menú y tablero

El menú base se define por semana del mes (1–5) y día laboral (lunes a viernes), con título, componentes ordenados y observaciones. Puede existir una sustitución para una fecha específica. El tablero muestra total confirmado y desglose por horario, sección y estado de comedor, además de la lista nominal autorizada. La vista estudiantil excluye profesores; la vista de profesores se habilita mediante filtro explícito.

## Contratos y pruebas de aceptación

El contrato público debe separar las operaciones de estado de comedor, cuentas y
movimientos de tiquetes, reservas, ingreso por carnet y estadísticas por tipo de
persona. Los errores mínimos son: persona inactiva, carnet no reconocido, tiquete
agotado, reserva duplicada e intento de ingreso sin reserva.

La integración se acepta únicamente con pruebas HTTP contra la aplicación y pruebas
de persistencia en SQL Server de staging para becados, no becados, profesores,
reservas, consumo único, concurrencia, históricos, migración y smoke test posterior.
Las pruebas de contrato no sustituyen las pruebas unitarias de backend ni las pruebas
de componentes del frontend. Las pruebas HTTP independientes están en
`backend/tests/integracion/test_requerimiento_comedor.py`.

## Corte y reconciliación de datos

La revisión Alembic `0033_horarios_origen_comedor` conserva el `IdHorario` de
`dbo.Horario` en `comedor.horario_operacion.id_horario_origen`. El mapeo soportado es
determinista: el primer horario por `IdHorario` es `diurno` y el segundo es
`nocturno`. Si el origen contiene más de dos horarios, el corte aborta con `50064`;
no se descartan límites ni se inventan turnos.

Antes de crear el índice único de carnés, la migración detecta carnés duplicados y
aborta con `50034`. El reconciliador puede ejecutarse en lectura y luego con
`--apply`, siempre con escrituras congeladas. Compara saldos, conteos totales y por
fecha, estados de comedor y profesores habilitados entre las tablas de origen
disponibles y el catálogo web. Las tablas de origen se consultan solo durante el
corte y nunca en la operación web.

Las revisiones `0034_migracion_datos_legados` y `0035_normaliza_estado_horario_comedor` trasladan usuarios, rutas, transporte,
asistencia, comedor y fotografías al modelo canónico. Las filas duplicadas de
comedor se conservan en `comedor.migracion_ingreso_0034` y se registran en
`comedor.reconciliacion_migracion`.

## Datos complementarios propuestos

- `ComedorPortal.CredencialEstudiante`: `IdUsuario`, hash/salt/iteraciones del PIN, `DebeCambiarPin`, intentos, bloqueo y fechas.
- `ComedorPortal.MenuPlantilla` y `MenuComponente`: semana, día, título, observaciones, orden y tipo de componente.
- `ComedorPortal.MenuSustitucion`: fecha, título, observaciones y usuario que modificó.
- `ComedorPortal.ConfirmacionAsistencia`: estudiante, fecha, `IdRegistroTransporte`, estado, fechas de confirmación/cancelación, administrador y motivo de corrección.
- `ComedorPortal.AuditoriaConfirmacion`: evento, detalle, fecha, IP y actores involucrados, incluido `ParametrosPortal`.
- `comedor.parametro` y `comedor.horario_operacion`: aviso previo y hora límite por horario, administrados por el módulo `parametros`.

La implementación definitiva deberá reemplazar este inventario histórico por tablas
canónicas web de personas habilitadas, estado de comedor, cuenta/movimiento de
tiquetes, reserva e ingreso. La migración requiere respaldo, congelamiento de
escrituras, reconciliación y aprobación del DBA; no se permite doble escritura.
