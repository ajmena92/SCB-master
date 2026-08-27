Option Strict On
Option Explicit On

Imports System.Data
Imports System.Globalization
Imports System.Text

Public Enum ImportacionExcelModo
    ActualizarTodos = 1
    SoloNuevos = 2
End Enum

Public Class ImportacionExcelMetricas
    Public Property FilasOrigen As Integer
    Public Property FilasValidas As Integer
    Public Property FilasOmitidasEstado As Integer
    Public Property FilasOmitidasCedula As Integer
    Public Property FilasDuplicadas As Integer
    Public Property FilasSinFechaNac As Integer
End Class

Public Class ImportacionExcelEtlResultado
    Public Sub New()
        ColumnasDetectadas = New List(Of String)()
        MapeoColumnas = New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        ErroresCriticos = New List(Of String)()
        Advertencias = New List(Of String)()
        ColumnasNoCriticasFaltantes = New List(Of String)()
        Metricas = New ImportacionExcelMetricas()
    End Sub

    Public Property EsValidoParaImportar As Boolean
    Public Property HojaUsada As String
    Public Property TablaNormalizada As DataTable
    Public Property ColumnasDetectadas As List(Of String)
    Public Property MapeoColumnas As Dictionary(Of String, String)
    Public Property ErroresCriticos As List(Of String)
    Public Property Advertencias As List(Of String)
    Public Property ColumnasNoCriticasFaltantes As List(Of String)
    Public Property FaltoColumnaEstado As Boolean
    Public Property ResumenEstructura As String
    Public Property Metricas As ImportacionExcelMetricas
End Class

Public Class ImportacionExcelEtlService
    Private Class DefinicionColumna
        Public Property Clave As String
        Public Property Etiqueta As String
        Public Property EsCritica As Boolean
        Public Property IndicesFallback As Integer()
        Public Property AliasEncabezados As String()
    End Class

    Public Function Transformar(ByVal tablaExcel As DataTable, ByVal hojaUsada As String) As ImportacionExcelEtlResultado
        Dim resultado As New ImportacionExcelEtlResultado()
        resultado.HojaUsada = If(hojaUsada, String.Empty)
        resultado.TablaNormalizada = CrearTablaNormalizada()
        resultado.Metricas.FilasOrigen = If(tablaExcel Is Nothing, 0, tablaExcel.Rows.Count)

        If tablaExcel Is Nothing Then
            resultado.ErroresCriticos.Add("La hoja seleccionada no contiene datos para importar.")
            resultado.ResumenEstructura = GenerarResumen(resultado)
            Return resultado
        End If

        For Each col As DataColumn In tablaExcel.Columns
            resultado.ColumnasDetectadas.Add(col.ColumnName)
        Next

        Dim definiciones As List(Of DefinicionColumna) = CrearDefiniciones()
        Dim indices As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)

        For Each definicion As DefinicionColumna In definiciones
            Dim indice As Integer = ResolverIndiceColumnaConFallback(tablaExcel, definicion.IndicesFallback, definicion.AliasEncabezados)
            indices(definicion.Clave) = indice

            If indice >= 0 Then
                resultado.MapeoColumnas(definicion.Clave) = tablaExcel.Columns(indice).ColumnName
            ElseIf definicion.EsCritica Then
                resultado.ErroresCriticos.Add("Falta la columna obligatoria '" & definicion.Etiqueta & "'.")
            Else
                resultado.ColumnasNoCriticasFaltantes.Add(definicion.Etiqueta)
            End If
        Next

        If indices("Estado") < 0 Then
            resultado.FaltoColumnaEstado = True
            resultado.Advertencias.Add("La hoja no incluye la columna Estado; se importarán todas las filas válidas.")
        End If

        If indices("Especialidad") < 0 Then
            resultado.Advertencias.Add("La hoja no incluye Especialidad; se usará el valor por defecto en la carga.")
        End If

        If indices("Telefono") < 0 AndAlso indices("Contacto1") < 0 AndAlso indices("Contacto2") < 0 Then
            resultado.Advertencias.Add("La hoja no incluye teléfonos ni contactos; el campo Teléfono quedará vacío.")
        End If

        Dim comparador As StringComparer = StringComparer.OrdinalIgnoreCase
        For Each faltante As String In resultado.ColumnasNoCriticasFaltantes
            If comparador.Equals(faltante, "Estado") OrElse
               comparador.Equals(faltante, "Especialidad") OrElse
               comparador.Equals(faltante, "Teléfono") Then
                Continue For
            End If
            resultado.Advertencias.Add("No se encontró la columna opcional '" & faltante & "'.")
        Next

        If resultado.ErroresCriticos.Count = 0 Then
            ProcesarFilas(tablaExcel, resultado, indices)
        End If

        resultado.EsValidoParaImportar = resultado.ErroresCriticos.Count = 0 AndAlso resultado.TablaNormalizada.Rows.Count > 0

        If resultado.ErroresCriticos.Count = 0 AndAlso resultado.TablaNormalizada.Rows.Count = 0 Then
            resultado.Advertencias.Add("No se encontraron filas válidas para importar después de aplicar las reglas ETL.")
        End If

        resultado.ResumenEstructura = GenerarResumen(resultado)
        Return resultado
    End Function

    Private Function CrearTablaNormalizada() As DataTable
        Dim dt As New DataTable("Importacion")
        dt.Columns.Add("Cedula", GetType(String))
        dt.Columns.Add("PrimerApellido", GetType(String))
        dt.Columns.Add("SegundoApellido", GetType(String))
        dt.Columns.Add("Nombre", GetType(String))
        dt.Columns.Add("Seccion", GetType(String))
        dt.Columns.Add("Especialidad", GetType(String))
        dt.Columns.Add("FechaNac", GetType(DateTime))
        dt.Columns.Add("Telefono", GetType(String))
        dt.Columns.Add("Sexo", GetType(Integer))
        Return dt
    End Function

    Private Function CrearDefiniciones() As List(Of DefinicionColumna)
        Dim defs As New List(Of DefinicionColumna)()
        defs.Add(New DefinicionColumna With {
            .Clave = "Cedula",
            .Etiqueta = "Cédula",
            .EsCritica = True,
            .IndicesFallback = New Integer() {0},
            .AliasEncabezados = New String() {"Cedula", "Cédula", "Identificacion", "Identificación", "Ced", "Documento"}
        })
        defs.Add(New DefinicionColumna With {
            .Clave = "PrimerApellido",
            .Etiqueta = "Primer Apellido",
            .EsCritica = True,
            .IndicesFallback = New Integer() {1},
            .AliasEncabezados = New String() {"PrimerApellido", "Apellido1", "Primer Apellido", "Title", "Apellido"}
        })
        defs.Add(New DefinicionColumna With {
            .Clave = "SegundoApellido",
            .Etiqueta = "Segundo Apellido",
            .EsCritica = True,
            .IndicesFallback = New Integer() {2},
            .AliasEncabezados = New String() {"SegundoApellido", "Apellido2", "Segundo Apellido"}
        })
        defs.Add(New DefinicionColumna With {
            .Clave = "Nombre",
            .Etiqueta = "Nombre",
            .EsCritica = True,
            .IndicesFallback = New Integer() {3},
            .AliasEncabezados = New String() {"Nombre", "Nombres"}
        })
        defs.Add(New DefinicionColumna With {
            .Clave = "Sexo",
            .Etiqueta = "Género",
            .EsCritica = True,
            .IndicesFallback = New Integer() {4},
            .AliasEncabezados = New String() {"Sexo", "Genero", "Género", "Sex"}
        })
        defs.Add(New DefinicionColumna With {
            .Clave = "FechaNac",
            .Etiqueta = "FechaNacimiento",
            .EsCritica = True,
            .IndicesFallback = New Integer() {5, 6},
            .AliasEncabezados = New String() {"FechaNac", "Fecha Nacimiento", "FechaNacimiento", "Nacimiento"}
        })
        defs.Add(New DefinicionColumna With {
            .Clave = "Seccion",
            .Etiqueta = "Sección",
            .EsCritica = True,
            .IndicesFallback = New Integer() {6, 8, 4},
            .AliasEncabezados = New String() {"Seccion", "Sección", "Grupo"}
        })
        defs.Add(New DefinicionColumna With {
            .Clave = "Telefono",
            .Etiqueta = "Teléfono",
            .EsCritica = False,
            .IndicesFallback = New Integer() {8, 11},
            .AliasEncabezados = New String() {"Telefono", "Teléfono", "Telefono Estudiante", "Teléfono Estudiante", "Celular"}
        })
        defs.Add(New DefinicionColumna With {
            .Clave = "Contacto1",
            .Etiqueta = "Contacto 1",
            .EsCritica = False,
            .IndicesFallback = New Integer() {9, 12},
            .AliasEncabezados = New String() {"Contacto 1", "Contacto1"}
        })
        defs.Add(New DefinicionColumna With {
            .Clave = "Contacto2",
            .Etiqueta = "Contacto 2",
            .EsCritica = False,
            .IndicesFallback = New Integer() {10, 13},
            .AliasEncabezados = New String() {"Contacto 2", "Contacto2"}
        })
        defs.Add(New DefinicionColumna With {
            .Clave = "Especialidad",
            .Etiqueta = "Especialidad",
            .EsCritica = False,
            .IndicesFallback = New Integer() {10, 5},
            .AliasEncabezados = New String() {"Especialidad", "Especilidad", "EspecialidadAcademica", "Especialidad Académica", "Especialidad Academica"}
        })
        defs.Add(New DefinicionColumna With {
            .Clave = "Estado",
            .Etiqueta = "Estado",
            .EsCritica = False,
            .IndicesFallback = New Integer() {22},
            .AliasEncabezados = New String() {"Estado", "Status", "Condicion", "Condición"}
        })
        defs.Add(New DefinicionColumna With {
            .Clave = "Bloque",
            .Etiqueta = "Bloque",
            .EsCritica = False,
            .IndicesFallback = New Integer() {7},
            .AliasEncabezados = New String() {"Bloque"}
        })
        Return defs
    End Function

    Private Sub ProcesarFilas(ByVal tablaExcel As DataTable,
                              ByVal resultado As ImportacionExcelEtlResultado,
                              ByVal indices As Dictionary(Of String, Integer))
        Dim cedulasImportadas As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Dim omitidasCedula As Integer = 0
        Dim omitidasDuplicadas As Integer = 0
        Dim omitidasEstado As Integer = 0
        Dim omitidasFecha As Integer = 0

        For Each row As DataRow In tablaExcel.Rows
            If indices("Estado") >= 0 Then
                Dim estado As String = NormalizarTextoComparacion(LeerIndice(row, indices("Estado")))
                If estado.Length > 0 AndAlso estado <> "regular" Then
                    omitidasEstado += 1
                    Continue For
                End If
            End If

            Dim cedula As String = NormalizarCedula(LeerIndice(row, indices("Cedula")))
            If Not EsFilaImportable(cedula) Then
                omitidasCedula += 1
                Continue For
            End If

            If cedulasImportadas.Contains(cedula) Then
                omitidasDuplicadas += 1
                Continue For
            End If

            Dim nueva As DataRow = resultado.TablaNormalizada.NewRow()
            nueva("Cedula") = cedula
            nueva("PrimerApellido") = NormalizarTextoVisible(LeerIndice(row, indices("PrimerApellido")))
            nueva("SegundoApellido") = NormalizarTextoVisible(LeerIndice(row, indices("SegundoApellido")))
            nueva("Nombre") = NormalizarTextoVisible(LeerIndice(row, indices("Nombre")))
            nueva("Seccion") = NormalizarTextoVisible(LeerIndice(row, indices("Seccion")))
            nueva("Especialidad") = NormalizarTextoVisible(LeerIndice(row, indices("Especialidad")))

            Dim tieneFechaNac As Boolean = False
            nueva("FechaNac") = ParseFechaNullable(LeerIndiceObjeto(row, indices("FechaNac")), tieneFechaNac)
            If Not tieneFechaNac Then
                omitidasFecha += 1
            End If

            nueva("Telefono") = ResolverTelefono(row, indices)
            nueva("Sexo") = ParseSexo(LeerIndiceObjeto(row, indices("Sexo")))
            resultado.TablaNormalizada.Rows.Add(nueva)
            cedulasImportadas.Add(cedula)
        Next

        resultado.Metricas.FilasValidas = resultado.TablaNormalizada.Rows.Count
        resultado.Metricas.FilasOmitidasCedula = omitidasCedula
        resultado.Metricas.FilasDuplicadas = omitidasDuplicadas
        resultado.Metricas.FilasOmitidasEstado = omitidasEstado
        resultado.Metricas.FilasSinFechaNac = omitidasFecha
    End Sub

    Private Function ResolverTelefono(ByVal row As DataRow, ByVal indices As Dictionary(Of String, Integer)) As String
        Dim telefonoPrincipal As String = NormalizarTextoVisible(LeerIndice(row, indices("Telefono")))
        If telefonoPrincipal.Length > 0 Then
            Return telefonoPrincipal
        End If

        Dim contacto1 As String = NormalizarTextoVisible(LeerIndice(row, indices("Contacto1")))
        If contacto1.Length > 0 Then
            Return contacto1
        End If

        Return NormalizarTextoVisible(LeerIndice(row, indices("Contacto2")))
    End Function

    Private Function ResolverIndiceColumna(ByVal tabla As DataTable, ParamArray ByVal nombresPosibles() As String) As Integer
        If tabla Is Nothing OrElse tabla.Columns Is Nothing OrElse tabla.Columns.Count = 0 Then
            Return -1
        End If

        For i As Integer = 0 To tabla.Columns.Count - 1
            Dim nombreActual As String = NormalizarNombreColumna(Convert.ToString(tabla.Columns(i).ColumnName))
            For Each nombrePosible As String In nombresPosibles
                If nombreActual = NormalizarNombreColumna(nombrePosible) Then
                    Return i
                End If
            Next
        Next

        Return -1
    End Function

    Private Function ResolverIndiceColumnaConFallback(ByVal tabla As DataTable,
                                                       ByVal indicesFallback As Integer(),
                                                       ParamArray ByVal nombresPosibles() As String) As Integer
        Dim indiceEncontrado As Integer = ResolverIndiceColumna(tabla, nombresPosibles)
        If indiceEncontrado >= 0 Then
            Return indiceEncontrado
        End If

        If indicesFallback IsNot Nothing Then
            For Each indice As Integer In indicesFallback
                If EsIndiceValido(tabla, indice) Then
                    Return indice
                End If
            Next
        End If

        Return -1
    End Function

    Private Function EsIndiceValido(ByVal tabla As DataTable, ByVal indice As Integer) As Boolean
        If tabla Is Nothing OrElse indice < 0 Then
            Return False
        End If

        Return indice < tabla.Columns.Count
    End Function

    Private Function NormalizarNombreColumna(ByVal nombre As String) As String
        Return NormalizarTextoComparacion(nombre).Replace(" ", String.Empty).Replace("_", String.Empty).Replace("-", String.Empty)
    End Function

    Private Function NormalizarTextoComparacion(ByVal valor As String) As String
        If String.IsNullOrWhiteSpace(valor) Then
            Return String.Empty
        End If

        Dim texto As String = valor.Replace(ChrW(160), " ").Trim().ToLowerInvariant()
        Return texto.Replace("á", "a").
            Replace("é", "e").
            Replace("í", "i").
            Replace("ó", "o").
            Replace("ú", "u")
    End Function

    Private Function NormalizarTextoVisible(ByVal valor As String) As String
        If String.IsNullOrWhiteSpace(valor) Then
            Return String.Empty
        End If

        Dim limpio As String = valor.Replace(ChrW(160), " ").Trim()
        While limpio.Contains("  ")
            limpio = limpio.Replace("  ", " ")
        End While
        Return limpio
    End Function

    Private Function LeerIndice(ByVal row As DataRow, ByVal index As Integer) As String
        If index < 0 OrElse row Is Nothing OrElse row.Table Is Nothing OrElse row.Table.Columns.Count <= index Then
            Return String.Empty
        End If

        If row.IsNull(index) Then
            Return String.Empty
        End If

        Return Convert.ToString(row(index)).Trim()
    End Function

    Private Function LeerIndiceObjeto(ByVal row As DataRow, ByVal index As Integer) As Object
        If index < 0 OrElse row Is Nothing OrElse row.Table Is Nothing OrElse row.Table.Columns.Count <= index Then
            Return Nothing
        End If

        If row.IsNull(index) Then
            Return Nothing
        End If

        Return row(index)
    End Function

    Private Function NormalizarCedula(ByVal raw As String) As String
        If String.IsNullOrWhiteSpace(raw) Then
            Return String.Empty
        End If

        Dim sb As New StringBuilder(raw.Length)
        For Each ch As Char In raw.Trim()
            If Char.IsLetterOrDigit(ch) Then
                sb.Append(Char.ToUpperInvariant(ch))
            End If
        Next
        Return sb.ToString()
    End Function

    Private Function EsFilaImportable(ByVal cedula As String) As Boolean
        If String.IsNullOrWhiteSpace(cedula) Then
            Return False
        End If

        Dim c As String = cedula.Trim().ToUpperInvariant()
        If c = "CEDULA" OrElse c = "CÉDULA" Then
            Return False
        End If

        Return c.Length >= 4
    End Function

    Private Function ParseFechaNullable(ByVal raw As Object, ByRef tieneValor As Boolean) As Object
        tieneValor = False
        If raw Is Nothing OrElse raw Is DBNull.Value Then
            Return DBNull.Value
        End If

        If TypeOf raw Is DateTime Then
            tieneValor = True
            Return DirectCast(raw, DateTime).Date
        End If

        If TypeOf raw Is Double Then
            Try
                Dim desdeOa As DateTime = DateTime.FromOADate(CDbl(raw)).Date
                tieneValor = True
                Return desdeOa
            Catch
            End Try
        End If

        Dim rawTexto As String = Convert.ToString(raw).Trim()
        If rawTexto.Length = 0 Then
            Return DBNull.Value
        End If

        Dim fecha As DateTime
        Dim formatos As String() = {"dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-M-yyyy", "yyyy-MM-dd", "MM/dd/yyyy", "M/d/yyyy"}
        If DateTime.TryParseExact(rawTexto, formatos, CultureInfo.GetCultureInfo("es-CR"), DateTimeStyles.None, fecha) Then
            tieneValor = True
            Return fecha
        End If

        If DateTime.TryParseExact(rawTexto, formatos, CultureInfo.InvariantCulture, DateTimeStyles.None, fecha) Then
            tieneValor = True
            Return fecha
        End If

        If DateTime.TryParse(rawTexto, fecha) Then
            tieneValor = True
            Return fecha
        End If

        Dim oa As Double
        If Double.TryParse(rawTexto, NumberStyles.Any, CultureInfo.InvariantCulture, oa) Then
            Try
                Dim desdeOa As DateTime = DateTime.FromOADate(oa).Date
                tieneValor = True
                Return desdeOa
            Catch
            End Try
        End If

        Return DBNull.Value
    End Function

    Private Function ParseSexo(ByVal raw As Object) As Integer
        If raw Is Nothing OrElse raw Is DBNull.Value Then
            Return 0
        End If

        Dim texto As String = NormalizarTextoComparacion(Convert.ToString(raw))
        If texto.Length = 0 Then
            Return 0
        End If

        Dim valorNumerico As Integer
        If Integer.TryParse(texto, valorNumerico) Then
            If valorNumerico = 1 OrElse valorNumerico = 2 Then
                Return valorNumerico
            End If
        End If

        If texto = "m" OrElse texto = "f" OrElse texto = "femenino" OrElse texto = "mujer" Then
            Return 2
        End If

        If texto = "h" OrElse texto = "masculino" OrElse texto = "hombre" Then
            Return 1
        End If

        Return 0
    End Function

    Private Function GenerarResumen(ByVal resultado As ImportacionExcelEtlResultado) As String
        Dim sb As New StringBuilder()
        sb.Append("Hoja: ")
        sb.Append(If(String.IsNullOrWhiteSpace(resultado.HojaUsada), "(sin nombre)", resultado.HojaUsada))
        sb.Append(". Columnas detectadas: ")
        sb.Append(String.Join(", ", resultado.ColumnasDetectadas.ToArray()))
        sb.Append(".")

        If resultado.MapeoColumnas.Count > 0 Then
            Dim partesMapeo As New List(Of String)()
            For Each par As KeyValuePair(Of String, String) In resultado.MapeoColumnas
                partesMapeo.Add(par.Key & "=" & par.Value)
            Next
            sb.Append(" Mapeo: ")
            sb.Append(String.Join("; ", partesMapeo.ToArray()))
            sb.Append(".")
        End If

        sb.Append(" Filas origen: ")
        sb.Append(resultado.Metricas.FilasOrigen.ToString(CultureInfo.InvariantCulture))
        sb.Append(", válidas: ")
        sb.Append(resultado.Metricas.FilasValidas.ToString(CultureInfo.InvariantCulture))
        sb.Append(", omitidas por estado: ")
        sb.Append(resultado.Metricas.FilasOmitidasEstado.ToString(CultureInfo.InvariantCulture))
        sb.Append(", omitidas por cédula: ")
        sb.Append(resultado.Metricas.FilasOmitidasCedula.ToString(CultureInfo.InvariantCulture))
        sb.Append(", duplicadas: ")
        sb.Append(resultado.Metricas.FilasDuplicadas.ToString(CultureInfo.InvariantCulture))
        sb.Append(", sin fecha nacimiento: ")
        sb.Append(resultado.Metricas.FilasSinFechaNac.ToString(CultureInfo.InvariantCulture))
        sb.Append(".")

        If resultado.ErroresCriticos.Count > 0 Then
            sb.Append(" Errores críticos: ")
            sb.Append(String.Join(" | ", resultado.ErroresCriticos.ToArray()))
            sb.Append(".")
        End If

        If resultado.Advertencias.Count > 0 Then
            sb.Append(" Advertencias: ")
            sb.Append(String.Join(" | ", resultado.Advertencias.ToArray()))
            sb.Append(".")
        End If

        Return sb.ToString()
    End Function
End Class
