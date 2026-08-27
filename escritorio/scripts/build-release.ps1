param(
  [string]$Repo = "C:\Dev\SCB-master",
  [string]$MSBuildPath = "C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe",
  [string]$WiXPath = "C:\Program Files (x86)\WiX Toolset v3.11\bin",
  [string]$Configuration = "Release",
  [string]$Platform = "x64"
)

$desktopRoot = Join-Path $Repo "escritorio"
$solution = Join-Path $desktopRoot "SCSC_Marcas.sln"
$msiProject = Join-Path $desktopRoot "Installer\Msi\SCSC.Installer.wixproj"
$bundleProject = Join-Path $desktopRoot "Installer\Bundle\SCSC.Bundle.wixproj"
$generatedFiles = Join-Path $desktopRoot "Installer\Msi\GeneratedFiles.wxs"
$appOutput = Join-Path $desktopRoot "SCSC\bin\x64\Release"
$heat = Join-Path $WiXPath "heat.exe"

& (Join-Path $desktopRoot "scripts\Set-Version.ps1") -Repo $Repo

if (-not (Test-Path $MSBuildPath)) {
  throw "No existe MSBuild en $MSBuildPath"
}
if (-not (Test-Path $heat)) {
  throw "No existe heat.exe en $heat"
}

& $MSBuildPath $solution /p:Configuration=$Configuration /p:Platform=$Platform /nologo /v:m
if ($LASTEXITCODE -ne 0) {
  throw "Fallo el build de la solución."
}

& $heat dir $appOutput -cg AppFiles -dr INSTALLFOLDER -gg -sfrag -sreg -srd -out $generatedFiles
if ($LASTEXITCODE -ne 0) {
  throw "Fallo heat al generar GeneratedFiles.wxs."
}

& $MSBuildPath $msiProject /p:Configuration=$Configuration /p:Platform=$Platform /nologo /v:m
if ($LASTEXITCODE -ne 0) {
  throw "Fallo el build del MSI."
}

& $MSBuildPath $bundleProject /p:Configuration=$Configuration /p:Platform=$Platform /nologo /v:m
if ($LASTEXITCODE -ne 0) {
  throw "Fallo el build del bundle."
}

Write-Host "Release generado correctamente."
