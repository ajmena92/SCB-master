Imports System.IO
Imports System.Linq
Imports System.Threading

Public Class ControlTransporte
    Implements DPFP.Capture.EventHandler
    Private Capturer As DPFP.Capture.Capture
    Private Template As DPFP.Template
    Private Verificator As DPFP.Verification.Verification
    Dim Ulthuella As String
    Dim ErrUltHuella, ErrTiquetes As Boolean '' controla el error de 2 marcas continuas y por cantidad de tiquetes
    Dim Cls As New FuncionesDB
    Dim Cn As New SqlClient.SqlConnection
    Dim DsUsuarios As New DataSet ' Datset para busqueda por carnet
    Dim DsRutas As New DataSet ' Datset para busqueda de rutas
    Dim TablasBusqueda As List(Of DataTable) ' Tablas para busqueda por huella
    Dim ListaBusquedas As List(Of Thread)
    Dim EstadoVerificado As Boolean = False

    Public Sub Verify(ByVal template As DPFP.Template)
        Me.Template = template
    End Sub

    Private Sub BtnSalir_Click(sender As Object, e As EventArgs) Handles BtnCerrar.Click

        Me.Close()
    End Sub

    Protected Overridable Sub Init()
        Try
            Capturer = New DPFP.Capture.Capture() ' Create a capture operation.            
            If (Not Capturer Is Nothing) Then
                Capturer.EventHandler = Me ' Subscribe for capturing events.
            Else
                SetPrompt("Can't initiate capture operation!")
            End If
        Catch ex As Exception
            MessageBox.Show("Can't initiate capture operation!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
        Me.Text = "Fingerprint Verification"
        Verificator = New DPFP.Verification.Verification()
        UpdateStatus(0)
    End Sub

    Protected Sub Process(ByVal Sample As DPFP.Sample)
        MensajeVisual(2)
        ErrUltHuella = False
        ErrTiquetes = False
        EstadoVerificado = False
        DrawPicture(ConvertSampleToBitmap(Sample))
        Dim features As DPFP.FeatureSet = ExtractFeatures(Sample, DPFP.Processing.DataPurpose.Verification)
        Dim ListBusqueda As List(Of Thread) = New List(Of Thread)
        If Not features Is Nothing Then
            For Each Tabla As DataTable In TablasBusqueda
                Dim NuevaBusqueda As BusquedaHuella2 = New BusquedaHuella2
                NuevaBusqueda.TablaBusqueda = Tabla
                NuevaBusqueda.features = features
                Dim MyHiloBusqueda As New Thread(AddressOf ProcesoBusqueda)
                ListBusqueda.Add(MyHiloBusqueda)
                MyHiloBusqueda.Start(NuevaBusqueda)
            Next
            For Each t As Thread In ListBusqueda ''Espera a que todos los hilos de busquueda concluyan
                t.Join()
            Next
            If (ErrTiquetes) Then
                MensajeVisual(4)
            ElseIf Not (EstadoVerificado) Then
                MensajeVisual(1)
            ElseIf ErrUltHuella Then
                MensajeVisual(3)
            Else
                MensajeVisual(0)
            End If
        End If
    End Sub

    Private Sub ProcesoBusqueda(ByVal pBuscar As BusquedaHuella2)
        Try
            For Each Row As DataRow In pBuscar.TablaBusqueda.Rows
                If EstadoVerificado = False Then
                    If Not (IsDBNull(Row!HuellaDactilar)) Then
                        Dim memStreamTemplate As MemoryStream
                        Dim byteTemplate As Byte()
                        byteTemplate = Row!HuellaDactilar
                        memStreamTemplate = New MemoryStream(byteTemplate)
                        Dim TemplateHuella As DPFP.Template = New DPFP.Template
                        TemplateHuella.DeSerialize(memStreamTemplate)
                        Dim result As DPFP.Verification.Verification.Result = New DPFP.Verification.Verification.Result()
                        Verificator.Verify(pBuscar.features, TemplateHuella, result)
                        'UpdateStatus(result.FARAchieved)                   
                        If result.Verified Then
                            If Ulthuella = Row!Cedula Then '' Aqui se valida que no marque dos veces consecutivas
                                ErrUltHuella = True
                            Else
                                ProcesarMarca(Row)
                            End If
                            MakeReport("The fingerprint was VERIFIED." & Row!IdUsuario)
                            EstadoVerificado = True
                            Exit For
                        End If
                    End If
                Else
                    Exit For
                End If
            Next
        Catch ex As Exception
            If Not (ex.Message.ToUpper = "Subproceso anulado.".ToUpper) Then
                MsgBox("Error SubProceso: " & ex.Message, MsgBoxStyle.Critical)
            End If
        End Try
    End Sub


    Protected Sub UpdateStatus(ByVal FAR As Integer)
        ' Show "False accept rate" value
        SetStatus(String.Format("False Accept Rate (FAR) = {0}", FAR))
    End Sub

    Protected Function ConvertSampleToBitmap(ByVal Sample As DPFP.Sample) As Bitmap
        Dim convertor As New DPFP.Capture.SampleConversion()    ' Create a sample convertor.
        Dim bitmap As Bitmap = Nothing                          ' TODO: the size doesn't matter
        convertor.ConvertToPicture(Sample, bitmap)              ' TODO: return bitmap as a result
        Return bitmap
    End Function

    Protected Function ExtractFeatures(ByVal Sample As DPFP.Sample, ByVal Purpose As DPFP.Processing.DataPurpose) As DPFP.FeatureSet
        Dim extractor As New DPFP.Processing.FeatureExtraction()        ' Create a feature extractor
        Dim feedback As DPFP.Capture.CaptureFeedback = DPFP.Capture.CaptureFeedback.None
        Dim features As New DPFP.FeatureSet()
        extractor.CreateFeatureSet(Sample, Purpose, feedback, features) ' TODO: return features as a result?
        If (feedback = DPFP.Capture.CaptureFeedback.Good) Then
            Return features
        Else
            Return Nothing
        End If
    End Function

    Protected Sub StartCapture()
        If (Not Capturer Is Nothing) Then
            Try
                Capturer.StartCapture()
                SetPrompt("Using the fingerprint reader, scan your fingerprint.")
            Catch ex As Exception
                SetPrompt("Can't initiate capture!")
            End Try
        End If
    End Sub

    Protected Sub StopCapture()
        If (Not Capturer Is Nothing) Then
            Try
                Capturer.StopCapture()
            Catch ex As Exception
                SetPrompt("Can't terminate capture!")
            End Try
        End If
    End Sub


    Private Sub CaptureForm_FormClosed(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles MyBase.FormClosed
        Try
            If Cn.State = ConnectionState.Open Then
                Cls.CerrarConexion(Cn)
            End If
            StopCapture()
        Catch ex As Exception
            ''Se cierre el formulario            
        End Try
        Me.Dispose()
    End Sub

    Sub OnComplete(ByVal Capture As Object, ByVal ReaderSerialNumber As String, ByVal Sample As DPFP.Sample) Implements DPFP.Capture.EventHandler.OnComplete
        MakeReport("The fingerprint sample was captured.")
        SetPrompt("Scan the same fingerprint again.")
        Try
            StopCapture()
            LimpiarPantalla(True)
            Process(Sample)
            StartCapture()
        Catch ex As Exception
            StartCapture()
            MsgBox("Error procesar captura: " & ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Sub OnFingerGone(ByVal Capture As Object, ByVal ReaderSerialNumber As String) Implements DPFP.Capture.EventHandler.OnFingerGone
        MakeReport("The finger was removed from the fingerprint reader.")
    End Sub

    Sub OnFingerTouch(ByVal Capture As Object, ByVal ReaderSerialNumber As String) Implements DPFP.Capture.EventHandler.OnFingerTouch
        MakeReport("The fingerprint reader was touched.")
    End Sub

    Sub OnReaderConnect(ByVal Capture As Object, ByVal ReaderSerialNumber As String) Implements DPFP.Capture.EventHandler.OnReaderConnect
        MakeReport("The fingerprint reader was connected.")
    End Sub

    Sub OnReaderDisconnect(ByVal Capture As Object, ByVal ReaderSerialNumber As String) Implements DPFP.Capture.EventHandler.OnReaderDisconnect
        MakeReport("The fingerprint reader was disconnected.")
    End Sub

    Sub OnSampleQuality(ByVal Capture As Object, ByVal ReaderSerialNumber As String, ByVal CaptureFeedback As DPFP.Capture.CaptureFeedback) Implements DPFP.Capture.EventHandler.OnSampleQuality
        If CaptureFeedback = DPFP.Capture.CaptureFeedback.Good Then
            MakeReport("The quality of the fingerprint sample is good.")
        Else
            MakeReport("The quality of the fingerprint sample is poor.")
        End If
    End Sub

    Protected Sub SetStatus(ByVal status)
        Invoke(New FunctionCall(AddressOf _SetStatus), status)
    End Sub

    Private Sub _SetStatus(ByVal status)
        StatusLine.Text = status
    End Sub

    Protected Sub SetPrompt(ByVal text)
        Invoke(New FunctionCall(AddressOf _SetPrompt), text)
    End Sub

    Private Sub _SetPrompt(ByVal text)
        Prompt.Text = text
    End Sub

    Protected Sub MakeReport(ByVal status)
        Invoke(New FunctionCall(AddressOf _MakeReport), status)
    End Sub

    Private Sub _MakeReport(ByVal status)
        StatusText.AppendText(status + Chr(13) + Chr(10))
    End Sub

    Protected Sub DrawPicture(ByVal bmp)
        Invoke(New FunctionCall(AddressOf _DrawPicture), bmp)
    End Sub

    Private Sub _DrawPicture(ByVal bmp)
        Picture.Image = New Bitmap(bmp, Picture.Size)
    End Sub

    Private Sub ControlComedor_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Cls.AbrirConexion(Cn, False)
            DsUsuarios = Cls.ConsultarTSQL("Usuario", "SELECT IdUsuario,ActivaBeca,HuellaDactilar,Nombre,PrimerApellido,SegundoApellido,CodTipo,IdRuta,Seccion,Cedula FROM Usuario WHERE Activo = 1", Cn:=Cn)
            DsRutas = Cls.ConsultarTSQL("Ruta", "SELECT * FROM Ruta", Cn:=Cn)
            Dim tableCount = Math.Ceiling(DsUsuarios.Tables(0).Rows.Count / 40)  'Cantidad de Tablas a Crear para las busquedas 
            Dim divisor = DsUsuarios.Tables(0).Rows.Count / tableCount 'Cantidad de registros por tablas
            TablasBusqueda = DsUsuarios.Tables(0).AsEnumerable().
            Select(Function(r, i) New With {.Row = r, .Index = i}).
            GroupBy(Function(x) Math.Floor(x.Index / divisor)).
            Select(Function(g) g.Select(Function(x) x.Row).CopyToDataTable()).ToList
            Init()
            StartCapture()
            Ulthuella = -1
            TxtCedula.Focus()
        Catch ex As Exception
            If Cn.State = ConnectionState.Open Then
                Cls.CerrarConexion(Cn)
            End If
            MsgBox("Error al cargar el Formulario: " & ex.Message, MsgBoxStyle.Critical)
            Me.Dispose()  'Cierro el formulario
        End Try
    End Sub

    Protected Sub ProcesarMarca(ByVal Usuario As DataRow)
        Invoke(New FunctionCall(AddressOf _ProcesarMarca), Usuario)
    End Sub

    Sub _ProcesarMarca(ByVal Usuario As DataRow)
        Try
            Dim cmd As String
            Dim CantTiquetes As Integer = 0
            Dim Valores() As FuncionesDB.Campos
            LblCedula.Text = Usuario!Cedula
            TxtUsuario.Text = CType(Usuario!Nombre & " " & Usuario!PrimerApellido & " " & Usuario!SegundoApellido, String)
            If (Usuario!CodTipo = 1) Then
                TxtTipo.Text = "ESTUDIANTE"
            Else
                TxtTipo.Text = "PROFESOR"
            End If
            TxtSeccion.Text = Usuario!Seccion
            For Each rut As DataRow In DsRutas.Tables(0).Rows
                If rut!IdRuta = Usuario!IdRuta Then
                    TxtRuta.Text = rut!Codigo
                    LblRuta.Text = rut!Descripcion & ": " & Usuario!Nombre & " " & Usuario!PrimerApellido
                End If
            Next

            Valores = Cls.InicializarArray
            Cls.ArmaValor(Valores, "IdUsuario", Usuario!IdUsuario)
            Cls.ArmaValor(Valores, "IdRuta", Usuario!IdRuta)
            Cls.ArmaValor(Valores, "Tipo", Usuario!CodTipo)
            Cls.ArmaValor(Valores, "Fecha", Date.Now)
            Cls.Insert("RegistroTransporte", Valores, Cn)
            Ulthuella = LblCedula.Text
        Catch ex As Exception
            LimpiarPantalla(True)
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Protected Sub LimpiarPantalla(ByVal Limpiar)
        Invoke(New FunctionCall(AddressOf _LimpiarPantalla), Limpiar)
    End Sub

    Private Sub _LimpiarPantalla(ByVal Limpiar)
        Try
            LblCedula.Clear()
            TxtTipo.Clear()
            TxtSeccion.Clear()
            TxtRuta.Clear()
            LblRuta.Text = ""
            TxtUsuario.Clear()
            Picture.Image = Nothing
            TxtCedula.Clear()
            TxtCedula.Focus()
        Catch ex As Exception
            MsgBox("Error al cargar el Formulario: " & ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub
    ''' <summary>
    ''' Imagen a mostrar en la pantalla
    ''' </summary>
    ''' <param name="TipoImagen">0=ok,1 = Error,2=Procesando,3= Doble verificacion</param>
    Protected Sub MensajeVisual(ByVal TipoImagen As Int16)
        Invoke(New FunctionCall(AddressOf _MensajeVisual), TipoImagen)
    End Sub

    Private Sub TxtCedula_KeyDown(sender As Object, e As KeyEventArgs) Handles TxtCedula.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True ' para evitar un 'beep'
            Dim Verificado = False
            Dim DobleLectura = False
            Picture.Image = My.Resources.huella_dactilar
            For Each Row As DataRow In DsUsuarios.Tables(0).Rows
                If Row!Cedula = TxtCedula.Text Then
                    If Ulthuella = TxtCedula.Text Then
                        DobleLectura = True
                    Else
                        ProcesarMarca(Row)
                        Verificado = True
                    End If
                    Exit For
                End If
            Next
            '' 0=ok,1 = Error,2=Procesando,3= Doble verificacion</param>
            If (Verificado) Then
                MensajeVisual(0)
            ElseIf (DobleLectura) Then
                MensajeVisual(3)
            Else
                MensajeVisual(1)
            End If
            TxtCedula.Focus()
            TxtCedula.SelectAll()
        End If
    End Sub


    Private Sub TxtTipo_TextChanged(sender As Object, e As EventArgs) Handles TxtTipo.TextChanged

    End Sub

    Private Sub _MensajeVisual(ByVal TipoImagen As Int16)
        Try

            Select Case TipoImagen
                Case 0
                    lblProcesando.Text = "Usuario verificado correctamente"
                    lblProcesando.ForeColor = Color.White
                    Imgprocess.Image = My.Resources.Verificado2
                    '   PictResult.Image = My.Resources.Verificado
                    PanelResult.BackColor = Color.Green
                Case 1
                    'PictResult.Image = My.Resources.ErrorAcceso
                    lblProcesando.Text = "Error, al verificar el usuario"
                    lblProcesando.ForeColor = Color.White
                    Imgprocess.Image = My.Resources.Error2
                    PanelResult.BackColor = Color.Red

                Case 2
                    lblProcesando.Text = "Procesando, por favor espere"
                    lblProcesando.ForeColor = Color.Black
                    Imgprocess.Image = My.Resources.Gif_cargando
                    PanelResult.BackColor = Color.Gainsboro
                    ''PictResult.Image = My.Resources.Procesado
                Case 3
                    lblProcesando.Text = "Se detecto doble verificación"
                    lblProcesando.ForeColor = Color.White
                    Imgprocess.Image = My.Resources.Double_check
                    PanelResult.BackColor = Color.Blue
                Case 4

                    Imgprocess.Image = My.Resources.Error2
                    lblProcesando.Text = "No tiene tiquetes disponibles."
                    lblProcesando.ForeColor = Color.White
                    PanelResult.BackColor = Color.Orange
                Case Else
                    lblProcesando.Text = ""
                    Imgprocess.Image = My.Resources.Gif_cargando
                    PanelResult.BackColor = Color.Gainsboro
            End Select

        Catch ex As Exception
            MsgBox("Error al cargar el Formulario: " & ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub TReloj_Tick(sender As Object, e As EventArgs) Handles TReloj.Tick
        LblFecha.Text = CType(Date.Now(), String)
        '.Text = FechaServer
    End Sub

    Private Sub LblFecha_Click(sender As Object, e As EventArgs) Handles LblFecha.Click

    End Sub

    Private Sub TxtCedula_TextChanged(sender As Object, e As EventArgs) Handles TxtCedula.TextChanged

    End Sub

    Private Sub ControlTransporte_Closed(sender As Object, e As EventArgs) Handles Me.Closed
        Try
            If Cn.State = ConnectionState.Open Then
                Cls.CerrarConexion(Cn)
            End If
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub
End Class

Public Class BusquedaHuella2
    Public features As DPFP.FeatureSet
    Public TablaBusqueda As DataTable
    Public Name As String
End Class
