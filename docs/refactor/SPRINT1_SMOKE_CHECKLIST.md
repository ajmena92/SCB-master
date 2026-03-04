# Sprint 1 - Smoke Checklist Manual

Fecha base: 2026-03-04
Objetivo: validar que la extracción a servicios no cambió comportamiento funcional.

## Precondiciones
- Base de datos accesible (`SCSC`).
- Aplicación compilada en entorno Windows con dependencias instaladas.
- Lector biométrico disponible para pruebas de marca.

## 1) Login
- Abrir aplicación.
- Ingresar usuario normal válido y contraseña válida.
- Verificar acceso correcto.
- Ingresar usuario normal con contraseña inválida.
- Verificar mensaje de error.
- Ingresar usuario admin configurado en `app.config` (`AdminUsuario`) con clave `AdminClaveSoporte`.
- Verificar acceso correcto.

Resultado esperado:
- No hay cierres inesperados.
- Mensajes de validación se muestran como antes.

## 2) Control Transporte
- Abrir módulo Transporte.
- Validar carga inicial de usuarios/rutas.
- Registrar marca de estudiante.
- Registrar marca de docente (primera marca del día).
- Registrar segunda marca de docente (debe cambiar TipoMarca según lógica existente).
- Intentar doble lectura consecutiva del mismo carnet.

Resultado esperado:
- Persistencia correcta en `RegistroTransporte` / `RegistroDocentes`.
- Mensajes visuales iguales al baseline.

## 3) Control Comedor
- Abrir módulo Comedor.
- Validar carga inicial (usuarios, becas, horarios, marcas transporte del día).
- Registrar estudiante becado en día válido.
- Registrar estudiante no becado con tiquetes disponibles.
- Registrar estudiante no becado sin tiquetes.
- Validar casos de advertencia por marca de transporte faltante o tardía.

Resultado esperado:
- Persistencia correcta en `RegistroComedor`.
- Descuento de tiquetes correcto en `Usuario`.
- Transacción consistente sin datos parciales.

## 4) Importación Excel
- Cargar archivo Excel con hoja `Lista`.
- Verificar visualización en `DGV1`.
- Ejecutar importación para estudiantes.
- Ejecutar importación para docentes.
- Confirmar actualización de:
  - `Actualizado`
  - `Activo`
  - Campos personales (cedula, nombres, sección, especialidad, etc.)

Resultado esperado:
- Progreso visual avanza sin errores.
- Datos insertados/actualizados en `Usuario` sin inconsistencias.

## 5) Regresión básica de menú principal
- Abrir/cerrar módulos: Estudiantes, Becas, Rutas, Recargas, Reportes.
- Confirmar que no hay fallos por carga de formularios.

## Evidencia recomendada
- Capturas de pantallas clave por flujo.
- Consulta SQL de conteos antes/después por tabla impactada.
- Registro breve de incidencias encontradas.
