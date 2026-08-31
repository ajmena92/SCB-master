# Requisitos vigentes del portal y la operación de comedor

## Alcance

La plataforma `web/` es la única aplicación operativa. PostgreSQL es la única
base de datos en ejecución; WinForms y SQL Server son únicamente referencias del
corte histórico y no participan en autenticación, consultas ni escrituras.

La instancia conserva un único padrón operativo. Las 214 matrículas y las rutas
exclusivas del nocturno fueron retiradas después de un respaldo verificable. La
interfaz no ofrece filtros, selectores ni reportes por horario.

## Identidad y carné

- Estudiantes y profesores ingresan al portal con **cédula y PIN de seis dígitos**.
- El código institucional se usa en el código de barras del carné y en los lectores
  operativos; no sustituye la cédula durante el inicio de sesión.
- El PIN inicial obliga a definir uno nuevo y las sesiones anteriores se revocan al
  cambiarlo.
- El carné muestra la matrícula anual vigente, sección, beneficio y ruta.
- `ruta.codigo` conserva literalmente la nomenclatura MEP; `ruta.descripcion`
  conserva el texto administrado por la persona usuaria.
- `ruta.color_hex` usa la paleta institucional recuperada y es la fuente única del
  color mostrado tanto en el catálogo como en el carné.

## Menú y reserva

- Las plantillas se organizan por las cinco semanas del mes y los cinco días
  laborales, con componentes ordenados.
- Una publicación para una fecha conserva una copia de los componentes de la
  plantilla y puede sustituirse sin alterar el historial de otras fechas.
- El portal muestra el menú publicado para hoy y permite reservar o cancelar para
  la propia persona; nunca acepta operar sobre una identidad ajena.
- Una persona beneficiaria reserva sin consumir saldo. Una persona no beneficiaria
  inmoviliza un tiquete al reservar; cancelar lo libera e ingresar lo consume.
- Los profesores ingresan directamente, pero consumen tiquete.

## Captura de comedor

`POST /api/v1/comedor/operacion` reemplaza el lector de teclado del WinForms:

1. El escáner USB escribe el código institucional y envía Enter.
2. La interfaz conserva el foco para la siguiente lectura.
3. La API identifica a la persona activa y valida matrícula anual, reserva o
   autorización excepcional, beneficio, saldo y duplicidad diaria.
4. La marca de transporte es informativa: su ausencia produce una advertencia, no
   autoriza ni rechaza por sí sola el ingreso.
5. Cada intento, aceptado o rechazado, se registra en
   `evento_operacion_comedor` con resultado, operador, duración y motivo.
6. `GET /api/v1/comedor/operacion/estado` publica meta, progreso, duplicados,
   rechazos y lecturas recientes.

Un segundo ingreso de la misma persona y fecha responde `409`. Una excepción para
un estudiante sin reserva requiere decisión y motivo asociados al operador.

## Transporte y rutas

El catálogo y la captura de transporte forman una sola funcionalidad visual. Las
rutas se leen y escriben exclusivamente mediante `/api/v1/rutas`; la marca diaria
se registra en `/api/v1/transporte/marcas`. No existe un segundo catálogo ni un
endpoint conectado a SQL Server.

La asignación se relaciona con la matrícula anual, tiene vigencia y no admite dos
vigencias solapadas para la misma matrícula. Una marca de transporte utiliza la
asignación vigente y es única por matrícula y fecha.

## Tablero y separación de personas

El tablero se construye desde PostgreSQL y conserva gráficas de semana laboral,
últimos cinco días, estado de comedor y rutas. La vista estudiantil excluye
profesores; la vista docente no reutiliza cifras estudiantiles. No hay desglose ni
filtro Diurno/Nocturno.

## Aceptación

La integración se acepta con:

- pruebas HTTP de autenticación, autorización, matrícula anual, rutas, reserva,
  consumo, duplicados, captura y carné;
- pruebas de componentes y compilación TypeScript del frontend;
- migraciones Alembic verificadas desde una base vacía, con `upgrade`, `downgrade`
  y nuevo `upgrade`;
- comprobación visual de Dashboard, Menú, Rutas, captura de comedor, portal y carné;
- respaldo anterior a cualquier depuración de datos y reconciliación posterior sin
  matrículas nocturnas ni identidades estudiantiles huérfanas.
