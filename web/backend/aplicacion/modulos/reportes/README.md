# Reportes

Consulta y exportación de información de la plataforma web. Expone reportes de
estudiantes y transporte en JSON (`reportes.leer`) y CSV (`reportes.exportar`).

Solo consulta los esquemas canónicos `estudiantes` y `transporte`; no contiene
reglas de negocio de otros dominios, no accede a SQL legacy y no genera PDF,
Excel ni integra Crystal Reports.
