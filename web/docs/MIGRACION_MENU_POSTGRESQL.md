# Migración de menú y calendario a PostgreSQL

La plataforma PostgreSQL conserva las 25 plantillas de menú: cinco semanas por cinco días laborales. Cada plantilla contiene título, observaciones y componentes ordenados y tipados.

El menú efectivo no requiere publicación diaria. La plantilla activa de la semana y día correspondiente es la fuente ordinaria del menú. El calendario habilita lunes a viernes por defecto y sus excepciones institucionales se almacenan por fecha. Una sustitución aplica a una fecha exacta y prevalece sobre la plantilla solo para ese día. Un cierre institucional impide el servicio, sin modificar la plantilla semanal.

La pantalla muestra el mes con sus fechas reales, incluidos sábados y domingos sin servicio. El menú efectivo se obtiene del ciclo institucional PANEA de cinco semanas, no de la semana del mes. La ancla vigente es el lunes 16 de marzo de 2026 como Semana 1, validada contra la operación: del 24 al 28 de agosto de 2026 corresponde a Semana 4. La administración modifica la plantilla semanal o registra una sustitución; no publica ni duplica el menú cada día.

La importación `web/scripts/importar_sqlserver_postgresql.py` lee primero las tablas canónicas `menu.*`; si no existen, usa las tablas `ComedorPortal.*` disponibles. Importa las sustituciones existentes por defecto. `--fecha-corte AAAA-MM-DD` es opcional si se desea limitar el conjunto a fechas posteriores. No inventa publicaciones históricas.

Para recuperar sustituciones una vez que el padrón ya está en PostgreSQL se usa exclusivamente `--solo-sustituciones`. Primero se ejecuta en simulación y luego con `--aplicar`. El proceso inserta fechas ausentes, reconoce las que ya son idénticas y detiene toda la transacción ante una sustitución con la misma fecha pero contenido distinto; nunca reemplaza una decisión tomada en la plataforma sin conciliación explícita.

Antes del corte se debe ejecutar la imagen aislada de migración, comprobar que Alembic tenga una sola cabeza, realizar la simulación, revisar su reporte y solo entonces aplicar la importación a PostgreSQL. SQL Server sigue siendo de solo lectura durante este proceso.
