# Guía visual de componentes

El contrato operativo para módulos nuevos, responsive, notificaciones y evidencia
de revisión está en [Estándar obligatorio de módulos frontend](ESTANDAR_MODULOS_FRONTEND.md).
Ambos documentos deben actualizarse en el mismo cambio cuando se modifique el
diseño o un estándar.

Esta guía preserva la apariencia histórica del panel: Segoe UI, azul institucional, superficies claras, radios moderados y controles legibles. Todo componente nuevo debe reutilizar estos estilos antes de crear una variante propia.

## Botones

Los botones usan la clase base `.button`; mantienen una altura mínima de 44 px, texto visible y foco accesible.

| Variante | Clase | Uso |
| --- | --- | --- |
| Principal | `.button.primary` | Una acción que avanza o confirma dentro de una superficie, por ejemplo “Guardar” o “Imprimir o guardar PDF”. |
| Secundario | `.button.secondary` | Acciones alternativas que no cambian el estado principal, por ejemplo descargar o copiar. |
| Advertencia | `.button.warning` | Operaciones delicadas que requieren revisión o una confirmación posterior, por ejemplo reiniciar PIN. No implica que la acción ya se ejecutó. |
| Enlace | `.button.link` | Acciones ligeras dentro de filtros o texto, por ejemplo limpiar filtros. |

Los botones con icono deben conservar una etiqueta de texto; el icono es de apoyo, tiene `aria-hidden="true"` y mide normalmente entre 18 y 22 px. Para grupos de acciones de resultado se usa una acción principal visible y las alternativas en botones secundarios.

La acción reversible “No asistiré” usa `.button.warning`: comunica un cambio de
preferencia sin presentarlo como eliminación irreversible. La variante
`destructive` queda reservada para borrado, bloqueo o revocación no reversible.

## Estados de carga

La carga inicial usa un estado compartido, centrado y compacto: spinner de 20 px,
texto breve contextual y una altura reservada de aproximadamente 11rem para evitar
saltos de contenido. No se utilizan mensajes largos ni animaciones invasivas.

## Selector de tema

El selector de tema se presenta como un único botón compacto que muestra el
tema activo y abre un menú desplegable con las opciones claro, oscuro y tema del
dispositivo. Las opciones se implementan como un grupo de radio para comunicar
que solo una puede estar activa. El botón y cada opción conservan un área táctil
mínima de 44 px, nombre accesible, foco visible y usan únicamente tokens
semánticos. El tooltip es una ayuda adicional para escritorio: nunca sustituye
el `aria-label` ni el nombre accesible, ya que en móvil no existe interacción de
hover confiable.

## Encabezados de página

El shell administrativo muestra una sola vez el nombre del colegio y el contexto
“Administración”. Cada vista interna usa un único título principal y una frase
breve orientada a la acción. No se repiten el grupo, el nombre del módulo ni el
texto “Acceso administrativo”.

## Operaciones sensibles y procesos largos

1. La entrada se identifica con `.button.warning` cuando puede invalidar datos de acceso o afectar a un grupo.
2. Antes de ejecutarla se explica el efecto y se pide confirmación explícita.
3. Mientras la solicitud está en curso, se bloquea el cierre y se muestra un estado con texto que describa el trabajo realizado; no se deja solo un botón deshabilitado.
4. El resultado de un lote muestra un resumen, no datos sensibles masivos. La salida segura se ofrece mediante PDF o CSV; copiar queda como alternativa consciente.

## Color y jerarquía

- Azul (`--brand`): navegación, confirmación y acción principal.
- Ámbar (`--warning` y `--warning-soft`): revisión previa de acciones sensibles.
- Rojo (`--danger`): eliminación o bloqueo irreversible.
- Verde (`--positive`): confirmación de éxito, nunca como color genérico de acciones administrativas.

Los toast reutilizan estos significados mediante los tokens compartidos: `success`
para confirmaciones, `warning` para advertencias, `destructive` para errores y
`primary` para información. El contenido debe poder partir líneas en móvil y
reservar espacio para el botón de cierre; no se permiten colores `slate`, `emerald`,
`red` o `blue` fijados dentro de una pantalla.

La superficie del toast es siempre opaca (`bg-card`): la semántica se comunica
con el borde y el icono del tipo. Cada notificación muestra un único icono
proporcionado por Sonner; el mensaje no debe incluir iconos decorativos
adicionales ni depender de truncamiento CSS. Los avisos consecutivos de la misma
operación reutilizan un identificador estable para actualizar el toast existente;
avisos distintos pueden coexistir hasta un máximo visible de cuatro.

No se agregan verdes, violetas o tipografías nuevas como estilo local de una pantalla. Las excepciones deben incorporarse a esta guía y reutilizarse en componentes compartidos.

## Gráficas operativas

Las gráficas usan colores `chart` definidos por tema y superficies de tooltip con tokens semánticos. Debajo de cada gráfica se ofrece “Ver datos del gráfico”, una alternativa textual plegable con período, valores y unidades. En móvil esa alternativa complementa la lectura rápida sin sustituir la gráfica; nunca se obliga a desplazamiento horizontal para interpretar indicadores.

El Dashboard del comedor no usa gráficas de distribución por sección: ese dato ya
está disponible como filtro del padrón y no orienta una decisión inmediata. En
su lugar presenta una tarjeta de capacidad estimada con padrón activo,
confirmaciones válidas y asistencia registrada para la fecha consultada. Las
reservas canceladas no son confirmaciones; las consumidas sí, porque ya
representan una asistencia confirmada.

En la lista nominal, la columna y el filtro se denominan **Asistencia hoy**:
“Ingresó al comedor” comunica un ingreso registrado y “Aún sin ingreso” una
persona activa sin marca para la fecha consultada. No se usan “Estado”,
“Presente” ni “Sin registro” como etiquetas aisladas. El filtro de sección se
deriva del padrón activo de la fecha consultada y se agrupa por nivel, desde
Séptimo hasta Duodécimo. No se fija una cantidad de grupos: puede mostrar, por
ejemplo, `7-1` hasta `7-9`; solo se presentan secciones existentes.

## Excepción operativa: lector de comedor

La estación de lectura (`/admin/panel/comedor`) usa una superficie carbón de alto
contraste (`slate-950`/negro) y texto claro para facilitar la lectura a distancia
en una fila de atención. Esta no es una segunda temática de la plataforma: es una
superficie técnica aislada. Sus indicadores verde, ámbar y rojo comunican estado
de conexión, espera y rechazo, respectivamente. Cualquier componente nuevo fuera
de esta estación debe usar los tokens `background`, `card`, `foreground`,
`muted-foreground`, `primary`, `warning`, `destructive` y `success`.

La estación conserva un encabezado compacto incluso en pantalla completa. Debe
mantener visible la identidad del colegio, conectividad, cámara, sonido y salida.
En móvil y tableta el respaldo manual se presenta como panel inferior, los
resultados no bloquean completamente el lector, y la confirmación táctil puede
usar vibración breve además del sonido.

## Impresión y PDF

Los reportes generados desde la interfaz fuerzan una composición clara para
impresión, independientemente del tema seleccionado. No deben consumir los
colores de superficie oscura ni depender del modo oscuro del navegador.

### Lista de control operativa

La lista nominal del Dashboard se exporta como **Lista de control**. Su panel
mantiene un selector obligatorio y excluyente de servicio: *Comedor* o
*Transporte*; no mezcla beneficios ni estados de ambos servicios en un mismo
archivo. Los tres formatos usan exactamente los filtros activos de la pantalla,
pero incluyen todas las coincidencias, no solo la página visible. Excel es la
acción principal; CSV es la alternativa interoperable y “Imprimir / PDF” abre
una composición clara que el navegador guarda como PDF. Las acciones conservan
texto, icono de apoyo, foco visible y altura táctil mínima.

Los filtros del padrón no presentan beneficios de comedor y transporte como si
fueran equivalentes: **Beneficio de comedor** se muestra únicamente en la lista
de Comedor; la pertenencia a una ruta se denomina **Asignación de transporte**.

## Carné digital

El carné mantiene una jerarquía compacta para uso principal desde el celular:
identidad y fotografía en la cabecera, datos lectivos en una grilla breve, y
QR como acción visual dominante. Los beneficios se presentan en un bloque
informativo propio con icono y token semántico azul; no se dejan insignias
flotantes debajo del QR. La ruta muestra un punto con su color de catálogo y
un texto legible, incluido el estado “Sin ruta asignada”. La acción de impresión
abre una vista clara del mismo carné y oculta los controles durante la impresión.
En escritorio y tabletas anchas el carné cambia a una composición horizontal:
identidad y fotografía destacada a la izquierda, datos, beneficio y QR a la
derecha. Esta adaptación no cambia el contenido ni la composición clara de
impresión.
En móvil los metadatos se compactan en dos filas: Año/Ruta y Sección/Comedor;
los textos usan tamaño fluido y pueden envolver sin solaparse.
En escritorio el QR ocupa la prioridad visual de la columna derecha y los
metadatos se muestran arriba en filas compactas. La impresión queda como acción
externa y genera la composición horizontal desktop centrada dentro de una hoja
carta vertical, con estilos aislados de impresión y orientación de página
`portrait`.

El carné mantiene una superficie blanca y usa el token fijo `carnet-foreground`
para sus textos, incluso en tema oscuro. El color de la ruta nunca se sustituye:
`obtenerColorTextoRuta` selecciona texto claro u oscuro según la luminancia del
color de catálogo. Las opacidades decorativas no se aplican a textos esenciales.
Las etiquetas mantienen como mínimo el tamaño `text-xs` y las instrucciones
operativas `text-sm` para conservar legibilidad en móvil. El estado de fotografía
pendiente se comunica con un único mensaje explicativo fuera del marco de imagen;
el marco usa el texto breve “Sin fotografía”. Cuando el código no existe, el QR no
se presenta como una acción: se muestra un estado accesible con indicación de que
podrá ampliarse cuando esté disponible. Con QR válido, la instrucción de ampliar y
la indicación de presentación ante el lector se mantienen en una sola línea para
evitar duplicidad visual.

## Calendario del menú

La vista operativa muestra únicamente lunes a viernes, que son los días
lectivos del comedor. Sábado y domingo se excluyen de la grilla visual sin
eliminarse del calendario recibido por la API; así se conservan fechas,
excepciones y sustituciones para auditoría. La grilla mantiene cinco columnas
y agrupa cada fecha en su semana natural de lunes a viernes.
