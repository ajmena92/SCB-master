Public Class FrmReporteComedor
    Dim Cls As New FuncionesDB
    Dim Ds As New DataSet
    Dim Cn As New SqlClient.SqlConnection

    Private Sub FrmReporteMarcas_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Try
            UIThemeManagerV2.Apply(Me, "reporte")
            ApplyModernReportParamsStyle()
            Cls.AbrirConexion(Cn, False)
            Ds = Cls.ConsultarTSQL("TipoUsuario", "Select Codtipo,Descripcion From TipoUsuario Where Activo = 1", Cn:=Cn)
            If Ds.Tables(0).Rows.Count > 0 Then
                CbTipoUsuario.Items.Clear()
                CbTipoUsuario.Items.Add(New LBItem("", "---- TODOS ----"))
                For I As Integer = 0 To Ds.Tables(0).Rows.Count - 1
                    CbTipoUsuario.Items.Add(New LBItem(Ds.Tables(0).Rows(I)("CodTipo"), Ds.Tables(0).Rows(I)("Descripcion")))
                Next
                CbTipoUsuario.SelectedIndex = 0
            End If

            Ds = Cls.ConsultarTSQL("Horario", "Select IdHorario,Descripcion From Horario Where Activo = 1", Cn:=Cn)
            If Ds.Tables(0).Rows.Count > 0 Then
                CbHorario.Items.Clear()
                CbHorario.Items.Add(New LBItem("", "---- TODOS ----"))
                For I As Integer = 0 To Ds.Tables(0).Rows.Count - 1
                    CbHorario.Items.Add(New LBItem(Ds.Tables(0).Rows(I)("IdHorario"), Ds.Tables(0).Rows(I)("Descripcion")))
                Next
                CbHorario.SelectedIndex = 0
            End If

            Ds = Cls.ConsultarTSQL("TipoBeca", "Select IdBeca,Descripcion From TipoBeca Where Activo = 1", Cn:=Cn)
            If Ds.Tables(0).Rows.Count > 0 Then
                CbBeca.Items.Clear()
                CbBeca.Items.Add(New LBItem("", "---- TODOS ----"))
                For I As Integer = 0 To Ds.Tables(0).Rows.Count - 1
                    CbBeca.Items.Add(New LBItem(Ds.Tables(0).Rows(I)("IdBeca"), Ds.Tables(0).Rows(I)("Descripcion")))
                Next
                CbBeca.SelectedIndex = 0
            End If
            Cls.CerrarConexion(Cn)
        Catch ex As Exception
            If Cn.State = ConnectionState.Open Then
                Cls.CerrarConexion(Cn)
            End If
            MsgBox("Error al cargar el Formulario: " & ex.Message, MsgBoxStyle.Critical)
            Me.Dispose()  'Cierro el formulario
        End Try


        FecIni.Value = Now.Date
        FecFinal.Value = Now.Date



    End Sub

    Private Sub ApplyModernReportParamsStyle()
        Me.BackColor = UIConstants.AppBackground
        Me.BackgroundImage = Nothing
        Me.Font = UIConstants.FontBody()
        ApplySurface(Me)
        StyleButtons(Me)
    End Sub

    Private Sub ApplySurface(ByVal root As Control)
        For Each ctrl As Control In root.Controls
            ctrl.BackgroundImage = Nothing
            If TypeOf ctrl Is Panel OrElse TypeOf ctrl Is GroupBox Then
                ctrl.BackColor = UIConstants.Surface
            End If
            If ctrl.HasChildren Then
                ApplySurface(ctrl)
            End If
        Next
    End Sub

    Private Sub StyleButtons(ByVal root As Control)
        For Each ctrl As Control In root.Controls
            If TypeOf ctrl Is Button Then
                Dim btn As Button = DirectCast(ctrl, Button)
                btn.FlatStyle = FlatStyle.Flat
                btn.FlatAppearance.BorderSize = 1
                btn.FlatAppearance.BorderColor = UIConstants.Border
                btn.BackColor = UIConstants.Surface
                btn.ForeColor = UIConstants.TextPrimary
                btn.Font = UIConstants.FontBodyStrong()
            End If
            If ctrl.HasChildren Then
                StyleButtons(ctrl)
            End If
        Next
    End Sub

    Private Sub BtnCancelar_Click(sender As Object, e As EventArgs) Handles BtnCancelar.Click
        Limpiar()
    End Sub

    Sub Limpiar()
        FecIni.Value = Now.Date
        FecFinal.Value = Now.Date
        RbGeneral.Checked = True
    End Sub

    Private Sub BtnRegresar_Click(sender As Object, e As EventArgs) Handles BtnRegresar.Click
        Me.Dispose()
    End Sub

    Private Sub BtnGuardar_Click(sender As Object, e As EventArgs) Handles BtnGuardar.Click
        If ArmaReporte() Then
            Try
                Dim F As New FrmReportViewer
                F.ShowDialog()
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
        End If
    End Sub

    Function ArmaReporte() As Boolean
        ArmaReporte = False
        Dim Criterio As String
        LimpiarSession()

        If Not IsDate(FecIni.Value) Then
            MsgBox("Debe Indicar una Fecha Válida de Inicio", MsgBoxStyle.Critical)
            FecIni.Focus()
        ElseIf Not IsDate(FecFinal.Value) Then
            MsgBox("Debe Indicar una Fecha Válida de Finalización", MsgBoxStyle.Critical)
            FecFinal.Focus()
        ElseIf FecFinal.Value < FecIni.Value Then
            MsgBox("Rango fechas erroneo.", MsgBoxStyle.Critical)
        Else

            ' todo bien, se saca reporte.
            'Arma el Criterio de la Consulta
            Criterio = ArmaFechaReporte("{RegistroComedor.Fecha}", CDate(FecIni.Text), CDate(FecFinal.Text))
            gSession.RangoDeFecha = "Desde: " & FecIni.Value & "  " & "Hasta: " & FecFinal.Value
            If CbTipoUsuario.SelectedIndex > 0 Then
                Criterio = Criterio + " and {TipoUsuario.CodTipo}=" & CbTipoUsuario.Items(CbTipoUsuario.SelectedIndex).valor
            End If
            If CbHorario.SelectedIndex > 0 Then
                Criterio = Criterio + " and {Horario.IdHorario}=" & CbHorario.Items(CbHorario.SelectedIndex).valor
                gSession.Valor1 = "Horario: " + CbHorario.Text
            End If
            If CbBeca.SelectedIndex > 0 Then
                Criterio = Criterio + " and {TipoBeca.IdBeca}=" & CbBeca.Items(CbBeca.SelectedIndex).valor
            End If
            If CbBeca.SelectedIndex > 0 Then
                Criterio = Criterio + " and {TipoBeca.IdBeca}=" & CbBeca.Items(CbBeca.SelectedIndex).valor
            End If

            gSession.Criterio = Criterio
            gSession.Titulo = "Reporte de Marcas por Rango de Fechas"
            If RbGeneral.Checked Then
                gSession.TipoReporte = "GENERAL"
            Else
                gSession.TipoReporte = "DETALLADO"
            End If
            gSession.Reporte = "FrmReporteMarcas"
        End If
        Return True
    End Function
End Class
