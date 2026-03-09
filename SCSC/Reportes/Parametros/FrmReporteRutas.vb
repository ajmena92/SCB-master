Option Strict On
Option Explicit On

Public Class FrmReporteRutas


    Private Sub FrmReporteRutas_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If CrudVisualHelper.IsInDesignMode(Me) Then
            Return
        End If
        Try
            CrudVisualHelper.ApplyReportStandard(Me)
            Dim Cls As New FuncionesDB
            FecIni.Value = Date.Today
            FecFinal.Value = Date.Today
            Dim Ds As New DataSet
            Ds = Cls.ConsultarTSQL("Ruta", "Select IdRuta,Descripcion From Ruta Where Activo = 1",)
            If Ds.Tables(0).Rows.Count > 0 Then
                CbTipoUsuario.Items.Clear()
                CbTipoUsuario.Items.Add(New LBItem("", "---- TODOS ----"))
                For I As Integer = 0 To Ds.Tables(0).Rows.Count - 1
                    CbTipoUsuario.Items.Add(New LBItem(CStr(Ds.Tables(0).Rows(I)("IdRuta")), CStr(Ds.Tables(0).Rows(I)("Descripcion"))))
                Next
                CbTipoUsuario.SelectedIndex = 0
            End If

            Ds = Cls.ConsultarTSQL("Horario", "Select IdHorario,Descripcion From Horario Where Activo = 1")
            If Ds.Tables(0).Rows.Count > 0 Then
                CbHorario.Items.Clear()
                CbHorario.Items.Add(New LBItem("", "---- TODOS ----"))
                For I As Integer = 0 To Ds.Tables(0).Rows.Count - 1
                    CbHorario.Items.Add(New LBItem(CStr(Ds.Tables(0).Rows(I)("IdHorario")), CStr(Ds.Tables(0).Rows(I)("Descripcion"))))
                Next
                CbHorario.SelectedIndex = 0
            End If
        Catch ex As Exception
            MsgBox("Error al cargar formulario", MsgBoxStyle.Critical)
            Me.Dispose()
        End Try

    End Sub

    Private Sub BtnCancelar_Click(sender As Object, e As EventArgs) Handles BtnCancelar.Click
        Limpiar()
    End Sub

    Private Sub Limpiar()
        FecIni.Value = Date.Today
        FecFinal.Value = Date.Today
        RbGeneral.Checked = True
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
            Criterio = ArmaFechaReporte("Date({V_RutaEstudiante_x_Fecha.Fecha})", FecIni.Value.Date, FecFinal.Value.Date)
            Dim request As New ReportRequest()
            request.DateRangeLabel = "Desde: " & FecIni.Value & "  " & "Hasta: " & FecFinal.Value
            If CbTipoUsuario.SelectedIndex > 0 Then
                Criterio = Criterio + " and {V_RutaEstudiante_x_Fecha.IdRuta}=" & DirectCast(CbTipoUsuario.Items(CbTipoUsuario.SelectedIndex), LBItem).Valor
            End If
            If CbHorario.SelectedIndex > 0 Then
                Criterio = Criterio + " and {V_RutaEstudiante_x_Fecha.IdHorario}=" & DirectCast(CbHorario.Items(CbHorario.SelectedIndex), LBItem).Valor
                request.ScheduleLabel = "Horario: " + CbHorario.Text
            End If

            request.SelectionFormula = Criterio
            request.Title = "Reporte de Marcas por Rango de Fechas"
            If RbGeneral.Checked Then
                request.ReportVariant = "GENERAL"
                request.Title = "Reporte Servicio de Transporte General"
            Else
                request.ReportVariant = "DETALLADO"
                request.Title = "Reporte Servicio de Transporte Detallado"
            End If
            request.ReportKey = "FrmReporteRutas"
            Return request
        End If
        Return Nothing
    End Function
End Class
