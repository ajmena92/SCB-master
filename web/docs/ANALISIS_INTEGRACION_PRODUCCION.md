# Análisis de integración a producción

## Dictamen

**No desplegar ni conectar este proyecto a la base de datos de producción todavía sin completar staging.** El backend actual usa SQL Server mediante consultas parametrizadas, sin MongoDB ni datos semilla; la puerta pendiente es validación de esquema, transacciones, seguridad de red y operación.

## Funcionalidad encontrada

| Área | Estado actual |
| --- | --- |
| Estudiante | Login con carné/PIN, cambio obligatorio de PIN, consulta de menú, confirmación/cancelación y cierre por horario. |
| Administración | Login, dashboard por horario/sección/beca, lista nominal, plantillas, sustituciones, restablecimiento de PIN, auditoría y corrección. |
| Menú | Plantilla semanas 1–5, lunes a viernes; sustitución por fecha y calendario mensual. |
| Reporte | Consulta y exportación CSV de la información simulada de `RegistroTransporte`. |

El frontend es React y el backend es Python/FastAPI contra SQL Server. La diferencia respecto a la preferencia inicial de Node.js/TypeScript queda explícitamente aceptada para conservar el backend generado e integrado; no existen colecciones MongoDB ni datos de demostración en el flujo productivo.

## Bloqueos P0 antes de producción

1. Las migraciones `001` y `002` autorizadas requieren respaldo, validación en staging y ejecución manual por el DBA; la API no ejecuta DDL.
2. Deben validarse los nombres/tipos reales de columnas de seguridad y los formatos de hash con una cuenta SQL inicialmente de solo lectura.
3. Confirmación, vínculo de marca y auditoría deben probarse bajo concurrencia y con rollback forzado en staging.
4. La cancelación desvincula primero por la FK y elimina solo una fila explícitamente vinculada y marcada `MarcaCreadaPorPortal=1`. Las marcas locales se reutilizan o desvinculan, nunca se borran ni se reinterpretan.
5. `Origen` es un valor calculado por el portal; no se agrega ninguna columna a `dbo.RegistroTransporte`.

## Integración recomendada

Usar una única base SQL Server institucional para las tablas existentes y el esquema nuevo `ComedorPortal`. El backend —preferiblemente reescrito en Node.js/TypeScript según la decisión del proyecto— debe usar procedimientos almacenados o transacciones parametrizadas para:

1. Verificar estudiante activo, horario y hora límite con la hora de SQL Server.
2. Insertar/actualizar `ConfirmacionAsistencia`, insertar o eliminar únicamente su `RegistroTransporte` vinculado y escribir auditoría en una misma transacción.
3. Resolver menú, PIN y panel desde SQL Server. Las contraseñas/PIN se guardan solo como hash con salt.

El navegador solo consume HTTPS del backend institucional. La base debe permanecer en la red interna, con una cuenta SQL de mínimo privilegio; nunca se expone el puerto SQL ni credenciales al frontend.

## Pendientes de requisito y validación

- Confirmar si la corrección administrativa permite fechas históricas. El prototipo permite cualquier fecha, pero registra la marca con la hora actual; eso desalinearía reportes por fecha.
- Definir el formato exacto del carné y el procedimiento de entrega/reinicio seguro de PIN.
- Acordar si Operador puede ver auditoría completa y exportar listados con datos personales.
- Confirmar qué pasa cuando no existe menú o el día es feriado/cierre institucional.
- Definir retención de auditoría, sesiones, respaldos, monitoreo, tasa máxima de intentos y recuperación ante fallos.
- Validar en una copia restaurada: esquema real de roles, datos nulos de `IdRuta`, restricciones de `RegistroTransporte`, reporte Crystal y concurrencia de dos confirmaciones simultáneas.

## Puerta de salida a producción

Solo continuar después de: revisión del modelo real en ambiente de prueba, migración versionada y revisada, pruebas de transacción/rollback y carga, usuarios de mínimo privilegio, secretos fuera del repositorio, CORS limitado al dominio institucional, HTTPS y plan de reversión probado. La primera conexión a producción debe ser de solo lectura y con respaldo verificado.
