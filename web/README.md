# Portal web de comedor SCSC

Portal de asistencia al comedor: frontend React y backend FastAPI integrado con SQL Server institucional. El navegador solo accede al mismo origen HTTPS; la API es la única ruta hacia la base de datos.

## Documentación

- [Requisitos funcionales](docs/REQUISITOS_COMEDOR.md)
- [Análisis y puerta de integración a producción](docs/ANALISIS_INTEGRACION_PRODUCCION.md)
- [Despliegue seguro en staging y producción](docs/DESPLIEGUE_PORTAL.md)
- [Operación del carnet digital](docs/CARNET_DIGITAL_OPERACION.md)

## Estado

La plataforma usa SQL Server mediante los módulos web y sus migraciones versionadas en `sql/migrations`, sin importar componentes de escritorio en ejecución. Las migraciones se ejecutan manualmente por el DBA, primero en staging. No habilitar variables de producción sin seguir la puerta de salida documentada.

### Avances funcionales registrados

- Parámetros exclusivos del portal para la hora límite por horario y el aviso previo, aplicados dinámicamente y auditados.
- Reloj y tiempo restante del estudiante sincronizados con SQL Server; apertura, cierre y extensiones de horario se reconcilian con el servidor.
- Confirmación con marca de hora del servidor: **Confirmar almuerzo** queda deshabilitado y **No asistiré** permite cancelar la marca web antes del cierre.
- Estado final de solo lectura después del cierre, sin acciones disponibles; se conserva el menú y se informa si el estudiante marcó asistencia.
- El contrato de asistencia usa el campo canónico `estado` en minúscula para evitar ambigüedad entre API y frontend.

## Operación futura

Los artefactos canónicos de contenedor están en `ops/`. La API se mantendrá privada, SQL Server no se expondrá al navegador y HTTPS será terminado por el proxy institucional. Copie `ops/.env.example` a un archivo local `ops/.env`; nunca suba secretos al repositorio. Consulte el [runbook de despliegue](docs/DESPLIEGUE_PORTAL.md) antes de construir o publicar imágenes.
