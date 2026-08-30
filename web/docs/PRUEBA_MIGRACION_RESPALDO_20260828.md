# Prueba de migración del respaldo 2026-08-28

## Alcance

Prueba local controlada de restauración del respaldo y extracción de fotografías
para preparar la migración única de `web/`. `escritorio/` no participa en la
ejecución.

## Respaldo

- Archivo: `C:\Dev\SCB-master\backups\20260828.bak`
- Base contenida: `SCSC`
- Servidor de origen: `SRV-Platanares`
- Versión de base: `998`
- Motor que creó el respaldo: SQL Server `17.0.1125.2`
- SHA-256: `456afee5de6469f5d010a0b0036bfada9afdc3ec4797c8d2eec99d84acc3af79`

El SQL Server local de desarrollo admite hasta la versión de base `957`; por eso
la restauración fue rechazada por el motor y no se forzó. La prueba de restauración
debe ejecutarse en una instancia SQL Server 17 compatible, en una base aislada, por
ejemplo `SCSC_MIGRACION_PRUEBA_20260828`, sin sobrescribir `SCSC`.

## Fotografías

La fuente real en producción es `ComedorPortal.FotoEstudiante`, no una carpeta del
servidor. Se verificó:

- 618 fotografías activas;
- aproximadamente 17 MB almacenados;
- relación `IdUsuario` → `dbo.Usuario.Cedula`;
- tabla `estudiantes.fotografia` de producción vacía.

El paquete local de prueba es:

- `backups/fotos_20260828.zip`
- 618 archivos;
- 0 nombres duplicados;
- ZIP validado sin errores;
- SHA-256: `9fbe321505c217c59d41a429b98dd2247703bc7fee177a32c8b38cb64b78c2cc`.

La revisión `0034_migracion_datos_legados` importa las fotografías activas y `0035_normaliza_estado_horario_comedor` normaliza los estados y turnos a
`estudiantes.fotografia` después de reconciliar `IdUsuario` con el estudiante.

## Prueba de restauración y migración

Se levantó una instancia temporal `mcr.microsoft.com/mssql/server:2025-latest`
con SQL Server `17.0.4015.4` y se restauró el respaldo como
`SCSC_MIGRACION_20260828`. La restauración terminó correctamente y la base quedó
`ONLINE`.

Alembic se ejecutó hasta `0037_valida_horarios_operativos (head)`. Durante la prueba
se corrigieron dos incompatibilidades reales del flujo histórico:

- `0017_migracion_total_dominios` agregaba columnas y las usaba en el mismo lote
  compilado por SQL Server;
- `0029_uso_transporte_y_auditoria_comedor` debía ampliar y recrear la PK de
  `alembic_version` porque el respaldo usa `varchar(32)`.

La validación estructural pasó para restricciones, carnets duplicados, rutas
activas duplicadas, vínculos huérfanos y horarios. Los horarios conservados fueron
`IdHorario=1` a las `10:20` y `IdHorario=2` a las `18:40`.

Las revisiones `0034` y `0035` trasladan y normalizan el contenido de estas tablas legadas del respaldo:

- `dbo.Usuario`: 1.778 filas;
- `dbo.RegistroComedor`: 21.832 filas;
- `dbo.RegistroTransporte`: 144 filas;
- `dbo.Ruta`: 19 filas;
- `ComedorPortal.FotoEstudiante`: 618 filas.

La validación final confirmó: 1.628 estudiantes, 19 rutas, 1.778 personas de
comedor, 618 fotografías, 114 marcas de asistencia estudiantil, 129 días de transporte y
21.753 ingresos únicos. Las 21.832 filas de comedor se conservan además en
`comedor.migracion_ingreso_0034`; los 79 duplicados históricos quedaron registrados
en reconciliación. El origen tenía 115 confirmaciones, pero una correspondía a un
profesor; queda fuera correctamente de las métricas y marcas estudiantiles.

## Estado

- Integridad del respaldo: verificada.
- Restauración compatible: completada en SQL Server 17 aislado.
- Alembic hasta `0037_valida_horarios_operativos (head)`: completado.
- Normalización por catálogo (`0036`) y validación de horarios (`0037`): completadas.
- Extracción de fotos: completada y validada.
- Traslado de datos legados: completado en la copia aislada.
- Reconciliación comparativa: 0 hallazgos pendientes.
- Ejecución analítica por ID: 1.628 señales generadas sin depender de la columna textual eliminada.
