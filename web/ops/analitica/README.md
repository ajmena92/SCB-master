# Ejecución de analítica del comedor

La analítica pandas se ejecuta fuera de FastAPI y solo lee las tablas canónicas.
Requiere instalar `web/backend/requirements-analitica.txt` en el entorno del
proceso, `SQL_CONNECTION_STRING` y una cuenta SQL de solo lectura.

Ejemplo de ejecución diaria:

```bash
cd web/backend
python -m analitica.ejecutor --dias 20 --salida /var/lib/scb/analitica-comedor.json
```

El archivo de salida es un artefacto regenerable; no se usa para autorizar
ingresos ni sustituye los datos transaccionales. La programación debe ejecutarse
después del cierre operativo mediante cron, systemd timer o el orquestador oficial.

## Reconciliación del corte

Antes de activar la web, ejecutar en modo lectura:

```bash
cd web/backend
python scripts/reconciliar_migracion_comedor.py
```

Con respaldo y escrituras congeladas, el DBA puede persistir los hallazgos con
`--apply`. Los casos ambiguos quedan en `comedor.reconciliacion_migracion` y no
se transforman automáticamente.
