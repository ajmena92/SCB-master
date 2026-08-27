# ADR-0001: monolito modular por dominios

- Estado: Aceptada
- Fecha: 2026-08-25
- Responsables: equipo del proyecto

## Contexto

La plataforma web creció alrededor de archivos centrales y componentes extensos. Esa forma dificulta localizar una funcionalidad, aislar reglas, probar permisos y migrar de forma segura las capacidades del sistema WinForms. El objetivo aprobado es una sustitución total: el sistema local no permanecerá integrado al entorno de ejecución web.

También se requiere que las convenciones propias del producto estén en español, reservando el inglés para obligaciones técnicas.

## Decisión

Adoptar un monolito modular orientado a dominios y desarrollado por cortes verticales.

- Cada dominio agrupa persistencia, reglas, API, permisos, interfaz, pruebas y documentación.
- Frontend y API permanecen como unidades desplegables coordinadas; no se introducen microservicios en esta etapa.
- Las dependencias entre dominios pasan por contratos públicos.
- El frontend usa `aplicacion`, `funcionalidades` y `compartido`; el backend usa `aplicacion/nucleo` y `aplicacion/modulos`.
- Los nombres controlados por el proyecto usan español ASCII en código y español ortográfico en UI/documentación.
- La migración desde WinForms es única, auditable y seguida por su retiro. No habrá integración en tiempo de ejecución, doble escritura ni sincronización permanente.

## Alternativas consideradas

### Capas globales

Descartadas porque dispersan una misma capacidad entre controladores, servicios y repositorios globales, y favorecen nuevos archivos monolíticos.

### Arquitectura limpia global estricta

Descartada como estructura principal porque añade abstracciones y repetición innecesarias para el tamaño actual. Sus reglas de dependencia se aplican dentro de cada módulo donde aportan aislamiento.

### Microservicios

Descartados porque agregarían despliegues, observabilidad, consistencia distribuida y soporte operativo sin una necesidad demostrada de escalado independiente.

### Integración permanente con WinForms

Descartada por duplicar autoridades de datos y permisos, prolongar dependencias de DigitalPersona/Crystal Reports y elevar el riesgo operativo.

## Consecuencias

Positivas:

- cada funcionalidad tiene un lugar único y límites comprobables;
- es posible migrar y aceptar dominios de extremo a extremo;
- disminuyen el acoplamiento y el tamaño de archivos centrales;
- un módulo podría extraerse en el futuro si existe evidencia operativa suficiente.

Costos y restricciones:

- el código actual requiere una transición gradual con pruebas de caracterización;
- deben definirse contratos públicos entre dominios;
- se necesita automatizar límites de imports, tipos, cobertura y vocabulario;
- los cambios de nombres se ejecutan junto con su dominio, sin aliases permanentes.

## Criterios de cumplimiento

La decisión se considera aplicada cuando:

- no existe lógica de negocio en endpoints ni SQL fuera de repositorios;
- los componentes no llaman HTTP directamente;
- no existen dependencias circulares o accesos al repositorio privado de otro dominio;
- los contratos son explícitos y el cliente TypeScript proviene de OpenAPI;
- el entorno de ejecución web no depende de `escritorio/`, DigitalPersona ni Crystal Reports;
- las [convenciones de nombres](../CONVENCIONES_NOMBRES.md) se validan en integración continua.
