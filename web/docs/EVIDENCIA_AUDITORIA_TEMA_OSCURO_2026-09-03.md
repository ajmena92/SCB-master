# Evidencia de auditoría visual y de accesibilidad — tema oscuro

Fecha: 2026-09-03  
Entorno: `http://127.0.0.1:8081`  
Herramienta: Chromium local (Playwright) y axe-core 4.x

## Resultados verificables

- El inicio de sesión administrativo real redirige a `/admin/panel/inicio` con la
  cuenta de pruebas existente; no se registraron credenciales en esta evidencia.
- En modo oscuro forzado, axe-core no reportó `color-contrast` en Dashboard.
- En escritorio, las rutas auditadas no presentaron desbordamiento horizontal
  (`scrollWidth > innerWidth` fue falso en la pasada autenticada).
- El tema oscuro usa la paleta raíz neutral definida en `src/index.css`; no se
  añadió una segunda hoja de estilos para el tema.
- El lector de comedor conserva su paleta oscura como excepción operativa,
  documentada en [GUIA_VISUAL_COMPONENTES](GUIA_VISUAL_COMPONENTES.md).
- Con datos controlados y una sesión administrativa simulada, Chromium generó
  capturas de Dashboard, Personas, Tiquetes, Rutas, Parámetros, Menú y Reportes;
  ninguna presentó desbordamiento horizontal. axe-core no detectó contraste en
  seis de siete pantallas; Tiquetes conservó un único nodo para revisión.

## Hallazgos y límites de la pasada

- La pasada axe sobre el tema claro todavía identifica contrastes insuficientes
  en textos `muted-foreground`; esos resultados no representan el tema oscuro y
  deben tratarse en la revisión clara.
- La navegación autenticada completa no pudo terminar dentro del tiempo límite
  del navegador en todas las pantallas; no se declara cierre WCAG global con una
  ejecución parcial.
- En la última comprobación, la sesión administrativa real respondió `401`.
  No se modificaron credenciales ni registros para forzar la validación.
- La impresión de PIN y tiquetes ya existe, pero su validación requiere reiniciar
  PIN o registrar una venta. No se ejecutaron esas mutaciones sobre datos reales.
  La impresión de carné y reportes quedó disponible sin mutación y se incorporó
  una acción visible de impresión/PDF en ambos componentes.
- El portal del estudiante requiere una sesión de estudiante independiente; no
  se inventó una cuenta ni se alteró el padrón para probarlo.

## Cambios funcionales derivados de la auditoría

- `TarjetaCarnet.tsx`: botón “Imprimir carné” con ventana de impresión aislada,
  sin estilos del tema administrativo y con el mismo contenido visual del carné.
- `ReportesOperativos.tsx`: botón “Imprimir / PDF” habilitado al existir filas,
  con tabla imprimible y estado deshabilitado mientras no hay resultados.

## Pendientes explícitos

1. Ejecutar axe-core por pantalla con una sesión estable y registrar el artefacto
   completo de cada pantalla.
2. Validar gráficas con datos reales visibles y conservar capturas claro/oscuro.
3. Ejecutar impresión real de PIN y tiquetes con registros de prueba autorizados.
4. Completar estados de error, carga y deshabilitado con datos reales.
5. Repetir la comparación claro/oscuro en móvil, tableta y escritorio.
6. Crear sesión de estudiante controlada para revisar el portal en ambos temas.
