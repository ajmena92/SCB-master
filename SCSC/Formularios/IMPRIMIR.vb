Imports System.IO

'Imports LibPrintTicket
'Imports System
'Imports System.Collections.Generic
'Imports System.ComponentModel
'Imports System.Data
'Imports System.Drawing
'Imports System.Linq
'Imports System.Text
'Imports System.Threading.Tasks
'Imports System.Windows.Forms





Public Class IMPRIMIR
    Private ImpresoraActual As New Printing.PrinterSettings
    Private Lector As StreamReader

    Private Sub btnCrear_Click(ByVal sender As System.Object, _
ByVal e As System.EventArgs) Handles btnCrear.Click
        Dim TamañoPersonal As Printing.PaperSize
        Dim Ancho As Short
        Dim Alto As Short
        Try
            Ancho = Short.Parse(txtAncho.Text)
            Alto = Short.Parse(txtAlto.Text)
            TamañoPersonal = New Printing.PaperSize(txtNombre.Text, Ancho, Alto)

            ' Asignamos la impresora seleccionada
            prdoDocumento.PrinterSettings = ImpresoraActual
            ' Asignamos el tamaño personalizado de papel
            prdoDocumento.DefaultPageSettings.PaperSize = TamañoPersonal
            MessageBox.Show("Nuevo tamaño asignado a documento")
        Catch ex As Exception
            MessageBox.Show(ex.Message, ex.Source, MessageBoxButtons.OK)
        End Try
    End Sub

    Private Sub btnSeleccionarImpresora_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSeleccionarImpresora.Click
        Try
            prdImpresoras.PrinterSettings = ImpresoraActual
            If prdImpresoras.ShowDialog = DialogResult.OK Then
                ImpresoraActual = prdImpresoras.PrinterSettings
                lblImpresoraActual.Text = ImpresoraActual.PrinterName
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message, ex.Source, MessageBoxButtons.OK)
        End Try
    End Sub

    Private Sub prdoDocumento_PrintPage(ByVal sender As System.Object, ByVal e As System.Drawing.Printing.PrintPageEventArgs) Handles prdoDocumento.PrintPage
        Try
            Dim linea As String = Nothing
            Dim posicion As Integer = 10
            Dim NroLineasImpresas As Integer = 0
            Dim Fuente As New Font("Verdana", 8)

            ' Calulando el numero de lineas por pagina
            Dim NroLineasPagina As Integer = e.PageBounds.Height / Fuente.GetHeight(e.Graphics)

            While NroLineasImpresas < NroLineasPagina
                linea = Lector.ReadLine()
                If Not linea Is Nothing Then
                    e.Graphics.DrawString(linea, Fuente, Brushes.Black, 10, posicion)
                    posicion += 15
                    NroLineasImpresas += 1
                Else
                    Exit While
                End If
            End While

            If Not linea Is Nothing Then
                e.HasMorePages = True
            Else
                e.HasMorePages = False
                Lector.Close()
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message, ex.Source, MessageBoxButtons.OK)
        End Try
    End Sub

    Private Sub btnVer_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnVer.Click
        ppdDocumento.ShowDialog(Me)
    End Sub

    Private Sub btnCerrar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btncerrar.Click
        Me.Close()
    End Sub

    Private Sub prdoDocumento_BeginPrint(ByVal sender As Object, ByVal e As System.Drawing.Printing.PrintEventArgs) Handles prdoDocumento.BeginPrint
        Lector = New StreamReader("..\Documento.txt", System.Text.ASCIIEncoding.Default)
    End Sub

    


End Class
