Imports System.IO
Imports System.Linq

Public Class FrmEstudiantes
    Implements DPFP.Capture.EventHandler
    Private Capturer As DPFP.Capture.Capture
    Private Enroller As DPFP.Processing.Enrollment
    'Private Data As AppData
    Public Event OnTemplate(ByVal template)
    Dim DsRutas As New DataSet
    Dim ActivaEdicion As Boolean = False
    Dim Cn As New SqlClient.SqlConnection
    Dim Cls As New FuncionesDB
    Private Sub FrmEstudiantes_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Cls.AbrirConexion(Cn, False)
            CargaRutas(CBRuta)
            CargaGenero(CBGenero)
            Init()
        Catch ex As Exception
            If Cn.State = ConnectionState.Open Then
                Cls.CerrarConexion(Cn)
            End If
            MsgBox("Error al cargar el Formulario: " & ex.Message, MsgBoxStyle.Critical)
            Me.Dispose()  'Cierro el formulario
        End Try
        TxtCedula.Focus()
    End Sub

    Sub CargaGenero(ByRef Combo As ComboBox)
        Combo.Items.Add(New LBItem(0, "NO INGRESADO"))
        Combo.Items.Add(New LBItem(1, "MASCULINO"))
        Combo.Items.Add(New LBItem(2, "FEMENINO"))
    End Sub
    Sub CargaRutas(ByRef Combo As ComboBox)
        Try
            Dim Valores(), Llave() As FuncionesDB.Campos
            Valores = Cls.InicializarArray
            Llave = Cls.InicializarArray
            Cls.ArmaValor(Valores, "IdRuta")
            Cls.ArmaValor(Valores, "Descripcion")
            Cls.ArmaValor(Valores, "Codigo")
            Cls.ArmaValor(Valores, "Activo")
            Cls.ArmaValor(Llave, "Activo", 1)
            DsRutas = Cls.Consultar("Ruta", Valores, Llave, Cn)
            If DsRutas.Tables(0).Rows.Count > 0 Then
                For i As Integer = 0 To DsRutas.Tables(0).Rows.Count - 1
                    Combo.Items.Add(New LBItem(CType((DsRutas.Tables(0).Rows(i)!IdRuta), String), CType((DsRutas.Tables(0).Rows(i)!Descripcion), String)))
                Next
            End If
            Combo.SelectedIndex = 0
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Sub LimpiarPantalla(Optional pFocus As Boolean = True)
        'TxtCedula.Clear()
        TxtApe1.Clear()
        TxtApe2.Clear()
        TxtNombre.Clear()
        TxtFecNac.Clear()
        TxtTipoUsuario.Clear()
        TxtSeccion.Clear()
        TxtTelefono.Clear()
        TxtFecNac.Clear()
        CBRuta.SelectedIndex = 0
        CBGenero.SelectedIndex = 0
        ChkBeca.Checked = False
        LblCantTiques.Text = "0 Tiquetes"
        If pFocus Then
            TxtCedula.Focus()
        End If
        Picture.Image = Nothing
        Picture.BackgroundImage = Nothing
        Try
            StopCapture()
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub Buscar_Click(sender As Object, e As EventArgs) Handles Buscar.Click
        Try
            TxtCedula.Clear()
            Dim Valores(), Llave() As FuncionesDB.Campos
            Valores = Cls.InicializarArray
            Llave = Cls.InicializarArray
            LimpiarSession()
            gSession.Titulo = "Usuarios del Sistema"
            gSession.Valor1 = "Usuario"   '  TABLA utilizada.
            gSession.Valor2 = "IdUsuario" ' ORDER BY
            gSession.Valor3 = "Cedula" ' codigo devuelto a la aplicacion en propiedad gsession.resultado
            'gSession.Valor4 = "Cedula,Nombre,PrimerApellido,SegundoApellido" ' Valor presentado al usuario
            gSession.Valor5 = "Nombre" ' campo para el filtro utilizado
            Cls.ArmaValor(Valores, "Cedula", "Cédula")
            Cls.ArmaValor(Valores, "Nombre", "Nombre")
            Cls.ArmaValor(Valores, "PrimerApellido", "1° Apellido")
            Cls.ArmaValor(Valores, "SegundoApellido", "2­­° Apellido")
            'Armado de la llave primaria, 1=1 para todos los registros
            Cls.ArmaValor(Llave, "Activo", "1")
            gSession.Valores = Valores
            gSession.Llave = Llave
            Dim F As New Busqueda
            F.ShowDialog()
            TxtCedula.Text = (gSession.Resultado(0))
            TxtCedula_Validated(sender, e)
            BtnGuardar.Select()
        Catch ex As Exception
            'MsgBox(MSJ.Mensajes.ErrorBusqueda)
        End Try

    End Sub

    Private Sub TxtCedula_Validated(sender As Object, e As EventArgs) Handles TxtCedula.Validated
        LimpiarPantalla(False)
        TxtCedula.Text = TxtCedula.Text.Trim
        Dim Ds As New DataSet
        Dim Valores(), Llave() As FuncionesDB.Campos
        If Len(TxtCedula.Text) > 0 Then
            Try
                Valores = Cls.InicializarArray
                Llave = Cls.InicializarArray
                Cls.ArmaValor(Llave, "Cedula", TxtCedula.Text)
                Cls.ArmaValor(Valores, "IdUsuario")
                Cls.ArmaValor(Valores, "Nombre")
                Cls.ArmaValor(Valores, "PrimerApellido")
                Cls.ArmaValor(Valores, "SegundoApellido")
                Cls.ArmaValor(Valores, "CantidadTiquetes")
                Cls.ArmaValor(Valores, "CodTipo")
                Cls.ArmaValor(Valores, "Sexo")
                Cls.ArmaValor(Valores, "Seccion")
                Cls.ArmaValor(Valores, "Telefono")
                Cls.ArmaValor(Valores, "FechaNac")
                Cls.ArmaValor(Valores, "IdRuta")
                Cls.ArmaValor(Valores, "ActivaBeca")
                Cls.ArmaValor(Valores, "HuellaDactilar")
                Cls.ArmaValor(Valores, "Activo")

                Ds = Cls.Consultar("Usuario", Valores, Llave, Cn)
                If Ds.Tables(0).Rows.Count > 0 Then
                    TxtNombre.Text = CType((Ds.Tables(0).Rows(0)!Nombre), String)
                    TxtApe1.Text = CType((Ds.Tables(0).Rows(0)!PrimerApellido), String)
                    TxtApe2.Text = CType((Ds.Tables(0).Rows(0)!SegundoApellido), String)
                    TxtFecNac.Text = Convert.ToString(Ds.Tables(0).Rows(0)!FechaNac)
                    TxtTelefono.Text = Convert.ToString(Ds.Tables(0).Rows(0)!Telefono)
                    TxtSeccion.Text = Convert.ToString(Ds.Tables(0).Rows(0)!Seccion)

                    If (Ds.Tables(0).Rows(0)!Sexo Is Nothing) Then
                        If (Ds.Tables(0).Rows(0)!Sexo = 1) Then
                            CBGenero.SelectedIndex = 1
                        ElseIf (Ds.Tables(0).Rows(0)!Sexo = 2) Then
                            CBGenero.SelectedIndex = 2
                        Else
                            CBGenero.SelectedIndex = 0
                        End If
                    Else
                        CBGenero.SelectedIndex = 0
                    End If

                    If (Ds.Tables(0).Rows(0)!IdRuta Is Nothing) Then
                        CBRuta.SelectedIndex = Ds.Tables(0).Rows(0)!IdRuta
                    Else
                        CBRuta.SelectedIndex = 0
                    End If


                    If (Ds.Tables(0).Rows(0)!CodTipo = 1) Then
                        TxtTipoUsuario.Text = "ESTUDIANTE"
                    Else
                        TxtTipoUsuario.Text = "PROFESOR"
                    End If
                    If IsDBNull(Ds.Tables(0).Rows(0)!HuellaDactilar) Then
                        Picture.BackgroundImage = Nothing
                    Else
                        Picture.BackgroundImage = My.Resources.huella_dactilar
                    End If
                    ChkBeca.Checked = CBool((Ds.Tables(0).Rows(0)!ActivaBeca))
                    TxtCedula.Tag = Ds.Tables(0).Rows(0)!IdUsuario
                    LblCantTiques.Text = Ds.Tables(0).Rows(0)!CantidadTiquetes & " Tiquetes"
                    ''Lector 
                    StartCapture()
                Else
                    LimpiarPantalla()
                    MsgBox("Usuario no ingresado en el sistema", MsgBoxStyle.Information)
                End If
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
        Else
            LimpiarPantalla(False)
        End If
    End Sub

    Private Sub TxtCedula_KeyDown(sender As Object, e As KeyEventArgs) Handles TxtCedula.KeyDown
        If e.KeyCode = Keys.Enter Then
            ChkBeca.Focus()
        ElseIf e.KeyCode = Keys.F2 Then
            Buscar.PerformClick()
        ElseIf e.KeyCode = Keys.F8 Then
            BtnGuardar.PerformClick()
        End If
    End Sub

    Private Sub BtnRegresar_Click(sender As Object, e As EventArgs) Handles BtnRegresar.Click
        Me.Close()
    End Sub

    Private Sub BtnGuardar_Click(sender As Object, e As EventArgs) Handles BtnGuardar.Click
        Dim Valores(), Llave() As FuncionesDB.Campos
        Try
            If ActivaEdicion Then
                If Enroller.TemplateStatus <> DPFP.Processing.Enrollment.Status.Ready Then
                    MsgBox("Debe Completar las marcas para guardar los datos.", MsgBoxStyle.Critical)
                    Exit Sub
                End If
            End If
            Valores = Cls.InicializarArray
            Llave = Cls.InicializarArray
            Cls.ArmaValor(Llave, "IdUsuario", TxtCedula.Tag)
            Cls.ArmaValor(Valores, "ActivaBeca", ChkBeca.Checked)
            Cls.ArmaValor(Valores, "IdRuta", CBRuta.SelectedIndex)
            Cls.ArmaValor(Valores, "Sexo", CBGenero.SelectedIndex)
            Cls.ArmaValor(Valores, "Activo", True)
            If ActivaEdicion Then
                Dim str As New MemoryStream
                Enroller.Template.Serialize(str)
                Dim serializedTemplate As Byte() = str.ToArray()
                Cls.ArmaValor(Valores, "HuellaDactilar", serializedTemplate)
            End If
            Cls.Update("Usuario", Valores, Llave, Cn)
            BtnCancelar_Click(sender, e)
        Catch ex As Exception
            MsgBox("Error al actulizar: " & ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnCancelar_Click(sender As Object, e As EventArgs) Handles BtnCancelar.Click
        ActivaEdicion = False
        LimpiarPantalla()
        TxtCedula.Clear()
        TxtCedula.Focus()
        Init()
    End Sub

    Protected Sub Init()
        Try
            Capturer = New DPFP.Capture.Capture()                   ' Create a capture operation.

            If (Not Capturer Is Nothing) Then
                Capturer.EventHandler = Me                              ' Subscribe for capturing events.
            Else
                MsgBox("No se puede iniciar la captura de la huella", MsgBoxStyle.Critical)
                SetPrompt("Can't initiate capture operation!")
            End If
            Enroller = New DPFP.Processing.Enrollment()         ' Create an enrollment.            
            UpdateStatus()
        Catch ex As Exception
            MessageBox.Show("Can't initiate capture operation!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Protected Sub Process(ByVal Sample As DPFP.Sample)
        DrawPicture(ConvertSampleToBitmap(Sample))

        ' Process the sample and create a feature set for the enrollment purpose.
        Dim features As DPFP.FeatureSet = ExtractFeatures(Sample, DPFP.Processing.DataPurpose.Enrollment)

        ' Check quality of the sample and add to enroller if it's good
        If (Not features Is Nothing) Then
            Try
                MakeReport("The fingerprint feature set was created.")
                Enroller.AddFeatures(features)              ' Add feature set to template.
            Finally
                UpdateStatus()
                ' Check if template has been created.
                Select Case Enroller.TemplateStatus
                    Case DPFP.Processing.Enrollment.Status.Ready        ' Report success and stop capturing
                        RaiseEvent OnTemplate(Enroller.Template)
                        SetPrompt("Click Close, and then click Fingerprint Verification.")
                        StopCapture()
                        ActivaEdicion = True
                    Case DPFP.Processing.Enrollment.Status.Failed       ' Report failure and restart capturing
                        Enroller.Clear()
                        StopCapture()
                        RaiseEvent OnTemplate(Nothing)
                        StartCapture()
                    Case DPFP.Processing.Enrollment.Status.Insufficient
                        ActivaEdicion = True
                End Select
            End Try
        End If
    End Sub

    Protected Sub UpdateStatus()
        ' Show number of samples needed.
        SetStatus(String.Format("Coloque su dedo, {0} veces para ingresar registro de huella.", Enroller.FeaturesNeeded))
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
        Process(Sample)
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
            MsgBox("Error al capturar la Huella, reintente", MsgBoxStyle.Critical)
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

    Private Sub CBRuta_SelectedIndexChanged(sender As Object, e As EventArgs) Handles CBRuta.SelectedIndexChanged
        Dim result = (From cust In DsRutas.Tables(0).Select("IdRuta = " & CBRuta.SelectedIndex)).SingleOrDefault()
        LblRuta.Text = result!Codigo
    End Sub
End Class