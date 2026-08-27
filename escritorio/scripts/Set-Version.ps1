param(
  [string]$Repo = "C:\Dev\SCB-master"
)

$desktopRoot = Join-Path $Repo "escritorio"
$versionProps = Join-Path $desktopRoot "build\version.props"
$assemblyInfo = Join-Path $desktopRoot "SCSC\My Project\AssemblyInfo.vb"
$msiProduct = Join-Path $desktopRoot "Installer\Msi\Product.wxs"
$bundle = Join-Path $desktopRoot "Installer\Bundle\Bundle.wxs"

if (-not (Test-Path $versionProps)) {
  throw "No existe $versionProps"
}

[xml]$xml = Get-Content $versionProps
$version = $xml.Project.PropertyGroup.SCSCVersion
if ([string]::IsNullOrWhiteSpace($version)) {
  throw "SCSCVersion no definida en version.props"
}

$assemblyContent = Get-Content $assemblyInfo -Raw
$assemblyContent = [regex]::Replace($assemblyContent, 'AssemblyVersion\("([^"]+)"\)', "AssemblyVersion(`"$version`")")
$assemblyContent = [regex]::Replace($assemblyContent, 'AssemblyFileVersion\("([^"]+)"\)', "AssemblyFileVersion(`"$version`")")
Set-Content -Path $assemblyInfo -Value $assemblyContent -Encoding UTF8

$files = @($msiProduct, $bundle)
foreach ($file in $files) {
  $content = Get-Content $file -Raw
  $content = $content -replace '\$\(var\.ProductVersion\)', '$(var.ProductVersion)'
  Set-Content -Path $file -Value $content -Encoding UTF8
}

Write-Host "Version sincronizada: $version"
