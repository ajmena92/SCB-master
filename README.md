# SCB — Plataforma web activa

`web/` es el único producto activo del repositorio. Contiene la aplicación
React/FastAPI, sus migraciones, operación y documentación vigente.

## Ubicación de cada cosa

| Ubicación | Estado | Uso |
| --- | --- | --- |
| `web/` | Activo | Producto, operaciones, migraciones y documentación vigente. |
| `escritorio/` | Histórico | Fuente WinForms de consulta durante la migración; no se ejecuta ni evoluciona. |
| `docs/` | Histórico | Análisis, refactor y material asociado a WinForms. |
| `docs/legado/` | Histórico | Documentos raíz previos a la migración web. |
| `web/docs/` | Activo | Arquitectura, ADR, operación, seguridad y manuales web. |

Consulte [la documentación web](web/docs/README.md) para operar o desarrollar la
plataforma activa, y [el índice histórico](docs/README.md) solo cuando sea
necesario validar la migración.

## Almacenamiento externo: nunca versionar

Los siguientes directorios están fuera del repositorio y no son fuentes de
código ni dependencias de ejecución:

- `C:\Dev\SCB-master.worktrees`: worktrees locales y desechables.
- `C:\Dev\SCB-datos-restringidos`: fotografías, documentos personales y sus
  respaldos cifrados; acceso institucional restringido.
- `C:\Dev\SCB-archivos-grandes`: binarios, respaldos y artefactos grandes.

No copie esos contenidos dentro del repositorio. Las fotografías se acceden por
el servicio privado definido en `web/docs/PRIVACIDAD_FOTOGRAFIAS_PERSONALES.md`.

## Pendiente de extracción histórica

Las listas de matrícula, el respaldo de base de datos y los binarios heredados
de `escritorio/Utilitarios` todavía pertenecen al historial WinForms. No deben
usarse por `web/` ni copiarse a sus carpetas. Su traslado al almacenamiento
restringido o de archivos grandes, seguido de una limpieza de historial, debe
realizarse como una fase de extracción aprobada y verificable.

## Límites de la migración

No existe integración en tiempo de ejecución, doble escritura ni nuevas
funciones en WinForms. Cualquier extracción desde el legado debe ser única,
documentada, verificable y terminar en `web/`.
