Option Strict On
Option Explicit On

Public Class FrmReporteComedor
    Private ReadOnly Cls As New FuncionesDB
    Private Ds As New DataSet
    Private ReadOnly Cn As New SqlClient.SqlConnection

    Private Sub FrmReporteComedor_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If CrudVisualHelper.IsInDesignMode(Me) Then
            Return
        End If

        Try
            CrudVisualHelper.ApplyReportStandard(Me)
            Cls.AbrirConexion(Cn, False)
            Ds = Cls.ConsultarTSQL("TipoUsuario", "Select Codtipo,Descripcion From TipoUsuario Where Activo = 1", Cn:=Cn)
            If Ds.Tables(0).Rows.Count > 0 Then
                CbTipoUsuario.Items.Clear()
                CbTipoUsuario.Items.Add(New LBItem("", "---- TODOS ----"))
                For I As Integer = 0 To Ds.Tables(0).Rows.Count - 1
                    CbTipoUsuario.Items.Add(New LBItem(CStr(Ds.Tables(0).Rows(I)("CodTipo")), CStr(Ds.Tables(0).Rows(I)("Descripcion"))))
                Next
                CbTipoUsuario.SelectedIndex = 0
            End If

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
        FecFinal.Value = Date.Today



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
        If ArmaReporte() Then
            Try
                Using frm As New FrmReportViewer()
                    frm.ShowDialog(Me)
                End Using
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
        End If
    End Sub

    Private Function ArmaReporte() As Boolean
        Dim Criterio As String = String.Empty
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
            Criterio = ArmaFechaReporte("{RegistroComedor.Fecha}", FecIni.Value.Date, FecFinal.Value.Date)
            gSession.RangoDeFecha = "Desde: " & FecIni.Value & "  " & "Hasta: " & FecFinal.Value
            If CbTipoUsuario.SelectedIndex > 0 Then
                Criterio = Criterio + " and {TipoUsuario.CodTipo}=" & DirectCast(CbTipoUsuario.Items(CbTipoUsuario.SelectedIndex), LBItem).Valor
            End If
            If CbHorario.SelectedIndex > 0 Then
                Criterio = Criterio + " and {Horario.IdHorario}=" & DirectCast(CbHorario.Items(CbHorario.SelectedIndex), LBItem).Valor
                gSession.Valor1 = "Horario: " + CbHorario.Text
            End If
            If CbBeca.SelectedIndex > 0 Then
                Criterio = Criterio + " and {TipoBeca.IdBeca}=" & DirectCast(CbBeca.Items(CbBeca.SelectedIndex), LBItem).Valor
            End If

            gSession.Criterio = Criterio
            gSession.Titulo = "Reporte de Marcas por Rango de Fechas"
            If RbGeneral.Checked Then
                gSession.TipoReporte = "GENERAL"
            Else
                gSession.TipoReporte = "DETALLADO"
            End If
            gSession.Reporte = "FrmReporteMarcas"
            Return True
        End If
        Return False
    End Function
End Class
