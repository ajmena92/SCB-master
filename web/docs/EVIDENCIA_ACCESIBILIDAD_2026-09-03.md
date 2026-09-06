# Evidencia de auditoría de accesibilidad — 2026-09-03

## Alcance ejecutado

- Chromium, viewport móvil de 390 × 844.
- `axe-core` ejecutado sobre `/admin/login` y `/portal`.
- Navegación por teclado con ocho pulsaciones de Tab en el acceso estudiantil.

## Resultados

| Control | Resultado |
| --- | --- |
| axe-core — acceso administrativo/estudiantil | 0 violaciones |
| axe-core — portal | 0 violaciones |
| Foco visible y orden de tabulación | Correcto: tema, cédula, PIN, ingresar y acceso administrativo |
| Etiquetas de campos | Correctas mediante `Label`/`htmlFor` |
| Contraste | Corregido el primario y texto secundario de la pantalla de acceso |
| Build frontend | Exitoso |

## Corrección aplicada

Se ajustaron los tokens claros `primary`, `ring`, `chart-1` y
`muted-foreground` para cumplir contraste AA sin crear colores locales. El texto
auxiliar de acceso usa `text-foreground` cuando el tamaño pequeño exige mayor
contraste.

La presentación de fotografías ahora usa `ImagenConFallback` en edición de
estudiantes, carné, venta de tiquetes, comedor y miniaturas. Si el blob no
existe, expira o falla su decodificación, se muestra el estado de foto pendiente
sin un icono de imagen rota.

## Verificación real posterior

Con sesión administrativa HTTPS en Chromium y viewport móvil, se abrió un
expediente real: la fotografía respondió correctamente (600 × 800), se mostró
en la edición, no hubo respuestas de imagen con error y no se produjo overflow
horizontal. Los dos mensajes de consola observados corresponden al beacon
externo de Cloudflare bloqueado por la CSP; no son errores de la aplicación.

## Verificación de estación móvil/tableta

Con sesión administrativa real en Chromium se recorrió `/admin/panel/comedor` en
390 × 844, 768 × 1024 y 1024 × 768. En las tres dimensiones no hubo overflow
horizontal ni errores de consola de la aplicación; `F3` abrió el respaldo y dejó
el foco en `captura-comedor`, `F4` abrió el historial y pantalla completa se
activó correctamente. Chromium sin dispositivo de cámara reportó el fallback
esperado para lector USB.

## Pendiente de cierre completo

La prueba de cámara física y lectura QR con dispositivo real queda fuera del
entorno automatizado. La repetición axe-core autenticada se completó y sus
hallazgos están registrados abajo; queda corregir esas violaciones y validar
lecturas con datos reales.

## Repetición axe-core autenticada — 2026-09-03

Se inició sesión administrativa real en Chromium bajo HTTPS y se ejecutó
axe-core 4.13 por pantalla, con viewport de escritorio de 1440 × 1000. La CSP
se mantuvo sin cambios en producción; únicamente se habilitó el bypass de CSP
en la sesión aislada de depuración para cargar el analizador.

| Pantalla | Violaciones | Reglas encontradas |
| --- | ---: | --- |
| Inicio | 1 | `color-contrast` (36 nodos) |
| Personas | 1 | `color-contrast` (15 nodos) |
| Años e importación | 1 | `color-contrast` (7 nodos) |
| Rutas | 2 | `aria-prohibited-attr` (9), `color-contrast` (15) |
| Menú | 2 | `aria-required-parent` (5), `color-contrast` (24) |
| Calendario de menú | 2 | `aria-allowed-role` (22), `color-contrast` (27) |
| Tiquetes | 1 | `color-contrast` (2) |
| Parámetros | 2 | `color-contrast` (9), `label` (1) |
| Comedor | 4 | `color-contrast` (4), `landmark-main-is-top-level` (1), `landmark-no-duplicate-main` (1), `landmark-unique` (1) |
| Reportes | 1 | `color-contrast` (5) |
| Usuarios | 1 | `color-contrast` (10) |

La ruta de expediente no expuso un enlace navegable desde el listado durante
esta pasada y queda pendiente ejecutarla con una referencia pública real. La
sesión y la carga de las once pantallas administrativas sí fueron verificadas;
los hallazgos anteriores son reales y deben pasar a la siguiente tanda de
correcciones de contraste y semántica ARIA.

## Correcciones de la primera fase

- Comedor conserva un único landmark `main` y su contenido interno usa un
  contenedor neutral.
- Las semanas del menú están dentro de un `tablist` horizontal.
- Las celdas interactivas del calendario ya no usan `article` con rol de botón.
- Los indicadores de color de rutas exponen `role="img"` con nombre accesible.
- Las ayudas de teclado de Comedor usan colores legibles sobre el fondo oscuro.
- `typecheck` y `npm run build` finalizaron correctamente después de los cambios.

La suite Vitest no se pudo completar en esta ejecución porque quedó sin salida
durante más de dos minutos; el proceso fue detenido para evitar dejar workers
huérfanos. Debe repetirse de forma aislada en la siguiente fase.
