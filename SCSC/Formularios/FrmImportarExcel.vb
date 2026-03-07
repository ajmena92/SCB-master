Option Strict On
Option Explicit On

Imports System.Data
Imports System.Globalization
Imports System.IO
Imports System.Collections.Generic
Imports OXPackaging = DocumentFormat.OpenXml.Packaging
Imports OXSpreadsheet = DocumentFormat.OpenXml.Spreadsheet

Public Class FrmImportarExcel
    Private Class ImportacionMetricas
        Public Property FilasOrigen As Integer
        Public Property FilasValidas As Integer
        Public Property FilasOmitidasEstado As Integer
        Public Property FilasOmitidasCedula As Integer
        Public Property FilasDuplicadas As Integer
        Public Property FilasSinFechaNac As Integer
    End Class

    Private ReadOnly Cls As New FuncionesDB()
    Private ReadOnly ImportSvc As New ImportacionExcelService(Cls)
    Private ReadOnly ParametroSvc As New ParametroSistemaService()
    Private ReadOnly OpenFileDialog As New OpenFileDialog()

    Private _lblArchivoExcel As Label
    Private _txtArchivoExcel As TextBox
    Private _btnArchivoExcel As Button
    Private _lblInfo As Label
    Private _lblHorario As Label
    Private _archivoExcelSeleccionado As String = String.Empty
    Private _ultimaMetrica As ImportacionMetricas

    Private Sub FrmImportarExcel_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If CrudVisualHelper.IsInDesignMode(Me) Then
            Return
        End If
        Try
            CrudVisualHelper.ApplyCrudStandard(Me, "operativo")
            ApplyModernImportLayout()
            CargaHorarios(CBHorario)
            RecalcularLayoutImportacion()
            LimpiarPantalla()
        Catch ex As Exception
            MsgBox("Error al cargar el formulario: " & ex.Message, MsgBoxStyle.Critical)
            Me.Dispose()
        End Try
    End Sub

    Private Sub FrmImportarExcel_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        If Me.IsDisposed Then
            Exit Sub
        End If
        RecalcularLayoutImportacion()
    End Sub

    Private Sub ApplyModernImportLayout()
        Me.BackColor = UIConstants.AppBackground
        Me.BackgroundImage = Nothing
        Me.Font = UIConstants.FontBody()
        Me.FormBorderStyle = FormBorderStyle.Sizable
        Me.ControlBox = True
        Me.MaximizeBox = True
        Me.MinimizeBox = True
        Me.MinimumSize = New Size(980, 700)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.Text = "Importación Excel"

        PanelAcciones.BackColor = Color.Transparent
        PanelAcciones.BorderStyle = BorderStyle.None

        LblTituloModulo.Font = New Font("Segoe UI Semibold", 20.0!, FontStyle.Bold)
        LblTituloModulo.ForeColor = UIConstants.TextPrimary
        LblTituloModulo.Text = "Importación desde Excel"

        GroupConfiguracionImportacion.BackColor = UIConstants.Surface
        GroupConfiguracionImportacion.ForeColor = UIConstants.TextPrimary
        GroupConfiguracionImportacion.Font = UIConstants.FontBodyStrong()
        GroupConfiguracionImportacion.Text = "Configuración de importación"

        RdEst.Text = "Actualizar estudiantes"
        RbDocentes.Text = "Actualizar docentes"
        RdEst.ForeColor = UIConstants.TextPrimary
        RbDocentes.ForeColor = UIConstants.TextPrimary

        CBHorario.BackColor = UIConstants.Surface
        CBHorario.FlatStyle = FlatStyle.Flat
        CBHorario.DropDownStyle = ComboBoxStyle.DropDownList

        ConfigurarBotonPrimario(BtnGuardar, "Importar")
        ConfigurarBotonSecundario(BtnCancelar, "Limpiar")
        ConfigurarBotonSecundario(BtnRegresar, "Cerrar")

        LblEstado.ForeColor = UIConstants.TextSecondary
        LblEstado.Font = UIConstants.FontBodyStrong()
        LblEstado.TextAlign = ContentAlignment.MiddleLeft

        Progreso.Style = ProgressBarStyle.Continuous
        Progreso.Value = 0

        ConfigurarGridPreview(DgvVistaPrevia)
        ConfigurarSelectorArchivo()
    End Sub

    Private Sub ConfigurarBotonPrimario(ByVal btn As Button, ByVal text As String)
        btn.FlatStyle = FlatStyle.Flat
        btn.FlatAppearance.BorderSize = 0
        btn.BackColor = UIConstants.Accent
        btn.ForeColor = Color.White
        btn.Text = text
        btn.BackgroundImage = Nothing
        btn.Font = UIConstants.FontBodyStrong()
    End Sub

    Private Sub ConfigurarBotonSecundario(ByVal btn As Button, ByVal text As String)
        btn.FlatStyle = FlatStyle.Flat
        btn.FlatAppearance.BorderColor = UIConstants.Border
        btn.FlatAppearance.BorderSize = 1
        btn.BackColor = UIConstants.Surface
        btn.ForeColor = UIConstants.TextPrimary
        btn.Text = text
        btn.BackgroundImage = Nothing
        btn.Font = UIConstants.FontBodyStrong()
    End Sub

    Private Sub ConfigurarGridPreview(ByVal grid As DataGridView)
        grid.ReadOnly = True
        grid.MultiSelect = False
        grid.AllowUserToAddRows = False
        grid.AllowUserToDeleteRows = False
        grid.RowHeadersVisible = False
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        grid.BackgroundColor = Color.White
        grid.BorderStyle = BorderStyle.None
        grid.GridColor = UIConstants.Border
        grid.EnableHeadersVisualStyles = False
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(232, 238, 248)
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(29, 42, 61)
        grid.ColumnHeadersDefaultCellStyle.Font = New Font("Segoe UI Semibold", 10.0!, FontStyle.Bold)
        grid.ColumnHeadersHeight = 36
    End Sub

    Private Sub ConfigurarSelectorArchivo()
        If _lblArchivoExcel IsNot Nothing Then
            Return
        End If

        _lblArchivoExcel = New Label()
        _lblArchivoExcel.AutoSize = True
        _lblArchivoExcel.Text = "Archivo Excel:"
        _lblArchivoExcel.Font = New Font("Segoe UI", 10.0!, FontStyle.Bold)
        _lblArchivoExcel.ForeColor = UIConstants.TextPrimary

        _txtArchivoExcel = New TextBox()
        _txtArchivoExcel.ReadOnly = True
        _txtArchivoExcel.BackColor = Color.White
        _txtArchivoExcel.BorderStyle = BorderStyle.FixedSingle
        _txtArchivoExcel.Font = UIConstants.FontBody()
        _txtArchivoExcel.ForeColor = UIConstants.TextSecondary

        _btnArchivoExcel = New Button()
        _btnArchivoExcel.Text = "Examinar..."
        _btnArchivoExcel.FlatStyle = FlatStyle.Flat
        _btnArchivoExcel.FlatAppearance.BorderColor = UIConstants.Border
        _btnArchivoExcel.FlatAppearance.BorderSize = 1
        _btnArchivoExcel.BackColor = UIConstants.Surface
        _btnArchivoExcel.ForeColor = UIConstants.TextPrimary
        _btnArchivoExcel.Font = UIConstants.FontBodyStrong()
        _btnArchivoExcel.Cursor = Cursors.Hand

        _lblInfo = New Label()
        _lblInfo.AutoSize = False
        _lblInfo.TextAlign = ContentAlignment.MiddleLeft
        _lblInfo.Font = New Font("Segoe UI", 9.0!, FontStyle.Italic)
        _lblInfo.ForeColor = UIConstants.TextSecondary
        _lblInfo.Text = "Seleccione un archivo .xlsx/.xlsm con hoja 'Lista'. Solo se importa Estado=Regular."

        _lblHorario = New Label()
        _lblHorario.AutoSize = True
        _lblHorario.Text = "Horario lectivo:"
        _lblHorario.Font = New Font("Segoe UI", 10.0!, FontStyle.Bold)
        _lblHorario.ForeColor = UIConstants.TextPrimary

        AddHandler _btnArchivoExcel.Click, AddressOf BtnArchivoExcel_Click

        GroupConfiguracionImportacion.Controls.Add(_lblArchivoExcel)
        GroupConfiguracionImportacion.Controls.Add(_txtArchivoExcel)
        GroupConfiguracionImportacion.Controls.Add(_btnArchivoExcel)
        GroupConfiguracionImportacion.Controls.Add(_lblInfo)
        GroupConfiguracionImportacion.Controls.Add(_lblHorario)
    End Sub

    Private Sub RecalcularLayoutImportacion()
        If Me.ClientSize.Width <= 0 OrElse Me.ClientSize.Height <= 0 Then
            Exit Sub
        End If

        Dim margen As Integer = 24
        Dim spacing As Integer = 12

        LblTituloModulo.SetBounds(margen, 16, Me.ClientSize.Width - (margen * 2), 38)
        LblTituloModulo.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right

        GroupConfiguracionImportacion.SetBounds(margen, LblTituloModulo.Bottom + spacing, Me.ClientSize.Width - (margen * 2), 170)
        GroupConfiguracionImportacion.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right

        RecalcularLayoutGrupoConfiguracion()

        LblEstado.SetBounds(margen, GroupConfiguracionImportacion.Bottom + 8, Me.ClientSize.Width - (margen * 2), 26)
        LblEstado.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right

        Progreso.SetBounds(margen, LblEstado.Bottom + 4, Me.ClientSize.Width - (margen * 2), 20)
        Progreso.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right

        PanelAcciones.SetBounds(Me.ClientSize.Width - margen - 366, Me.ClientSize.Height - 56, 366, 38)
        PanelAcciones.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right

        BtnGuardar.SetBounds(0, 0, 146, 38)
        BtnCancelar.SetBounds(154, 0, 102, 38)
        BtnRegresar.SetBounds(264, 0, 102, 38)

        Dim topGrid As Integer = Progreso.Bottom + 10
        Dim bottomGrid As Integer = PanelAcciones.Top - 10
        Dim gridHeight As Integer = Math.Max(140, bottomGrid - topGrid)
        DgvVistaPrevia.SetBounds(margen, topGrid, Me.ClientSize.Width - (margen * 2), gridHeight)
        DgvVistaPrevia.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
    End Sub

    Private Sub RecalcularLayoutGrupoConfiguracion()
        If _lblArchivoExcel Is Nothing Then
            Exit Sub
        End If

        Dim leftLabel As Integer = 16
        Dim leftControl As Integer = 150
        Dim groupWidth As Integer = GroupConfiguracionImportacion.ClientSize.Width

        _lblArchivoExcel.Location = New Point(leftLabel, 34)

        Dim btnWidth As Integer = 120
        Dim archivoWidth As Integer = Math.Max(220, groupWidth - leftControl - btnWidth - 30)
        _txtArchivoExcel.SetBounds(leftControl, 31, archivoWidth, 27)
        _btnArchivoExcel.SetBounds(leftControl + archivoWidth + 8, 30, btnWidth, 29)

        _lblInfo.SetBounds(leftControl, 60, groupWidth - leftControl - 16, 20)

        Dim tipoTop As Integer = 96
        RdEst.Location = New Point(leftLabel, tipoTop)
        RbDocentes.Location = New Point(leftLabel + 210, tipoTop)

        _lblHorario.Location = New Point(leftControl + 290, tipoTop + 2)
        CBHorario.SetBounds(leftControl + 400, tipoTop - 2, Math.Max(180, groupWidth - (leftControl + 420)), 30)
    End Sub

    Private Sub BtnArchivoExcel_Click(sender As Object, e As EventArgs)
        SeleccionarArchivoExcel(True)
    End Sub

    Private Function SeleccionarArchivoExcel(ByVal forzarDialogo As Boolean) As String
        If Not forzarDialogo AndAlso
           Not String.IsNullOrWhiteSpace(_archivoExcelSeleccionado) AndAlso
           File.Exists(_archivoExcelSeleccionado) Then
            Return _archivoExcelSeleccionado
        End If

        OpenFileDialog.InitialDirectory = My.Computer.FileSystem.SpecialDirectories.MyDocuments
        OpenFileDialog.Filter = "Archivos Excel (*.xlsx;*.xlsm)|*.xlsx;*.xlsm|Todos (*.*)|*.*"
        OpenFileDialog.Title = "Seleccione el archivo Excel a importar"

        If OpenFileDialog.ShowDialog(Me) <> DialogResult.OK Then
            Return String.Empty
        End If

        _archivoExcelSeleccionado = New FileInfo(OpenFileDialog.FileName).FullName
        If _txtArchivoExcel IsNot Nothing Then
            _txtArchivoExcel.Text = _archivoExcelSeleccionado
        End If
        Return _archivoExcelSeleccionado
    End Function

    Private Sub LimpiarPantalla()
        LblEstado.Text = "Seleccione archivo y horario para iniciar la importación."
        Progreso.Style = ProgressBarStyle.Continuous
        Progreso.Value = 0
        DgvVistaPrevia.DataSource = Nothing
        _ultimaMetrica = Nothing
    End Sub

    Private Sub BtnCancelar_Click(sender As Object, e As EventArgs) Handles BtnCancelar.Click
        LimpiarPantalla()
    End Sub

    Private Sub BtnRegresar_Click(sender As Object, e As EventArgs) Handles BtnRegresar.Click
        Me.Dispose()
    End Sub

    Private Function Validaciion() As Boolean
        If String.IsNullOrWhiteSpace(SeleccionarArchivoExcel(False)) Then
            MsgBox("Seleccione un archivo Excel para importar.", MsgBoxStyle.Exclamation)
            Return False
        End If

        If CBHorario.SelectedIndex <= 0 Then
            MsgBox("Ingrese el horario de los estudiantes a importar.", MsgBoxStyle.Exclamation)
            CBHorario.Focus()
            Return False
        End If

        Return True
    End Function

    Private Function ObtenerTipoUsuario() As Integer
        If RdEst.Checked Then
            Return 1
        End If
        Return 2
    End Function

    Private Function ObtenerIdHorarioSeleccionado() As Integer
        Dim horario As LBItem = TryCast(CBHorario.SelectedItem, LBItem)
        If horario Is Nothing Then
            Throw New InvalidOperationException("Debe seleccionar un horario válido.")
        End If

        Dim idHorario As Integer
        If Not Integer.TryParse(horario.Valor, idHorario) OrElse idHorario <= 0 Then
            Throw New InvalidOperationException("El horario seleccionado no es válido.")
        End If

        Return idHorario
    End Function

    Private Sub BtnGuardar_Click(sender As Object, e As EventArgs) Handles BtnGuardar.Click
        If Not Validaciion() Then
            Exit Sub
        End If

        Try
            ImportarDesdeExcel()
        Catch ex As Exception
            MsgBox("Error de importación: " & ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub ImportarDesdeExcel()
        Dim rutaExcel As String = SeleccionarArchivoExcel(False)
        If String.IsNullOrWhiteSpace(rutaExcel) Then
            LblEstado.Text = "Importación cancelada. No se seleccionó archivo."
            Exit Sub
        End If

        Try
            LblEstado.Text = "Leyendo archivo Excel..."
            Progreso.Style = ProgressBarStyle.Marquee
            Progreso.MarqueeAnimationSpeed = 20
            Refresh()

            Dim tablaExcel As DataTable = LeerTablaExcel(rutaExcel)
            Dim resumenEstructura As String = String.Empty
            Dim metricas As ImportacionMetricas = Nothing
            Dim normalizada As DataTable = NormalizarDesdeExcel(tablaExcel, resumenEstructura, metricas)
            _ultimaMetrica = metricas
            DgvVistaPrevia.DataSource = normalizada
            If resumenEstructura.Length > 0 Then
                LblEstado.Text = resumenEstructura
            End If
            EjecutarImportacion(normalizada)
        Finally
            Progreso.Style = ProgressBarStyle.Continuous
        End Try
    End Sub

    Private Function LeerTablaExcel(ByVal rutaExcel As String) As DataTable
        If Not File.Exists(rutaExcel) Then
            Throw New FileNotFoundException("No se encontró el archivo seleccionado.", rutaExcel)
        End If

        Dim extension As String = Path.GetExtension(rutaExcel).ToLowerInvariant()
        If extension <> ".xlsx" AndAlso extension <> ".xlsm" Then
            Throw New InvalidOperationException("Formato de archivo no soportado. Use un archivo .xlsx o .xlsm.")
        End If

        Return LeerTablaExcelOpenXml(rutaExcel)
    End Function

    Private Function LeerTablaExcelOpenXml(ByVal rutaExcel As String) As DataTable
        Dim tabla As New DataTable("Excel")

        Using documento As OXPackaging.SpreadsheetDocument = OXPackaging.SpreadsheetDocument.Open(rutaExcel, False)
            Dim libro As OXPackaging.WorkbookPart = documento.WorkbookPart
            If libro Is Nothing OrElse libro.Workbook Is Nothing Then
                Throw New InvalidOperationException("El archivo Excel no contiene un libro válido.")
            End If

            Dim hoja As OXSpreadsheet.Sheet = ResolverHojaOpenXml(libro)
            If hoja Is Nothing Then
                Throw New InvalidOperationException("No se encontró ninguna hoja de datos en el archivo Excel.")
            End If
            If hoja.Id Is Nothing Then
                Throw New InvalidOperationException("La hoja seleccionada no tiene un identificador válido.")
            End If

            Dim hojaPart As OXPackaging.WorksheetPart = TryCast(libro.GetPartById(hoja.Id.Value), OXPackaging.WorksheetPart)
            If hojaPart Is Nothing OrElse hojaPart.Worksheet Is Nothing Then
                Throw New InvalidOperationException("No se pudo leer la hoja seleccionada en el archivo Excel.")
            End If

            Dim sharedStrings As List(Of String) = CargarSharedStrings(libro)
            Dim datos As OXSpreadsheet.SheetData = hojaPart.Worksheet.GetFirstChild(Of OXSpreadsheet.SheetData)()
            If datos Is Nothing Then
                Throw New InvalidOperationException("La hoja seleccionada no contiene datos.")
            End If

            Dim filas As List(Of OXSpreadsheet.Row) = New List(Of OXSpreadsheet.Row)(datos.Elements(Of OXSpreadsheet.Row)())
            If filas.Count = 0 Then
                Throw New InvalidOperationException("La hoja seleccionada está vacía.")
            End If

            Dim filaEncabezado As OXSpreadsheet.Row = filas(0)
            Dim encabezados As Dictionary(Of Integer, String) = ExtraerValoresFila(filaEncabezado, sharedStrings)
            If encabezados.Count = 0 Then
                Throw New InvalidOperationException("No se pudieron leer encabezados en la hoja seleccionada.")
            End If

            Dim maxColumna As Integer = -1
            For Each indice As Integer In encabezados.Keys
                If indice > maxColumna Then
                    maxColumna = indice
                End If
            Next

            For i As Integer = 0 To maxColumna
                Dim encabezado As String = String.Empty
                If encabezados.ContainsKey(i) Then
                    encabezado = encabezados(i)
                End If

                If String.IsNullOrWhiteSpace(encabezado) Then
                    encabezado = "Columna" & (i + 1).ToString(CultureInfo.InvariantCulture)
                End If

                Dim nombreUnico As String = ObtenerNombreColumnaUnico(tabla, encabezado.Trim())
                tabla.Columns.Add(nombreUnico, GetType(String))
            Next

            For i As Integer = 1 To filas.Count - 1
                Dim mapaValores As Dictionary(Of Integer, String) = ExtraerValoresFila(filas(i), sharedStrings)
                Dim valores(tabla.Columns.Count - 1) As Object

                For j As Integer = 0 To tabla.Columns.Count - 1
                    Dim valor As String = String.Empty
                    If mapaValores.ContainsKey(j) Then
                        valor = mapaValores(j)
                    End If
                    valores(j) = valor
                Next

                tabla.Rows.Add(valores)
            Next
        End Using

        Return tabla
    End Function

    Private Function ResolverHojaOpenXml(ByVal libro As OXPackaging.WorkbookPart) As OXSpreadsheet.Sheet
        Dim hojas As OXSpreadsheet.Sheets = libro.Workbook.GetFirstChild(Of OXSpreadsheet.Sheets)()
        If hojas Is Nothing Then
            Return Nothing
        End If

        Dim primeraHoja As OXSpreadsheet.Sheet = Nothing

        For Each hoja As OXSpreadsheet.Sheet In hojas.Elements(Of OXSpreadsheet.Sheet)()
            If primeraHoja Is Nothing Then
                primeraHoja = hoja
            End If

            Dim nombre As String = Convert.ToString(hoja.Name)
            If String.IsNullOrWhiteSpace(nombre) Then
                Continue For
            End If

            Dim normalizado As String = NormalizarTextoComparacion(nombre)
            If normalizado = "lista" Then
                Return hoja
            End If
        Next

        Return primeraHoja
    End Function

    Private Function CargarSharedStrings(ByVal libro As OXPackaging.WorkbookPart) As List(Of String)
        Dim resultado As New List(Of String)()
        If libro Is Nothing OrElse libro.SharedStringTablePart Is Nothing OrElse libro.SharedStringTablePart.SharedStringTable Is Nothing Then
            Return resultado
        End If

        For Each item As OXSpreadsheet.SharedStringItem In libro.SharedStringTablePart.SharedStringTable.Elements(Of OXSpreadsheet.SharedStringItem)()
            resultado.Add(item.InnerText)
        Next

        Return resultado
    End Function

    Private Function ExtraerValoresFila(ByVal fila As OXSpreadsheet.Row, ByVal sharedStrings As List(Of String)) As Dictionary(Of Integer, String)
        Dim resultado As New Dictionary(Of Integer, String)()
        If fila Is Nothing Then
            Return resultado
        End If

        For Each celda As OXSpreadsheet.Cell In fila.Elements(Of OXSpreadsheet.Cell)()
            Dim referencia As String = Convert.ToString(celda.CellReference)
            Dim indiceColumna As Integer = ObtenerIndiceColumna(referencia)
            If indiceColumna < 0 Then
                Continue For
            End If

            Dim valor As String = ObtenerValorCelda(celda, sharedStrings)
            If Not resultado.ContainsKey(indiceColumna) Then
                resultado.Add(indiceColumna, valor)
            Else
                resultado(indiceColumna) = valor
            End If
        Next

        Return resultado
    End Function

    Private Function ObtenerIndiceColumna(ByVal referenciaCelda As String) As Integer
        If String.IsNullOrWhiteSpace(referenciaCelda) Then
            Return -1
        End If

        Dim acumulado As Integer = 0
        For Each ch As Char In referenciaCelda.ToUpperInvariant()
            If ch < "A"c OrElse ch > "Z"c Then
                Exit For
            End If
            acumulado = (acumulado * 26) + (AscW(ch) - AscW("A"c) + 1)
        Next

        If acumulado <= 0 Then
            Return -1
        End If

        Return acumulado - 1
    End Function

    Private Function ObtenerValorCelda(ByVal celda As OXSpreadsheet.Cell, ByVal sharedStrings As List(Of String)) As String
        If celda Is Nothing Then
            Return String.Empty
        End If

        Dim valorBase As String = celda.InnerText
        If celda.DataType Is Nothing Then
            Return valorBase.Trim()
        End If

        Select Case celda.DataType.Value
            Case OXSpreadsheet.CellValues.SharedString
                Dim indice As Integer
                If Integer.TryParse(valorBase, indice) Then
                    If sharedStrings IsNot Nothing AndAlso indice >= 0 AndAlso indice < sharedStrings.Count Then
                        Return sharedStrings(indice).Trim()
                    End If
                End If

            Case OXSpreadsheet.CellValues.Boolean
                Return If(valorBase = "1", "TRUE", "FALSE")

            Case OXSpreadsheet.CellValues.InlineString
                Return celda.InnerText.Trim()
        End Select

        Return valorBase.Trim()
    End Function

    Private Function ObtenerNombreColumnaUnico(ByVal tabla As DataTable, ByVal nombreBase As String) As String
        If tabla Is Nothing Then
            Return nombreBase
        End If

        Dim baseNormalizada As String = nombreBase
        If baseNormalizada.Length = 0 Then
            baseNormalizada = "Columna"
        End If

        Dim nombre As String = baseNormalizada
        Dim sufijo As Integer = 2
        While tabla.Columns.Contains(nombre)
            nombre = baseNormalizada & "_" & sufijo.ToString(CultureInfo.InvariantCulture)
            sufijo += 1
        End While

        Return nombre
    End Function

    Private Function NormalizarDesdeExcel(ByVal tablaExcel As DataTable, ByRef resumen As String, ByRef metricas As ImportacionMetricas) As DataTable
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
        metricas = New ImportacionMetricas()
        metricas.FilasOrigen = If(tablaExcel Is Nothing, 0, tablaExcel.Rows.Count)

        Dim idxCedula As Integer = ResolverIndiceColumnaConFallback(tablaExcel, New Integer() {0}, "Cedula", "Cédula", "Identificacion", "Identificación", "Ced", "Documento")
        Dim idxPrimerApellido As Integer = ResolverIndiceColumnaConFallback(tablaExcel, New Integer() {1}, "PrimerApellido", "Apellido1", "Primer Apellido", "Title", "Apellido")
        Dim idxSegundoApellido As Integer = ResolverIndiceColumnaConFallback(tablaExcel, New Integer() {2}, "SegundoApellido", "Apellido2", "Segundo Apellido")
        Dim idxNombre As Integer = ResolverIndiceColumnaConFallback(tablaExcel, New Integer() {3}, "Nombre", "Nombres")
        Dim idxSeccion As Integer = ResolverIndiceColumnaConFallback(tablaExcel, New Integer() {8, 4}, "Seccion", "Sección", "Grupo")
        Dim idxEspecialidad As Integer = ResolverIndiceColumnaConFallback(tablaExcel, New Integer() {10, 5}, "Especialidad", "Especilidad", "EspecialidadAcademica", "Especialidad Académica", "Especialidad Academica")
        Dim idxFechaNac As Integer = ResolverIndiceColumnaConFallback(tablaExcel, New Integer() {5, 6}, "FechaNac", "Fecha Nacimiento", "FechaNacimiento", "Nacimiento")
        Dim idxTelefono As Integer = ResolverIndiceColumnaConFallback(tablaExcel, New Integer() {11, 12, 13, 8}, "Telefono", "Teléfono", "Telefono Estudiante", "Teléfono Estudiante", "Celular", "Contacto 1", "Contacto1", "Contacto 2", "Contacto2")
        Dim idxSexo As Integer = ResolverIndiceColumnaConFallback(tablaExcel, New Integer() {4}, "Sexo", "Genero", "Género", "Sex")
        Dim idxEstado As Integer = ResolverIndiceColumnaConFallback(tablaExcel, New Integer() {22}, "Estado", "Status", "Condicion", "Condición")

        Dim columnasFaltantes As New List(Of String)()
        If idxCedula < 0 Then
            columnasFaltantes.Add("Cédula")
        End If
        If idxPrimerApellido < 0 Then
            columnasFaltantes.Add("Primer Apellido (Title)")
        End If
        If idxSegundoApellido < 0 Then
            columnasFaltantes.Add("Segundo Apellido")
        End If
        If idxNombre < 0 Then
            columnasFaltantes.Add("Nombre")
        End If
        If idxSeccion < 0 Then
            columnasFaltantes.Add("Sección")
        End If
        If idxEspecialidad < 0 Then
            columnasFaltantes.Add("Especialidad")
        End If
        If idxFechaNac < 0 Then
            columnasFaltantes.Add("FechaNacimiento")
        End If
        If idxTelefono < 0 Then
            columnasFaltantes.Add("Teléfono Estudiante")
        End If
        If idxSexo < 0 Then
            columnasFaltantes.Add("Género")
        End If
        If idxEstado < 0 Then
            columnasFaltantes.Add("Estado")
        End If

        If columnasFaltantes.Count > 0 Then
            Throw New InvalidOperationException("La plantilla Excel no contiene columnas obligatorias: " &
                String.Join(", ", columnasFaltantes.ToArray()) &
                ". Encabezados detectados: " & ObtenerEncabezados(tablaExcel))
        End If

        Dim cedulasImportadas As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Dim omitidasCedula As Integer = 0
        Dim omitidasDuplicadas As Integer = 0
        Dim omitidasEstado As Integer = 0
        Dim omitidasFecha As Integer = 0

        For Each row As DataRow In tablaExcel.Rows
            Dim estado As String = NormalizarTextoComparacion(LeerValor(row, idxEstado, -1))
            If estado <> "regular" Then
                omitidasEstado += 1
                Continue For
            End If

            Dim cedula As String = NormalizarCedula(LeerValor(row, idxCedula, -1))
            If Not EsFilaImportable(cedula) Then
                omitidasCedula += 1
                Continue For
            End If

            If cedulasImportadas.Contains(cedula) Then
                omitidasDuplicadas += 1
                Continue For
            End If

            Dim nueva As DataRow = dt.NewRow()
            nueva("Cedula") = cedula
            nueva("PrimerApellido") = LeerValor(row, idxPrimerApellido, 1)
            nueva("SegundoApellido") = LeerValor(row, idxSegundoApellido, 2)
            nueva("Nombre") = LeerValor(row, idxNombre, 3)
            nueva("Seccion") = LeerValor(row, idxSeccion, 8)
            nueva("Especialidad") = LeerValor(row, idxEspecialidad, 10)
            Dim tieneFechaNac As Boolean = False
            nueva("FechaNac") = ParseFechaNullable(LeerIndiceObjeto(row, idxFechaNac), tieneFechaNac)
            If Not tieneFechaNac Then
                omitidasFecha += 1
            End If
            nueva("Telefono") = LeerValor(row, idxTelefono, 11)
            nueva("Sexo") = ParseSexo(LeerIndiceObjeto(row, idxSexo))
            dt.Rows.Add(nueva)
            cedulasImportadas.Add(cedula)
        Next

        metricas.FilasValidas = dt.Rows.Count
        metricas.FilasOmitidasCedula = omitidasCedula
        metricas.FilasDuplicadas = omitidasDuplicadas
        metricas.FilasOmitidasEstado = omitidasEstado
        metricas.FilasSinFechaNac = omitidasFecha

        resumen = "Estructura validada. Filas origen: " & tablaExcel.Rows.Count.ToString(CultureInfo.InvariantCulture) &
            ", válidas: " & dt.Rows.Count.ToString(CultureInfo.InvariantCulture) &
            ", cédula inválida: " & omitidasCedula.ToString(CultureInfo.InvariantCulture) &
            ", duplicadas: " & omitidasDuplicadas.ToString(CultureInfo.InvariantCulture) &
            ", estado distinto de Regular: " & omitidasEstado.ToString(CultureInfo.InvariantCulture) &
            ", sin fecha nacimiento: " & omitidasFecha.ToString(CultureInfo.InvariantCulture) & "."

        Return dt
    End Function

    Private Function NormalizarCedula(ByVal raw As String) As String
        If String.IsNullOrWhiteSpace(raw) Then
            Return String.Empty
        End If

        Dim sb As New System.Text.StringBuilder(raw.Length)
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

        If c.Length < 4 Then
            Return False
        End If

        Return True
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

        If texto = "m" OrElse texto = "femenino" OrElse texto = "mujer" Then
            Return 2
        End If

        If texto = "h" OrElse texto = "masculino" OrElse texto = "hombre" Then
            Return 1
        End If

        Return 0
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
        If String.IsNullOrWhiteSpace(nombre) Then
            Return String.Empty
        End If

        Dim normalizado As String = nombre.ToLowerInvariant()
        Return normalizado.Replace(" ", String.Empty).
            Replace("_", String.Empty).
            Replace("-", String.Empty).
            Replace("á", "a").
            Replace("é", "e").
            Replace("í", "i").
            Replace("ó", "o").
            Replace("ú", "u").
            Trim()
    End Function

    Private Function NormalizarTextoComparacion(ByVal valor As String) As String
        If String.IsNullOrWhiteSpace(valor) Then
            Return String.Empty
        End If

        Return valor.ToLowerInvariant().
            Replace("á", "a").
            Replace("é", "e").
            Replace("í", "i").
            Replace("ó", "o").
            Replace("ú", "u").
            Trim()
    End Function

    Private Function LeerValor(ByVal row As DataRow, ByVal indicePreferido As Integer, ByVal indiceAlterno As Integer) As String
        Dim valor As String = LeerIndice(row, indicePreferido)
        If valor.Length > 0 Then
            Return valor
        End If
        Return LeerIndice(row, indiceAlterno)
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

    Private Function ObtenerEncabezados(ByVal tabla As DataTable) As String
        If tabla Is Nothing OrElse tabla.Columns Is Nothing OrElse tabla.Columns.Count = 0 Then
            Return "(sin encabezados)"
        End If

        Dim nombres As New List(Of String)()
        For Each col As DataColumn In tabla.Columns
            nombres.Add(col.ColumnName)
        Next
        Return String.Join(", ", nombres.ToArray())
    End Function

    Private Sub EjecutarImportacion(ByVal tabla As DataTable)
        If tabla Is Nothing OrElse tabla.Rows.Count = 0 Then
            LblEstado.Text = "No hay registros válidos para importar."
            Progreso.Value = 0
            MsgBox("No se encontraron registros válidos para importar.", MsgBoxStyle.Exclamation)
            Exit Sub
        End If

        Dim cn As New SqlClient.SqlConnection()
        Dim pTransac As SqlClient.SqlTransaction = Nothing
        Dim tipoUsuario As Integer = ObtenerTipoUsuario()
        Dim idHorario As Integer = ObtenerIdHorarioSeleccionado()
        Dim cfg As ParametroSistemaService.ParametroSistemaConfig = Nothing
        Dim desactivarNoImportados As Boolean = True
        Dim auditarImportacion As Boolean = True
        Dim metrica As ImportacionMetricas = ObtenerMetricasConFallback(tabla)
        Dim usuarioEjecucion As String = If(String.IsNullOrWhiteSpace(NombreUsuario), Environment.UserName, NombreUsuario)
        Dim mensajeAuditoria As String = String.Empty

        Try
            LblEstado.Text = "Preparando transacción..."
            Progreso.Style = ProgressBarStyle.Continuous
            Progreso.Maximum = 4
            Progreso.Value = 0
            Refresh()

            Cls.AbrirConexion(cn, False)
            cfg = ObtenerConfigImportacion(cn)
            If cfg IsNot Nothing Then
                desactivarNoImportados = cfg.DesactivarNoImportadosExcel
                auditarImportacion = cfg.AuditarImportacionExcel
            End If
            pTransac = cn.BeginTransaction()

            ImportSvc.MarcarUsuariosComoNoActualizados(cn, pTransac, tipoUsuario, idHorario)
            Progreso.Value = 1
            Refresh()

            LblEstado.Text = "Importando datos en lote..."
            Refresh()

            ImportSvc.ImportarUsuariosNormalizadosEnLote(tabla, tipoUsuario, idHorario, cn, pTransac)
            Progreso.Value = 3
            Refresh()

            If desactivarNoImportados Then
                ImportSvc.DesactivarNoActualizados(cn, pTransac, tipoUsuario, idHorario)
            End If

            mensajeAuditoria = "Importacion completada. Filas validas=" & metrica.FilasValidas.ToString(CultureInfo.InvariantCulture) &
                ", omitidas estado=" & metrica.FilasOmitidasEstado.ToString(CultureInfo.InvariantCulture) &
                ", omitidas cedula=" & metrica.FilasOmitidasCedula.ToString(CultureInfo.InvariantCulture) &
                ", duplicadas=" & metrica.FilasDuplicadas.ToString(CultureInfo.InvariantCulture) &
                ", sin fecha nacimiento=" & metrica.FilasSinFechaNac.ToString(CultureInfo.InvariantCulture) &
                ", desactivar no importados=" & desactivarNoImportados.ToString()

            If auditarImportacion Then
                Dim auditoriaOk As New ImportacionExcelService.ImportacionAuditoria()
                auditoriaOk.ArchivoOrigen = _archivoExcelSeleccionado
                auditoriaOk.TipoUsuario = tipoUsuario
                auditoriaOk.IdHorario = idHorario
                auditoriaOk.FilasOrigen = metrica.FilasOrigen
                auditoriaOk.FilasValidas = metrica.FilasValidas
                auditoriaOk.FilasOmitidasEstado = metrica.FilasOmitidasEstado
                auditoriaOk.FilasOmitidasCedula = metrica.FilasOmitidasCedula
                auditoriaOk.FilasDuplicadas = metrica.FilasDuplicadas
                auditoriaOk.FilasSinFechaNac = metrica.FilasSinFechaNac
                auditoriaOk.DesactivarNoImportados = desactivarNoImportados
                auditoriaOk.Exito = True
                auditoriaOk.Mensaje = mensajeAuditoria
                auditoriaOk.UsuarioEjecucion = usuarioEjecucion
                Try
                    ImportSvc.RegistrarAuditoriaImportacion(cn, pTransac, auditoriaOk)
                Catch exAudit As Exception
                    ErrorLogger.LogException("FrmImportarExcel.EjecutarImportacion.AuditoriaOk", exAudit)
                End Try
            End If

            Cls.CerrarConexion(cn, pTransac)

            ErrorLogger.LogInfo("FrmImportarExcel.EjecutarImportacion", mensajeAuditoria)
            LblEstado.Text = "Importación finalizada correctamente."
            Progreso.Value = Progreso.Maximum
            Refresh()
            MsgBox("Importación concluida con éxito. Registros procesados: " & tabla.Rows.Count.ToString(CultureInfo.InvariantCulture), MsgBoxStyle.Information)
            Me.Dispose()
        Catch ex As Exception
            Progreso.Value = 0
            If pTransac IsNot Nothing Then
                Cls.RollSQL(pTransac)
            End If
            If cn.State = ConnectionState.Open Then
                Cls.CerrarConexion(cn)
            End If
            Dim mensajeError As String = "Error al ejecutar la importación: " & ex.Message
            ErrorLogger.LogException("FrmImportarExcel.EjecutarImportacion", ex, "Archivo=" & _archivoExcelSeleccionado)

            If auditarImportacion Then
                RegistrarAuditoriaFallida(_archivoExcelSeleccionado, tipoUsuario, idHorario, metrica, desactivarNoImportados, usuarioEjecucion, mensajeError)
            End If

            MsgBox("Error al ejecutar la importación: " & ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Function ObtenerMetricasConFallback(ByVal tabla As DataTable) As ImportacionMetricas
        If _ultimaMetrica IsNot Nothing Then
            Return _ultimaMetrica
        End If

        Dim m As New ImportacionMetricas()
        m.FilasOrigen = If(tabla Is Nothing, 0, tabla.Rows.Count)
        m.FilasValidas = If(tabla Is Nothing, 0, tabla.Rows.Count)
        m.FilasOmitidasEstado = 0
        m.FilasOmitidasCedula = 0
        m.FilasDuplicadas = 0
        m.FilasSinFechaNac = 0
        Return m
    End Function

    Private Function ObtenerConfigImportacion(ByVal cn As SqlClient.SqlConnection) As ParametroSistemaService.ParametroSistemaConfig
        ParametroSvc.AsegurarEsquema(cn)
        ParametroSvc.CrearFila1(cn)
        Return ParametroSvc.ObtenerFila1(cn)
    End Function

    Private Sub RegistrarAuditoriaFallida(ByVal archivo As String,
                                          ByVal tipoUsuario As Integer,
                                          ByVal idHorario As Integer,
                                          ByVal metrica As ImportacionMetricas,
                                          ByVal desactivarNoImportados As Boolean,
                                          ByVal usuarioEjecucion As String,
                                          ByVal mensaje As String)
        Dim cnAudit As New SqlClient.SqlConnection()
        Try
            Cls.AbrirConexion(cnAudit, False)
            Dim auditoriaErr As New ImportacionExcelService.ImportacionAuditoria()
            auditoriaErr.ArchivoOrigen = archivo
            auditoriaErr.TipoUsuario = tipoUsuario
            auditoriaErr.IdHorario = idHorario
            auditoriaErr.FilasOrigen = metrica.FilasOrigen
            auditoriaErr.FilasValidas = metrica.FilasValidas
            auditoriaErr.FilasOmitidasEstado = metrica.FilasOmitidasEstado
            auditoriaErr.FilasOmitidasCedula = metrica.FilasOmitidasCedula
            auditoriaErr.FilasDuplicadas = metrica.FilasDuplicadas
            auditoriaErr.FilasSinFechaNac = metrica.FilasSinFechaNac
            auditoriaErr.DesactivarNoImportados = desactivarNoImportados
            auditoriaErr.Exito = False
            auditoriaErr.Mensaje = mensaje
            auditoriaErr.UsuarioEjecucion = usuarioEjecucion
            ImportSvc.RegistrarAuditoriaImportacion(cnAudit, Nothing, auditoriaErr)
            Cls.CerrarConexion(cnAudit)
        Catch ex As Exception
            ErrorLogger.LogException("FrmImportarExcel.RegistrarAuditoriaFallida", ex)
            If cnAudit.State = ConnectionState.Open Then
                Cls.CerrarConexion(cnAudit)
            End If
        End Try
    End Sub

    Private Sub CargaHorarios(ByRef combo As ComboBox)
        Try
            Dim cn As New SqlClient.SqlConnection()

            Cls.AbrirConexion(cn, False)
            Dim ds As DataSet = ImportSvc.ObtenerHorariosActivos(cn)

            combo.Items.Clear()
            combo.Items.Add(New LBItem("0", " SELECCIONE -"))

            If ds.Tables.Count > 0 AndAlso ds.Tables(0).Rows.Count > 0 Then
                For i As Integer = 0 To ds.Tables(0).Rows.Count - 1
                    combo.Items.Add(New LBItem(CStr(ds.Tables(0).Rows(i)!IdHorario), CStr(ds.Tables(0).Rows(i)!Descripcion)))
                Next
            End If

            Cls.CerrarConexion(cn)
            combo.SelectedIndex = 0
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub
End Class
