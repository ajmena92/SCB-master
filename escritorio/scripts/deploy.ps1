param(
  [string]$Repo = "C:\Dev\SCB-master",
  [string]$Configuration = "Release",
  [string]$AppPlatform = "Any CPU",
  [string]$InstallerPlatform = "x64",
  [string]$MSBuildPath = "",
  [string]$WiXPath = "",
  [string]$DotnetPath = "",
  [string]$SignToolPath = "",
  [string]$PfxPath = "",
  [string]$PfxPassword = "",
  [string]$CertThumbprint = "",
  [string]$ReleaseRoot = "",
  [switch]$SignArtifacts,
  [switch]$SkipTests,
  [switch]$NoOpen
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Test-LastExitCode {
  if (Get-Variable -Name LASTEXITCODE -Scope Global -ErrorAction SilentlyContinue) {
    return $global:LASTEXITCODE
  }

  return 0
}

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

function Assert-PathExists {
  param(
    [Parameter(Mandatory = $true)]
    [string]$PathValue,
    [Parameter(Mandatory = $true)]
    [string]$Label
  )

  if (-not (Test-Path $PathValue)) {
    throw "No existe ${Label}: $PathValue"
  }
}

function Get-VersionInfo {
  param(
    [Parameter(Mandatory = $true)]
    [string]$VersionPropsPath
  )

  [xml]$xml = Get-Content $VersionPropsPath
  return [pscustomobject]@{
    AppVersion = [string]$xml.Project.PropertyGroup.SCSCVersion
    InstallerVersion = [string]$xml.Project.PropertyGroup.SCSCInstallerVersion
    ProductName = [string]$xml.Project.PropertyGroup.SCSCProductName
    Manufacturer = [string]$xml.Project.PropertyGroup.SCSCManufacturer
  }
}

function Get-FileSha256 {
  param(
    [Parameter(Mandatory = $true)]
    [string]$PathValue
  )

  return (Get-FileHash -Path $PathValue -Algorithm SHA256).Hash
}

function Get-BundleLoosePayloads {
  param(
    [Parameter(Mandatory = $true)]
    [string]$BundleOutputDirectory,
    [Parameter(Mandatory = $true)]
    [string]$BundleExeName
  )

  if (-not (Test-Path $BundleOutputDirectory)) {
    return @()
  }

  return @(Get-ChildItem -Path $BundleOutputDirectory -File | Where-Object {
      $_.Name -ne $BundleExeName -and
      $_.Extension -ne ".wixpdb"
    })
}

function Get-RedistFiles {
  param(
    [Parameter(Mandatory = $true)]
    [string]$RedistDirectory
  )

  if (-not (Test-Path $RedistDirectory)) {
    return @()
  }

  return @(Get-ChildItem -Path $RedistDirectory -File | Where-Object {
      $_.Extension -ne ".md"
    })
}

function Ensure-ReleaseRoot {
  param(
    [Parameter(Mandatory = $true)]
    [string]$BasePath,
    [Parameter(Mandatory = $true)]
    [string]$InstallerVersion
  )

  $releaseDir = Join-Path $BasePath $InstallerVersion
  New-Item -ItemType Directory -Path $releaseDir -Force | Out-Null
  return $releaseDir
}

function Try-SignFile {
  param(
    [Parameter(Mandatory = $true)]
    [string]$TargetFile,
    [Parameter(Mandatory = $true)]
    [string]$ResolvedSignToolPath,
    [string]$ResolvedPfxPath,
    [string]$ResolvedPfxPassword,
    [string]$ResolvedCertThumbprint
  )

  $args = @("sign", "/fd", "SHA256", "/td", "SHA256")
  if (-not [string]::IsNullOrWhiteSpace($ResolvedPfxPath)) {
    $args += @("/f", $ResolvedPfxPath)
    if (-not [string]::IsNullOrWhiteSpace($ResolvedPfxPassword)) {
      $args += @("/p", $ResolvedPfxPassword)
    }
  } elseif (-not [string]::IsNullOrWhiteSpace($ResolvedCertThumbprint)) {
    $args += @("/sha1", $ResolvedCertThumbprint)
  } else {
    throw "Para firmar artefactos debe indicar PfxPath o CertThumbprint."
  }

  $args += $TargetFile
  & $ResolvedSignToolPath @args
  if ((Test-LastExitCode) -ne 0) {
    throw "Fallo la firma digital de: $TargetFile"
  }
}

function Validate-Prerequisites {
  param(
    [Parameter(Mandatory = $true)]
    [string[]]$RequiredPaths,
    [Parameter(Mandatory = $true)]
    [string]$LogoPath
  )

  foreach ($pathItem in $RequiredPaths) {
    if (-not (Test-Path $pathItem)) {
      throw "Falta prerequisito requerido: $pathItem"
    }
  }

  if (-not (Test-Path $LogoPath)) {
    throw "Falta el recurso grafico principal para el icono embebido: $LogoPath"
  }
}

function Sync-ApplicationIcon {
  param(
    [Parameter(Mandatory = $true)]
    [string]$RepoPath
  )

  $sourceCandidates = @(
    (Join-Path $RepoPath "SCSC\Resources\LogoIcon.png"),
    (Join-Path $RepoPath "SCSC\Resources\Login.png")
  )
  $sourceImage = $null

  foreach ($candidate in $sourceCandidates) {
    if (Test-Path $candidate) {
      $sourceImage = $candidate
      break
    }
  }

  if (-not $sourceImage) {
    Write-Warning "No se encontro imagen fuente para sincronizar favicon.ico."
    return
  }

  $targetIcon = Join-Path $RepoPath "SCSC\favicon.ico"
  Add-Type -AssemblyName System.Drawing

  $bitmap = $null
  $resized = $null
  $icon = $null
  $stream = $null

  try {
    $bitmap = [System.Drawing.Bitmap]::FromFile($sourceImage)
    $resized = New-Object System.Drawing.Bitmap($bitmap, (New-Object System.Drawing.Size(256, 256)))
    $icon = [System.Drawing.Icon]::FromHandle($resized.GetHicon())
    $stream = [System.IO.File]::Create($targetIcon)
    $icon.Save($stream)
    Write-Host "Icono sincronizado desde: $sourceImage" -ForegroundColor DarkCyan
  }
  finally {
    if ($stream) { $stream.Dispose() }
    if ($icon) { $icon.Dispose() }
    if ($resized) { $resized.Dispose() }
    if ($bitmap) { $bitmap.Dispose() }
  }
}

if ([string]::IsNullOrWhiteSpace($MSBuildPath)) {
  $MSBuildPath = Resolve-FirstExistingPath -Label "MSBuild.exe" -Candidates @(
    "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
  )
}

if ([string]::IsNullOrWhiteSpace($WiXPath)) {
  $WiXPath = Resolve-FirstExistingPath -Label "WiX Toolset" -Candidates @(
    "C:\Program Files (x86)\WiX Toolset v3.14\bin",
    "C:\Program Files (x86)\WiX Toolset v3.11\bin"
  )
}

if ([string]::IsNullOrWhiteSpace($DotnetPath)) {
  $DotnetPath = Resolve-FirstExistingPath -Label "dotnet.exe" -Candidates @(
    "C:\Program Files\dotnet\dotnet.exe"
  )
}

if ($SignArtifacts -and [string]::IsNullOrWhiteSpace($SignToolPath)) {
  $SignToolPath = Resolve-FirstExistingPath -Label "signtool.exe" -Candidates @(
    "C:\Program Files (x86)\Windows Kits\10\App Certification Kit\signtool.exe",
    "C:\Program Files (x86)\Windows Kits\10\bin\x64\signtool.exe",
    "C:\Program Files (x86)\Microsoft SDKs\ClickOnce\SignTool\signtool.exe"
  )
}

$normalizedAppPlatform = switch ($AppPlatform) {
  "Any CPU" { "AnyCPU" }
  default { $AppPlatform }
}

$desktopRoot = Join-Path $Repo "escritorio"
$solution = Join-Path $desktopRoot "SCSC_Marcas.sln"
$appProject = Join-Path $desktopRoot "SCSC\SCSC_Marcas.vbproj"
$setVersionScript = Join-Path $desktopRoot "scripts\Set-Version.ps1"
$testScript = Join-Path $desktopRoot "scripts\test.ps1"
$versionProps = Join-Path $desktopRoot "build\version.props"
$msiProject = Join-Path $desktopRoot "Installer\Msi\SCSC.Installer.wixproj"
$bundleProject = Join-Path $desktopRoot "Installer\Bundle\SCSC.Bundle.wixproj"
$generatedFiles = Join-Path $desktopRoot "Installer\Msi\GeneratedFiles.wxs"
$appOutput = switch ($normalizedAppPlatform) {
  "x64" { Join-Path $desktopRoot "SCSC\bin\x64\Release" }
  "x86" { Join-Path $desktopRoot "SCSC\bin\x86\Release" }
  default { Join-Path $desktopRoot "SCSC\bin\Release" }
}
$heat = Join-Path $WiXPath "heat.exe"
$crystalRuntime = Join-Path $desktopRoot "Installer\Redist\CRRuntime_64bit_13_0_40.msi"
$logoIcon = Join-Path $desktopRoot "SCSC\Resources\LogoIcon.png"
$licenseTerms = Join-Path $Repo "docs\legal\TERMINOS_LICENCIA_CR.md"
$smokeChecklist = Join-Path $Repo "docs\deployment\SMOKE_TEST_CHECKLIST.md"

if ([string]::IsNullOrWhiteSpace($ReleaseRoot)) {
  $ReleaseRoot = Join-Path $Repo "artifacts\releases"
}

Validate-Prerequisites -RequiredPaths @(
  $solution,
  $appProject,
  $setVersionScript,
  $testScript,
  $versionProps,
  $msiProject,
  $bundleProject,
  $heat,
  $crystalRuntime,
  $DotnetPath,
  $licenseTerms,
  $smokeChecklist
) -LogoPath $logoIcon

$versionInfo = Get-VersionInfo -VersionPropsPath $versionProps
$versionedMsiName = "SCSC-App-$($versionInfo.InstallerVersion)"
$versionedBundleName = "SCSC-Setup-$($versionInfo.InstallerVersion)"
$msiOutput = Join-Path $desktopRoot ("Installer\Msi\bin\" + $Configuration + "\" + $versionedMsiName + ".msi")
$bundleOutputDir = Join-Path $desktopRoot "Installer\Bundle\bin\$Configuration"
$bundleOutput = Join-Path $bundleOutputDir ($versionedBundleName + ".exe")
$appExePath = Join-Path $appOutput "SCSC_Marcas.exe"
$releaseDir = Ensure-ReleaseRoot -BasePath $ReleaseRoot -InstallerVersion $versionInfo.InstallerVersion
$releaseManifestPath = Join-Path $releaseDir "release-manifest.json"
$checksumsPath = Join-Path $releaseDir "checksums.txt"
$releaseNotesPath = Join-Path $releaseDir "release-notes.txt"

Write-Host "Sincronizando version..." -ForegroundColor Cyan
& $setVersionScript -Repo $Repo
if ((Test-LastExitCode) -ne 0) {
  throw "Fallo Set-Version.ps1."
}

Write-Host "Sincronizando icono embebido..." -ForegroundColor Cyan
Sync-ApplicationIcon -RepoPath $desktopRoot

Write-Host "Compilando aplicacion..." -ForegroundColor Cyan
& $MSBuildPath $appProject /p:Configuration=$Configuration /p:Platform=$normalizedAppPlatform /nologo /v:m
if ((Test-LastExitCode) -ne 0) {
  throw "Fallo el build del proyecto principal."
}

if (-not (Test-Path (Join-Path $appOutput "SCSC_Marcas.exe"))) {
  throw "No se encontro SCSC_Marcas.exe en $appOutput"
}

if (-not $SkipTests) {
  Write-Host "Ejecutando pruebas antes del instalador..." -ForegroundColor Cyan
  $testArgs = @(
    "-ExecutionPolicy", "Bypass",
    "-File", $testScript,
    "-Repo", $Repo,
    "-Configuration", $Configuration,
    "-MSBuildPath", $MSBuildPath,
    "-SkipBuildApp"
  )

  if (-not [string]::IsNullOrWhiteSpace($DotnetPath)) {
    $testArgs += @("-DotnetPath", $DotnetPath)
  }

  & powershell @testArgs
  if ((Test-LastExitCode) -ne 0) {
    throw "Las pruebas fallaron. Se cancela la generacion del instalador."
  }
}

Write-Host "Regenerando archivos del MSI..." -ForegroundColor Cyan
& $heat dir $appOutput -cg AppFiles -dr INSTALLFOLDER -gg -sfrag -sreg -srd -var var.SourceDir -out $generatedFiles
if ((Test-LastExitCode) -ne 0) {
  throw "Fallo heat al generar GeneratedFiles.wxs."
}

Write-Host "Compilando MSI..." -ForegroundColor Cyan
& $MSBuildPath $msiProject /p:Configuration=$Configuration /p:Platform=$InstallerPlatform /p:OutputName=$versionedMsiName /nologo /v:m
if ((Test-LastExitCode) -ne 0) {
  throw "Fallo el build del MSI."
}

Assert-PathExists -PathValue $msiOutput -Label "MSI generado"

Write-Host "Compilando instalador final..." -ForegroundColor Cyan
& $MSBuildPath $bundleProject /p:Configuration=$Configuration /p:Platform=$InstallerPlatform /p:ProductVersion=$($versionInfo.InstallerVersion) /p:MsiSource=$msiOutput /p:AppExePath=$appExePath /p:OutputName=$versionedBundleName /nologo /v:m
if ((Test-LastExitCode) -ne 0) {
  throw "Fallo el build del bundle."
}

if (-not (Test-Path $bundleOutput)) {
  throw "No se genero el instalador final en: $bundleOutput"
}

if ($SignArtifacts) {
  Write-Host "Firmando artefactos..." -ForegroundColor Cyan
  Try-SignFile -TargetFile $msiOutput -ResolvedSignToolPath $SignToolPath -ResolvedPfxPath $PfxPath -ResolvedPfxPassword $PfxPassword -ResolvedCertThumbprint $CertThumbprint
  Try-SignFile -TargetFile $bundleOutput -ResolvedSignToolPath $SignToolPath -ResolvedPfxPath $PfxPath -ResolvedPfxPassword $PfxPassword -ResolvedCertThumbprint $CertThumbprint
}

Write-Host "Preparando release versionada..." -ForegroundColor Cyan
$releaseSetupName = $versionedBundleName + ".exe"
$releaseMsiName = $versionedMsiName + ".msi"
$releaseSetup = Join-Path $releaseDir $releaseSetupName
$releaseMsi = Join-Path $releaseDir $releaseMsiName
$releaseTerms = Join-Path $releaseDir "TERMINOS_LICENCIA_CR.md"
$releaseChecklist = Join-Path $releaseDir "SMOKE_TEST_CHECKLIST.md"
$releaseRedistReadme = Join-Path $releaseDir "REDIST_README.md"
$bundleLoosePayloads = Get-BundleLoosePayloads -BundleOutputDirectory $bundleOutputDir -BundleExeName (Split-Path $bundleOutput -Leaf)
$redistPayloads = Get-RedistFiles -RedistDirectory (Join-Path $desktopRoot "Installer\Redist")
$payloadMap = @{}

Copy-Item $bundleOutput $releaseSetup -Force
Copy-Item $msiOutput $releaseMsi -Force
Copy-Item $licenseTerms $releaseTerms -Force
Copy-Item $smokeChecklist $releaseChecklist -Force
if (Test-Path (Join-Path $desktopRoot "Installer\Redist\README.md")) {
  Copy-Item (Join-Path $desktopRoot "Installer\Redist\README.md") $releaseRedistReadme -Force
}

foreach ($payload in $bundleLoosePayloads) {
  $payloadMap[$payload.Name] = $payload.FullName
}

foreach ($payload in $redistPayloads) {
  $payloadMap[$payload.Name] = $payload.FullName
}

foreach ($payloadName in $payloadMap.Keys) {
  Copy-Item $payloadMap[$payloadName] (Join-Path $releaseDir $payloadName) -Force
}

$releaseArtifacts = New-Object System.Collections.Generic.List[object]
$releaseArtifacts.Add([pscustomobject]@{
    name = $releaseSetupName
    path = $releaseSetup
    sha256 = Get-FileSha256 -PathValue $releaseSetup
  })
$releaseArtifacts.Add([pscustomobject]@{
    name = $releaseMsiName
    path = $releaseMsi
    sha256 = Get-FileSha256 -PathValue $releaseMsi
  })

foreach ($payloadName in ($payloadMap.Keys | Sort-Object)) {
  $releasePayloadPath = Join-Path $releaseDir $payloadName
  $releaseArtifacts.Add([pscustomobject]@{
      name = $payloadName
      path = $releasePayloadPath
      sha256 = Get-FileSha256 -PathValue $releasePayloadPath
    })
}

$releaseArtifacts | ForEach-Object {
  "SHA256  {0}  {1}" -f $_.name.PadRight(24), $_.sha256
} | Set-Content -Path $checksumsPath -Encoding UTF8

@(
  "Producto: $($versionInfo.ProductName)",
  "Fabricante: $($versionInfo.Manufacturer)",
  "Version app: $($versionInfo.AppVersion)",
  "Version instalador: $($versionInfo.InstallerVersion)",
  "Fecha UTC: $(Get-Date -Format 'yyyy-MM-ddTHH:mm:ssZ')",
  "Pruebas: $(if ($SkipTests) { 'omitidas manualmente' } else { 'ejecutadas OK' })",
  "Firma digital: $(if ($SignArtifacts) { 'aplicada' } else { 'no aplicada' })",
  "Payloads externos incluidos: $(if ($payloadMap.Count -gt 0) { (($payloadMap.Keys | Sort-Object) -join ', ') } else { 'ninguno' })"
) | Set-Content -Path $releaseNotesPath -Encoding UTF8

$manifestTests = if ($SkipTests) { "skipped" } else { "passed" }
$manifestArtifacts = $releaseArtifacts.ToArray()

$manifest = [pscustomobject]@{
  productName = $versionInfo.ProductName
  manufacturer = $versionInfo.Manufacturer
  appVersion = $versionInfo.AppVersion
  installerVersion = $versionInfo.InstallerVersion
  generatedAtUtc = (Get-Date).ToUniversalTime().ToString("o")
  tests = $manifestTests
  signed = [bool]$SignArtifacts
  artifacts = $manifestArtifacts
} | ConvertTo-Json -Depth 6

Set-Content -Path $releaseManifestPath -Value $manifest -Encoding UTF8

Write-Host ""
Write-Host "Deploy generado correctamente." -ForegroundColor Green
Write-Host "Instalador: $bundleOutput"
Write-Host "Carpeta:    $bundleOutputDir"
Write-Host "Release:    $releaseDir"

if (-not $NoOpen) {
  Start-Process explorer.exe $releaseDir
}
