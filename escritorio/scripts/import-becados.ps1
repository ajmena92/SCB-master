#requires -Version 5.1
<#
.SYNOPSIS
  Limpia BECADOS.xlsx y activa estudiantes como becados completos.

.DESCRIPTION
  Script one-shot para:
  - leer y limpiar `Lista inicial\BECADOS.xlsx`
  - o reutilizar un CSV limpio ya generado
  - exportar CSV limpio y conflictos
  - opcionalmente quedarse en modo solo limpieza (`-SkipDatabase`)
  - activar en `Usuario.TipoBeca` a los estudiantes encontrados como becados completos
  - opcionalmente reiniciar registros, recargas y becas existentes

  Este script no modifica `IdRuta` ni `PendienteBecaTransporte`.
#>
<#
Uso rapido:
  powershell -ExecutionPolicy Bypass -File "C:\Dev\SCB-master\escritorio\scripts\import-becados.ps1" -SkipDatabase

  powershell -ExecutionPolicy Bypass -File "C:\Dev\SCB-master\escritorio\scripts\import-becados.ps1" `
    -ResetRegistros `
    -ResetRecargas `
    -ResetBecasEstudiantes
#>

param(
  [string]$Repo = "C:\Dev\SCB-master",
  [string]$ExcelPath = "",
  [string]$CsvPath = "",
  [string]$ConnectionString = "",
  [string]$DeploymentConfigPath = "",
  [string]$AppConfigPath = "",
  [Alias("ConnectionKey")]
  [string]$ConnectionName = "Conexion",
  [Nullable[int]]$BecaCompletaId = 2,
  [Nullable[int]]$SinBecaId = 1,
  [string]$BecaCompletaDescripcion = "COMPLETA",
  [string]$SinBecaDescripcion = "NO BENEFICIARIO",
  [string]$ControlCarnetPrefix = "",
  [string]$OutputDirectory = "",
  [switch]$SkipDatabase,
  [switch]$ResetRegistros,
  [switch]$ResetRecargas,
  [Alias("SincronizarListadoCompleto")]
  [switch]$ResetBecasEstudiantes,
  [switch]$AllowDuplicateCedula,
  [switch]$AllowMissingUsers
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
    if (-not [string]::IsNullOrWhiteSpace($candidate) -and (Test-Path $candidate)) {
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

function Remove-Diacritics {
  param(
    [AllowNull()]
    [string]$Value
  )

  if ([string]::IsNullOrWhiteSpace($Value)) {
    return ""
  }

  $normalized = $Value.Normalize([System.Text.NormalizationForm]::FormD)
  $builder = New-Object System.Text.StringBuilder
  foreach ($ch in $normalized.ToCharArray()) {
    if ([Globalization.CharUnicodeInfo]::GetUnicodeCategory($ch) -ne [Globalization.UnicodeCategory]::NonSpacingMark) {
      [void]$builder.Append($ch)
    }
  }

  return $builder.ToString().Normalize([System.Text.NormalizationForm]::FormC)
}

function Normalize-Whitespace {
  param(
    [AllowNull()]
    [string]$Value
  )

  if ([string]::IsNullOrWhiteSpace($Value)) {
    return ""
  }

  return ([regex]::Replace($Value.Trim(), "\s+", " ")).Trim()
}

function Normalize-ComparisonText {
  param(
    [AllowNull()]
    [string]$Value
  )

  $clean = Remove-Diacritics (Normalize-Whitespace $Value)
  if ([string]::IsNullOrWhiteSpace($clean)) {
    return ""
  }

  return ([regex]::Replace($clean.ToUpperInvariant(), "[^A-Z0-9]", "")).Trim()
}

function Normalize-DisplayText {
  param(
    [AllowNull()]
    [string]$Value
  )

  $clean = Normalize-Whitespace $Value
  if ([string]::IsNullOrWhiteSpace($clean)) {
    return ""
  }

  return $clean.ToUpperInvariant()
}

function Normalize-Cedula {
  param(
    [AllowNull()]
    [string]$Value,
    [string]$PrefixToStrip = ""
  )

  $clean = Normalize-Whitespace $Value
  if ([string]::IsNullOrWhiteSpace($clean)) {
    return ""
  }

  if (-not [string]::IsNullOrWhiteSpace($PrefixToStrip)) {
    $escapedPrefix = [regex]::Escape($PrefixToStrip)
    $clean = [regex]::Replace($clean, $escapedPrefix, "", [Text.RegularExpressions.RegexOptions]::IgnoreCase)
  }

  $clean = [regex]::Replace($clean, "CTPP", "", [Text.RegularExpressions.RegexOptions]::IgnoreCase)
  $normalized = ([regex]::Replace($clean.ToUpperInvariant(), "[^A-Z0-9]", "")).Trim()
  if ($normalized -match '^[0-9]{10}$' -and $normalized.StartsWith("0", [System.StringComparison]::Ordinal)) {
    $normalized = $normalized.Substring(1)
  }

  return $normalized
}

function Join-NonEmptyValues {
  param(
    [Parameter(Mandatory = $true)]
    [AllowNull()]
    [AllowEmptyString()]
    [string[]]$Values
  )

  $items = New-Object System.Collections.Generic.List[string]
  foreach ($item in $Values) {
    if (-not [string]::IsNullOrWhiteSpace($item)) {
      [void]$items.Add((Normalize-Whitespace $item))
    }
  }

  return (($items.ToArray()) -join " ").Trim().ToUpperInvariant()
}

function Get-UniqueColumnName {
  param(
    [Parameter(Mandatory = $true)]
    [System.Data.DataTable]$Table,
    [Parameter(Mandatory = $true)]
    [string]$BaseName
  )

  $resolvedBase = $BaseName
  if ([string]::IsNullOrWhiteSpace($resolvedBase)) {
    $resolvedBase = "Columna"
  }

  $candidate = $resolvedBase
  $suffix = 2
  while ($Table.Columns.Contains($candidate)) {
    $candidate = "{0}_{1}" -f $resolvedBase, $suffix
    $suffix += 1
  }

  return $candidate
}

function Get-CellColumnIndex {
  param(
    [AllowNull()]
    [string]$CellReference
  )

  if ([string]::IsNullOrWhiteSpace($CellReference)) {
    return -1
  }

  $accumulator = 0
  foreach ($ch in $CellReference.ToUpperInvariant().ToCharArray()) {
    if ($ch -lt [char]"A" -or $ch -gt [char]"Z") {
      break
    }

    $accumulator = ($accumulator * 26) + ([int][char]$ch - [int][char]"A" + 1)
  }

  if ($accumulator -le 0) {
    return -1
  }

  return $accumulator - 1
}

function Get-ZipEntryText {
  param(
    [Parameter(Mandatory = $true)]
    [System.IO.Compression.ZipArchive]$Archive,
    [Parameter(Mandatory = $true)]
    [string]$EntryName
  )

  $entry = $Archive.GetEntry($EntryName)
  if ($null -eq $entry) {
    return $null
  }

  $stream = $null
  $reader = $null
  try {
    $stream = $entry.Open()
    $reader = New-Object System.IO.StreamReader($stream)
    return $reader.ReadToEnd()
  }
  finally {
    if ($null -ne $reader) {
      $reader.Dispose()
    }
    elseif ($null -ne $stream) {
      $stream.Dispose()
    }
  }
}

function New-SpreadsheetNamespaceManager {
  param(
    [Parameter(Mandatory = $true)]
    [System.Xml.XmlDocument]$Document
  )

  $manager = New-Object System.Xml.XmlNamespaceManager($Document.NameTable)
  [void]$manager.AddNamespace("x", "http://schemas.openxmlformats.org/spreadsheetml/2006/main")
  [void]$manager.AddNamespace("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships")
  [void]$manager.AddNamespace("pr", "http://schemas.openxmlformats.org/package/2006/relationships")
  return $manager
}

function Resolve-XmlNamespaceManager {
  param(
    [AllowNull()]
    [object]$NamespaceManager
  )

  if ($null -eq $NamespaceManager) {
    return $null
  }

  if ($NamespaceManager -is [System.Xml.XmlNamespaceManager]) {
    return $NamespaceManager
  }

  if (($NamespaceManager -is [System.Collections.IEnumerable]) -and -not ($NamespaceManager -is [string])) {
    foreach ($candidate in $NamespaceManager) {
      $resolvedCandidate = Resolve-XmlNamespaceManager -NamespaceManager $candidate
      if ($null -ne $resolvedCandidate) {
        return $resolvedCandidate
      }
    }
  }

  return $null
}

function Get-XmlDocumentFromNode {
  param(
    [Parameter(Mandatory = $true)]
    [System.Xml.XmlNode]$Node
  )

  if ($Node -is [System.Xml.XmlDocument]) {
    return $Node
  }

  if ($null -ne $Node.OwnerDocument) {
    return $Node.OwnerDocument
  }

  throw "No se pudo resolver el documento XML asociado al nodo."
}

function Get-ResolvedSpreadsheetNamespaceManager {
  param(
    [Parameter(Mandatory = $true)]
    [System.Xml.XmlNode]$Node,
    [AllowNull()]
    [object]$NamespaceManager
  )

  $resolvedNamespaceManager = Resolve-XmlNamespaceManager -NamespaceManager $NamespaceManager
  if ($null -ne $resolvedNamespaceManager) {
    return $resolvedNamespaceManager
  }

  $document = Get-XmlDocumentFromNode -Node $Node
  return New-SpreadsheetNamespaceManager -Document $document
}

function Select-XmlSingleNode {
  param(
    [Parameter(Mandatory = $true)]
    [System.Xml.XmlNode]$Node,
    [Parameter(Mandatory = $true)]
    [string]$XPath,
    [AllowNull()]
    [object]$NamespaceManager
  )

  if ($null -eq $NamespaceManager) {
    return $Node.PSBase.SelectSingleNode($XPath)
  }

  $resolvedNamespaceManager = Get-ResolvedSpreadsheetNamespaceManager -Node $Node -NamespaceManager $NamespaceManager
  return $Node.PSBase.SelectSingleNode($XPath, $resolvedNamespaceManager)
}

function Select-XmlNodes {
  param(
    [Parameter(Mandatory = $true)]
    [System.Xml.XmlNode]$Node,
    [Parameter(Mandatory = $true)]
    [string]$XPath,
    [AllowNull()]
    [object]$NamespaceManager
  )

  if ($null -eq $NamespaceManager) {
    return $Node.PSBase.SelectNodes($XPath)
  }

  $resolvedNamespaceManager = Get-ResolvedSpreadsheetNamespaceManager -Node $Node -NamespaceManager $NamespaceManager
  return $Node.PSBase.SelectNodes($XPath, $resolvedNamespaceManager)
}

function Resolve-SharedStringList {
  param(
    [AllowNull()]
    [AllowEmptyString()]
    [object]$SharedStrings
  )

  $resolvedList = New-Object 'System.Collections.Generic.List[string]'
  if ($null -eq $SharedStrings) {
    return $resolvedList
  }

  if ($SharedStrings -is [System.Collections.Generic.List[string]]) {
    return $SharedStrings
  }

  if (($SharedStrings -is [string]) -and [string]::IsNullOrWhiteSpace($SharedStrings)) {
    return $resolvedList
  }

  if (($SharedStrings -is [System.Collections.IEnumerable]) -and -not ($SharedStrings -is [string])) {
    foreach ($item in $SharedStrings) {
      if ($null -eq $item) {
        continue
      }

      [void]$resolvedList.Add([string]$item)
    }

    return $resolvedList
  }

  [void]$resolvedList.Add([string]$SharedStrings)
  return $resolvedList
}

function Get-WorksheetCellValue {
  param(
    [Parameter(Mandatory = $true)]
    [System.Xml.XmlElement]$CellNode,
    [AllowNull()]
    [object]$NamespaceManager,
    [AllowNull()]
    [AllowEmptyString()]
    [object]$SharedStrings
  )

  $resolvedSharedStrings = Resolve-SharedStringList -SharedStrings $SharedStrings
  $valueNode = Select-XmlSingleNode -Node $CellNode -XPath "./*[local-name()='v']" -NamespaceManager $null
  $baseValue = ""
  if ($null -ne $valueNode) {
    $baseValue = Normalize-Whitespace ([string]$valueNode.InnerText)
  }

  $dataType = Normalize-Whitespace ([string]$CellNode.GetAttribute("t"))
  switch ($dataType) {
    "s" {
      $index = 0
      if ([int]::TryParse($baseValue, [ref]$index) -and $index -ge 0 -and $index -lt $resolvedSharedStrings.Count) {
        return ($resolvedSharedStrings[$index]).Trim()
      }
    }
    "b" {
      if ($baseValue -eq "1") {
        return "TRUE"
      }
      return "FALSE"
    }
    "inlineStr" {
      $inlineNode = Select-XmlSingleNode -Node $CellNode -XPath "./*[local-name()='is']/*[local-name()='t']" -NamespaceManager $null
      if ($null -ne $inlineNode) {
        return Normalize-Whitespace ([string]$inlineNode.InnerText)
      }
    }
  }

  return $baseValue
}

function Read-ExcelFirstSheetOpenXml {
  param(
    [Parameter(Mandatory = $true)]
    [string]$PathValue
  )

  Add-Type -AssemblyName System.IO.Compression.FileSystem
  $table = New-Object System.Data.DataTable "BecadosRaw"
  $archive = [System.IO.Compression.ZipFile]::OpenRead($PathValue)

  try {
    $workbookText = Get-ZipEntryText -Archive $archive -EntryName "xl/workbook.xml"
    if ([string]::IsNullOrWhiteSpace($workbookText)) {
      throw "El archivo Excel no contiene libro valido."
    }

    [xml]$workbookXml = $workbookText
    $sheet = Select-XmlSingleNode -Node $workbookXml -XPath "/*[local-name()='workbook']/*[local-name()='sheets']/*[local-name()='sheet'][1]" -NamespaceManager $null
    if ($null -eq $sheet) {
      throw "No se encontro una hoja valida para importar."
    }

    $relationshipId = Normalize-Whitespace ([string]$sheet.GetAttribute("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships"))
    if ([string]::IsNullOrWhiteSpace($relationshipId)) {
      throw "No se pudo resolver la relacion de la hoja seleccionada."
    }

    $relsText = Get-ZipEntryText -Archive $archive -EntryName "xl/_rels/workbook.xml.rels"
    if ([string]::IsNullOrWhiteSpace($relsText)) {
      throw "No se pudo resolver la ruta fisica de la hoja seleccionada."
    }

    [xml]$relsXml = $relsText
    $relationshipNode = Select-XmlSingleNode -Node $relsXml -XPath "/*[local-name()='Relationships']/*[local-name()='Relationship'][@Id='$relationshipId']" -NamespaceManager $null
    if ($null -eq $relationshipNode) {
      throw "No se encontro la definicion de la hoja seleccionada."
    }

    $target = Normalize-Whitespace ([string]$relationshipNode.GetAttribute("Target"))
    if ([string]::IsNullOrWhiteSpace($target)) {
      throw "No se pudo abrir la hoja seleccionada."
    }

    $sheetEntryName = $target
    if (-not $sheetEntryName.StartsWith("xl/", [System.StringComparison]::OrdinalIgnoreCase)) {
      $sheetEntryName = "xl/{0}" -f $sheetEntryName.TrimStart([char]"/")
    }

    $sheetText = Get-ZipEntryText -Archive $archive -EntryName $sheetEntryName
    if ([string]::IsNullOrWhiteSpace($sheetText)) {
      throw "La hoja seleccionada no contiene datos."
    }

    [xml]$sheetXml = $sheetText
    $rows = Select-XmlNodes -Node $sheetXml -XPath "/*[local-name()='worksheet']/*[local-name()='sheetData']/*[local-name()='row']" -NamespaceManager $null
    if ($null -eq $rows -or $rows.Count -eq 0) {
      throw "La hoja seleccionada no contiene datos."
    }

    $sharedStrings = New-Object System.Collections.Generic.List[string]
    $sharedStringsText = Get-ZipEntryText -Archive $archive -EntryName "xl/sharedStrings.xml"
    if (-not [string]::IsNullOrWhiteSpace($sharedStringsText)) {
      [xml]$sharedStringsXml = $sharedStringsText
      foreach ($item in (Select-XmlNodes -Node $sharedStringsXml -XPath "/*[local-name()='sst']/*[local-name()='si']" -NamespaceManager $null)) {
        [void]$sharedStrings.Add((Normalize-Whitespace ([string]$item.InnerText)))
      }
    }
    $headerMap = @{}

    foreach ($cell in (Select-XmlNodes -Node $rows.Item(0) -XPath "./*[local-name()='c']" -NamespaceManager $null)) {
      $columnIndex = Get-CellColumnIndex -CellReference ([string]$cell.GetAttribute("r"))
      if ($columnIndex -lt 0) {
        continue
      }

      $headerMap[$columnIndex] = Get-WorksheetCellValue -CellNode $cell -NamespaceManager $null -SharedStrings $sharedStrings
    }

    if ($headerMap.Count -eq 0) {
      throw "No se pudieron leer encabezados en el Excel."
    }

    $maxColumnIndex = ($headerMap.Keys | Measure-Object -Maximum).Maximum
    for ($i = 0; $i -le $maxColumnIndex; $i += 1) {
      $headerName = ""
      if ($headerMap.ContainsKey($i)) {
        $headerName = Normalize-Whitespace ([string]$headerMap[$i])
      }

      if ([string]::IsNullOrWhiteSpace($headerName)) {
        $headerName = "Columna{0}" -f ($i + 1)
      }

      $resolvedColumnName = Get-UniqueColumnName -Table $table -BaseName $headerName
      [void]$table.Columns.Add($resolvedColumnName, [string])
    }

    for ($rowIndex = 1; $rowIndex -lt $rows.Count; $rowIndex += 1) {
      $valuesByIndex = @{}
      foreach ($cell in (Select-XmlNodes -Node $rows.Item($rowIndex) -XPath "./*[local-name()='c']" -NamespaceManager $null)) {
        $columnIndex = Get-CellColumnIndex -CellReference ([string]$cell.GetAttribute("r"))
        if ($columnIndex -lt 0) {
          continue
        }

        $valuesByIndex[$columnIndex] = Get-WorksheetCellValue -CellNode $cell -NamespaceManager $null -SharedStrings $sharedStrings
      }

      $newRow = $table.NewRow()
      for ($columnIndex = 0; $columnIndex -lt $table.Columns.Count; $columnIndex += 1) {
        if ($valuesByIndex.ContainsKey($columnIndex)) {
          $newRow[$columnIndex] = [string]$valuesByIndex[$columnIndex]
        }
        else {
          $newRow[$columnIndex] = ""
        }
      }

      [void]$table.Rows.Add($newRow)
    }

    return ,$table
  }
  finally {
    if ($null -ne $archive) {
      $archive.Dispose()
    }
  }
}

function Resolve-ColumnIndex {
  param(
    [Parameter(Mandatory = $true)]
    [System.Data.DataTable]$Table,
    [Parameter(Mandatory = $true)]
    [int[]]$FallbackIndexes,
    [Parameter(Mandatory = $true)]
    [string[]]$CandidateNames
  )

  $targets = New-Object System.Collections.Generic.HashSet[string]
  foreach ($candidate in $CandidateNames) {
    [void]$targets.Add((Normalize-ComparisonText $candidate))
  }

  for ($index = 0; $index -lt $Table.Columns.Count; $index += 1) {
    $normalizedColumn = Normalize-ComparisonText ([string]$Table.Columns[$index].ColumnName)
    if ($targets.Contains($normalizedColumn)) {
      return $index
    }
  }

  foreach ($fallback in $FallbackIndexes) {
    if ($fallback -ge 0 -and $fallback -lt $Table.Columns.Count) {
      return $fallback
    }
  }

  return -1
}

function Get-RowValue {
  param(
    [Parameter(Mandatory = $true)]
    [System.Data.DataRow]$Row,
    [int]$ColumnIndex
  )

  if ($ColumnIndex -lt 0 -or $ColumnIndex -ge $Row.Table.Columns.Count) {
    return ""
  }

  return Normalize-Whitespace ([string]$Row[$ColumnIndex])
}

function New-BecadosCleanTable {
  $table = New-Object System.Data.DataTable "BecadosLimpio"
  [void]$table.Columns.Add("Numero", [string])
  [void]$table.Columns.Add("Cedula", [string])
  [void]$table.Columns.Add("PrimerApellido", [string])
  [void]$table.Columns.Add("SegundoApellido", [string])
  [void]$table.Columns.Add("PrimerNombre", [string])
  [void]$table.Columns.Add("SegundoNombre", [string])
  [void]$table.Columns.Add("NombreCompleto", [string])
  [void]$table.Columns.Add("Nivel", [string])
  [void]$table.Columns.Add("Modalidad", [string])
  [void]$table.Columns.Add("CodigoSolicitud", [string])
  [void]$table.Columns.Add("FilaExcel", [int])
  return ,$table
}

function New-BecadosConflictTable {
  $table = New-Object System.Data.DataTable "BecadosConflicto"
  [void]$table.Columns.Add("Cedula", [string])
  [void]$table.Columns.Add("FilaExcelExistente", [int])
  [void]$table.Columns.Add("FilaExcelDuplicada", [int])
  [void]$table.Columns.Add("NombreExistente", [string])
  [void]$table.Columns.Add("NombreDuplicado", [string])
  [void]$table.Columns.Add("PrimerApellidoExistente", [string])
  [void]$table.Columns.Add("PrimerApellidoDuplicado", [string])
  [void]$table.Columns.Add("SegundoApellidoExistente", [string])
  [void]$table.Columns.Add("SegundoApellidoDuplicado", [string])
  [void]$table.Columns.Add("NivelExistente", [string])
  [void]$table.Columns.Add("NivelDuplicado", [string])
  [void]$table.Columns.Add("ModalidadExistente", [string])
  [void]$table.Columns.Add("ModalidadDuplicada", [string])
  return ,$table
}

function Convert-BecadosExcelData {
  param(
    [Parameter(Mandatory = $true)]
    [System.Data.DataTable]$RawTable,
    [string]$ControlCarnetPrefix = ""
  )

  $idxNumero = Resolve-ColumnIndex -Table $RawTable -FallbackIndexes @(0) -CandidateNames @("N", "No", "Nro", "Numero")
  $idxCedula = Resolve-ColumnIndex -Table $RawTable -FallbackIndexes @(1) -CandidateNames @("Identificacion", "Cedula")
  $idxApellido1 = Resolve-ColumnIndex -Table $RawTable -FallbackIndexes @(2) -CandidateNames @("Apellido1", "Apellido 1", "PrimerApellido")
  $idxApellido2 = Resolve-ColumnIndex -Table $RawTable -FallbackIndexes @(3) -CandidateNames @("Apellido2", "Apellido 2", "SegundoApellido")
  $idxNombre1 = Resolve-ColumnIndex -Table $RawTable -FallbackIndexes @(4) -CandidateNames @("Nombre1", "Nombre 1", "PrimerNombre")
  $idxNombre2 = Resolve-ColumnIndex -Table $RawTable -FallbackIndexes @(5) -CandidateNames @("Nombre2", "Nombre 2", "SegundoNombre")
  $idxNivel = Resolve-ColumnIndex -Table $RawTable -FallbackIndexes @(6) -CandidateNames @("Nivel")
  $idxModalidad = Resolve-ColumnIndex -Table $RawTable -FallbackIndexes @(7) -CandidateNames @("Modalidad")
  $idxSolicitud = Resolve-ColumnIndex -Table $RawTable -FallbackIndexes @(8) -CandidateNames @("PresentoSolicitudDigite1", "Presento Solicitud Digite 1", "Solicitud")

  foreach ($requiredIndex in @($idxCedula, $idxApellido1, $idxNombre1, $idxNivel, $idxModalidad)) {
    if ($requiredIndex -lt 0) {
      throw "El archivo BECADOS.xlsx no tiene la estructura esperada."
    }
  }

  $cleanTable = New-BecadosCleanTable
  $conflictTable = New-BecadosConflictTable
  $metrics = [ordered]@{
    FilasOrigen = $RawTable.Rows.Count
    FilasLimpias = 0
    FilasVacias = 0
    FilasSinCedula = 0
    DuplicadosExactos = 0
    DuplicadosConflicto = 0
  }

  $seenByCedula = @{}

  for ($rowIndex = 0; $rowIndex -lt $RawTable.Rows.Count; $rowIndex += 1) {
    $row = $RawTable.Rows[$rowIndex]
    $numero = Normalize-Whitespace (Get-RowValue -Row $row -ColumnIndex $idxNumero)
    $cedula = Normalize-Cedula -Value (Get-RowValue -Row $row -ColumnIndex $idxCedula) -PrefixToStrip $ControlCarnetPrefix
    $primerApellido = Normalize-DisplayText (Get-RowValue -Row $row -ColumnIndex $idxApellido1)
    $segundoApellido = Normalize-DisplayText (Get-RowValue -Row $row -ColumnIndex $idxApellido2)
    $primerNombre = Normalize-DisplayText (Get-RowValue -Row $row -ColumnIndex $idxNombre1)
    $segundoNombre = Normalize-DisplayText (Get-RowValue -Row $row -ColumnIndex $idxNombre2)
    $nivel = Normalize-DisplayText (Get-RowValue -Row $row -ColumnIndex $idxNivel)
    $modalidad = Normalize-DisplayText (Get-RowValue -Row $row -ColumnIndex $idxModalidad)
    $codigoSolicitud = Normalize-DisplayText (Get-RowValue -Row $row -ColumnIndex $idxSolicitud)
    $filaExcel = $rowIndex + 2

    if ([string]::IsNullOrWhiteSpace($cedula) -and
        [string]::IsNullOrWhiteSpace($primerApellido) -and
        [string]::IsNullOrWhiteSpace($segundoApellido) -and
        [string]::IsNullOrWhiteSpace($primerNombre) -and
        [string]::IsNullOrWhiteSpace($segundoNombre) -and
        [string]::IsNullOrWhiteSpace($nivel) -and
        [string]::IsNullOrWhiteSpace($modalidad)) {
      $metrics.FilasVacias += 1
      continue
    }

    if ([string]::IsNullOrWhiteSpace($cedula)) {
      $metrics.FilasSinCedula += 1
      continue
    }

    $nombreCompleto = Join-NonEmptyValues -Values @($primerNombre, $segundoNombre)
    $signature = "{0}|{1}|{2}|{3}|{4}|{5}|{6}" -f $primerApellido, $segundoApellido, $primerNombre, $segundoNombre, $nivel, $modalidad, $codigoSolicitud

    if ($seenByCedula.ContainsKey($cedula)) {
      $existing = $seenByCedula[$cedula]
      if ([string]$existing.Signature -eq $signature) {
        $metrics.DuplicadosExactos += 1
        continue
      }

      $metrics.DuplicadosConflicto += 1
      $conflictRow = $conflictTable.NewRow()
      $conflictRow["Cedula"] = $cedula
      $conflictRow["FilaExcelExistente"] = [int]$existing.FilaExcel
      $conflictRow["FilaExcelDuplicada"] = [int]$filaExcel
      $conflictRow["NombreExistente"] = [string]$existing.NombreCompleto
      $conflictRow["NombreDuplicado"] = $nombreCompleto
      $conflictRow["PrimerApellidoExistente"] = [string]$existing.PrimerApellido
      $conflictRow["PrimerApellidoDuplicado"] = $primerApellido
      $conflictRow["SegundoApellidoExistente"] = [string]$existing.SegundoApellido
      $conflictRow["SegundoApellidoDuplicado"] = $segundoApellido
      $conflictRow["NivelExistente"] = [string]$existing.Nivel
      $conflictRow["NivelDuplicado"] = $nivel
      $conflictRow["ModalidadExistente"] = [string]$existing.Modalidad
      $conflictRow["ModalidadDuplicada"] = $modalidad
      [void]$conflictTable.Rows.Add($conflictRow)
      continue
    }

    $newRow = $cleanTable.NewRow()
    $newRow["Numero"] = $numero
    $newRow["Cedula"] = $cedula
    $newRow["PrimerApellido"] = $primerApellido
    $newRow["SegundoApellido"] = $segundoApellido
    $newRow["PrimerNombre"] = $primerNombre
    $newRow["SegundoNombre"] = $segundoNombre
    $newRow["NombreCompleto"] = $nombreCompleto
    $newRow["Nivel"] = $nivel
    $newRow["Modalidad"] = $modalidad
    $newRow["CodigoSolicitud"] = $codigoSolicitud
    $newRow["FilaExcel"] = $filaExcel
    [void]$cleanTable.Rows.Add($newRow)

    $seenByCedula[$cedula] = [pscustomobject]@{
      Signature = $signature
      FilaExcel = $filaExcel
      NombreCompleto = $nombreCompleto
      PrimerApellido = $primerApellido
      SegundoApellido = $segundoApellido
      Nivel = $nivel
      Modalidad = $modalidad
    }
    $metrics.FilasLimpias += 1
  }

  return [pscustomobject]@{
    CleanTable = $cleanTable
    ConflictTable = $conflictTable
    Metrics = [pscustomobject]$metrics
  }
}

function Export-DataTableCsv {
  param(
    [Parameter(Mandatory = $true)]
    [System.Data.DataTable]$Table,
    [Parameter(Mandatory = $true)]
    [string]$PathValue
  )

  if ($null -eq $Table -or $Table.Rows.Count -eq 0) {
    return
  }

  $rows = foreach ($row in $Table.Rows) {
    $item = [ordered]@{}
    foreach ($column in $Table.Columns) {
      $item[[string]$column.ColumnName] = $row[[string]$column.ColumnName]
    }
    [pscustomobject]$item
  }

  $rows | Export-Csv -Path $PathValue -NoTypeInformation -Encoding UTF8
}

function Read-CsvDataTable {
  param(
    [Parameter(Mandatory = $true)]
    [string]$PathValue
  )

  Assert-PathExists -PathValue $PathValue -Label "archivo CSV de becados"
  $items = @(
    Import-Csv -Path $PathValue -Encoding UTF8
  )

  $table = New-Object System.Data.DataTable "BecadosRaw"
  if ($items.Count -eq 0) {
    return ,$table
  }

  $propertyNames = @(
    $items[0].PSObject.Properties |
      Where-Object { $_.MemberType -eq "NoteProperty" -or $_.MemberType -eq "Property" } |
      ForEach-Object { [string]$_.Name }
  )

  foreach ($propertyName in $propertyNames) {
    $resolvedColumnName = Get-UniqueColumnName -Table $table -BaseName $propertyName
    [void]$table.Columns.Add($resolvedColumnName, [string])
  }

  foreach ($item in $items) {
    $row = $table.NewRow()
    for ($columnIndex = 0; $columnIndex -lt $table.Columns.Count; $columnIndex += 1) {
      $columnName = [string]$table.Columns[$columnIndex].ColumnName
      $property = $item.PSObject.Properties[$columnName]
      if ($null -eq $property) {
        $row[$columnIndex] = ""
      }
      else {
        $row[$columnIndex] = Normalize-Whitespace ([string]$property.Value)
      }
    }
    [void]$table.Rows.Add($row)
  }

  return ,$table
}

function Resolve-PlaceholderValue {
  param(
    [AllowNull()]
    [string]$Value
  )

  if ([string]::IsNullOrWhiteSpace($Value)) {
    return ""
  }

  $prefix = "__SET_IN_ENV__:"
  if ($Value.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase)) {
    $envName = $Value.Substring($prefix.Length).Trim()
    return [string]([Environment]::GetEnvironmentVariable($envName))
  }

  return $Value
}

function Get-DefaultDeploymentConfigPath {
  $commonAppData = [Environment]::GetFolderPath([Environment+SpecialFolder]::CommonApplicationData)
  return Join-Path $commonAppData "SCSC\deployment.config.json"
}

function Get-DeploymentConfigEntropyBytes {
  return [System.Text.Encoding]::UTF8.GetBytes("SCSC_DEPLOYMENT_CONFIG_V1")
}

function Get-DeploymentConfigPassword {
  param(
    [AllowNull()]
    [string]$EncryptedPassword
  )

  if ([string]::IsNullOrWhiteSpace($EncryptedPassword)) {
    return ""
  }

  try {
    $protectedBytes = [Convert]::FromBase64String($EncryptedPassword)
    $clearBytes = [System.Security.Cryptography.ProtectedData]::Unprotect(
      $protectedBytes,
      (Get-DeploymentConfigEntropyBytes),
      [System.Security.Cryptography.DataProtectionScope]::LocalMachine
    )
    return [System.Text.Encoding]::UTF8.GetString($clearBytes)
  }
  catch {
    throw "No se pudo descifrar la clave SQL desde deployment.config.json: $($_.Exception.Message)"
  }
}

function Resolve-InstalledConnectionString {
  param(
    [Parameter(Mandatory = $true)]
    [string]$PathValue
  )

  Assert-PathExists -PathValue $PathValue -Label "deployment.config.json"
  $json = Get-Content -Path $PathValue -Raw
  $config = $json | ConvertFrom-Json
  if ($null -eq $config) {
    throw "El archivo deployment.config.json esta vacio o invalido."
  }

  $server = Normalize-Whitespace ([string]$config.Server)
  $database = Normalize-Whitespace ([string]$config.Database)
  $authMode = Normalize-Whitespace ([string]$config.AuthenticationMode)
  $userName = Normalize-Whitespace ([string]$config.UserName)

  if ([string]::IsNullOrWhiteSpace($server)) {
    $server = "."
  }

  if ([string]::IsNullOrWhiteSpace($database)) {
    $database = "SCSC"
  }

  $useIntegratedSecurity = [string]::Equals($authMode, "Windows", [System.StringComparison]::OrdinalIgnoreCase)
  $builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
  $builder.DataSource = $server
  $builder.InitialCatalog = $database
  $builder.IntegratedSecurity = $useIntegratedSecurity
  $builder.MultipleActiveResultSets = $false
  $builder.ConnectTimeout = 15
  $builder.ApplicationName = "SCSC"

  if (-not $useIntegratedSecurity) {
    $builder.UserID = $userName
    $builder.Password = Get-DeploymentConfigPassword -EncryptedPassword ([string]$config.EncryptedPassword)
  }

  return $builder.ConnectionString
}

function Resolve-ConnectionSettings {
  param(
    [Parameter(Mandatory = $true)]
    [string]$RepoPath,
    [AllowNull()]
    [string]$ExplicitConnectionString,
    [AllowNull()]
    [string]$InstalledConfigPath,
    [AllowNull()]
    [string]$ConfigPath,
    [Parameter(Mandatory = $true)]
    [string]$ConfigConnectionName
  )

  if (-not [string]::IsNullOrWhiteSpace($ExplicitConnectionString)) {
    return [pscustomobject]@{
      ConnectionString = $ExplicitConnectionString
      Source = "cadena explicita"
    }
  }

  $resolvedInstalledConfigPath = $InstalledConfigPath
  $installedPathExplicit = -not [string]::IsNullOrWhiteSpace($resolvedInstalledConfigPath)
  if ([string]::IsNullOrWhiteSpace($resolvedInstalledConfigPath)) {
    $resolvedInstalledConfigPath = Get-DefaultDeploymentConfigPath
  }

  if ($installedPathExplicit -or (Test-Path $resolvedInstalledConfigPath)) {
    $installedConnectionString = Resolve-InstalledConnectionString -PathValue $resolvedInstalledConfigPath
    if (-not [string]::IsNullOrWhiteSpace($installedConnectionString)) {
      return [pscustomobject]@{
        ConnectionString = $installedConnectionString
        Source = "deployment.config.json ($resolvedInstalledConfigPath)"
      }
    }
  }

  $resolvedConfigPath = $ConfigPath
  if ([string]::IsNullOrWhiteSpace($resolvedConfigPath)) {
    $resolvedConfigPath = Join-Path $RepoPath "escritorio\SCSC\app.config"
  }

  Assert-PathExists -PathValue $resolvedConfigPath -Label "app.config"
  [xml]$config = Get-Content -Path $resolvedConfigPath -Raw

  $xpathConnectionString = "/configuration/connectionStrings/add[@name='{0}']" -f $ConfigConnectionName
  $connectionNode = $config.SelectSingleNode($xpathConnectionString)
  if ($null -ne $connectionNode) {
    $connectionValue = Resolve-PlaceholderValue ([string]$connectionNode.connectionString)
    if (-not [string]::IsNullOrWhiteSpace($connectionValue)) {
      return [pscustomobject]@{
        ConnectionString = $connectionValue.Trim()
        Source = "app.config connectionStrings '$ConfigConnectionName' ($resolvedConfigPath)"
      }
    }
  }

  $xpathAppSetting = "/configuration/appSettings/add[@key='{0}']" -f $ConfigConnectionName
  $appSettingNode = $config.SelectSingleNode($xpathAppSetting)
  if ($null -ne $appSettingNode) {
    $appSettingValue = Resolve-PlaceholderValue ([string]$appSettingNode.value)
    if (-not [string]::IsNullOrWhiteSpace($appSettingValue)) {
      return [pscustomobject]@{
        ConnectionString = $appSettingValue.Trim()
        Source = "app.config appSettings '$ConfigConnectionName' ($resolvedConfigPath)"
      }
    }
  }

  throw "No se encontro la cadena de conexion '$ConfigConnectionName' en $resolvedConfigPath (ni en connectionStrings ni en appSettings)."
}

function Add-SqlParameters {
  param(
    [Parameter(Mandatory = $true)]
    [System.Data.SqlClient.SqlCommand]$Command,
    [hashtable]$Parameters
  )

  if ($null -eq $Parameters) {
    return
  }

  foreach ($key in $Parameters.Keys) {
    $parameterName = [string]$key
    if (-not $parameterName.StartsWith("@")) {
      $parameterName = "@{0}" -f $parameterName
    }

    $parameterValue = $Parameters[$key]
    if ($null -eq $parameterValue) {
      $parameterValue = [DBNull]::Value
    }

    [void]$Command.Parameters.AddWithValue($parameterName, $parameterValue)
  }
}

function Invoke-SqlDataTable {
  param(
    [Parameter(Mandatory = $true)]
    [System.Data.SqlClient.SqlConnection]$Connection,
    [Parameter(Mandatory = $true)]
    [string]$CommandText,
    [hashtable]$Parameters,
    [System.Data.SqlClient.SqlTransaction]$Transaction
  )

  $command = $Connection.CreateCommand()
  $adapter = $null
  try {
    $command.CommandText = $CommandText
    $command.CommandTimeout = 0
    if ($null -ne $Transaction) {
      $command.Transaction = $Transaction
    }

    Add-SqlParameters -Command $command -Parameters $Parameters
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($command)
    $table = New-Object System.Data.DataTable
    [void]$adapter.Fill($table)
    return ,$table
  }
  finally {
    if ($null -ne $adapter) {
      $adapter.Dispose()
    }
    if ($null -ne $command) {
      $command.Dispose()
    }
  }
}

function Invoke-SqlNonQuery {
  param(
    [Parameter(Mandatory = $true)]
    [System.Data.SqlClient.SqlConnection]$Connection,
    [Parameter(Mandatory = $true)]
    [string]$CommandText,
    [hashtable]$Parameters,
    [System.Data.SqlClient.SqlTransaction]$Transaction
  )

  $command = $Connection.CreateCommand()
  try {
    $command.CommandText = $CommandText
    $command.CommandTimeout = 0
    if ($null -ne $Transaction) {
      $command.Transaction = $Transaction
    }

    Add-SqlParameters -Command $command -Parameters $Parameters
    return $command.ExecuteNonQuery()
  }
  finally {
    if ($null -ne $command) {
      $command.Dispose()
    }
  }
}

function Resolve-BecaId {
  param(
    [Parameter(Mandatory = $true)]
    [System.Data.SqlClient.SqlConnection]$Connection,
    [Nullable[int]]$Id,
    [string]$Description,
    [switch]$AllowFallbackToIdOne
  )

  $catalog = Invoke-SqlDataTable -Connection $Connection -CommandText @"
SELECT IdBeca, Descripcion, ISNULL(DiasBeca, '') AS DiasBeca, ISNULL(Activo, 1) AS Activo
FROM TipoBeca;
"@ -Parameters $null -Transaction $null

  if ($Id.HasValue) {
    $idRows = @($catalog.Select("IdBeca = $($Id.Value)"))
    if ($idRows.Count -ne 1) {
      throw "No se encontro TipoBeca con IdBeca=$($Id.Value)."
    }
    return [int]$idRows[0]["IdBeca"]
  }

  $normalizedDescription = Normalize-ComparisonText $Description
  if (-not [string]::IsNullOrWhiteSpace($normalizedDescription)) {
    $exactMatches = New-Object System.Collections.Generic.List[System.Data.DataRow]
    foreach ($row in $catalog.Rows) {
      if (-not [bool]$row["Activo"]) {
        continue
      }

      if ((Normalize-ComparisonText ([string]$row["Descripcion"])) -eq $normalizedDescription) {
        [void]$exactMatches.Add($row)
      }
    }

    if ($exactMatches.Count -eq 1) {
      return [int]$exactMatches[0]["IdBeca"]
    }

    if ($exactMatches.Count -gt 1) {
      throw "La descripcion de beca '$Description' coincide con mas de un registro activo en TipoBeca."
    }
  }

  if ($AllowFallbackToIdOne) {
    $fallbackRows = @($catalog.Select("IdBeca = 1"))
    if ($fallbackRows.Count -eq 1) {
      return 1
    }
  }

  $descriptionMatches = New-Object System.Collections.Generic.List[System.Data.DataRow]
  foreach ($row in $catalog.Rows) {
    if (-not [bool]$row["Activo"]) {
      continue
    }

    if ([int]$row["IdBeca"] -eq 1) {
      continue
    }

    $normalizedRowDescription = Normalize-ComparisonText ([string]$row["Descripcion"])
    if ($normalizedRowDescription -like "*COMPLET*" -or $normalizedRowDescription -like "*TOTAL*") {
      [void]$descriptionMatches.Add($row)
    }
  }

  if ($descriptionMatches.Count -eq 1) {
    return [int]$descriptionMatches[0]["IdBeca"]
  }

  $daysMatches = New-Object System.Collections.Generic.List[System.Data.DataRow]
  foreach ($row in $catalog.Rows) {
    if (-not [bool]$row["Activo"]) {
      continue
    }

    if ([int]$row["IdBeca"] -eq 1) {
      continue
    }

    $diasBeca = [string]$row["DiasBeca"]
    if ($diasBeca.Contains("2") -and $diasBeca.Contains("3") -and $diasBeca.Contains("4") -and $diasBeca.Contains("5") -and $diasBeca.Contains("6")) {
      [void]$daysMatches.Add($row)
    }
  }

  if ($daysMatches.Count -eq 1) {
    return [int]$daysMatches[0]["IdBeca"]
  }

  throw "No se encontro una beca activa resoluble para '$Description'. Use -BecaCompletaId o -SinBecaId segun corresponda."
}

function Get-ControlCarnetPrefixFromDb {
  param(
    [Parameter(Mandatory = $true)]
    [System.Data.SqlClient.SqlConnection]$Connection
  )

  try {
    $table = Invoke-SqlDataTable -Connection $Connection -CommandText "SELECT TOP 1 ControlCarnet FROM Parametro ORDER BY Id;" -Parameters $null -Transaction $null
    if ($table.Rows.Count -eq 1) {
      return Normalize-Whitespace ([string]$table.Rows[0]["ControlCarnet"])
    }
  }
  catch {
  }

  return ""
}

function Get-NormalizedUsuarioCedulaSqlExpression {
  $baseExpression = "UPPER(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(ISNULL(U.Cedula,''))), ISNULL(@ControlCarnetPrefix,''), ''), 'CTPP', ''), ' ', ''), '-', ''), '.', ''), '/', ''))"
  return "CASE WHEN $baseExpression NOT LIKE '%[^0-9]%' AND LEN($baseExpression) = 10 AND LEFT($baseExpression, 1) = '0' THEN SUBSTRING($baseExpression, 2, 9) ELSE $baseExpression END"
}

function Initialize-BecadosStaging {
  param(
    [Parameter(Mandatory = $true)]
    [System.Data.SqlClient.SqlConnection]$Connection
  )

  $sql = @"
CREATE TABLE #BecadosImport (
  Numero NVARCHAR(50) NULL,
  Cedula NVARCHAR(50) NOT NULL,
  PrimerApellido NVARCHAR(150) NULL,
  SegundoApellido NVARCHAR(150) NULL,
  PrimerNombre NVARCHAR(150) NULL,
  SegundoNombre NVARCHAR(150) NULL,
  NombreCompleto NVARCHAR(300) NULL,
  Nivel NVARCHAR(200) NULL,
  Modalidad NVARCHAR(200) NULL,
  CodigoSolicitud NVARCHAR(50) NULL,
  FilaExcel INT NOT NULL
);
"@

  [void](Invoke-SqlNonQuery -Connection $Connection -CommandText $sql -Parameters $null -Transaction $null)
}

function Write-BecadosStaging {
  param(
    [Parameter(Mandatory = $true)]
    [System.Data.SqlClient.SqlConnection]$Connection,
    [Parameter(Mandatory = $true)]
    [System.Data.DataTable]$Table
  )

  $bulkCopy = New-Object System.Data.SqlClient.SqlBulkCopy($Connection)
  try {
    $bulkCopy.DestinationTableName = "#BecadosImport"
    $bulkCopy.BulkCopyTimeout = 0
    foreach ($column in $Table.Columns) {
      [void]$bulkCopy.ColumnMappings.Add([string]$column.ColumnName, [string]$column.ColumnName)
    }

    $bulkCopy.WriteToServer($Table)
  }
  finally {
    if ($null -ne $bulkCopy) {
      $bulkCopy.Close()
      $bulkCopy.Dispose()
    }
  }
}

function Get-BecadosNotFoundTable {
  param(
    [Parameter(Mandatory = $true)]
    [System.Data.SqlClient.SqlConnection]$Connection,
    [string]$ControlCarnetPrefix = ""
  )

  $matchExpression = "{0} = B.Cedula" -f (Get-NormalizedUsuarioCedulaSqlExpression)
  $sql = @"
SELECT
  B.Cedula,
  B.PrimerApellido,
  B.SegundoApellido,
  B.PrimerNombre,
  B.SegundoNombre,
  B.Nivel,
  B.Modalidad,
  B.CodigoSolicitud,
  B.FilaExcel
FROM #BecadosImport B
LEFT JOIN Usuario U
  ON $matchExpression
 AND U.CodTipo = 1
WHERE U.IdUsuario IS NULL
ORDER BY B.Cedula, B.FilaExcel;
"@

  return Invoke-SqlDataTable -Connection $Connection -CommandText $sql -Parameters @{ ControlCarnetPrefix = $ControlCarnetPrefix } -Transaction $null
}

function Get-BecadosDbDuplicateTable {
  param(
    [Parameter(Mandatory = $true)]
    [System.Data.SqlClient.SqlConnection]$Connection,
    [string]$ControlCarnetPrefix = ""
  )

  $matchExpression = "{0} = B.Cedula" -f (Get-NormalizedUsuarioCedulaSqlExpression)
  $sql = @"
SELECT
  B.Cedula,
  COUNT(1) AS UsuariosCoincidentes
FROM #BecadosImport B
INNER JOIN Usuario U
  ON $matchExpression
 AND U.CodTipo = 1
GROUP BY B.Cedula
HAVING COUNT(1) > 1
ORDER BY B.Cedula;
"@

  return Invoke-SqlDataTable -Connection $Connection -CommandText $sql -Parameters @{ ControlCarnetPrefix = $ControlCarnetPrefix } -Transaction $null
}

function Invoke-BecadosImport {
  param(
    [Parameter(Mandatory = $true)]
    [System.Data.SqlClient.SqlConnection]$Connection,
    [Parameter(Mandatory = $true)]
    [int]$IdBecaCompleta,
    [Parameter(Mandatory = $true)]
    [int]$IdBecaSinBeca,
    [string]$ControlCarnetPrefix = "",
    [switch]$ResetRows,
    [switch]$ResetTicketBalances,
    [switch]$ResetScholarships
  )

  $matchExpression = "{0} = B.Cedula" -f (Get-NormalizedUsuarioCedulaSqlExpression)
  $sql = @"
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @RegistrosComedorEliminados INT = 0;
DECLARE @RegistrosTransporteEliminados INT = 0;
DECLARE @RegistrosDocentesEliminados INT = 0;
DECLARE @UsuariosRecargasReiniciadas INT = 0;
DECLARE @UsuariosBecaReiniciada INT = 0;
DECLARE @UsuariosActualizados INT = 0;

IF @ResetRegistros = 1
BEGIN
  DELETE FROM RegistroComedor;
  SET @RegistrosComedorEliminados = @@ROWCOUNT;

  DELETE FROM RegistroTransporte;
  SET @RegistrosTransporteEliminados = @@ROWCOUNT;

  DELETE FROM RegistroDocentes;
  SET @RegistrosDocentesEliminados = @@ROWCOUNT;
END

IF @ResetRecargas = 1
BEGIN
  UPDATE U
  SET U.CantidadTiquetes = 0
  FROM Usuario U
  WHERE U.CodTipo = 1;

  SET @UsuariosRecargasReiniciadas = @@ROWCOUNT;
END

IF @ResetBecas = 1
BEGIN
  UPDATE U
  SET U.TipoBeca = @IdBecaSinBeca
  FROM Usuario U
  WHERE U.CodTipo = 1
    AND ISNULL(U.TipoBeca, -1) <> @IdBecaSinBeca;

  SET @UsuariosBecaReiniciada = @@ROWCOUNT;
END

UPDATE U
SET
  U.TipoBeca = @IdBecaCompleta,
  U.Activo = 1
FROM Usuario U
INNER JOIN #BecadosImport B
  ON $matchExpression
WHERE U.CodTipo = 1
  AND (
    ISNULL(U.TipoBeca, -1) <> @IdBecaCompleta
    OR ISNULL(CAST(U.Activo AS INT), 0) <> 1
  );

SET @UsuariosActualizados = @@ROWCOUNT;

SELECT
  @RegistrosComedorEliminados AS RegistrosComedorEliminados,
  @RegistrosTransporteEliminados AS RegistrosTransporteEliminados,
  @RegistrosDocentesEliminados AS RegistrosDocentesEliminados,
  @UsuariosRecargasReiniciadas AS UsuariosRecargasReiniciadas,
  @UsuariosBecaReiniciada AS UsuariosBecaReiniciada,
  @UsuariosActualizados AS UsuariosActualizados,
  @IdBecaCompleta AS IdBecaCompleta,
  @IdBecaSinBeca AS IdBecaSinBeca,
  (SELECT COUNT(1) FROM #BecadosImport) AS TotalExcel,
  (
    SELECT COUNT(1)
    FROM Usuario U
    INNER JOIN #BecadosImport B
      ON $matchExpression
    WHERE U.CodTipo = 1
  ) AS UsuariosEncontrados;
"@

  $parameters = @{
    IdBecaCompleta = $IdBecaCompleta
    IdBecaSinBeca = $IdBecaSinBeca
    ControlCarnetPrefix = $ControlCarnetPrefix
    ResetRegistros = [int]$ResetRows.IsPresent
    ResetRecargas = [int]$ResetTicketBalances.IsPresent
    ResetBecas = [int]$ResetScholarships.IsPresent
  }

  $transaction = $Connection.BeginTransaction()
  try {
    $result = Invoke-SqlDataTable -Connection $Connection -CommandText $sql -Parameters $parameters -Transaction $transaction
    $transaction.Commit()
    return $result
  }
  catch {
    try {
      $transaction.Rollback()
    }
    catch {
    }
    throw
  }
  finally {
    if ($null -ne $transaction) {
      $transaction.Dispose()
    }
  }
}

if ([string]::IsNullOrWhiteSpace($CsvPath) -and [string]::IsNullOrWhiteSpace($ExcelPath)) {
  $ExcelPath = Join-Path $Repo "Lista inicial\BECADOS.xlsx"
}

if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
  $OutputDirectory = Join-Path $Repo "artifacts\import-becados"
}

Assert-PathExists -PathValue $Repo -Label "repositorio"
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$cleanCsvPath = Join-Path $OutputDirectory ("becados-limpio-{0}.csv" -f $timestamp)
$conflictCsvPath = Join-Path $OutputDirectory ("becados-conflictos-{0}.csv" -f $timestamp)
$notFoundCsvPath = Join-Path $OutputDirectory ("becados-no-encontrados-{0}.csv" -f $timestamp)
$dbDuplicateCsvPath = Join-Path $OutputDirectory ("becados-cedulas-duplicadas-db-{0}.csv" -f $timestamp)

if (-not [string]::IsNullOrWhiteSpace($CsvPath)) {
  Write-Host "Leyendo CSV limpio de becados..." -ForegroundColor Cyan
  $rawTable = Read-CsvDataTable -PathValue $CsvPath
}
else {
  Assert-PathExists -PathValue $ExcelPath -Label "archivo Excel de becados"

  $openXmlFrameworkPath = Resolve-FirstExistingPath -Label "DocumentFormat.OpenXml.Framework.dll" -Candidates @(
    (Join-Path $Repo "escritorio\packages\DocumentFormat.OpenXml.Framework.3.4.1\lib\net46\DocumentFormat.OpenXml.Framework.dll"),
    (Join-Path $Repo "escritorio\packages\DocumentFormat.OpenXml.Framework.3.4.1\lib\netstandard2.0\DocumentFormat.OpenXml.Framework.dll")
  )

  $openXmlPath = Resolve-FirstExistingPath -Label "DocumentFormat.OpenXml.dll" -Candidates @(
    (Join-Path $Repo "escritorio\packages\DocumentFormat.OpenXml.3.4.1\lib\net46\DocumentFormat.OpenXml.dll"),
    (Join-Path $Repo "escritorio\packages\DocumentFormat.OpenXml.3.4.1\lib\netstandard2.0\DocumentFormat.OpenXml.dll")
  )

  Add-Type -Path $openXmlFrameworkPath
  Add-Type -Path $openXmlPath

  Write-Host "Leyendo archivo Excel de becados..." -ForegroundColor Cyan
  $rawTable = Read-ExcelFirstSheetOpenXml -PathValue $ExcelPath
}

$conversion = Convert-BecadosExcelData -RawTable $rawTable -ControlCarnetPrefix $ControlCarnetPrefix
$cleanTable = $conversion.CleanTable
$conflictTable = $conversion.ConflictTable
$metrics = $conversion.Metrics

if ($cleanTable.Rows.Count -eq 0) {
  throw "No se encontraron filas validas de becados en el Excel."
}

Export-DataTableCsv -Table $cleanTable -PathValue $cleanCsvPath

Write-Host ("Excel limpio exportado en: {0}" -f $cleanCsvPath) -ForegroundColor DarkCyan
Write-Host ("Filas origen: {0}" -f $metrics.FilasOrigen) -ForegroundColor Gray
Write-Host ("Filas limpias: {0}" -f $metrics.FilasLimpias) -ForegroundColor Gray
Write-Host ("Filas vacias: {0}" -f $metrics.FilasVacias) -ForegroundColor Gray
Write-Host ("Filas sin cedula: {0}" -f $metrics.FilasSinCedula) -ForegroundColor Gray
Write-Host ("Duplicados exactos omitidos: {0}" -f $metrics.DuplicadosExactos) -ForegroundColor Gray
Write-Host ("Duplicados con conflicto: {0}" -f $metrics.DuplicadosConflicto) -ForegroundColor Gray

if ($conflictTable.Rows.Count -gt 0) {
  Export-DataTableCsv -Table $conflictTable -PathValue $conflictCsvPath
  Write-Host ("Se detectaron cedulas duplicadas con datos conflictivos. Detalle: {0}" -f $conflictCsvPath) -ForegroundColor Yellow
  if (-not $AllowDuplicateCedula) {
    throw "El Excel contiene cedulas duplicadas con datos distintos. Corrija el archivo o use -AllowDuplicateCedula para continuar con la primera ocurrencia."
  }
}

if ($SkipDatabase) {
  Write-Host "Modo solo limpieza completado. No se realizaron cambios en base de datos." -ForegroundColor Green
  return
}

$resolvedConnection = Resolve-ConnectionSettings `
  -RepoPath $Repo `
  -ExplicitConnectionString $ConnectionString `
  -InstalledConfigPath $DeploymentConfigPath `
  -ConfigPath $AppConfigPath `
  -ConfigConnectionName $ConnectionName

Write-Host ("Usando conexion SQL desde: {0}" -f $resolvedConnection.Source) -ForegroundColor DarkCyan
$connection = New-Object System.Data.SqlClient.SqlConnection($resolvedConnection.ConnectionString)

try {
  Write-Host "Conectando a SQL Server..." -ForegroundColor Cyan
  $connection.Open()

  if ([string]::IsNullOrWhiteSpace($ControlCarnetPrefix)) {
    $ControlCarnetPrefix = Get-ControlCarnetPrefixFromDb -Connection $connection
    if (-not [string]::IsNullOrWhiteSpace($ControlCarnetPrefix)) {
      $conversion = Convert-BecadosExcelData -RawTable $rawTable -ControlCarnetPrefix $ControlCarnetPrefix
      $cleanTable = $conversion.CleanTable
      $conflictTable = $conversion.ConflictTable
      $metrics = $conversion.Metrics
      if ($cleanTable.Rows.Count -eq 0) {
        throw "No quedaron filas validas despues de aplicar el prefijo de carnet '$ControlCarnetPrefix'."
      }
      Export-DataTableCsv -Table $cleanTable -PathValue $cleanCsvPath
      if ($conflictTable.Rows.Count -gt 0) {
        Export-DataTableCsv -Table $conflictTable -PathValue $conflictCsvPath
        if (-not $AllowDuplicateCedula) {
          throw "El Excel contiene cedulas duplicadas con datos distintos despues de aplicar el prefijo de carnet. Revise: $conflictCsvPath"
        }
      }
    }
  }

  Initialize-BecadosStaging -Connection $connection
  Write-BecadosStaging -Connection $connection -Table $cleanTable

  $idBecaCompleta = Resolve-BecaId -Connection $connection -Id $BecaCompletaId -Description $BecaCompletaDescripcion
  $idBecaSinBeca = Resolve-BecaId -Connection $connection -Id $SinBecaId -Description $SinBecaDescripcion -AllowFallbackToIdOne

  Write-Host ("Beca completa resuelta: {0} (IdBeca={1})" -f $BecaCompletaDescripcion, $idBecaCompleta) -ForegroundColor DarkCyan
  Write-Host ("Beca base para reinicio: {0} (IdBeca={1})" -f $SinBecaDescripcion, $idBecaSinBeca) -ForegroundColor DarkCyan

  $dbDuplicateTable = Get-BecadosDbDuplicateTable -Connection $connection -ControlCarnetPrefix $ControlCarnetPrefix
  if ($dbDuplicateTable.Rows.Count -gt 0) {
    Export-DataTableCsv -Table $dbDuplicateTable -PathValue $dbDuplicateCsvPath
    throw "Hay cedulas del Excel asociadas a varios estudiantes en Usuario. Revise: $dbDuplicateCsvPath"
  }

  $notFoundTable = Get-BecadosNotFoundTable -Connection $connection -ControlCarnetPrefix $ControlCarnetPrefix
  if ($notFoundTable.Rows.Count -gt 0) {
    Export-DataTableCsv -Table $notFoundTable -PathValue $notFoundCsvPath
    Write-Host ("Usuarios no encontrados en Usuario: {0}. Detalle: {1}" -f $notFoundTable.Rows.Count, $notFoundCsvPath) -ForegroundColor Yellow
    if (-not $AllowMissingUsers) {
      throw "Hay becados del Excel que no existen en Usuario. Revise el detalle y use -AllowMissingUsers solo si desea continuar con coincidencias parciales."
    }
  }
  else {
    Write-Host "Todos los becados del Excel fueron localizados en Usuario." -ForegroundColor DarkGreen
  }

  if ($notFoundTable.Rows.Count -ge $cleanTable.Rows.Count) {
    throw "Ningun registro del Excel fue localizado en Usuario. Revise la normalizacion de cedulas antes de ejecutar cambios."
  }

  $summaryTable = Invoke-BecadosImport `
    -Connection $connection `
    -IdBecaCompleta $idBecaCompleta `
    -IdBecaSinBeca $idBecaSinBeca `
    -ControlCarnetPrefix $ControlCarnetPrefix `
    -ResetRows:$ResetRegistros `
    -ResetTicketBalances:$ResetRecargas `
    -ResetScholarships:$ResetBecasEstudiantes

  if ($summaryTable.Rows.Count -eq 0) {
    throw "No se recibio resumen de la importacion."
  }

  $summary = $summaryTable.Rows[0]
  Write-Host "" 
  Write-Host "Importacion completada." -ForegroundColor Green
  Write-Host ("Total Excel: {0}" -f $summary["TotalExcel"]) -ForegroundColor Gray
  Write-Host ("Usuarios encontrados: {0}" -f $summary["UsuariosEncontrados"]) -ForegroundColor Gray
  Write-Host ("Usuarios actualizados a beca completa: {0}" -f $summary["UsuariosActualizados"]) -ForegroundColor Gray
  Write-Host ("Registros comedor eliminados: {0}" -f $summary["RegistrosComedorEliminados"]) -ForegroundColor Gray
  Write-Host ("Registros transporte eliminados: {0}" -f $summary["RegistrosTransporteEliminados"]) -ForegroundColor Gray
  Write-Host ("Registros docentes eliminados: {0}" -f $summary["RegistrosDocentesEliminados"]) -ForegroundColor Gray
  Write-Host ("Usuarios con recargas reiniciadas: {0}" -f $summary["UsuariosRecargasReiniciadas"]) -ForegroundColor Gray
  Write-Host ("Usuarios con beca reiniciada: {0}" -f $summary["UsuariosBecaReiniciada"]) -ForegroundColor Gray
}
finally {
  if ($null -ne $connection) {
    if ($connection.State -eq [System.Data.ConnectionState]::Open) {
      $connection.Close()
    }
    $connection.Dispose()
  }
}
