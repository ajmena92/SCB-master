Option Strict On
Option Explicit On

Public Class FrmProyeccionComedor
    Private ReadOnly Cls As New FuncionesDB
    Private Ds As New DataSet
    Private ReadOnly Cn As New SqlClient.SqlConnection

    Private Sub FrmProyeccionComedor_Load(sender As Object, e As EventArgs) Handles MyBase.Load
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

            Ds = Cls.ConsultarTSQL("TipoBeca", "Select IdBeca,Descripcion From TipoBeca Where Activo = 1", Cn:=Cn)
            If Ds.Tables(0).Rows.Count > 0 Then
                CbBeca.Items.Clear()
                CbBeca.Items.Add(New LBItem("", "---- TODOS ----"))
                For I As Integer = 0 To Ds.Tables(0).Rows.Count - 1
                    CbBeca.Items.Add(New LBItem(CStr(Ds.Tables(0).Rows(I)("IdBeca")), CStr(Ds.Tables(0).Rows(I)("Descripcion"))))
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


        FecIni.Value = Date.Today

    End Sub

    Private Sub BtnCancelar_Click(sender As Object, e As EventArgs) Handles BtnCancelar.Click
        Limpiar()
    End Sub

    Private Sub Limpiar()
        FecIni.Value = Date.Today
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
        Else

            ' todo bien, se saca reporte.
            'Arma el Criterio de la Consulta
            Criterio = ArmaFechaReporte("{RegistroTransporte.Fecha}", FecIni.Value.Date, FecIni.Value.Date)
            Dim request As New ReportRequest()
            request.DateRangeLabel = "Fecha: " & FecIni.Value
            If CbBeca.SelectedIndex > 0 Then
                Criterio = Criterio + " and {TipoBeca.IdBeca}=" & DirectCast(CbBeca.Items(CbBeca.SelectedIndex), LBItem).Valor
            End If
            If CbHorario.SelectedIndex > 0 Then
                Criterio = Criterio + " and {RegistroTransporte.IdHorario}=" & DirectCast(CbHorario.Items(CbHorario.SelectedIndex), LBItem).Valor
                request.ScheduleLabel = "Horario: " + CbHorario.Text
            End If
            Criterio = Criterio + " and {Usuario.CodTipo}=1"
            request.SelectionFormula = Criterio
            request.Title = "Reporte Proyección Asistencia Servicio Comedor"
            request.ReportKey = "FrmProyeccionComedor"
            Return request
        End If
        Return Nothing
    End Function
End Class
