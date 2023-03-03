Delegate Sub FunctionCall(ByVal param)

Public Class FrmPrincipal

    Private Sub FrmPrincipal_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        FechaServer = Date.Now()
        Login.ShowDialog()
    End Sub

    Private Sub UsuariosToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles UsuariosToolStripMenuItem.Click
        FrmEstudiantes.ShowDialog()
    End Sub

    Private Sub ControlDeMarcasToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ControlDeMarcasToolStripMenuItem.Click
        ControlComedor.ShowDialog()
    End Sub

    Private Sub AyudaToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AyudaToolStripMenuItem.Click
        FrmAyuda.ShowDialog()
    End Sub

    'Private Sub ReporteDeEstudiantesToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ReporteDeEstudiantesToolStripMenuItem.Click
    '    ReporteEstudiantes.ShowDialog()
    'End Sub

    'Private Sub ReporteDiariosToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ReporteDiariosToolStripMenuItem.Click
    '    RpTransacciones.ShowDialog()
    'End Sub


    'Private Sub ContadorToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ContadorToolStripMenuItem.Click
    '    FrmReport.ShowDialog()
    'End Sub

    Private Sub ImportarDatosPIADToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ImportarDatosPIADToolStripMenuItem.Click
        FrmImportarExcel.ShowDialog()
    End Sub

    Private Sub ReporteDiariosToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ReporteDiariosToolStripMenuItem.Click
        FrmReporteComedor.ShowDialog()
    End Sub

    Private Sub ImprimirToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ImprimirToolStripMenuItem.Click
        IMPRIMIR.ShowDialog()

    End Sub

    Private Sub UtilitariosToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles UtilitariosToolStripMenuItem.Click

    End Sub

    Private Sub ImportarDatosListaPIADToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ImportarDatosListaPIADToolStripMenuItem.Click
        ControlTransporte.ShowDialog()

    End Sub

    Private Sub BtnCerrar_Click(sender As Object, e As EventArgs) Handles BtnCerrar.Click
        Me.Dispose()
    End Sub

    Private Sub ReporteDeServicioTransporteToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ReporteDeServicioTransporteToolStripMenuItem.Click
        FrmReporteRutas.ShowDialog()
    End Sub

    Private Sub RecargasToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles RecargasToolStripMenuItem1.Click
        FrmRecarga.ShowDialog()
    End Sub

    Private Sub GestiónRutasToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles GestiónRutasToolStripMenuItem.Click
        FrmRutas.ShowDialog()
    End Sub

    Private Sub GestiónBecasToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles GestiónBecasToolStripMenuItem.Click
        FrmBecas.ShowDialog()
    End Sub

    Private Sub ReporteProyecciónComedorToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ReporteProyecciónComedorToolStripMenuItem.Click
        FrmProyeccionComedor.ShowDialog()
    End Sub

    Private Sub ReporteEstudiantesBecadosToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ReporteEstudiantesBecadosToolStripMenuItem.Click
        FrmBecados.ShowDialog()
    End Sub

    Private Sub AgregarEstudianteManualToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AgregarEstudianteManualToolStripMenuItem.Click
        FrmAgregarEstudiante.ShowDialog()
    End Sub
End Class