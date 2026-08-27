param(
  [string]$Repo = "C:\Dev\SCB-master",
  [string]$Configuration = "Release",
  [string]$AppPlatform = "Any CPU",
  [string]$InstallerPlatform = "x64",
  [string]$MSBuildPath = "",
  [string]$WiXPath = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Assert-PathExists {
  param(
    [Parameter(Mandatory = $true)]
    [string]$PathValue,
    [Parameter(Mandatory = $true)]
    [string]$Label
  )

  if (-not (Test-Path $PathValue)) {
    throw "Falta ${Label}: $PathValue"
  }
}

function Assert-FileContains {
  param(
    [Parameter(Mandatory = $true)]
    [string]$PathValue,
    [Parameter(Mandatory = $true)]
    [string]$Pattern,
    [Parameter(Mandatory = $true)]
    [string]$Label
  )

  $matches = Select-String -Path $PathValue -Pattern $Pattern -SimpleMatch -ErrorAction SilentlyContinue
  if (-not $matches) {
    throw "No se encontro '$Pattern' en ${Label}: $PathValue"
  }
}

function Assert-FileNotContains {
  param(
    [Parameter(Mandatory = $true)]
    [string]$PathValue,
    [Parameter(Mandatory = $true)]
    [string]$Pattern,
    [Parameter(Mandatory = $true)]
    [string]$Label
  )

  $matches = Select-String -Path $PathValue -Pattern $Pattern -SimpleMatch -ErrorAction SilentlyContinue
  if ($matches) {
    throw "Se detecto '$Pattern' en ${Label}: $PathValue"
  }
}

$desktopRoot = Join-Path $Repo "escritorio"
$deployScript = Join-Path $desktopRoot "scripts\deploy.ps1"
$crystalRuntime = Join-Path $desktopRoot "Installer\Redist\CRRuntime_64bit_13_0_40.msi"
$appExe = Join-Path $desktopRoot "SCSC\bin\$Configuration\SCSC_Marcas.exe"
$msiFile = Join-Path $desktopRoot "Installer\Msi\bin\$Configuration\SCSC-App.msi"
$setupFile = Join-Path $desktopRoot "Installer\Bundle\bin\$Configuration\SCSC-Setup.exe"
$generatedFiles = Join-Path $desktopRoot "Installer\Msi\GeneratedFiles.wxs"
$bundleWxs = Join-Path $desktopRoot "Installer\Bundle\Bundle.wxs"
$versionProps = Join-Path $desktopRoot "build\version.props"

Assert-PathExists -PathValue $deployScript -Label "script deploy"
Assert-PathExists -PathValue $crystalRuntime -Label "prerequisito Crystal Runtime"
Assert-PathExists -PathValue $bundleWxs -Label "Bundle.wxs"
Assert-PathExists -PathValue $versionProps -Label "version.props"

Write-Host "Ejecutando deploy smoke..." -ForegroundColor Cyan
$deployArgs = @(
  "-ExecutionPolicy", "Bypass",
  "-File", $deployScript,
  "-Repo", $Repo,
  "-Configuration", $Configuration,
  "-AppPlatform", $AppPlatform,
  "-InstallerPlatform", $InstallerPlatform,
  "-NoOpen"
)

if (-not [string]::IsNullOrWhiteSpace($MSBuildPath)) {
  $deployArgs += @("-MSBuildPath", $MSBuildPath)
}

if (-not [string]::IsNullOrWhiteSpace($WiXPath)) {
  $deployArgs += @("-WiXPath", $WiXPath)
}

& powershell @deployArgs
if ($LASTEXITCODE -ne 0) {
  throw "deploy.ps1 devolvio codigo $LASTEXITCODE"
}

Write-Host "Validando artefactos..." -ForegroundColor Cyan
Assert-PathExists -PathValue $appExe -Label "ejecutable principal"
Assert-PathExists -PathValue $msiFile -Label "MSI"
Assert-PathExists -PathValue $setupFile -Label "Setup.exe"
Assert-PathExists -PathValue $generatedFiles -Label "GeneratedFiles.wxs"

Write-Host "Validando versionado..." -ForegroundColor Cyan
Assert-FileContains -PathValue $versionProps -Pattern "<SCSCVersion>" -Label "version.props"
Assert-FileContains -PathValue $versionProps -Pattern "<SCSCInstallerVersion>" -Label "version.props"

Write-Host "Validando WiX cosechado..." -ForegroundColor Cyan
Assert-FileContains -PathValue $generatedFiles -Pattern '$(var.SourceDir)' -Label "GeneratedFiles.wxs"
Assert-FileNotContains -PathValue $generatedFiles -Pattern 'C:\Dev\SCB-master\' -Label "GeneratedFiles.wxs"

Write-Host "Validando bundle..." -ForegroundColor Cyan
Assert-FileContains -PathValue $bundleWxs -Pattern 'CRRuntime_64bit_13_0_40.msi' -Label "Bundle.wxs"
Assert-FileContains -PathValue $bundleWxs -Pattern 'mailto:ajmena92@gmail.com' -Label "Bundle.wxs"

Write-Host ""
Write-Host "Smoke build OK." -ForegroundColor Green
Write-Host "App:   $appExe"
Write-Host "MSI:   $msiFile"
Write-Host "Setup: $setupFile"
