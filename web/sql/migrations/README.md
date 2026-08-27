# Migraciones SQL

Las migraciones se aplican manualmente y en orden, primero sobre una copia de staging. La API nunca ejecuta DDL.

- `001_menu_storage.sql`: alcance autorizado para tablas de menú; sigue requiriendo validación y ejecución manual del DBA.
- `002_portal_state.sql`: alcance autorizado para PIN, sesión, confirmación y auditoría. Sigue requiriendo revisión, respaldo, prueba en staging y ejecución manual del DBA.
- `003_portal_settings.sql`: alcance autorizado para la hora límite por horario, `MinutosAvisoPrevio` y auditoría de parámetros, todos exclusivos de `ComedorPortal`. Inicializa la hora desde `dbo.Horario` sin alterarla, amplía de forma migrable el evento `ParametrosPortal` de la auditoría existente y elimina en cascada la configuración portal si el escritorio borra un horario.
- `004_student_cards.sql`: fotografía protegida del estudiante, restricciones de tamaño/dimensiones y eventos de auditoría para fotos, beneficios y generación de carnets. Reutiliza `dbo.Usuario.TipoBeca` y no cambia el contrato del sistema local.
- `005_route_card_colors.sql`: configuración visual por ruta vinculada a `dbo.Ruta`, colores iniciales y auditoría de cambios de ruta. `dbo.Usuario.IdRuta` sigue siendo la asignación operativa.
- `006_rutas_catalogo.sql`: actualiza las descripciones de las rutas 2026 desde los documentos Word y habilita la auditoría del CRUD web de rutas. No cambia las asignaciones de `dbo.Usuario.IdRuta`.
- `007_transporte_autonomo.sql`: crea el esquema canónico independiente `transporte` para rutas, asignaciones y auditoría. Es repetible y no lee ni modifica tablas heredadas; requiere aplicar primero en staging con respaldo verificado.
- `008_identidad_canonica.sql`: crea de forma repetible el esquema `identidad` para usuarios, sesiones, permisos y asignaciones, con Argon2id, restricciones e índices. No lee ni modifica tablas heredadas. Su rollback controlado está en `008_identidad_canonica_revertir.sql` y es destructivo: requiere respaldo y autorización explícita del DBA.
- `010_asistencia_autonoma.sql`: crea de forma repetible el esquema canónico independiente `asistencia` para marcas y correcciones. No lee ni modifica tablas heredadas.
- `011_beneficios_autonomo.sql`: crea de forma repetible el esquema canónico independiente `beneficios` para catálogo y asignaciones. No lee ni modifica tablas heredadas.
- `012_cuentas_autonomas.sql`: crea de forma repetible el esquema canónico independiente `cuentas` para saldos y movimientos idempotentes. No lee ni modifica tablas heredadas.

`002` no altera tablas `dbo` ni `Seguridad`. Sus claves foráneas protegen confirmaciones históricas, preservan auditoría mediante `SET NULL` y revocan sesiones si se elimina su identidad. La aplicación conserva la procedencia: al cancelar solo puede borrar una fila de `dbo.RegistroTransporte` si la confirmación vinculada la marca explícitamente como `MarcaCreadaPorPortal=1`; una marca reutilizada del escritorio se desvincula, nunca se borra.

No ejecutar ninguna migración desde el contenedor ni con la cuenta de ejecución de la API.
