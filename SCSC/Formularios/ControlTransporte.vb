Imports System.Linq

Public Class ControlTransporte
    Dim Ulthuella As String
    Dim ErrUltHuella, ErrTiquetes As Boolean '' controla el error de 2 marcas continuas y por cantidad de tiquetes
    Dim Cls As New FuncionesDB
    Dim TransporteSvc As New TransporteDataService(Cls)
    Dim Cn As New SqlClient.SqlConnection
    Dim DsUsuarios As New DataSet ' Datset para busqueda por carnet
    Dim DsRutas As New DataSet ' Datset para busqueda de rutas
    Dim EstadoVerificado As Boolean = False

    Private Sub BtnSalir_Click(sender As Object, e As EventArgs) Handles BtnCerrar.Click

        Me.Close()
    End Sub

    Private Sub CaptureForm_FormClosed(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles MyBase.FormClosed
        Try
            If Cn.State = ConnectionState.Open Then
                Cls.CerrarConexion(Cn)
            End If
        Catch ex As Exception
            ''Se cierre el formulario            
        End Try
        Me.Dispose()
    End Sub

    Private Sub ControlTransporte_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Cls.AbrirConexion(Cn, False)
            DsUsuarios = TransporteSvc.CargarUsuariosActivos(Cn)
            DsRutas = TransporteSvc.CargarRutas(Cn)
            Ulthuella = -1
            LblFecha.Text = FechaServer
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
            LblCedula.Text = Usuario!Cedula
            TxtUsuario.Text = CType(Usuario!Nombre & " " & Usuario!PrimerApellido & " " & Usuario!SegundoApellido, String)
            TxtSeccion.Text = Usuario!Seccion
            For Each rut As DataRow In DsRutas.Tables(0).Rows
                If rut!IdRuta = Usuario!IdRuta Then
                    TxtRuta.Text = rut!Codigo
                    LblRuta.Text = rut!Descripcion & ": " & Usuario!Nombre & " " & Usuario!PrimerApellido
                End If
            Next
            If Not IsDBNull(Usuario!PermisoSalida) Then
                If Usuario!PermisoSalida Then
                    TxtPermisoSalida.Text = "SI Autorizado"
                Else
                    TxtPermisoSalida.Text = "NO Autorizado"
                End If
            Else
                TxtPermisoSalida.Text = "NO Autorizado"
            End If
            Ulthuella = LblCedula.Text
            If (Usuario!CodTipo = 1) Then
                TxtTipo.Text = "ESTUDIANTE"
                LblTitulo.Text = "ESTUDIANTE: " & TxtUsuario.Text
            Else
                TxtTipo.Text = "PROFESOR"
                LblTitulo.Text = "PROF.: " & TxtUsuario.Text
            End If
            TransporteSvc.RegistrarMarca(Usuario, Cn, FechaServer)
            EstadoVerificado = True
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
            TxtPermisoSalida.Clear()
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
            If Cls.VereficaCarnet(TxtCedula.Text) Then
                EstadoVerificado = False
                Dim DobleLectura = False
                Picture.Image = My.Resources.huella_dactilar
                For Each Row As DataRow In DsUsuarios.Tables(0).Rows
                    If Row!Cedula = TxtCedula.Text Then
                        If Ulthuella = TxtCedula.Text Then
                            DobleLectura = True
                        Else
                            ProcesarMarca(Row)
                        End If
                        Exit For
                    End If
                Next
                '' 0=ok,1 = Error,2=Procesando,3= Doble verificacion</param>
                If (EstadoVerificado) Then
                    MensajeVisual(0)
                ElseIf (DobleLectura) Then
                    MensajeVisual(3)
                Else
                    MensajeVisual(1)
                End If
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
                    LblTitulo.ForeColor = Color.White
                    Imgprocess.Image = My.Resources.Verificado2
                    '   PictResult.Image = My.Resources.Verificado
                    PanelResult.BackColor = Color.Green
                Case 1
                    'PictResult.Image = My.Resources.ErrorAcceso
                    lblProcesando.Text = "Error, al verificar el usuario"
                    lblProcesando.ForeColor = Color.White
                    LblTitulo.ForeColor = Color.White
                    Imgprocess.Image = My.Resources.Error2
                    PanelResult.BackColor = Color.Red
                    LblCedula.Clear()
                    TxtTipo.Clear()
                    TxtSeccion.Clear()
                    TxtRuta.Clear()
                    LblRuta.Text = ""
                    TxtUsuario.Clear()
                    LblTitulo.Text = "Sistema de Control de Marcas"
                Case 2
                    lblProcesando.Text = "Procesando, por favor espere"
                    lblProcesando.ForeColor = Color.Black
                    LblTitulo.ForeColor = Color.Black
                    Imgprocess.Image = My.Resources.Gif_cargando
                    PanelResult.BackColor = Color.Gainsboro
                    ''PictResult.Image = My.Resources.Procesado
                Case 3
                    lblProcesando.Text = "Se detecto doble verificación"
                    lblProcesando.ForeColor = Color.White
                    LblTitulo.ForeColor = Color.White
                    Imgprocess.Image = My.Resources.Double_check
                    PanelResult.BackColor = Color.Blue
                Case 4

                    Imgprocess.Image = My.Resources.Error2
                    lblProcesando.Text = "No tiene tiquetes disponibles."
                    lblProcesando.ForeColor = Color.White
                    LblTitulo.ForeColor = Color.White
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
    'Private Sub _MensajeVisual(ByVal TipoImagen As Int16)
    '    Try

    '        Select Case TipoImagen
    '            Case 0
    '                lblProcesando.Text = "Usuario verificado correctamente"
    '                lblProcesando.ForeColor = Color.White
    '                Imgprocess.Image = My.Resources.Verificado2
    '                '   PictResult.Image = My.Resources.Verificado
    '                PanelResult.BackColor = Color.Green
    '            Case 1
    '                'PictResult.Image = My.Resources.ErrorAcceso
    '                lblProcesando.Text = "Error, al verificar el usuario"
    '                lblProcesando.ForeColor = Color.White
    '                Imgprocess.Image = My.Resources.Error2
    '                PanelResult.BackColor = Color.Red

    '            Case 2
    '                lblProcesando.Text = "Procesando, por favor espere"
    '                lblProcesando.ForeColor = Color.Black
    '                Imgprocess.Image = My.Resources.Gif_cargando
    '                PanelResult.BackColor = Color.Gainsboro
    '                ''PictResult.Image = My.Resources.Procesado
    '            Case 3
    '                lblProcesando.Text = "Se detecto doble verificación"
    '                lblProcesando.ForeColor = Color.White
    '                Imgprocess.Image = My.Resources.Double_check
    '                PanelResult.BackColor = Color.Blue
    '            Case 4

    '                Imgprocess.Image = My.Resources.Error2
    '                lblProcesando.Text = "No tiene tiquetes disponibles."
    '                lblProcesando.ForeColor = Color.White
    '                PanelResult.BackColor = Color.Orange
    '            Case Else
    '                lblProcesando.Text = ""
    '                Imgprocess.Image = My.Resources.Gif_cargando
    '                PanelResult.BackColor = Color.Gainsboro
    '        End Select

    '    Catch ex As Exception
    '        MsgBox("Error al cargar el Formulario: " & ex.Message, MsgBoxStyle.Critical)
    '    End Try
    'End Sub

    'Private Sub TReloj_Tick(sender As Object, e As EventArgs) Handles TReloj.Tick
    '    LblFecha.Text = CType(Date.Now(), String)
    '    '.Text = FechaServer
    'End Sub

    Private Sub LblFecha_Click(sender As Object, e As EventArgs) Handles LblFecha.Click

    End Sub

    Private Sub TxtCedula_TextChanged(sender As Object, e As EventArgs) Handles TxtCedula.TextChanged

    End Sub

    Private Sub LblTitulo_Click(sender As Object, e As EventArgs) Handles LblTitulo.Click
        LimpiarPantalla(True)
    End Sub

    Private Sub lblProcesando_Click(sender As Object, e As EventArgs) Handles lblProcesando.Click
        LimpiarPantalla(True)
    End Sub

    Private Sub PanelResult_Paint(sender As Object, e As PaintEventArgs) Handles PanelResult.Paint

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
