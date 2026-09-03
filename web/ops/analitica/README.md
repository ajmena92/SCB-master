# Ejecución de analítica del comedor

La analítica pandas se ejecuta fuera de FastAPI y consulta exclusivamente las
tablas canónicas PostgreSQL. Sus indicadores son alertas para revisión humana;
nunca modifican becas, matrícula ni rutas.
Requiere instalar `web/backend/requirements-analitica.txt` y `DATABASE_URL`
con una cuenta de lectura y escritura limitada a `indicador_analitico_comedor`.

Ejemplo de ejecución diaria:

```bash
cd web/backend
DATABASE_URL='postgresql+psycopg://…' python -m analitica.ejecutor --dias 20 --salida /var/lib/scb/analitica-comedor.json
```

El archivo de salida es un artefacto regenerable; no se usa para autorizar
ingresos ni sustituye los datos transaccionales. Prográmelo después del cierre
operativo mediante cron, systemd timer o el orquestador oficial.

## Reconciliación del corte

Antes de activar la web, ejecutar en modo lectura:

```bash
cd web/backend
python scripts/reconciliar_migracion_comedor.py
```

Con respaldo y escrituras congeladas, el DBA puede persistir los hallazgos con
`--apply`. Los casos ambiguos quedan en `comedor.reconciliacion_migracion` y no
se transforman automáticamente.
