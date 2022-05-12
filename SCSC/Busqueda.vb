Public Class Busqueda

    Dim RegistroSeleccionado As Integer = -1
    ' Dim ColumnaSeleccionada As FuncionesDB.Campos

    Sub CargarGrid()
        Try
            Dim Cls As New FuncionesDB
            Dim Cn As New SqlClient.SqlConnection

            Cls.AbrirConexion(Cn, False)

            'Gsession.Valor1: contiene nombre de la tabla
            'Gsession.Valor2: order by para la consulta(solo campos ej: nombre, ruta)
            'Gsession.Valor3: Campo de la consulta que se devuelve en la seleccion x el usuario.
            'Gsession.Valor4: Campo que se muestra al usuario en la seleccion de registros
            'Gsession.Valor5: Campo para uso de filtrado.

            ' gSession.ResultadoDset = Cls.Consultar(Cn, gSession.Valor1, gSession.Valores, gSession.Llave, gSession.Valor2, , gSession.Valor6)
            gSession.ResultadoDset = Cls.Consultar(gSession.Valor1, gSession.Valores, gSession.Llave, Cn, , gSession.Valor2)

            GridConsulta.DataSource = gSession.ResultadoDset
            GridConsulta.DataMember = gSession.Valor1

            ' Tamaño de celdas
            GridConsulta.Columns(0).Width = 75
            GridConsulta.Columns(0).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
            'If gSession.Valor1 = "Productos" Then
            ' GridConsulta.Columns(1).Width = 80
            ' GridConsulta.Columns(1).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter

            'GridConsulta.Columns(3).Width = 100
            'GridConsulta.Columns(3).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft
            ' End If
            'GridConsulta.Columns(3).DefaultCellStyle.Format = "hh:mm:ss"
            'GridConsulta.Columns(1).AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells

            ' Aplicar formato a columnas, campo valor de gsession.valores
            ' Gsession.valores(I).valor = FORMATO PARA EL CAMPO A MOSTRAR
            For I As Integer = 0 To UBound(gSession.Valores)
                GridConsulta.Columns(I).DefaultCellStyle.Format = gSession.Valores(I).Formato
                If gSession.Valores(I).Nombre = "" Then
                Else
                    GridConsulta.Columns(I).HeaderText = gSession.Valores(I).Valor
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
            'LblSeleccion.Text = "Registro Seleccionado: " & GridConsulta.Rows(e.RowIndex).Cells(gSession.Valor4).Value
            gSession.Valor5 = GridConsulta.Columns(e.ColumnIndex).Name
            LblFiltrado.Text = "Filtrado x " & GridConsulta.Columns(e.ColumnIndex).HeaderText
        Catch ex As Exception
            MsgBox(ex.Message)
            'LblMensajes.Text = ex.Message
        End Try
    End Sub

    Private Sub GridConsulta_CellContentClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles GridConsulta.CellContentClick
        Try
            ' LblSeleccion.Text = "Registro Seleccionado: " & GridConsulta.Rows(e.RowIndex).Cells(gSession.Valor4).Value
            RegistroSeleccionado = e.RowIndex
            gSession.Valor5 = GridConsulta.Columns(e.ColumnIndex).Name
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
        gSession.Valor5 = GridConsulta.Columns(e.ColumnIndex).Name
        LblFiltrado.Text = "Filtrado x " & GridConsulta.Columns(e.ColumnIndex).HeaderText
        For I As Short = 0 To UBound(gSession.Valores)
            If InStr(gSession.Valores(I).Nombre, gSession.Valor5) > 0 Then
                If gSession.Valores(I).Valor = "" Then

                Else
                    gSession.Valor5 = gSession.Valores(I).Nombre
                End If

                Exit For
            End If
        Next
    End Sub
    Private Sub Guardar_Click(sender As Object, e As EventArgs) Handles Guardar.Click
        Try
            If RegistroSeleccionado >= 0 And GridConsulta.Rows.Count - 1 >= RegistroSeleccionado Then
                'GridConsulta.Select()
                Dim PosComa As Int32 = 0, Parametro As String = ""
                ' revisar ciclo, se estaq enciclando.
                Do
                    PosComa = InStr(gSession.Valor3, ",")
                    If PosComa > 0 Then
                        ' vienen varios parametros
                        Parametro = Mid(gSession.Valor3, 1, PosComa - 1)
                        gSession.Valor3 = gSession.Valor3.Replace(Parametro & ",", "")

                    Else
                        ' No hay mas parametros.
                        Parametro = gSession.Valor3
                    End If

                    ReDim Preserve gSession.Resultado(UBound(gSession.Resultado) + 1)
                    gSession.Resultado(UBound(gSession.Resultado)) = GridConsulta.Rows(RegistroSeleccionado).Cells(Parametro).Value
                Loop Until PosComa <= 0

                'gSession.Resultado = GridConsulta.Rows(RegistroSeleccionado).Cells(gSession.Valor3).Value
                'gSession.Resultado = GridConsulta.SelectedRows(0).Cells(gSession.Valor3).Value
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
            Cls.AbrirConexion(Cn, False)
            Llave = Cls.InicializarArray
            Cls.ArmaValor(Llave, gSession.Valor5, "%" & TxtFiltro.Text & "%")
            gSession.ResultadoDset = Cls.Consultar(gSession.Valor1, gSession.Valores, Llave, Cn, , gSession.Valor2)
            GridConsulta.DataSource = gSession.ResultadoDset
            GridConsulta.DataMember = gSession.Valor1
            Cls.CerrarConexion(Cn)
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
            LblMensajes.Text = ex.Message
        End Try
    End Sub

    Private Sub Busqueda_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            LblTitulo.Text = gSession.Titulo
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
End Class