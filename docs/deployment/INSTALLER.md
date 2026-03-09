# Instalador SCSC

## Componentes
- `Installer/Msi`: paquete MSI de la aplicación.
- `Installer/Bundle`: `Setup.exe` con prerequisitos y ejecución del bootstrap de configuración.
- `scripts/New-SCSCLicense.ps1`: genera activaciones offline firmadas.
- `scripts/build-release.ps1`: compila app, cosecha archivos y construye MSI + bundle.

## Flujo de instalación
1. Ejecutar `Setup.exe`.
2. Instalar o validar `.NET Framework 4.6.1`.
3. Instalar `Crystal Reports Runtime`.
4. Instalar el MSI de SCSC.
5. Lanzar `SCSC_Marcas.exe /deployment-setup`.
6. Capturar conexión SQL, probarla y pegar código de activación.

## Carpeta de operación
- Configuración: `%ProgramData%\SCSC\deployment.config.json`
- Licencia: `%ProgramData%\SCSC\license\license.dat`

## Release
```powershell
pwsh -File .\scripts\build-release.ps1 -Repo C:\Dev\SCB-master
```
