# Smoke Test De Release

## Objetivo
Checklist minimo para validar una nueva version de `SCSC-Setup.exe` antes de entregarla.

## Instalacion limpia
- Ejecutar `SCSC-Setup.exe`.
- Confirmar instalacion de `Crystal Reports Runtime`.
- Confirmar instalacion de `SCSC-App.msi`.
- Confirmar apertura del flujo `/deployment-setup`.
- Configurar servidor SQL, base de datos y autenticacion.
- Guardar configuracion y validar que se genere `%ProgramData%\SCSC\deployment.config.json`.
- Activar licencia y validar `%ProgramData%\SCSC\license\license.dat`.

## Arranque
- Abrir la aplicacion instalada desde acceso directo.
- Confirmar icono correcto en acceso directo y ventana principal.
- Confirmar login exitoso.
- Confirmar shell principal sin errores visuales.

## Operacion
- Registrar una marca de comedor.
- Registrar una marca de transporte.
- Abrir mantenimiento de estudiantes.
- Abrir mantenimiento de rutas.
- Ejecutar una busqueda y cancelar sin seleccion.
- Ejecutar una busqueda sin resultados y cerrar sin error.

## Reportes
- Abrir los 4 formularios de parametros.
- Confirmar titulo grande en negrita.
- Confirmar boton principal `Imprimir`.
- Generar al menos un reporte de comedor.
- Generar al menos un reporte de transporte.
- Validar que Crystal Reports conecte a la base correcta.

## Upgrade
- Instalar una version previa.
- Ejecutar la nueva `SCSC-Setup.exe`.
- Confirmar reemplazo de version sin desinstalacion manual.
- Confirmar conservacion de configuracion externa y licencia.

## Cierre
- Validar desinstalacion desde Programas y caracteristicas.
- Confirmar si se conserva o elimina `%ProgramData%\SCSC` segun politica definida.
