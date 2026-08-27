# Arquitectura de la plataforma web

## Estado y propósito

Este documento define la arquitectura objetivo de SCB. La plataforma web sustituirá por completo al sistema WinForms mediante una migración única y auditable. No se permite integración en tiempo de ejecución, doble escritura ni sincronización permanente con el sistema local.

La decisión formal se registra en [ADR-0001: monolito modular por dominios](decisiones/0001-monolito-modular-por-dominios.md). El avance se controla en el [plan de migración total](PLAN_MIGRACION_TOTAL_WEB.md).

## Modelo arquitectónico

Se adopta un **monolito modular orientado a dominios**, implementado mediante cortes verticales. Cada capacidad se entrega de extremo a extremo: persistencia, reglas, API, permisos, interfaz, pruebas y documentación.

Dominios iniciales:

- identidad;
- estudiantes;
- comedor;
- asistencia;
- transporte;
- beneficios;
- cuentas;
- importaciones;
- reportes;
- auditoria.

La lista expresa límites de negocio, no servicios desplegables. Frontend y API continúan como un solo producto y un despliegue coordinado durante esta etapa.

## Estructura objetivo

```text
web/
├── frontend/src/
│   ├── aplicacion/           # Enrutamiento, proveedores y composición
│   ├── funcionalidades/      # Cortes verticales por dominio
│   └── compartido/           # UI, HTTP y utilidades sin reglas de dominio
├── backend/aplicacion/
│   ├── nucleo/               # Configuración, DB, seguridad y observabilidad
│   └── modulos/              # Módulos de dominio
├── sql/                      # Estado actual y transición de migraciones
├── migraciones/              # Objetivo: migraciones versionadas del modelo web
└── docs/                     # Arquitectura, ADR, operación y control
```

Una funcionalidad frontend contiene `consultas/`, `componentes/`, `estado/`, `modelo/` y `paginas/`. Un módulo backend contiene `api.py`, `esquemas.py`, `servicio.py`, `repositorio.py`, `modelos.py`, `errores.py` y sus pruebas.

Esta es una estructura objetivo: su presencia en este documento no significa que el código existente ya fue reorganizado.

## Reglas de dependencia

```text
interfaz/API → servicio de aplicación → dominio → puerto de repositorio
                                             ↑
                              adaptador de persistencia
```

- Los componentes React consumen clientes de consulta; no llaman Axios o `fetch` directamente.
- Los endpoints validan contratos y delegan casos de uso; no contienen SQL ni reglas de negocio.
- Los servicios coordinan reglas y transacciones; no dependen de FastAPI ni de componentes visuales.
- Los repositorios implementan persistencia; no toman decisiones de negocio.
- Un dominio usa contratos públicos de otro dominio, nunca su repositorio o implementación privada.
- `compartido` y `nucleo` contienen capacidades transversales, no lógica específica de un dominio.
- La API valida autenticación y autorización en cada operación protegida.

## Contratos y datos

- Las rutas nuevas usan `/api/v1/` y sustantivos en español.
- Los JSON usan `camelCase` en español; los contratos backend son esquemas Pydantic explícitos.
- El cliente TypeScript se genera desde OpenAPI; no se duplican contratos manualmente.
- Los errores públicos incluyen código estable, mensaje, detalles de validación cuando correspondan e identificador de trazabilidad.
- Las rupturas futuras se versionan; no se mantienen aliases heredados permanentes.
- SQL Server continúa como motor, con esquemas web canónicos y nombres `snake_case` en español.
- Alembic será la autoridad de cambios del nuevo modelo una vez incorporado en la fase de datos.

## Migración y retiro del sistema local

1. Caracterizar el comportamiento correcto y definir reconciliaciones.
2. Construir cada dominio web completo sobre el modelo canónico.
3. Ejecutar ensayos con copias anonimizadas.
4. Respaldar y congelar escrituras durante la ventana de corte.
5. Ejecutar una migración única, repetible y auditable.
6. Reconciliar estudiantes, rutas, beneficios, menús, marcas, saldos y permisos.
7. Activar la plataforma web, invalidar sesiones anteriores y retirar accesos de WinForms.
8. Conservar el sistema local solo como historial de solo lectura fuera de la rama activa después de la aceptación.

DigitalPersona se sustituye por QR, código de barras y PIN web. Crystal Reports se sustituye por reportes web, PDF, CSV y Excel. Ninguna de estas tecnologías formará parte del nuevo entorno de ejecución.

## Atributos y puertas de calidad

- TypeScript estricto y Python tipado sin errores.
- Cero dependencias circulares, SQL fuera de repositorios o HTTP directo desde componentes.
- Cobertura global mínima de 80 % y 90 % para seguridad, saldos y asistencia.
- Autorización probada por endpoint y nunca confiada únicamente a la interfaz.
- WCAG AA, movimiento reducido y validación adaptable en dispositivos reales.
- Construcciones reproducibles, versiones fijadas y migraciones verificadas desde una base vacía.
- Archivos mayores de 300 líneas generan advertencia y requieren división o excepción documentada.

## Gobierno

Las decisiones estructurales se registran como ADR en `docs/decisiones/`. La convención lingüística está en [CONVENCIONES_NOMBRES.md](CONVENCIONES_NOMBRES.md). Una fase solo se marca completada cuando su evidencia está registrada en el plan de control.
