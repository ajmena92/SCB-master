Public Class FrmReporteComedor

    Private Sub FrmReporteMarcas_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim Cls As New FuncionesDB
        FecIni.Value = Now.Date
        FecFinal.Value = Now.Date
        Dim Ds As New DataSet
        Ds = Cls.ConsultarTSQL("TipoUsuario", "Select Codtipo,Descripcion From TipoUsuario Where Activo = 1",)
        If Ds.Tables(0).Rows.Count > 0 Then
            CbRuta.Items.Clear()
            CbRuta.Items.Add(New LBItem("", "---- TODOS ----"))
            For I As Integer = 0 To Ds.Tables(0).Rows.Count - 1
                CbRuta.Items.Add(New LBItem(Ds.Tables(0).Rows(I)("CodTipo"), Ds.Tables(0).Rows(I)("Descripcion")))
            Next
            CbRuta.SelectedIndex = 0
        End If
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
            Criterio = ArmaFechaReporte("{Transacciones.Fecha}", CDate(FecIni.Text), CDate(FecFinal.Text))
            gSession.RangoDeFecha = "Desde: " & FecIni.Value & "  " & "Hasta: " & FecFinal.Value
            If CbRuta.SelectedIndex > 0 Then
                Criterio = Criterio + " and {TipoUsuario.CodTipo}=" & CbRuta.Items(CbRuta.SelectedIndex).valor
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