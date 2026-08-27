# Estudiantes

Dominio autónomo para consultar, crear y editar estudiantes del esquema
`estudiantes`. Expone contratos camelCase y requiere `estudiantes.leer` para
lectura y `estudiantes.editar` más CSRF para cambios.

No importa componentes históricos ni módulos de persistencia globales, y no
lee tablas `dbo` o `Seguridad`. La composición acepta sus dependencias mediante
`dependencias_estudiantes`; el ensamblador de producción debe proporcionar el
repositorio y las dependencias canónicas de identidad.
