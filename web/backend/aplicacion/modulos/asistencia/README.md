# Asistencia

Dominio canónico para consultar marcas diarias, registrar asistencia y corregir una marca con motivo auditable.

- `GET /api/v1/asistencia/marcas?fecha=AAAA-MM-DD` requiere `asistencia.leer`.
- `POST /api/v1/asistencia/marcas` requiere `asistencia.editar` y CSRF.
- `PUT /api/v1/asistencia/marcas/{id_marca}/correccion` requiere `asistencia.editar` y CSRF.

El módulo solo depende de sus contratos y del puerto de conexión del núcleo. No importa el servidor ni estructuras históricas.
