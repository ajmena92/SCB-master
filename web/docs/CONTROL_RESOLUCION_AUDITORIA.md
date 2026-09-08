# Control de resolución de auditoría

Fecha: 2026-09-03  
Alcance: exclusivamente `web/` y sus subdirectorios.

## Corrección de autorización de reservas — 2026-09-08

Estado: corregida técnicamente.

Problema identificado: los endpoints de autoservicio `POST` y `DELETE`
`/api/v1/comedor/reservas` aceptaban sesiones administrativas mediante la
dependencia `portal_operativo`. Con una cédula enviada en el cuerpo, una cuenta
administrativa podía modificar la reserva y los tiquetes de otra persona.

Corrección aplicada:

- `portal_operativo` exige una sesión de tipo `portal`.
- Los contratos públicos de reserva y cancelación ya no aceptan `cedula`.
- Los casos de uso resuelven siempre la persona desde la identidad de portal.
- Los contratos TypeScript generados se actualizaron desde OpenAPI.

Evidencia ejecutada:

- `./.venv/bin/pytest -q pruebas_postgresql/test_operacion.py`: 10 aprobadas.
- `npm test -- --run src/funcionalidades/comedor/consultas/reservas.test.ts`:
  2 aprobadas.
- `npm run verificar:cliente`, Ruff focal y `git diff --check`: aprobados.

Cobertura de regresión: una sesión administrativa recibe `403` al crear o
cancelar reservas, sin modificar saldo ni reservas; un cuerpo de portal con una
cédula adicional se rechaza con `422` y no modifica datos.

## Paso 1: modelo de identidad

Estado: corregido focalmente; no representa un cierre global de la auditoría.

Problema identificado: campos y restricciones propios de `SesionAcceso` estaban desplazados a `IntentoAutenticacion`, lo que impedía la coherencia del modelo y bloqueaba pruebas relacionadas.

Corrección aplicada:

- Se devolvieron a `SesionAcceso` los campos, la restricción y el índice correspondientes.
- Se inicializó el contador de intentos.
- Se ajustó el manejo UTC para SQLite.

Fuentes de trazabilidad: migraciones Alembic `0001`, `0004` y `0018`.

Evidencia ejecutada:

- Pruebas focales de integridad y seguridad: `4 passed`.
- Ruff sobre los dos archivos corregidos: aprobado.

Pendientes:

- La suite completa sigue en curso y debe cerrarse antes de declarar resuelto el paso.
- `test_modelo_alembic` es una prueba histórica pendiente de revisión, pues importa un componente retirado.

## Paso 2: importaciones y calidad estática

Estado: correcciones focales verificadas; falta repetir la suite backend completa.

Correcciones aplicadas:

- La importación Excel rechaza solicitudes mayores de 12 MiB antes de procesar el formulario multipart y conserva una comprobación de contenido posterior.
- La validación JSON de importaciones devuelve `422` ante datos no admitidos.
- Las pruebas de CI validan el entrypoint y el perfil Compose activos sin depender de formatos retirados.
- La prueba de operación de comedor valida parámetros SQLAlchemy, no SQL renderizado.
- Ruff fue corregido en todo el backend; la excepción de analítica documenta el import posterior a `pytest.importorskip`.

Evidencia ejecutada:

- Prueba de límite Excel (413): aprobada.
- Prueba de duplicados y exportación CSV: aprobada.
- Pruebas CI: `6 passed`.
- Prueba de operación de comedor: aprobada.
- Ruff global: aprobado en WSL el 2026-09-03.
- Suite backend completa: `77 passed in 36.30s` en WSL el 2026-09-03.
- Mypy de producción: sin incidencias en `47` archivos fuente en WSL el 2026-09-03.

Pendiente:

- Ejecutar las puertas frontend para consolidar tipado y construcción.
