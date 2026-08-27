# SQL local de desarrollo

Estos scripts existen únicamente para una copia local de desarrollo que no
contiene el esquema `Seguridad` del ambiente institucional. No forman parte de
las migraciones de producción.

`001_seguridad_compat_desarrollo.sql` crea el mínimo RBAC requerido por el
portal y una cuenta aislada de prueba: `portal_dev_admin`. Requiere la variable
explícita `LOCAL_DEV_ONLY=true` al ejecutarse con `sqlcmd`.

No use estos archivos en producción ni para reparar una base institucional.
