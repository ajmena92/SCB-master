Option Strict On
Option Explicit On

Public Class FrmBecados
    Private ReadOnly Cls As New FuncionesDB
    Private Ds As New DataSet
    Private ReadOnly Cn As New SqlClient.SqlConnection

    Private Sub FrmBecados_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If CrudVisualHelper.IsInDesignMode(Me) Then
            Return
        End If

        Try
            CrudVisualHelper.ApplyReportStandard(Me)
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

    Private Sub BtnCancelar_Click(sender As Object, e As EventArgs) Handles BtnCancelar.Click
        Limpiar()
    End Sub

    Private Sub Limpiar()
        RbBecaComedor.Checked = False
        RbBecaTransporte.Checked = False
        RbPermisoSalida.Checked = False
        CbHorario.SelectedIndex = 0
    End Sub

    Private Sub BtnRegresar_Click(sender As Object, e As EventArgs) Handles BtnRegresar.Click
        Me.Dispose()
    End Sub

    Private Sub BtnGuardar_Click(sender As Object, e As EventArgs) Handles BtnGuardar.Click
        Dim request As ReportRequest = ArmaReporte()
        If request IsNot Nothing Then
            Try
                Using frm As New FrmReportViewer()
                    frm.Request = request
                    frm.ShowDialog(Me)
                End Using
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
        End If
    End Sub

    Private Function ArmaReporte() As ReportRequest
        Dim Criterio As String = String.Empty

        If (RbBecaComedor.Checked = False And RbBecaTransporte.Checked = False And RbPermisoSalida.Checked = False) Then
            MsgBox("Debe Indicar el tipo de reporte a mostrar (Comedror-Transporte)", MsgBoxStyle.Critical)
        Else
            ' todo bien, se saca reporte.
            'Arma el Criterio de la Consulta

            Dim request As New ReportRequest()
            Criterio = Criterio + " {Usuario.CodTipo}=1 and {Usuario.Activo}= True"
            If RbBecaComedor.Checked Then
                request.Title = "Reporte Beneficiarios Servicio de Comedor"
                Criterio = Criterio + " and {Usuario.TipoBeca}<>1"
                request.ReportVariant = "BecadosComedor"
            ElseIf RbBecaTransporte.Checked Then
                request.Title = "Reporte Beneficiarios Servicio de Transporte"
                Criterio = Criterio + " and {Usuario.IdRuta}<>1"
                If RbGeneral.Checked Then
                    request.ReportVariant = "FrmBecadosTransporteGeneral"
                Else
                    request.ReportVariant = "FrmBecadosTansporteDetallado"
                End If
            Else
                request.Title = "Reporte Estudiantes con Permiso de Salida"
                Criterio = Criterio + " and {Usuario.PermisoSalida}=1"
            End If
            If CbHorario.SelectedIndex > 0 Then
                Criterio = Criterio + " and {Usuario.IdHorario}=" & DirectCast(CbHorario.Items(CbHorario.SelectedIndex), LBItem).Valor
                request.ScheduleLabel = "Horario: " + CbHorario.Text
            End If
            request.ReportKey = "FrmBecados"
            request.SelectionFormula = Criterio
            Return request
        End If
        Return Nothing
    End Function

    Private Sub RbGeneral_CheckedChanged(sender As Object, e As EventArgs) Handles RbGeneral.CheckedChanged

    End Sub

    Private Sub RbDetallo_CheckedChanged(sender As Object, e As EventArgs) Handles RbDetallo.CheckedChanged

    End Sub
End Class
