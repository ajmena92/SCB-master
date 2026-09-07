# Estándar obligatorio de módulos frontend

Este documento es parte del contrato de la aplicación. Todo cambio de diseño,
responsive, tema, modal, toast o componente visual debe actualizar esta guía o
registrar explícitamente por qué no la modifica.

## Regla crítica

Una funcionalidad nueva no se considera lista si no cumple este documento y no
incluye evidencia de revisión en Chromium para móvil (390 px), tableta (768 px)
y escritorio (1440 px), en tema claro y oscuro cuando aplique.

La revisión del cambio debe enlazar esta guía en el PR/commit y marcar el
checklist correspondiente. La CI ejecuta `npm run verificar:diseno` y rechaza
imports directos de notificaciones fuera de la capa compartida.

## Contrato visual

- El shell muestra una sola vez el nombre del colegio y el contexto
  “Administración”. Cada página aporta un único título principal y una frase
  operativa; no se repiten grupo, módulo ni “Acceso administrativo”.
- Usar `EncabezadoPagina`, `EstadoCarga`, `EstadoVacio`, `EstadoError`,
  `EstadoExito`, `Aviso`, `Tabla` y los componentes de `components/ui`.
- Usar únicamente tokens semánticos Tailwind (`bg-card`, `text-foreground`,
  `border-border`, `text-muted-foreground`, `primary`, `warning`,
  `destructive`, `success`).
- No crear colores, radios, sombras, tipografías o botones locales sin añadir
  primero una decisión a [GUIA_VISUAL_COMPONENTES.md](GUIA_VISUAL_COMPONENTES.md).
- Las notificaciones se invocan mediante
  `src/compartido/notificaciones/notificaciones.ts`; no se importa `sonner`
  directamente desde un módulo.
- Toast: confirmaciones breves, errores recuperables y avisos de operación.
  Modal: confirmación destructiva, edición compleja o resultado extenso.
- Procesos asíncronos muestran estado en pantalla mediante consulta controlada,
  no mediante un toast persistente. Si producen datos sensibles de descarga
  única, la interfaz explica esa condición, solicita el resultado solo tras
  completarse y no lo conserva fuera del estado temporal de la pantalla.
- Las credenciales administrativas se gestionan fuera de formularios de edición
  de cuentas: toda sesión administrativa dispone de un acceso visible para
  cambiar su propia contraseña; el restablecimiento de otra cuenta solo aparece
  en la gestión autorizada de usuarios y nunca revela la contraseña previa.

## Contrato responsive

- En móvil, las tablas extensas se presentan como tarjetas apiladas; en
  `md` y superior se conserva la tabla.
- No se permite scroll horizontal accidental ni controles menores de 44 px.
- Los gráficos incluyen una alternativa textual con su período, valores y unidades; el SVG y su tooltip no pueden ser la única fuente de información.
- Las acciones principales permanecen disponibles en móvil y teclado.
- La estación de comedor es una excepción operativa: usa encabezado oscuro
  compacto, pero conserva siempre identidad del colegio, conexión, cámara,
  sonido y salida; en pantalla completa se reduce sin desaparecer.
- Los estados de carga, vacío, error y éxito deben ser visibles y accesibles.
- Las exportaciones que operan sobre listados paginados explican el alcance:
  descargan todas las coincidencias de los filtros activos, no únicamente las
  filas visibles. Si existen servicios mutuamente excluyentes, la interfaz
  obliga a seleccionar uno antes de elegir el formato.
- En listas nominales con servicios distintos, el servicio se selecciona como
  contexto mediante tarjetas accesibles con icono y descripción. Cada servicio
  conserva su borrador de filtros; la tabla solo se actualiza con una acción
  explícita de “Aplicar filtros” y conserva los resultados visibles durante la
  actualización. Las exportaciones usan exactamente el conjunto aplicado.
- Las vistas por tipo de persona no reutilizan métricas ajenas: la vista de
  profesorado muestra padrón docente e ingresos al comedor; no presenta
  secciones, rutas, becas ni alertas analíticas propias del estudiantado.

## Checklist obligatorio

- [ ] Componentes compartidos reutilizados; no hay variante visual duplicada.
- [ ] Tema claro/oscuro revisado.
- [ ] Móvil, tableta y escritorio revisados en Chromium.
- [ ] Sin overflow horizontal accidental.
- [ ] Teclado, foco, nombres accesibles y contraste revisados.
- [ ] Estados de carga/vacío/error/éxito cubiertos.
- [ ] Toast/modal sigue la regla de esta guía.
- [ ] Build, formato y `npm run verificar:diseno` exitosos.
- [ ] Esta guía o la guía visual fueron actualizadas si cambió el estándar.

## Cómo se mantiene vigente

La persona que modifica una pantalla es responsable de actualizar la guía en el
mismo cambio. Las revisiones de código deben rechazar cambios visuales sin este
checklist y sin evidencia. Las excepciones deben documentarse con alcance,
motivo, fecha de revisión y componente responsable; no se aceptan excepciones
silenciosas.
