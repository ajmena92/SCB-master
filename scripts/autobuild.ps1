param(
  [string]$Repo = "C:\Dev\SCB-master",
  [string]$Solution = "SCSC_Marcas.sln",
  [string]$MSBuildPath = "C:\Program Files\Microsoft SQL Server Management Studio 22\Release\MSBuild\Current\Bin\MSBuild.exe",
  [string]$Configuration = "Debug",
  [string]$Platform = "Any CPU",
  [int]$DebounceSeconds = 2,
  [int]$MaxLogFiles = 120
)

$script:repo = $Repo
$script:sln = Join-Path $Repo $Solution
$script:msb = $MSBuildPath
$script:logDir = Join-Path $Repo "logs"
$script:watchExtensions = @(".vb", ".vbproj", ".resx", ".config")
$script:nextBuildAt = [DateTime]::MinValue
$script:buildQueued = $false
$script:isBuilding = $false
$script:lastChange = ""

if (-not (Test-Path $script:repo)) {
  throw "No existe el repositorio: $script:repo"
}
if (-not (Test-Path $script:sln)) {
  throw "No existe la solucion: $script:sln"
}
if (-not (Test-Path $script:msb)) {
  throw "No existe MSBuild: $script:msb"
}

New-Item -ItemType Directory -Force -Path $script:logDir | Out-Null

function Cleanup-OldLogs {
  if ($MaxLogFiles -le 0) { return }
  $items = Get-ChildItem -Path $script:logDir -Filter "build-*.txt" -File | Sort-Object LastWriteTime -Descending
  if ($items.Count -le $MaxLogFiles) { return }
  $items | Select-Object -Skip $MaxLogFiles | Remove-Item -Force
}

function Run-Build {
  if ($script:isBuilding) { return }
  $script:isBuilding = $true
  try {
    $ts = Get-Date -Format "yyyyMMdd-HHmmss"
    $log = Join-Path $script:logDir "build-$ts.txt"
    Write-Host ("[{0}] RUNNING build..." -f (Get-Date -Format "yyyy-MM-dd HH:mm:ss")) -ForegroundColor Cyan
    & $script:msb $script:sln /p:Configuration=$Configuration /p:Platform="$Platform" /nologo /v:m 2>&1 | Out-File -FilePath $log -Encoding utf8
    Copy-Item $log (Join-Path $script:logDir "build-latest.txt") -Force
    if ($LASTEXITCODE -eq 0) {
      "OK" | Set-Content (Join-Path $script:logDir "build-status.txt") -Encoding utf8
      Write-Host ("[{0}] OK   -> {1}" -f (Get-Date -Format "yyyy-MM-dd HH:mm:ss"), $log) -ForegroundColor Green
    }
    else {
      "FAIL" | Set-Content (Join-Path $script:logDir "build-status.txt") -Encoding utf8
      Write-Host ("[{0}] FAIL -> {1}" -f (Get-Date -Format "yyyy-MM-dd HH:mm:ss"), $log) -ForegroundColor Red
    }
    Cleanup-OldLogs
  }
  finally {
    $script:isBuilding = $false
  }
}

function Queue-Build {
  $script:nextBuildAt = (Get-Date).AddSeconds($DebounceSeconds)
  $script:buildQueued = $true
}

function Handle-Change {
  param([string]$Path)

  if ([string]::IsNullOrWhiteSpace($Path)) { return }
  $ext = [System.IO.Path]::GetExtension($Path)
  if ($script:watchExtensions -notcontains $ext) { return }
  $script:lastChange = $Path
  Queue-Build
}

Run-Build

Write-Host ("Watcher activo: {0} (debounce {1}s)." -f ($script:watchExtensions -join ", "), $DebounceSeconds) -ForegroundColor Yellow
Write-Host ("Build: Config={0} Platform={1}" -f $Configuration, $Platform) -ForegroundColor Yellow
Write-Host ("Logs: {0}" -f $script:logDir) -ForegroundColor Yellow

$fsw = New-Object IO.FileSystemWatcher $script:repo, "*.*"
$fsw.IncludeSubdirectories = $true
$fsw.EnableRaisingEvents = $true

Register-ObjectEvent $fsw Changed -Action { Handle-Change -Path $Event.SourceEventArgs.FullPath } | Out-Null
Register-ObjectEvent $fsw Created -Action { Handle-Change -Path $Event.SourceEventArgs.FullPath } | Out-Null
Register-ObjectEvent $fsw Deleted -Action { Handle-Change -Path $Event.SourceEventArgs.FullPath } | Out-Null
Register-ObjectEvent $fsw Renamed -Action { Handle-Change -Path $Event.SourceEventArgs.FullPath } | Out-Null

while ($true) {
  if ($script:buildQueued -and (Get-Date) -ge $script:nextBuildAt) {
    $script:buildQueued = $false
    if (-not [string]::IsNullOrWhiteSpace($script:lastChange)) {
      Write-Host ("[{0}] Cambio detectado: {1}" -f (Get-Date -Format "yyyy-MM-dd HH:mm:ss"), $script:lastChange) -ForegroundColor DarkYellow
    }
    Run-Build
  }
  Start-Sleep -Milliseconds 250
}
