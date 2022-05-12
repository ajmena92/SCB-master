Public Class FrmReporteRutas


    Private Sub FrmReporteMarcas_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Dim Cls As New FuncionesDB
            FecIni.Value = Now.Date
            FecFinal.Value = Now.Date
            Dim Ds As New DataSet
            Ds = Cls.ConsultarTSQL("Ruta", "Select IdRuta,Descripcion From Ruta Where Activo = 1",)
            If Ds.Tables(0).Rows.Count > 0 Then
                CbTipoUsuario.Items.Clear()
                CbTipoUsuario.Items.Add(New LBItem("", "---- TODOS ----"))
                For I As Integer = 0 To Ds.Tables(0).Rows.Count - 1
                    CbTipoUsuario.Items.Add(New LBItem(Ds.Tables(0).Rows(I)("IdRuta"), Ds.Tables(0).Rows(I)("Descripcion")))
                Next
                CbTipoUsuario.SelectedIndex = 0
            End If
        Catch ex As Exception
            MsgBox("Error al cargar formulario", MsgBoxStyle.Critical)
            Me.Dispose()
        End Try

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
            Criterio = ArmaFechaReporte("Date({V_RutaEstudiante_x_Fecha.Fecha})", (FecIni.Text), (FecFinal.Text))
            gSession.RangoDeFecha = "Desde: " & FecIni.Value & "  " & "Hasta: " & FecFinal.Value
            If CbTipoUsuario.SelectedIndex > 0 Then
                Criterio = Criterio + " and {V_RutaEstudiante_x_Fecha.IdRuta}=" & SCM(CbTipoUsuario.Items(CbTipoUsuario.SelectedIndex).valor)
            End If

            gSession.Criterio = Criterio
            gSession.Titulo = "Reporte de Marcas por Rango de Fechas"
            If RbGeneral.Checked Then
                gSession.TipoReporte = "GENERAL"
                gSession.Titulo = "Reporte Servicio de Transporte General"
            Else
                gSession.TipoReporte = "DETALLADO"
                gSession.Titulo = "Reporte Servicio de Transporte Detallado"
            End If
            gSession.Reporte = "FrmReporteRutas"
        End If
        Return True
    End Function
End Class