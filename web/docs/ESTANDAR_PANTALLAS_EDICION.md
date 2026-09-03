# Estándar de pantallas de edición administrativa

Este estándar aplica a toda pantalla nueva de edición del panel administrativo.
Su objetivo es que las acciones locales no parezcan formularios genéricos y que
la navegación, jerarquía y comportamiento móvil sean iguales en los módulos.

## Ruta y seguridad

- La URL no expone claves primarias internas ni cédulas. Se usa una referencia
  pública opaca, persistente y no secuencial: `.../expediente/{referencia}`.
- La API resuelve la referencia y mantiene la autorización por permiso. La
  referencia no reemplaza al ID interno en reglas de negocio ni auditoría.
- Las acciones sensibles permanecen autorizadas en la API aunque la interfaz
  las oculte o deshabilite.

## Estructura obligatoria

1. Breadcrumb con regreso al listado de origen y el nombre del expediente.
2. Encabezado con nombre, metadatos operativos y estado; no mostrar ID interno.
3. Contenido organizado por responsabilidades: datos importados de solo lectura,
   datos administrables en SCB y resumen operativo.
4. Pie de acciones fijo con estado de guardado y un único botón principal.
5. Confirmación corta para PIN, desactivación u operaciones irreversibles.

## Estilo y accesibilidad

- Usar Segoe UI, azul histórico y superficies claras definidos en
  `GUIA_VISUAL_COMPONENTES.md`. No introducir violeta como color de marca.
- Peso normal 400–500; 600 únicamente para nombre, títulos y acciones. Evitar
  negrita generalizada.
- Cada acción debe incluir texto e icono consistente; los iconos no sustituyen
  etiquetas en acciones de riesgo.
- Diseño en dos columnas en escritorio y una sola columna sin desbordamiento
  horizontal en móvil. Las áreas pulsables miden al menos 44 px.
- El pie fijo informa `Sin cambios`, `Cambios sin guardar` o `Guardando` y nunca
  oculta contenido: el formulario reserva espacio inferior suficiente.
