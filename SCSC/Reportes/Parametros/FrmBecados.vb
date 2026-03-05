Public Class FrmBecados
    Dim Cls As New FuncionesDB
    Dim Ds As New DataSet
    Dim Cn As New SqlClient.SqlConnection

    Private Sub FrmReporteMarcas_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Try
            UIThemeManagerV2.Apply(Me, "reporte")
            ApplyModernReportParamsStyle()
            Cls.AbrirConexion(Cn, False)

            Ds = Cls.ConsultarTSQL("Horario", "Select IdHorario,Descripcion From Horario Where Activo = 1", Cn:=Cn)
            If Ds.Tables(0).Rows.Count > 0 Then
                CbHorario.Items.Clear()
                CbHorario.Items.Add(New LBItem("", "---- TODOS ----"))
                For I As Integer = 0 To Ds.Tables(0).Rows.Count - 1
                    CbHorario.Items.Add(New LBItem(CStr(Ds.Tables(0).Rows(I)("IdHorario")), CStr(Ds.Tables(0).Rows(I)("Descripcion"))))
                Next
                CbHorario.SelectedIndex = 0
            End If
            Cls.CerrarConexion(Cn)
        Catch ex As Exception
            If Cn.State = ConnectionState.Open Then
                Cls.CerrarConexion(Cn)
            End If
            MsgBox("Error al cargar el Formulario: " & ex.Message, MsgBoxStyle.Critical)
            Me.Dispose()  'Cierro el formulario
        End Try
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
        RbBecaComedor.Checked = False
        RbBecaTransporte.Checked = False
        RbPermisoSalida.Checked = False
        CbHorario.SelectedIndex = 0
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

    Private Function ArmaReporte() As Boolean
        Dim Criterio As String = String.Empty
        LimpiarSession()

        If (RbBecaComedor.Checked = False And RbBecaTransporte.Checked = False And RbPermisoSalida.Checked = False) Then
            MsgBox("Debe Indicar el tipo de reporte a mostrar (Comedror-Transporte)", MsgBoxStyle.Critical)
        Else
            ' todo bien, se saca reporte.
            'Arma el Criterio de la Consulta

            Criterio = Criterio + " {Usuario.CodTipo}=1 and {Usuario.Activo}= True"
            If RbBecaComedor.Checked Then
                gSession.Titulo = "Reporte Beneficiarios Servicio de Comedor"
                Criterio = Criterio + " and {Usuario.TipoBeca}<>1"
                gSession.TipoReporte = "BecadosComedor"
            ElseIf RbBecaTransporte.Checked Then
                gSession.Titulo = "Reporte Beneficiarios Servicio de Transporte"
                Criterio = Criterio + " and {Usuario.IdRuta}<>1"
                If RbGeneral.Checked Then
                    gSession.TipoReporte = "FrmBecadosTransporteGeneral"
                Else
                    gSession.TipoReporte = "FrmBecadosTansporteDetallado"
                End If
            Else
                gSession.Titulo = "Reporte Estudiantes con Permiso de Salida"
                Criterio = Criterio + " and {Usuario.PermisoSalida}=1"
            End If
            If CbHorario.SelectedIndex > 0 Then
                Criterio = Criterio + " and {Usuario.IdHorario}=" & DirectCast(CbHorario.Items(CbHorario.SelectedIndex), LBItem).Valor
                gSession.Valor1 = "Horario: " + CbHorario.Text
            End If
            gSession.Reporte = "FrmBecados"
            gSession.Criterio = Criterio
            Return True
        End If
        Return False
    End Function

    Private Sub RbGeneral_CheckedChanged(sender As Object, e As EventArgs) Handles RbGeneral.CheckedChanged

    End Sub

    Private Sub RbDetallo_CheckedChanged(sender As Object, e As EventArgs) Handles RbDetallo.CheckedChanged

    End Sub
End Class
