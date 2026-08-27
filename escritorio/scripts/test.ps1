param(
  [string]$Repo = "C:\Dev\SCB-master",
  [string]$Configuration = "Release",
  [string]$MSBuildPath = "",
  [string]$DotnetPath = "",
  [switch]$SkipBuildApp
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Resolve-FirstExistingPath {
  param(
    [Parameter(Mandatory = $true)]
    [string[]]$Candidates,
    [Parameter(Mandatory = $true)]
    [string]$Label
  )

  foreach ($candidate in $Candidates) {
    if ($candidate -and (Test-Path $candidate)) {
      return $candidate
    }
  }

  throw "No se encontro $Label. Rutas probadas: $($Candidates -join ', ')"
}

if ([string]::IsNullOrWhiteSpace($MSBuildPath)) {
  $MSBuildPath = Resolve-FirstExistingPath -Label "MSBuild.exe" -Candidates @(
    "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
  )
}

if ([string]::IsNullOrWhiteSpace($DotnetPath)) {
  $DotnetPath = Resolve-FirstExistingPath -Label "dotnet.exe" -Candidates @(
    "C:\Program Files\dotnet\dotnet.exe"
  )
}

$desktopRoot = Join-Path $Repo "escritorio"
$testProject = Join-Path $desktopRoot "SCSC.Tests\SCSC.Tests.vbproj"
$appProject = Join-Path $desktopRoot "SCSC\SCSC_Marcas.vbproj"
$testDll = Join-Path $desktopRoot ("SCSC.Tests\bin\" + $Configuration + "\net48\SCSC.Tests.dll")

foreach ($requiredPath in @($testProject, $appProject, $MSBuildPath, $DotnetPath)) {
  if (-not (Test-Path $requiredPath)) {
    throw "No existe la ruta requerida: $requiredPath"
  }
}

if (-not $SkipBuildApp) {
  Write-Host "Compilando aplicacion base..." -ForegroundColor Cyan
  & $MSBuildPath $appProject /p:Configuration=$Configuration /p:Platform=AnyCPU /nologo /v:m
  if ($LASTEXITCODE -ne 0) {
    throw "Fallo la compilacion del proyecto principal."
  }
}

Write-Host "Restaurando y compilando pruebas..." -ForegroundColor Cyan
& $DotnetPath test $testProject -c $Configuration --no-restore
if ($LASTEXITCODE -ne 0) {
  Write-Host "Reintentando con restore..." -ForegroundColor Yellow
  & $DotnetPath test $testProject -c $Configuration
}
if ($LASTEXITCODE -ne 0) {
  throw "Fallo la ejecucion del proyecto de pruebas."
}

if (-not (Test-Path $testDll)) {
  throw "No se encontro el ensamblado de pruebas: $testDll"
}

Write-Host ""
Write-Host "Pruebas completadas correctamente." -ForegroundColor Green
Write-Host "DLL: $testDll"
