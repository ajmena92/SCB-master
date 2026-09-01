# Auditoría final de la plataforma web — 2026-08-31

## Resultado ejecutivo

| Dimensión | Puntaje | Evidencia principal |
| --- | ---: | --- |
| Accesibilidad | 4/4 | Todos los controles de las rutas activas tienen nombre accesible; foco de captura y navegación por teclado comprobados. |
| Rendimiento | 4/4 | Carga diferida por pantalla; ningún fragmento JavaScript supera 400 kB sin comprimir. |
| Adaptación responsive | 4/4 | Dashboard, rutas, personas, menú, comedor, años, reportes, usuarios y vinculación sin desbordamiento horizontal; vinculación comprobada también a 320 px. |
| Tematización | 3/4 | Tokens compartidos predominantes; los colores de rutas y carné son valores institucionales de dominio. |
| Antipatrones | 3/4 | No hay texto degradado, franjas laterales decorativas ni glassmorphism; se conservan tarjetas y sombras del diseño histórico. |
| **Total** | **18/20** | **Excelente; sin bloqueos de salida.** |

## Veredicto visual

La interfaz conserva una identidad propia del sistema original: paleta violeta, navegación
administrativa, gráficos operativos, menú por semanas, rutas coloreadas y carné. No presenta
los indicadores principales de una interfaz genérica generada por IA. La repetición de
tarjetas redondeadas es moderada y responde al lenguaje visual histórico solicitado.

## Hallazgos por severidad

- **P0:** ninguno.
- **P1:** ninguno.
- **P2:** ninguno.
- **P3 — colores institucionales fuera de tokens globales.** Ubicación:
  `frontend/src/funcionalidades/estudiantes/componentes/TarjetaCarnet.tsx` y catálogo de
  rutas. Impacto: una futura variante oscura requeriría revisar el carné. Decisión: conservar;
  los colores son datos de ruta y parte del requisito visual, no decoración intercambiable.
- **P3 — tarjetas y sombras reiteradas.** Ubicación: Dashboard, Rutas y carné. Impacto:
  únicamente estético. Decisión: conservar para respetar el frontend original validado por
  la persona usuaria.

## Calidad técnica y seguridad operacional

- La composición activa de FastAPI publica 32 rutas PostgreSQL y no publica
  `/api/v1/transporte/rutas` ni adaptadores SQL Server.
- Alembic está en `0008_usuarios_administrativos` y `alembic check` no detecta operaciones pendientes.
- La reconciliación contiene 729 matrículas del único padrón operativo, cero matrículas con
  `turno = '2'`, cero identidades estudiantiles huérfanas y cero rutas `02`/`08`.
- El respaldo previo a la depuración existe y mide 182 630 bytes.
- La cuenta heredada `administrador` conserva su clave, tiene sus sesiones anteriores
  revocadas y queda obligada a vincularse una sola vez con un profesor activo. La clave solo
  existe en el archivo local protegido con permiso `0600`.
- Administradores y operadores se gestionan desde el portal. Toda cuenta queda vinculada
  uno a uno con un profesor; el backend valida rol, permisos vigentes, estado y protección
  del último administrador.

## Verificaciones ejecutadas

- Backend: Ruff aprobado en todos los archivos del corte, MyPy aprobado en los 16 módulos
  activos afectados y 27/27 pruebas aprobadas.
- Frontend: TypeScript, ESLint, Prettier y reglas arquitectónicas aprobadas; 101/101 pruebas
  unitarias, 8/8 recorridos E2E y compilación Vite aprobadas.
- Base de datos: ciclo Alembic completo en base temporal, revisión actual y `alembic check`.
- Despliegue: PostgreSQL 17.6, API y frontend saludables en Docker Compose.
- Navegador real: acceso administrativo, cinco gráficos, menú, rutas, captura, personas,
  comedor y adaptación móvil comprobados. El acceso estudiantil con cédula y PIN respondió
  200 y exigió el cambio inicial de PIN. El flujo administrativo pendiente cargó profesores,
  impidió evadir la vinculación inicial y no produjo errores de consola ni desborde a 320 px.

## Pendientes

No quedan pendientes funcionales, de datos, migración o despliegue dentro del alcance
aprobado. Las dos observaciones P3 anteriores son decisiones visuales conscientes para
preservar el diseño original.
