# Politica De Versionado Y Actualizacion

## Objetivo
Definir como versionar la aplicacion, el instalador y el proceso de actualizacion de una instalacion existente.

## Dos versiones distintas
El proyecto maneja dos versiones separadas en [escritorio/build/version.props](/mnt/c/Dev/SCB-master/escritorio/build/version.props):

- `SCSCVersion`
  - version visible de la aplicacion
  - formato recomendado: `AAAA.MM.DD.Revision`
  - ejemplo: `2026.03.09.0`

- `SCSCInstallerVersion`
  - version usada por `MSI` y `Setup.exe`
  - formato recomendado: `Major.Minor.Patch`
  - ejemplo: `26.3.9`

## Por que se separan
`MSI` no acepta una version como `2026.03.09.0` porque el componente mayor debe ser menor que `256`.

Por eso:
- la app usa una version larga orientada a fecha
- el instalador usa una version corta compatible con Windows Installer

## Regla de cambio
Cada vez que saques una nueva entrega debes actualizar ambas versiones.

### 1. Correccion de bug
Usa esto cuando arreglas un error sin cambiar funcionalidad principal.

- `SCSCVersion`: cambia fecha o revision
- `SCSCInstallerVersion`: incrementa `Patch`

Ejemplo:

```xml
<SCSCVersion>2026.03.10.0</SCSCVersion>
<SCSCInstallerVersion>26.3.10</SCSCInstallerVersion>
```

### 2. Nueva funcionalidad
Usa esto cuando agregas pantallas, opciones o cambios visibles al usuario.

- `SCSCVersion`: cambia fecha o revision
- `SCSCInstallerVersion`: incrementa `Minor` y reinicia `Patch`

Ejemplo:

```xml
<SCSCVersion>2026.04.02.0</SCSCVersion>
<SCSCInstallerVersion>26.4.0</SCSCInstallerVersion>
```

### 3. Cambio grande o potencialmente incompatible
Usa esto cuando cambias estructura de instalacion, comportamiento base o despliegue.

- `SCSCVersion`: cambia fecha o revision
- `SCSCInstallerVersion`: incrementa `Major`

Ejemplo:

```xml
<SCSCVersion>2027.01.15.0</SCSCVersion>
<SCSCInstallerVersion>27.0.0</SCSCInstallerVersion>
```

## Flujo de actualizacion de una instalacion existente
1. Corregir el codigo.
2. Actualizar [escritorio/build/version.props](/mnt/c/Dev/SCB-master/escritorio/build/version.props).
3. Ejecutar el comando deploy:

```powershell
powershell -ExecutionPolicy Bypass -File "C:\Dev\SCB-master\escritorio\scripts\deploy.ps1"
```

4. Entregar el nuevo [SCSC-Setup.exe](/mnt/c/Dev/SCB-master/escritorio/Installer/Bundle/bin/Release/SCSC-Setup.exe).
5. Ejecutar ese `Setup.exe` en la maquina cliente.

## Como actualiza Windows
- El producto mantiene un `UpgradeCode` fijo.
- El `Product Id="*"` genera un nuevo `ProductCode` para cada build del MSI.
- Cuando la nueva `SCSCInstallerVersion` es mayor que la instalada, Windows Installer reemplaza la version anterior.

## Que se conserva al actualizar
La configuracion y la licencia quedan fuera del binario:

- configuracion: `%ProgramData%\SCSC\deployment.config.json`
- licencia: `%ProgramData%\SCSC\license\license.dat`

Eso permite actualizar la app sin reconfigurar todo en cada despliegue.

## Recomendacion operativa
Antes de entregar una nueva version:

1. Ejecuta `deploy.ps1`.
2. Instala la version anterior en una maquina de prueba.
3. Ejecuta el `Setup.exe` nuevo encima.
4. Verifica:
   - login
   - conexion a base de datos
   - reportes Crystal
   - configuracion externa preservada
   - licencia preservada

## Comando deploy
Si necesitas recordar el comando de generacion:

```powershell
powershell -ExecutionPolicy Bypass -File "C:\Dev\SCB-master\escritorio\scripts\deploy.ps1"
```
