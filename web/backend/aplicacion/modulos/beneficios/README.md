# Beneficios

Gestiona el catálogo de beneficios alimentarios y la asignación vigente de un beneficio a cada estudiante.

## Contratos

- `GET /api/v1/beneficios`: catálogo activo (`beneficios.leer`).
- `POST/PUT /api/v1/beneficios`: administración del catálogo (`beneficios.editar`).
- `GET /api/v1/beneficios/estudiantes/{id_estudiante}`: asignación (`beneficios.leer`).
- `PUT /api/v1/beneficios/estudiantes/{id_estudiante}`: reemplaza o elimina la asignación (`beneficios.editar`).

La persistencia usa únicamente el esquema canónico `beneficios`. El módulo no consulta tablas, repositorios ni servicios de otros dominios.
