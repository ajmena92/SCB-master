param(
  [Parameter(Mandatory = $true)][string]$PrivateKeyPath,
  [Parameter(Mandatory = $true)][string]$CustomerName,
  [Parameter(Mandatory = $true)][string]$SiteName,
  [Parameter(Mandatory = $true)][string]$ClientId,
  [Parameter(Mandatory = $true)][string]$LicensedTo,
  [Parameter(Mandatory = $true)][string]$HardwareFingerprint,
  [string]$Edition = "Standard",
  [Nullable[int]]$MaxSeats = $null,
  [Nullable[datetime]]$ExpiryDateUtc = $null,
  [string]$OutputPath = ""
)

if (-not (Test-Path $PrivateKeyPath)) {
  throw "No existe la llave privada: $PrivateKeyPath"
}

$privateKeyXml = Get-Content $PrivateKeyPath -Raw
$payload = [ordered]@{
  CustomerName = $CustomerName.Trim()
  SiteName = $SiteName.Trim()
  ClientId = $ClientId.Trim()
  LicensedTo = $LicensedTo.Trim()
  HardwareFingerprint = $HardwareFingerprint.Trim()
  IssueDateUtc = [DateTime]::UtcNow.ToString("o")
  ExpiryDateUtc = if ($ExpiryDateUtc) { $ExpiryDateUtc.Value.ToUniversalTime().ToString("o") } else { $null }
  Edition = $Edition.Trim()
  MaxSeats = if ($MaxSeats) { $MaxSeats.Value } else { $null }
}

$payloadJson = $payload | ConvertTo-Json -Compress -Depth 4
$payloadBytes = [System.Text.Encoding]::UTF8.GetBytes($payloadJson)

$rsa = New-Object System.Security.Cryptography.RSACryptoServiceProvider
$rsa.FromXmlString($privateKeyXml)
$signatureBytes = $rsa.SignData($payloadBytes, [System.Security.Cryptography.CryptoConfig]::MapNameToOID("SHA256"))

$envelope = [ordered]@{
  PayloadBase64 = [Convert]::ToBase64String($payloadBytes)
  SignatureBase64 = [Convert]::ToBase64String($signatureBytes)
}

$envelopeJson = $envelope | ConvertTo-Json -Compress -Depth 3
$activationCode = [Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($envelopeJson))

if (-not [string]::IsNullOrWhiteSpace($OutputPath)) {
  Set-Content -Path $OutputPath -Value $activationCode -Encoding UTF8
}

$activationCode
