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
            If GridConsulta.Rows.Count > 0 Then
                RegistroSeleccionado = GridConsulta.CurrentRow.Index
            End If
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
            LblFiltrado.Text = "Filtrado x " & "Nombre o Descripción (Búsqueda x Omisión)"
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

        LblTitulo.ForeColor = UIConstants.TextPrimary
        LblFiltrado.ForeColor = UIConstants.TextSecondary
        LblMensajes.ForeColor = UIConstants.TextSecondary
        LblSeleccion.ForeColor = UIConstants.TextSecondary

        TxtFiltro.BackColor = UIConstants.Surface
        TxtFiltro.ForeColor = UIConstants.TextPrimary
        TxtFiltro.BorderStyle = BorderStyle.FixedSingle

        BtnFiltro.BackColor = UIConstants.Surface
        BtnFiltro.ForeColor = UIConstants.TextPrimary
        BtnFiltro.FlatStyle = FlatStyle.Flat
        BtnFiltro.FlatAppearance.BorderColor = UIConstants.Border
        BtnFiltro.FlatAppearance.BorderSize = 1
        BtnFiltro.FlatAppearance.MouseOverBackColor = UIConstants.SurfaceAlt
        BtnFiltro.FlatAppearance.MouseDownBackColor = UIConstants.SurfaceAlt

        For Each actionButton As Button In New Button() {Guardar, Cancelar}
            actionButton.BackColor = UIConstants.Surface
            actionButton.ForeColor = UIConstants.TextPrimary
            actionButton.FlatStyle = FlatStyle.Flat
            actionButton.FlatAppearance.BorderColor = UIConstants.Border
            actionButton.FlatAppearance.BorderSize = 1
            actionButton.FlatAppearance.MouseOverBackColor = UIConstants.SurfaceAlt
            actionButton.FlatAppearance.MouseDownBackColor = UIConstants.SurfaceAlt
        Next
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
End Class
