# Evidencia Fase 2 — arquitectura y mantenibilidad

Fecha: 2026-09-03  
Estado: **Completada técnicamente; cierre global pendiente**

## Alcance del corte

Se redujo la deuda de mantenibilidad mediante divisiones cohesionadas de los
archivos identificados por el verificador arquitectónico. No se declara el
cierre global del plan ni la eliminación de todos los residuos de transición.

## Resultado verificado

- Los **9 archivos** que excedían el límite se dividieron y quedaron en **300
  líneas o menos**.
- `npm run verificar:arquitectura`: **aprobado**.
- `npm run typecheck`: **aprobado**.
- `npm test -- --run`: **aprobado**.
- Puerta focal de backend: **9 pruebas aprobadas** (`tests/test_entrada.py`,
  `test_menu_query_efficiency.py`, `test_portal_settings.py`,
  `test_rbac_dependencies.py` y `test_composicion_operacion.py`).
- Suite histórica SQL Server: **17 pruebas aprobadas** tras instalar
  `pyodbc==5.3.0` en el entorno virtual aislado.
- Ruff: **aprobado**.
- `git diff --check`: **aprobado**.

Estas comprobaciones validan el corte técnico de arquitectura y los contratos
modificados. La evidencia no sustituye las puertas completas que corresponden a
la Fase 3 ni autoriza el cierre operativo o global del plan.

## Correcciones P2 aplicadas

- Calendario ya consume exclusivamente `consultas/calendario_menu.ts`; no quedan
  llamadas HTTP directas desde el componente.
- `usePortalEstudiante.ts` se separó en módulos de tipos, carga, fecha y reloj.
- Los repositorios y servicios se dividieron por responsabilidad; las pruebas de
  composición confirman la delegación operativa.
- Las fachadas `RepositorioCatalogos` y `ServicioOperacion` ya no usan herencia
  múltiple: delegan a componentes explícitos mediante composición.

## Residuos P2 registrados

1. La fachada de operación conserva métodos transversales compartidos (persona,
   matrícula y saldo); ahora están centralizados en `ServicioOperacionBase`, pero
   el desacoplamiento completo requiere una decisión de contratos.
2. Tres exenciones ya documentadas permanecen vigentes; esta fase no las amplía
   ni las usa para ocultar incumplimientos nuevos.

## Dictamen y siguiente control

La Fase 2 queda **completada técnicamente** para el corte de división y eliminación
de HTTP directo. Los residuos P2 de MRO y desacoplamiento transversal requieren un
corte posterior y no permiten afirmar el cierre global. La siguiente fase
habilitada es la Fase 3, para reproducibilidad, cobertura y puertas completas de
calidad.
