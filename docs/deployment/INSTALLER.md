# Instalador SCSC

## Componentes
- `escritorio/Installer/Msi`: paquete MSI de la aplicación.
- `escritorio/Installer/Bundle`: `Setup.exe` con prerequisitos y ejecución del bootstrap de configuración.
- `escritorio/scripts/New-SCSCLicense.ps1`: genera un código simple de activación ligado a cliente/sede/base de datos.
- `escritorio/scripts/deploy.ps1`: compila app, regenera `GeneratedFiles.wxs`, construye MSI + bundle y abre la carpeta final.
- `escritorio/SCSC/Clases/LicenseService.vb`: valida un código corto de activación ligado a la base de datos instalada.

## Flujo de instalación
1. Ejecutar `Setup.exe`.
2. Instalar `Crystal Reports Runtime`.
3. Instalar el MSI de SCSC.
4. Lanzar `SCSC_Marcas.exe /deployment-setup`.
5. Capturar conexión SQL, probarla y pegar código de activación.

## Carpeta de operación
- Configuración: `%ProgramData%\SCSC\deployment.config.json`
- Licencia: `%ProgramData%\SCSC\license\license.dat`

## Release
```powershell
powershell -ExecutionPolicy Bypass -File "C:\Dev\SCB-master\escritorio\scripts\deploy.ps1"
```

El deploy ahora ejecuta pruebas automaticas antes de generar el instalador final. Si las pruebas fallan, no se construye `SCSC-Setup.exe`.

Ademas:
- valida prerequisitos de build
- crea una carpeta de release versionada
- copia `SCSC-Setup.exe`, `SCSC-App.msi`, terminos de licencia y checklist
- copia tambien los payloads externos generados por el bundle, por ejemplo redistribuibles y ejecutables auxiliares
- genera `checksums.txt`
- genera `release-manifest.json`
- permite firma digital opcional con `signtool`

Para forzar un deploy sin pruebas:

```powershell
powershell -ExecutionPolicy Bypass -File "C:\Dev\SCB-master\escritorio\scripts\deploy.ps1" -SkipTests
```

Para firmar artefactos:

```powershell
powershell -ExecutionPolicy Bypass -File "C:\Dev\SCB-master\escritorio\scripts\deploy.ps1" -SignArtifacts -PfxPath "C:\Certs\scsc-code-sign.pfx" -PfxPassword "CLAVE_AQUI"
```

Smoke automatico de build:

```powershell
powershell -ExecutionPolicy Bypass -File "C:\Dev\SCB-master\escritorio\scripts\smoke-build.ps1"
```

Pruebas base de logica:

```powershell
powershell -ExecutionPolicy Bypass -File "C:\Dev\SCB-master\escritorio\scripts\test.ps1"
```

Notas operativas:
- `deploy.ps1` sincroniza el icono embebido del `.exe` desde `escritorio\SCSC\Resources\LogoIcon.png` antes de compilar.
- El bundle usa Crystal Runtime redistribuible local desde `escritorio\Installer\Redist\CRRuntime_64bit_13_0_40.msi`.
- El vínculo de licencia/soporte del instalador apunta al correo de soporte para evitar placeholders no productivos.

Artefacto final:
- `C:\Dev\SCB-master\escritorio\Installer\Bundle\bin\Release\SCSC-Setup.exe`

## Versionado
La politica de versionado y actualizacion esta documentada en:

- [VERSIONING.md](/mnt/c/Dev/SCB-master/docs/deployment/VERSIONING.md)
- [SMOKE_TEST_CHECKLIST.md](/mnt/c/Dev/SCB-master/docs/deployment/SMOKE_TEST_CHECKLIST.md)

## Licencia
Los terminos generales de licencia del software estan documentados en:

- [TERMINOS_LICENCIA_CR.md](/mnt/c/Dev/SCB-master/docs/legal/TERMINOS_LICENCIA_CR.md)
