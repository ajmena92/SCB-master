# Integración local a partir de copia de producción — 2026-09-06

## Alcance y resguardo

Se creó un respaldo lógico de producción a solicitud del responsable técnico y
se verificaron las cuatro sumas SHA-256 publicadas por el procedimiento de
respaldo. La copia se transfirió por SSH a almacenamiento local fuera del
repositorio, con directorio `0700` y archivos `0600`.

La restauración se realizó en un contenedor PostgreSQL nuevo, aislado y
publicado únicamente en loopback. No se sobrescribieron las bases locales
existentes ni se copiaron secretos al repositorio. Se creó una única cuenta
sintética local para ejecutar los recorridos de integración.

## Evidencia

| Comprobación | Resultado |
| --- | --- |
| Verificación SHA-256 en producción | Correcta: dump lógico, roles, base y WAL |
| Verificación SHA-256 del dump local | Correcta |
| Restauración lógica aislada | Correcta |
| Personas restauradas | 884 |
| Matrículas restauradas | 734 |
| API: salud, login y dashboard | `200` |
| Frontend con proxy a la copia | Correcto en `127.0.0.1:5173` |
| Suite backend | 97 aprobadas, 8 omitidas |

## Hallazgo resuelto

La copia incluía `fotografia_persona`, aunque `alembic_version` señalaba la
revisión `0018_control_intentos_autenticacion`. La migración `0020` intentaba
crear de nuevo una tabla compatible y fallaba con `DuplicateTable`.

La migración ahora detecta la tabla existente, comprueba sus columnas mínimas
y continúa únicamente si su estructura es compatible. La copia restaurada se
actualizó desde `0018` hasta `0020_fotografia_persona` sin pérdida de datos.

## Observación operativa

El perfil Compose de respaldo recreó el contenedor PostgreSQL durante el
ensayo. El volumen persistente se conservó y los servicios recuperaron estado
saludable, pero este efecto debe eliminarse del procedimiento antes de usarlo
de nuevo en una ventana operativa: un respaldo no debe recrear dependencias de
producción.
