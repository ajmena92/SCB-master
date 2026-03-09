# Baseline de Build y Smoke Manual

Fecha: 2026-03-09

## Build esperado
- Entorno oficial: Windows con Visual Studio 2019+ o MSBuild compatible con .NET Framework 4.6.1.
- Restauracion:
```bash
nuget restore SCSC_Marcas.sln
```
- Build base:
```bash
msbuild SCSC_Marcas.sln /p:Configuration=Debug /p:Platform="Any CPU"
```

## Estado actual del workspace
- En este entorno WSL no hay `msbuild` ni `nuget`, por lo que la verificacion de compilacion no se pudo ejecutar aqui.
- A partir de esta fecha el proyecto debe compilar con warnings visibles; cualquier warning nuevo debe revisarse, no ocultarse.

## Smoke manual minimo
1. Login con usuario valido.
2. Login con usuario invalido y validacion de mensaje.
3. Apertura de `ControlComedor` y lectura manual de carnet/cedula.
4. Apertura de `ControlTransporte` y lectura manual de carnet/cedula.
5. Consulta/edicion basica en `FrmEstudiantes`.
6. Recarga valida en `FrmRecarga`.
7. Importacion de prueba en `FrmImportarExcel`.
8. Generacion de reportes:
   - comedor
   - rutas
   - becados
   - proyeccion comedor

## Evidencia a registrar
- Fecha de ejecucion.
- Ambiente utilizado.
- Usuario de prueba.
- Resultado por flujo: OK / fallo.
- Captura o log si el flujo falla.
