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
- `017_intentos_autenticacion.sql`: crea el estado compartido de bloqueo de autenticación en `identidad`, almacenando únicamente el hash del identificador para que la política sea común entre workers y réplicas.
- `018_plantillas_menu.sql`: crea las tablas canónicas `menu.plantilla` y `menu.componente` consumidas por la API administrativa.
- `019_migra_menu_historico.sql`: traslada de forma idempotente las plantillas y componentes existentes desde `ComedorPortal` al esquema canónico `menu`, sin eliminar la fuente histórica.
- `020_completa_componentes_menu.sql`: completa de forma idempotente los componentes si una ejecución anterior de la transferencia solo alcanzó a crear las plantillas.
- `024_corte_comedor_tiquetes.sql`: materializa el padrón de comedor con estado explícito `becado_comedor`/`no_becado_comedor`, cuentas, reservas, movimientos e ingresos atómicos; respalda y reconcilia el histórico de `comedor.registro` sin eliminarlo ni perder filas. Requiere respaldo, congelamiento de escrituras y validación DBA en staging.
- `026_idempotencia_corte_comedor.sql`: agrega la huella de idempotencia de recargas y conserva los movimientos históricos; requiere ejecutar después de `024` y de las revisiones `025` existentes.
- `027_catalogo_profesores_identidad.sql`: incorpora únicamente usuarios web existentes con un rol activo exactamente `Profesor` o `Docente` al catálogo de comedor y crea su cuenta inicial en cero. No crea usuarios, no consulta `dbo` y deja `colegio` NULL porque identidad no contiene ese dato. Requiere el preflight de correspondencia de nombres/colegio cuando el nombre de usuario no sea suficiente.

### Preflight obligatorio para profesores

La identidad canónica solo permite derivar profesores cuando el siguiente
catálogo devuelve los roles esperados y sus usuarios coinciden con el padrón
institucional autorizado:

```sql
SELECT r.nombre, COUNT(DISTINCT ur.id_usuario) AS cantidad_usuarios
FROM identidad.rol AS r
LEFT JOIN identidad.usuario_rol AS ur ON ur.id_rol = r.id_rol
WHERE r.activo = 1
  AND LOWER(LTRIM(RTRIM(r.nombre))) IN (N'profesor', N'docente')
GROUP BY r.nombre;
```

Antes de ejecutar `027`, el DBA debe documentar: cantidad esperada y cantidad
encontrada de usuarios, los `id_usuario` candidatos, que no exista una persona
de comedor con el mismo usuario y otro tipo, y la correspondencia del
`nombre_usuario` con el nombre visible del profesor. El colegio no puede
derivarse de identidad y debe quedar `NULL` o completarse mediante un padrón
autorizado posterior. Si no existe un rol activo exactamente `Profesor` o
`Docente`, el origen de profesores no es determinable automáticamente y la
migración no debe complementarse inventando usuarios.

## Retiro controlado del legado

Después de aplicar `019_migra_menu_historico.sql` o la revisión Alembic equivalente,
validar en staging, respaldar producción y comprobar que los conteos canónicos coinciden,
el DBA puede retirar únicamente `ComedorPortal.MenuComponente` y
`ComedorPortal.MenuPlantilla` con:

```bash
CONFIRMAR_MIGRACION_DBA=SI \
CONFIRMAR_BORRADO_TABLAS_DEPRECIADAS=SI \
./web/scripts/retirar_tablas_menu_legacy.sh
```

La rutina ejecuta el borrado dentro de una transacción, elimina primero la tabla hija,
comprueba la existencia de ambas tablas canónicas y aborta si los conteos no coinciden.
No se borran otras tablas `ComedorPortal`, no se ejecuta durante el despliegue y no debe
usarse sin respaldo verificado y ventana aprobada.

La transferencia inicial del padrón está en `021_transfiere_padron_web.sql`.
El DBA debe ejecutarla sobre la base autorizada para copiar estudiantes,
rutas, becas y asignaciones desde las fuentes históricas al modelo web. Es
idempotente, conserva las claves y no elimina el origen. Los PIN binarios
heredados no se convierten silenciosamente a Argon2; deben generarse mediante
el flujo web de reinicio de PIN.

`002` no altera tablas `dbo` ni `Seguridad`. Sus claves foráneas protegen confirmaciones históricas, preservan auditoría mediante `SET NULL` y revocan sesiones si se elimina su identidad. La aplicación conserva la procedencia: al cancelar solo puede borrar una fila de `dbo.RegistroTransporte` si la confirmación vinculada la marca explícitamente como `MarcaCreadaPorPortal=1`; una marca reutilizada del escritorio se desvincula, nunca se borra.

## Migración de datos antes del despliegue

Una migración de estructura sin transferencia de datos es incompleta. Antes de
promover la aplicación, el DBA debe respaldar la base, registrar conteos de las
fuentes, ejecutar las transferencias idempotentes y reconciliar claves y
relaciones en staging y producción. En particular, `019_migra_menu_historico.sql`
y `020_completa_componentes_menu.sql` trasladan el menú histórico a
`menu.plantilla` y `menu.componente`; no deben eliminar las tablas fuente durante
esa etapa. El retiro posterior está documentado en
`RUNBOOK_DEPLOY_PRODUCCION.md` y solo se autoriza con respaldo, conteos iguales
y aprobación independiente del DBA.

No ejecutar ninguna migración desde el contenedor ni con la cuenta de ejecución de la API.
