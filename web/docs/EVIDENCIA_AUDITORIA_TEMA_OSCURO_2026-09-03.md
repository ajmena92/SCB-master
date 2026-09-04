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
- La comparación claro/oscuro de Tiquetes en móvil (390 px), tableta (768 px) y
  escritorio (1440 px) no presentó desbordamiento horizontal en ningún tema.
  Las capturas están en `output/playwright/tiquetes-{mobile,tablet,desktop}-{light,dark}.png`.
- Tras recuperar los secretos, CSRF responde `204` y los logins administrativo y
  estudiantil reales responden `200`. El Dashboard real devuelve 663 estudiantes,
  550 beneficiarios y desglose por rutas. El portal devuelve menú, carné y foto;
  el 2026-09-03 el período está cerrado y el límite es 14:40.
- Los estados compartidos de carga, vacío, error y éxito quedaron centralizados
  en `ElementosComunes.tsx` mediante `EstadoPanel`; Reportes, Rutas, Usuarios y
  tablas reutilizan ahora la misma semántica, roles ARIA y tokens de color.
- Chromium verificó la navegación por teclado del portal: tema, cédula, PIN,
  ingreso y enlace administrativo reciben foco en orden; no hay overflow móvil.
- Con cookies de sesión reales adaptadas al entorno local, axe-core no encontró
  violaciones en Dashboard ni Personas en tema oscuro. El Dashboard real entregó
  datos de 663 estudiantes y 550 beneficiarios para sus gráficas.

## Hallazgos y límites de la pasada

- La pasada axe sobre el tema claro todavía identifica contrastes insuficientes
  en textos `muted-foreground`; esos resultados no representan el tema oscuro y
  deben tratarse en la revisión clara.
- La navegación autenticada completa no pudo terminar dentro del tiempo límite
  del navegador en todas las pantallas; no se declara cierre WCAG global con una
  ejecución parcial.
- Chromium local no puede enviar cookies `Secure` desde `http://127.0.0.1`; la
  navegación visual autenticada debe ejecutarse bajo el dominio HTTPS autorizado.
- La impresión de PIN y tiquetes ya existe, pero su validación requiere reiniciar
  PIN o registrar una venta. No se ejecutaron esas mutaciones sobre datos reales.
  La impresión de carné y reportes quedó disponible sin mutación y se incorporó
  una acción visible de impresión/PDF en ambos componentes.
- La sesión estudiantil real ya fue verificada por API; falta repetir la captura
  visual bajo el dominio HTTPS autorizado.

## Cambios funcionales derivados de la auditoría

- `TarjetaCarnet.tsx`: botón “Imprimir carné” con ventana de impresión aislada,
  sin estilos del tema administrativo y con el mismo contenido visual del carné.
- `ReportesOperativos.tsx`: botón “Imprimir / PDF” habilitado al existir filas,
  con tabla imprimible y estado deshabilitado mientras no hay resultados.

## Pendientes explícitos

1. Ejecutar axe-core por pantalla bajo el dominio HTTPS autorizado y conservar
   los artefactos completos.
2. Capturar gráficas reales en Chromium; el endpoint ya devuelve datos reales.
3. Ejecutar impresión real de PIN y tiquetes con registros de prueba autorizados.
4. Completar estados de error, carga y deshabilitado con datos reales.
5. Repetir comparación claro/oscuro para el resto de módulos bajo HTTPS.
6. Capturar el portal estudiantil real en ambos temas bajo HTTPS.
