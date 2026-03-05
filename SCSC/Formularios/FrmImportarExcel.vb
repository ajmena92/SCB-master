Imports System.Data
Imports System.Data.OleDb
Imports System.Globalization
Imports System.IO
Imports System.Collections.Generic

Public Class FrmImportarExcel
    Private ReadOnly Cls As New FuncionesDB()
    Private ReadOnly ImportSvc As New ImportacionExcelService(Cls)
    Private ReadOnly OpenFileDialog As New OpenFileDialog()

    Private _lblArchivoExcel As Label
    Private _txtArchivoExcel As TextBox
    Private _btnArchivoExcel As Button
    Private _lblInfo As Label
    Private _lblHorario As Label
    Private _archivoExcelSeleccionado As String = String.Empty

    Private Sub FrmImportarExcel_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            UIThemeManagerV2.Apply(Me, "operativo")
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

        Panel4.BackColor = Color.Transparent
        Panel4.BorderStyle = BorderStyle.None

        Label1.Font = New Font("Segoe UI Semibold", 20.0!, FontStyle.Bold)
        Label1.ForeColor = UIConstants.TextPrimary
        Label1.Text = "Importación desde Excel"

        GroupBox1.BackColor = UIConstants.Surface
        GroupBox1.ForeColor = UIConstants.TextPrimary
        GroupBox1.Font = UIConstants.FontBodyStrong()
        GroupBox1.Text = "Configuración de importación"

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

        ConfigurarGridPreview(DGV1)
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
        _lblInfo.Text = "Seleccione un archivo .xlsx/.xls/.xlsm con hoja 'Lista' (o la primera hoja disponible)."

        _lblHorario = New Label()
        _lblHorario.AutoSize = True
        _lblHorario.Text = "Horario lectivo:"
        _lblHorario.Font = New Font("Segoe UI", 10.0!, FontStyle.Bold)
        _lblHorario.ForeColor = UIConstants.TextPrimary

        AddHandler _btnArchivoExcel.Click, AddressOf BtnArchivoExcel_Click

        GroupBox1.Controls.Add(_lblArchivoExcel)
        GroupBox1.Controls.Add(_txtArchivoExcel)
        GroupBox1.Controls.Add(_btnArchivoExcel)
        GroupBox1.Controls.Add(_lblInfo)
        GroupBox1.Controls.Add(_lblHorario)
    End Sub

    Private Sub RecalcularLayoutImportacion()
        If Me.ClientSize.Width <= 0 OrElse Me.ClientSize.Height <= 0 Then
            Exit Sub
        End If

        Dim margen As Integer = 24
        Dim spacing As Integer = 12

        Label1.SetBounds(margen, 16, Me.ClientSize.Width - (margen * 2), 38)
        Label1.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right

        GroupBox1.SetBounds(margen, Label1.Bottom + spacing, Me.ClientSize.Width - (margen * 2), 170)
        GroupBox1.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right

        RecalcularLayoutGrupoConfiguracion()

        LblEstado.SetBounds(margen, GroupBox1.Bottom + 8, Me.ClientSize.Width - (margen * 2), 26)
        LblEstado.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right

        Progreso.SetBounds(margen, LblEstado.Bottom + 4, Me.ClientSize.Width - (margen * 2), 20)
        Progreso.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right

        Panel4.SetBounds(Me.ClientSize.Width - margen - 366, Me.ClientSize.Height - 56, 366, 38)
        Panel4.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right

        BtnGuardar.SetBounds(0, 0, 146, 38)
        BtnCancelar.SetBounds(154, 0, 102, 38)
        BtnRegresar.SetBounds(264, 0, 102, 38)

        Dim topGrid As Integer = Progreso.Bottom + 10
        Dim bottomGrid As Integer = Panel4.Top - 10
        Dim gridHeight As Integer = Math.Max(140, bottomGrid - topGrid)
        DGV1.SetBounds(margen, topGrid, Me.ClientSize.Width - (margen * 2), gridHeight)
        DGV1.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
    End Sub

    Private Sub RecalcularLayoutGrupoConfiguracion()
        If _lblArchivoExcel Is Nothing Then
            Exit Sub
        End If

        Dim leftLabel As Integer = 16
        Dim leftControl As Integer = 150
        Dim groupWidth As Integer = GroupBox1.ClientSize.Width

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
        OpenFileDialog.Filter = "Archivos Excel (*.xlsx;*.xls;*.xlsm)|*.xlsx;*.xls;*.xlsm|Todos (*.*)|*.*"
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
        DGV1.DataSource = Nothing
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
            Dim normalizada As DataTable = NormalizarDesdeExcel(tablaExcel)
            DGV1.DataSource = normalizada
            EjecutarImportacion(normalizada)
        Finally
            Progreso.Style = ProgressBarStyle.Continuous
        End Try
    End Sub

    Private Function LeerTablaExcel(ByVal rutaExcel As String) As DataTable
        If Not File.Exists(rutaExcel) Then
            Throw New FileNotFoundException("No se encontró el archivo seleccionado.", rutaExcel)
        End If

        Using conn As New OleDbConnection(ObtenerCadenaConexionExcel(rutaExcel))
            conn.Open()

            Dim hoja As String = ResolverNombreHoja(conn)
            If String.IsNullOrWhiteSpace(hoja) Then
                Throw New InvalidOperationException("No se encontró ninguna hoja de datos en el archivo Excel.")
            End If

            Dim sql As String = "SELECT * FROM [" & hoja & "]"
            Using dta As New OleDbDataAdapter(sql, conn)
                Dim dt As New DataTable()
                dta.Fill(dt)
                Return dt
            End Using
        End Using
    End Function

    Private Function ObtenerCadenaConexionExcel(ByVal rutaExcel As String) As String
        Dim extension As String = Path.GetExtension(rutaExcel).ToLowerInvariant()
        If extension = ".xls" Then
            Return "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" & rutaExcel & ";Extended Properties='Excel 8.0;HDR=YES;IMEX=1;'"
        End If

        Return "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" & rutaExcel & ";Extended Properties='Excel 12.0 Xml;HDR=YES;IMEX=1;'"
    End Function

    Private Function ResolverNombreHoja(ByVal conn As OleDbConnection) As String
        Dim schema As DataTable = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, Nothing)
        If schema Is Nothing OrElse schema.Rows.Count = 0 Then
            Return String.Empty
        End If

        Dim hojaPreferida As String = String.Empty
        Dim primeraHoja As String = String.Empty

        For Each row As DataRow In schema.Rows
            Dim nombre As String = Convert.ToString(row("TABLE_NAME"))
            If String.IsNullOrWhiteSpace(nombre) Then
                Continue For
            End If

            Dim n As String = nombre.Trim()
            If n.EndsWith("$") OrElse n.EndsWith("$'") Then
                If primeraHoja.Length = 0 Then
                    primeraHoja = n
                End If

                Dim nUpper As String = n.ToUpperInvariant()
                If nUpper = "LISTA$" OrElse nUpper = "'LISTA$'" Then
                    hojaPreferida = n
                    Exit For
                End If
            End If
        Next

        If hojaPreferida.Length > 0 Then
            Return hojaPreferida
        End If

        If primeraHoja.Length > 0 Then
            Return primeraHoja
        End If

        Return String.Empty
    End Function

    Private Function NormalizarDesdeExcel(ByVal tablaExcel As DataTable) As DataTable
        Dim dt As New DataTable("Importacion")
        dt.Columns.Add("Cedula", GetType(String))
        dt.Columns.Add("PrimerApellido", GetType(String))
        dt.Columns.Add("SegundoApellido", GetType(String))
        dt.Columns.Add("Nombre", GetType(String))
        dt.Columns.Add("Seccion", GetType(String))
        dt.Columns.Add("Especialidad", GetType(String))
        dt.Columns.Add("FechaNac", GetType(DateTime))
        dt.Columns.Add("Telefono", GetType(String))

        Dim idxCedula As Integer = ResolverIndiceColumna(tablaExcel, "Cedula", "Cédula", "Identificacion", "Identificación", "Ced")
        Dim idxPrimerApellido As Integer = ResolverIndiceColumna(tablaExcel, "PrimerApellido", "Apellido1", "Primer Apellido")
        Dim idxSegundoApellido As Integer = ResolverIndiceColumna(tablaExcel, "SegundoApellido", "Apellido2", "Segundo Apellido")
        Dim idxNombre As Integer = ResolverIndiceColumna(tablaExcel, "Nombre", "Nombres")
        Dim idxSeccion As Integer = ResolverIndiceColumna(tablaExcel, "Seccion", "Sección", "Grupo")
        Dim idxEspecialidad As Integer = ResolverIndiceColumna(tablaExcel, "Especialidad", "EspecialidadAcademica", "Especialidad Académica")
        Dim idxFechaNac As Integer = ResolverIndiceColumna(tablaExcel, "FechaNac", "Fecha Nacimiento", "FechaNacimiento")
        Dim idxTelefono As Integer = ResolverIndiceColumna(tablaExcel, "Telefono", "Teléfono", "Celular")

        Dim cedulasImportadas As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        For Each row As DataRow In tablaExcel.Rows
            Dim cedula As String = NormalizarCedula(LeerValor(row, idxCedula, 0))
            If Not EsFilaImportable(cedula) Then
                Continue For
            End If

            If cedulasImportadas.Contains(cedula) Then
                Continue For
            End If

            Dim nueva As DataRow = dt.NewRow()
            nueva("Cedula") = cedula
            nueva("PrimerApellido") = LeerValor(row, idxPrimerApellido, 1)
            nueva("SegundoApellido") = LeerValor(row, idxSegundoApellido, 2)
            nueva("Nombre") = LeerValor(row, idxNombre, 3)
            nueva("Seccion") = LeerValor(row, idxSeccion, 4)
            nueva("Especialidad") = LeerValor(row, idxEspecialidad, 5)
            nueva("FechaNac") = ParseFecha(LeerValor(row, idxFechaNac, 6))
            nueva("Telefono") = LeerValor(row, idxTelefono, 8)
            dt.Rows.Add(nueva)
            cedulasImportadas.Add(cedula)
        Next

        Return dt
    End Function

    Private Function NormalizarCedula(ByVal raw As String) As String
        If String.IsNullOrWhiteSpace(raw) Then
            Return String.Empty
        End If
        Return raw.Replace("-", String.Empty).Replace(" ", String.Empty).Trim()
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

    Private Function ParseFecha(ByVal raw As String) As DateTime
        If String.IsNullOrWhiteSpace(raw) Then
            Return DateTime.Now.Date
        End If

        Dim fecha As DateTime
        Dim formatos As String() = {"dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-M-yyyy", "yyyy-MM-dd", "MM/dd/yyyy", "M/d/yyyy"}
        If DateTime.TryParseExact(raw.Trim(), formatos, CultureInfo.GetCultureInfo("es-CR"), DateTimeStyles.None, fecha) Then
            Return fecha
        End If

        If DateTime.TryParseExact(raw.Trim(), formatos, CultureInfo.InvariantCulture, DateTimeStyles.None, fecha) Then
            Return fecha
        End If

        If DateTime.TryParse(raw, fecha) Then
            Return fecha
        End If

        Dim oa As Double
        If Double.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, oa) Then
            Try
                Return DateTime.FromOADate(oa)
            Catch
            End Try
        End If

        Return DateTime.Now.Date
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

    Private Function LeerValor(ByVal row As DataRow, ByVal indicePreferido As Integer, ByVal indiceAlterno As Integer) As String
        Dim valor As String = LeerIndice(row, indicePreferido)
        If valor.Length > 0 Then
            Return valor
        End If
        Return LeerIndice(row, indiceAlterno)
    End Function

    Private Function LeerIndice(ByVal row As DataRow, ByVal index As Integer) As String
        If row Is Nothing OrElse row.Table Is Nothing OrElse row.Table.Columns.Count <= index Then
            Return String.Empty
        End If

        If row.IsNull(index) Then
            Return String.Empty
        End If

        Return Convert.ToString(row(index)).Trim()
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
        Dim contador As Integer = 0
        Dim tipoUsuario As Integer = ObtenerTipoUsuario()
        Dim idHorario As Integer = ObtenerIdHorarioSeleccionado()

        Try
            LblEstado.Text = "Preparando transacción..."
            Progreso.Style = ProgressBarStyle.Continuous
            Progreso.Maximum = tabla.Rows.Count
            Progreso.Step = 1
            Progreso.Value = 0
            Refresh()

            Cls.AbrirConexion(cn, True, pTransac)
            ImportSvc.MarcarUsuariosComoNoActualizados(cn, pTransac, tipoUsuario, idHorario)

            LblEstado.Text = "Importando datos..."
            Refresh()

            For Each row As DataRow In tabla.Rows
                ImportSvc.GuardarUsuarioNormalizado(row, tipoUsuario, idHorario, cn, pTransac)
                If Progreso.Value < Progreso.Maximum Then
                    Progreso.Value += 1
                End If

                contador += 1
                If (contador Mod 25) = 0 Then
                    Application.DoEvents()
                End If
            Next

            ImportSvc.DesactivarNoActualizados(cn, pTransac, tipoUsuario, idHorario)
            Cls.CerrarConexion(cn, pTransac)

            LblEstado.Text = "Importación finalizada correctamente."
            Progreso.Value = tabla.Rows.Count
            Refresh()
            MsgBox("Importación concluida con éxito.", MsgBoxStyle.Information)
            Me.Dispose()
        Catch ex As Exception
            Progreso.Value = 0
            If pTransac IsNot Nothing Then
                Cls.RollSQL(pTransac)
            End If
            If cn.State = ConnectionState.Open Then
                Cls.CerrarConexion(cn)
            End If
            MsgBox("Error al actualizar registro " & contador.ToString() & ": " & ex.Message, MsgBoxStyle.Critical)
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
