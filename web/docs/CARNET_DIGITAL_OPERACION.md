# Carnet digital estudiantil

## Alcance

El portal construye el carnet dentro de la vista **Mi carnet digital** como una tarjeta HTML con fotografía, nombre, cédula/carné, sección, ruta, beneficio de comedor y código de barras Code 128 en SVG. Cada ruta tiene un color configurado en `ComedorPortal.RutaCarnetConfiguracion`; ese color se muestra como acento del carnet para distinguirlo visualmente, nunca como estado. Las rutas 2026 se inicializan con la paleta oficial de CTP Platanares en `006_rutas_catalogo.sql`; Administración puede escoger luego cualquier color HEX válido desde el catálogo web. El valor codificado conserva el prefijo `ControlCarnet` configurado para que el lector del sistema local continúe resolviendo la cédula sin cambios. PNG y PDF se mantienen como formatos de descarga.

El estudiante puede consultar y descargar su carnet desde **Mi carnet digital**. Administración puede cargar o reemplazar fotografías, asignar el `TipoBeca` existente, seleccionar la ruta de transporte y descargar el carnet de cualquier estudiante activo autorizado. La opción **Sin ruta** deja el carnet con el color institucional por defecto.

## Publicación de la base de datos

1. Ejecutar `004_student_cards.sql`, `005_route_card_colors.sql` y luego `006_rutas_catalogo.sql` manualmente por el DBA en staging.
2. Verificar las tablas `ComedorPortal.FotoEstudiante` y `ComedorPortal.RutaCarnetConfiguracion`, además de los nuevos eventos de auditoría.
3. Ejecutar las pruebas de carga y acceso antes de producción.
4. Aplicar la misma migración en producción con respaldo y ventana aprobada.

La API no ejecuta migraciones ni expone fotografías como archivos públicos.

## Importación inicial de fotografías

El script trabaja en simulación por defecto. Los archivos se asocian por los dígitos del nombre contra `dbo.Usuario.Cedula`.

```bash
cd web/backend
SQL_CONNECTION_STRING="..." python scripts/import_student_photos.py \
  --folder "../CTP Platanares CARNET COMEDOR 2026/fotos" \
  --report /tmp/fotos-reporte.csv
```

Revisar las coincidencias, faltantes y archivos inválidos. Para aplicar la carga se requiere indicar el usuario administrativo:

```bash
SQL_CONNECTION_STRING="..." python scripts/import_student_photos.py \
  --folder "../CTP Platanares CARNET COMEDOR 2026/fotos" \
  --apply --admin-id 123 \
  --report /tmp/fotos-reporte-aplicado.csv
```

El script acepta JPG, PNG y WEBP de hasta 5 MB, aplica la orientación EXIF y guarda una versión JPEG progresiva de máximo 800 píxeles por lado. El reporte CSV contiene coincidencias, faltantes y archivos inválidos. El proceso es idempotente para una misma cédula.

Las miniaturas administrativas se solicitan mediante `?size=thumb` y se entregan con caché privado y ETag. La fotografía completa solo se solicita al carnet o al detalle del estudiante.

## Validación operativa

- Escanear un carnet PNG desde el lector local y confirmar que se recibe la cédula esperada.
- Confirmar que el prefijo configurado en `ControlCarnet` se conserva.
- Probar un estudiante con beca en un día autorizado por `DiasBeca`.
- Probar un estudiante sin fotografía: debe aparecer como carnet provisional.
- Reemplazar una fotografía desde Administración y confirmar que la vista previa cambia.
- Editar un estudiante, seleccionar una ruta y confirmar que el carnet muestra su nombre/código y color.
- Confirmar que Operador y Administrador pueden gestionar las acciones acordadas y que cada operación aparece en Auditoría.
