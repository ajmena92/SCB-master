# Guía visual de componentes

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

No se agregan verdes, violetas o tipografías nuevas como estilo local de una pantalla. Las excepciones deben incorporarse a esta guía y reutilizarse en componentes compartidos.
