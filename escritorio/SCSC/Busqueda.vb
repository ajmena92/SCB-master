Option Strict On
Option Explicit On

Public Class Busqueda
    Public Property Request As SearchRequest
    Public ReadOnly Property SelectedValues As String()
        Get
            Return _selectedValues
        End Get
    End Property

    Dim RegistroSeleccionado As Integer = -1
    Private _activeRequest As SearchRequest
    Private _currentFilterField As String = String.Empty
    Private _selectedValues() As String = New String() {}
    Private _layoutReady As Boolean = False
    ' Dim ColumnaSeleccionada As FuncionesDB.Campos

    Sub CargarGrid()
        Try
            Dim Cls As New FuncionesDB
            Dim Cn As New SqlClient.SqlConnection
            Dim ds As DataSet
            Cls.AbrirConexion(Cn, False)
            ds = Cls.Consultar(_activeRequest.TableName, _activeRequest.Values, _activeRequest.Keys, Cn, , _activeRequest.OrderBy)
            GridConsulta.DataSource = ds
            GridConsulta.DataMember = _activeRequest.TableName

            ' Tamaño de celdas
            GridConsulta.Columns(0).Width = 75
            GridConsulta.Columns(0).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
            ' GridConsulta.Columns(1).Width = 80
            ' GridConsulta.Columns(1).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter

            'GridConsulta.Columns(3).Width = 100
            'GridConsulta.Columns(3).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft
            ' End If
            'GridConsulta.Columns(3).DefaultCellStyle.Format = "hh:mm:ss"
            'GridConsulta.Columns(1).AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells

            For I As Integer = 0 To UBound(_activeRequest.Values)
                GridConsulta.Columns(I).DefaultCellStyle.Format = _activeRequest.Values(I).Formato
                If _activeRequest.Values(I).Nombre = "" Then
                Else
                    GridConsulta.Columns(I).HeaderText = Convert.ToString(_activeRequest.Values(I).Valor)
                End If
            Next
            UpdateSelectionState()
            Cls.CerrarConexion(Cn)
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
            LblMensajes.Text = ex.Message
        End Try
    End Sub
    Private Sub GridConsulta_CellClick(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles GridConsulta.CellClick
        Try
            RegistroSeleccionado = e.RowIndex
            _currentFilterField = GridConsulta.Columns(e.ColumnIndex).Name
            LblFiltrado.Text = "Filtrado x " & GridConsulta.Columns(e.ColumnIndex).HeaderText
        Catch ex As Exception
            MsgBox(ex.Message)
            'LblMensajes.Text = ex.Message
        End Try
    End Sub

    Private Sub GridConsulta_CellContentClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles GridConsulta.CellContentClick
        Try
            RegistroSeleccionado = e.RowIndex
            _currentFilterField = GridConsulta.Columns(e.ColumnIndex).Name
            LblFiltrado.Text = "Filtrado x " & GridConsulta.Columns(e.ColumnIndex).HeaderText
        Catch ex As Exception
            ' MsgBox(ex.Message)
            ' LblMensajes.Text = ex.Message
        End Try
    End Sub
    Private Sub GridConsulta_CellDoubleClick(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles GridConsulta.CellDoubleClick
        RegistroSeleccionado = e.RowIndex
        Guardar_Click(sender, e)
    End Sub

    Private Sub GridConsulta_ColumnHeaderMouseClick(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellMouseEventArgs) Handles GridConsulta.ColumnHeaderMouseClick
        _currentFilterField = GridConsulta.Columns(e.ColumnIndex).Name
        LblFiltrado.Text = "Filtrado x " & GridConsulta.Columns(e.ColumnIndex).HeaderText
        For I As Integer = 0 To UBound(_activeRequest.Values)
            If String.Equals(_activeRequest.Values(I).Nombre, _currentFilterField, StringComparison.OrdinalIgnoreCase) Then
                If String.IsNullOrEmpty(Convert.ToString(_activeRequest.Values(I).Valor)) Then

                Else
                    _currentFilterField = _activeRequest.Values(I).Nombre
                End If

                Exit For
            End If
        Next
    End Sub
    Private Sub Guardar_Click(sender As Object, e As EventArgs) Handles Guardar.Click
        Try
            If RegistroSeleccionado >= 0 And GridConsulta.Rows.Count - 1 >= RegistroSeleccionado Then
                _selectedValues = ObtenerValoresSeleccionados()
                Me.DialogResult = DialogResult.OK
                Me.Close()
            ElseIf GridConsulta.Rows.Count = 0 Then
                MsgBox("No hay registros para Seleccionar.", MsgBoxStyle.Information)
                LblMensajes.Text = "No hay registros para Seleccionar."
            Else
                MsgBox("Seleccione un Registro.", MsgBoxStyle.Information)
                LblMensajes.Text = "Seleccione un Registro."
            End If

        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
            LblMensajes.Text = ex.Message
        End Try
    End Sub

    Private Sub Cancelar_Click(sender As Object, e As EventArgs) Handles Cancelar.Click
        _selectedValues = New String() {}
        Me.DialogResult = DialogResult.Cancel
        Me.Dispose()
    End Sub

    Private Sub BtnFiltro_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BtnFiltro.Click
        Try
            Dim Cls As New FuncionesDB
            Dim Cn As New SqlClient.SqlConnection
            Dim Llave() As FuncionesDB.Campos
            Dim ds As DataSet
            Cls.AbrirConexion(Cn, False)
            Llave = Cls.InicializarArray
            Cls.ArmaValor(Llave, _currentFilterField, "%" & TxtFiltro.Text & "%")
            ds = Cls.Consultar(_activeRequest.TableName, _activeRequest.Values, Llave, Cn, , _activeRequest.OrderBy)
            GridConsulta.DataSource = ds
            GridConsulta.DataMember = _activeRequest.TableName
            UpdateSelectionState()
            Cls.CerrarConexion(Cn)
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
            LblMensajes.Text = ex.Message
        End Try
    End Sub

    Private Sub Busqueda_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If CrudVisualHelper.IsInDesignMode(Me) Then
            Return
        End If
        Try
            If Request Is Nothing Then
                Throw New InvalidOperationException("Busqueda requiere un SearchRequest explícito.")
            End If
            _activeRequest = Request
            _currentFilterField = If(String.IsNullOrWhiteSpace(_activeRequest.DefaultFilterField), "Nombre", _activeRequest.DefaultFilterField)
            CrudVisualHelper.ApplyCrudStandard(Me, "dialogo")
            ApplyModernSearchLayout()
            LblTitulo.Text = _activeRequest.Title
            LblFiltrado.Text = "Buscar por nombre o descripcion"
            CargarGrid()

            BtnFiltro.PerformClick()
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
            LblMensajes.Text = ex.Message
        End Try
    End Sub

    Private Sub TxtFiltro_KeyDown(sender As Object, e As KeyEventArgs) Handles TxtFiltro.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True ' para evitar un 'beep'
            BtnFiltro.PerformClick()
            TxtFiltro.Focus()
        End If
    End Sub

    Private Sub TxtFiltro_Click(sender As Object, e As EventArgs) Handles TxtFiltro.Click, TxtFiltro.Enter
        TxtFiltro.SelectAll()
    End Sub

    Private Sub LblTitulo_Click(sender As Object, e As EventArgs) Handles LblTitulo.Click

    End Sub

    Private Sub ApplyModernSearchLayout()
        Me.BackgroundImage = Nothing
        Me.FormBorderStyle = FormBorderStyle.FixedSingle
        Me.MinimumSize = New Size(820, 560)
        Me.ClientSize = New Size(980, 560)
        Me.Padding = New Padding(0)

        LblTitulo.Font = New Font("Segoe UI Semibold", 25.0!, FontStyle.Bold)
        LblTitulo.ForeColor = UIConstants.TextPrimary
        LblTitulo.AutoSize = True
        LblTitulo.MaximumSize = New Size(Math.Max(420, Me.ClientSize.Width - 40), 0)

        LblFiltrado.Font = New Font("Segoe UI", 10.0!, FontStyle.Regular)
        LblFiltrado.ForeColor = UIConstants.TextSecondary
        LblFiltrado.AutoSize = True
        LblMensajes.ForeColor = UIConstants.TextSecondary
        LblMensajes.Font = New Font("Segoe UI", 9.0!, FontStyle.Regular)
        LblMensajes.AutoEllipsis = True
        LblSeleccion.ForeColor = UIConstants.TextSecondary
        LblSeleccion.Font = New Font("Segoe UI Semibold", 9.0!, FontStyle.Bold)

        TxtFiltro.BackColor = UIConstants.Surface
        TxtFiltro.ForeColor = UIConstants.TextPrimary
        TxtFiltro.BorderStyle = BorderStyle.FixedSingle
        TxtFiltro.Font = UIConstants.FontBody()

        BtnFiltro.BackColor = UIConstants.Surface
        BtnFiltro.ForeColor = UIConstants.TextPrimary
        BtnFiltro.FlatStyle = FlatStyle.Flat
        BtnFiltro.FlatAppearance.BorderColor = UIConstants.Border
        BtnFiltro.FlatAppearance.BorderSize = 1
        BtnFiltro.FlatAppearance.MouseOverBackColor = UIConstants.SurfaceAlt
        BtnFiltro.FlatAppearance.MouseDownBackColor = UIConstants.SurfaceAlt
        BtnFiltro.Font = UIConstants.FontBodyStrong()
        BtnFiltro.Text = "Buscar"
        BtnFiltro.TextImageRelation = TextImageRelation.ImageBeforeText
        BtnFiltro.ImageAlign = ContentAlignment.MiddleLeft
        BtnFiltro.Height = 36
        BtnFiltro.Width = 112

        Guardar.Text = "Seleccionar"
        Guardar.BackColor = Color.FromArgb(36, 112, 191)
        Guardar.ForeColor = Color.White
        Guardar.FlatStyle = FlatStyle.Flat
        Guardar.FlatAppearance.BorderColor = Color.FromArgb(31, 99, 171)
        Guardar.FlatAppearance.BorderSize = 1
        Guardar.FlatAppearance.MouseOverBackColor = Color.FromArgb(31, 99, 171)
        Guardar.FlatAppearance.MouseDownBackColor = Color.FromArgb(24, 83, 145)
        Guardar.Font = UIConstants.FontBodyStrong()
        Guardar.Height = 38
        Guardar.Width = 132
        Guardar.BackgroundImage = Nothing
        Guardar.Image = Nothing
        Guardar.TextAlign = ContentAlignment.MiddleCenter

        Cancelar.Text = "Salir"
        Cancelar.BackColor = Color.FromArgb(114, 44, 61)
        Cancelar.ForeColor = Color.White
        Cancelar.FlatStyle = FlatStyle.Flat
        Cancelar.FlatAppearance.BorderColor = Color.FromArgb(95, 36, 50)
        Cancelar.FlatAppearance.BorderSize = 1
        Cancelar.FlatAppearance.MouseOverBackColor = Color.FromArgb(133, 54, 72)
        Cancelar.FlatAppearance.MouseDownBackColor = Color.FromArgb(95, 36, 50)
        Cancelar.Font = UIConstants.FontBodyStrong()
        Cancelar.Height = 38
        Cancelar.Width = 110
        Cancelar.BackgroundImage = Nothing
        Cancelar.Image = Nothing
        Cancelar.TextAlign = ContentAlignment.MiddleCenter

        GridConsulta.AllowUserToOrderColumns = False
        GridConsulta.Anchor = AnchorStyles.None
        GridConsulta.RowTemplate.Height = 36
        GridConsulta.ColumnHeadersHeight = 38

        LblSeleccion.Text = "Doble clic o Enter para seleccionar."
        LblMensajes.Text = String.Empty

        ReflowSearchLayout()
        _layoutReady = True
    End Sub

    Private Sub Busqueda_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        If Not _layoutReady OrElse CrudVisualHelper.IsInDesignMode(Me) Then
            Return
        End If
        ReflowSearchLayout()
    End Sub

    Private Sub ReflowSearchLayout()
        Dim marginX As Integer = 22
        Dim titleTop As Integer = 20
        Dim headerGap As Integer = 10
        Dim searchGap As Integer = 12
        Dim footerHeight As Integer = 54
        Dim footerTop As Integer = Me.ClientSize.Height - footerHeight - 16

        LblTitulo.MaximumSize = New Size(Math.Max(420, Me.ClientSize.Width - 40), 0)
        LblTitulo.Location = New Point(marginX, titleTop)

        LblFiltrado.Location = New Point(marginX, LblTitulo.Bottom + headerGap)

        Dim searchTop As Integer = LblFiltrado.Bottom + 8
        Dim searchButtonWidth As Integer = 112
        Dim searchWidth As Integer = Math.Max(260, Me.ClientSize.Width - (marginX * 2) - searchButtonWidth - searchGap)
        TxtFiltro.SetBounds(marginX, searchTop, searchWidth, 36)
        BtnFiltro.SetBounds(TxtFiltro.Right + searchGap, searchTop, searchButtonWidth, 36)

        Dim gridTop As Integer = TxtFiltro.Bottom + 14
        Dim gridHeight As Integer = Math.Max(260, footerTop - gridTop - 12)
        GridConsulta.SetBounds(marginX, gridTop, Me.ClientSize.Width - (marginX * 2), gridHeight)

        LblSeleccion.AutoSize = True
        LblSeleccion.Location = New Point(marginX, footerTop + 6)

        LblMensajes.AutoSize = False
        LblMensajes.SetBounds(marginX, LblSeleccion.Bottom + 2, Math.Max(240, Me.ClientSize.Width - 320), 18)

        Cancelar.SetBounds(Me.ClientSize.Width - marginX - Cancelar.Width, footerTop, Cancelar.Width, Cancelar.Height)
        Guardar.SetBounds(Cancelar.Left - 10 - Guardar.Width, footerTop, Guardar.Width, Guardar.Height)
    End Sub

    Private Function ObtenerValoresSeleccionados() As String()
        Dim valores As New List(Of String)()
        Dim camposSeleccion As String = _activeRequest.ReturnFieldsCsv

        Do
            Dim posComa As Integer = InStr(camposSeleccion, ",")
            Dim parametro As String
            If posComa > 0 Then
                parametro = Mid(camposSeleccion, 1, posComa - 1)
                camposSeleccion = camposSeleccion.Replace(parametro & ",", "")
            Else
                parametro = camposSeleccion
            End If

            Dim cellValue As Object = GridConsulta.Rows(RegistroSeleccionado).Cells(parametro).Value
            valores.Add(If(cellValue Is Nothing OrElse IsDBNull(cellValue), String.Empty, CStr(cellValue)))
            If posComa <= 0 Then
                Exit Do
            End If
        Loop

        Return valores.ToArray()
    End Function

    Private Sub UpdateSelectionState()
        RegistroSeleccionado = -1
        If GridConsulta.Rows.Count > 0 AndAlso GridConsulta.CurrentRow IsNot Nothing Then
            RegistroSeleccionado = GridConsulta.CurrentRow.Index
            LblMensajes.Text = String.Empty
            LblSeleccion.Text = String.Format("{0} registros disponibles. Doble clic o Enter para seleccionar.", GridConsulta.Rows.Count)
        Else
            LblMensajes.Text = "No se encontraron registros."
            LblSeleccion.Text = "Sin resultados para seleccionar."
        End If
    End Sub
End Class
