Public Class FrmBecados
    Dim Cls As New FuncionesDB
    Dim Ds As New DataSet
    Dim Cn As New SqlClient.SqlConnection

    Private Sub FrmReporteMarcas_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Try
            Cls.AbrirConexion(Cn, False)

            Ds = Cls.ConsultarTSQL("Horario", "Select IdHorario,Descripcion From Horario Where Activo = 1", Cn:=Cn)
            If Ds.Tables(0).Rows.Count > 0 Then
                CbHorario.Items.Clear()
                CbHorario.Items.Add(New LBItem("", "---- TODOS ----"))
                For I As Integer = 0 To Ds.Tables(0).Rows.Count - 1
                    CbHorario.Items.Add(New LBItem(Ds.Tables(0).Rows(I)("IdHorario"), Ds.Tables(0).Rows(I)("Descripcion")))
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

    Function ArmaReporte() As Boolean
        ArmaReporte = False
        Dim Criterio As String
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
                Criterio = Criterio + " and {Usuario.IdHorario}=" & CbHorario.Items(CbHorario.SelectedIndex).valor
                gSession.Valor1 = "Horario: " + CbHorario.Text
            End If
            gSession.Reporte = "FrmBecados"
            gSession.Criterio = Criterio
            Return True
            End If
    End Function

    Private Sub RbGeneral_CheckedChanged(sender As Object, e As EventArgs) Handles RbGeneral.CheckedChanged

    End Sub

    Private Sub RbDetallo_CheckedChanged(sender As Object, e As EventArgs) Handles RbDetallo.CheckedChanged

    End Sub
End Class