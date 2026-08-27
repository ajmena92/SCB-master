param(
  [Parameter(Mandatory = $true)][string]$CustomerName,
  [Parameter(Mandatory = $true)][string]$SiteName,
  [Parameter(Mandatory = $true)][string]$ClientId,
  [Parameter(Mandatory = $true)][string]$DatabaseServer,
  [Parameter(Mandatory = $true)][string]$DatabaseName,
  [string]$Edition = "Standard",
  [string]$OutputPath = ""
)

$secret = "SCSC_ESCOLAR_2026"

function Normalize-Value([string]$Value) {
  if ([string]::IsNullOrWhiteSpace($Value)) {
    return ""
  }
  return $Value.Trim().ToUpperInvariant()
}

function Format-ActivationCode([string]$Raw) {
  $normalized = $Raw.Replace("-", "").Replace(" ", "").ToUpperInvariant()
  $groups = @()
  for ($i = 0; $i -lt $normalized.Length; $i += 4) {
    $length = [Math]::Min(4, $normalized.Length - $i)
    $groups += $normalized.Substring($i, $length)
  }
  return ($groups -join "-")
}

$raw = "{0}|{1}|{2}|{3}|{4}|{5}|{6}" -f `
  (Normalize-Value $CustomerName), `
  (Normalize-Value $SiteName), `
  (Normalize-Value $ClientId), `
  (Normalize-Value $DatabaseServer), `
  (Normalize-Value $DatabaseName), `
  (Normalize-Value $Edition), `
  $secret

$sha = [System.Security.Cryptography.SHA256]::Create()
$hashBytes = $sha.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($raw))
$base = [Convert]::ToBase64String($hashBytes).TrimEnd('=').Replace('+','-').Replace('/','_').ToUpperInvariant()
if ($base.Length -gt 24) {
  $base = $base.Substring(0, 24)
}
$activationCode = Format-ActivationCode $base

if (-not [string]::IsNullOrWhiteSpace($OutputPath)) {
  Set-Content -Path $OutputPath -Value $activationCode -Encoding UTF8
}

$activationCode
