# Recuperación de fotografías del padrón

El Web vigente guarda una fotografía privada por persona en
`fotografia_persona`. Las fotografías históricas no se leen en tiempo de
ejecución desde el sistema anterior.

## Fuente y criterio de asociación

El script `backend/scripts/importar_fotografias_padron.py` recibe una carpeta
de imágenes cuyo nombre de archivo es la cédula. Elimina separadores al
comparar el nombre con `persona.cedula`; por tanto, una foto sin coincidencia
no se importa ni se asigna manualmente.

Antes de persistirla, cada imagen se orienta usando EXIF, se recorta centrada
al formato vertical de carnet (600 × 800) y se guarda como JPEG progresivo
optimizado. La fuente no se modifica.

## Ejecución

La simulación es el modo predeterminado. Desde `web/ops`, monte la carpeta
histórica en solo lectura y ejecute el contenedor de migración:

```bash
docker compose --env-file .env -f compose.production.yml run --rm \
  --entrypoint python -e PYTHONPATH=/app \
  -v "/ruta/a/fotos:/fotos:ro" migracion \
  scripts/importar_fotografias_padron.py --carpeta /fotos
```

Tras revisar los contadores, agregue `--aplicar`. El proceso conserva una foto
ya cargada; `--reemplazar` solo debe usarse cuando se haya aprobado sustituirla.

La tabla requiere permisos explícitos para `scb_api` y `scb_migrador`,
incluidos en `sql/local/002_fotografias_personas_postgresql.sql`.
