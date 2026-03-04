Imports System.Linq

Public Class ControlComedor
    Dim Ulthuella As String
    Dim ErrUltHuella, ErrTiquetes, AdverNOMarca, AdverMarcaTardia As Boolean '' controla el error de 2 marcas continuas y por cantidad de tiquetes
    Dim Cls As New FuncionesDB
    Dim ComedorSvc As New ComedorDataService(Cls)
    Dim Cn As New SqlClient.SqlConnection
    Dim Ds As New DataSet ' Datset para busqueda por carnet
    Dim DsBeca As New DataSet ' Datset para almacenar los tipos de becas
    Dim DsHorarios As New DataSet ' Datset para almacenar los horarios
    Dim EstadoVerificado As Boolean = False

    Private Sub BtnSalir_Click(sender As Object, e As EventArgs) Handles BtnSalir.Click
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

    Private Sub ControlComedor_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Cls.AbrirConexion(Cn, False)
            Ds = ComedorSvc.CargarUsuariosConMarcaTransporte(Cn, FechaServer)
            DsBeca = ComedorSvc.CargarBecas(Cn)
            DsHorarios = ComedorSvc.CargarHorarios(Cn)
            LblFecha.Text = FechaServer
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
            LblCedula.Text = Usuario!Cedula
            TxtUsuario.Text = Usuario!Nombre & " " & Usuario!PrimerApellido & " " & Usuario!SegundoApellido
            If (Usuario!CodTipo = 1) Then
                TxtTipo.Text = "ESTUDIANTE"
                LblTitulo.Text = "ESTUDIANTE: " & TxtUsuario.Text
            Else
                TxtTipo.Text = "PROFESOR"
                LblTitulo.Text = "PROFESOR: " & TxtUsuario.Text
            End If
            Dim resultado As ComedorDataService.MarcaComedorResultado = ComedorSvc.RegistrarMarca(Usuario, DsBeca, DiaSemana, Cn)
            TxtTiquetes.Text = resultado.TextoTiquetes
            ErrTiquetes = resultado.ErrorTiquetes
            If resultado.RegistroGuardado Then
                Ulthuella = LblCedula.Text
            End If
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
            LblTitulo.Text = ("Sistema de Control de Marcas Comedor")
            TxtTipo.Clear()
            TxtTiquetes.Clear()
            TxtUsuario.Clear()
            Picture.Image = Nothing
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
            MensajeVisual(2)
            If Cls.VereficaCarnet(TxtCedula.Text) Then
                ErrUltHuella = False
                AdverNOMarca = False
                ErrTiquetes = False
                AdverMarcaTardia = False
                EstadoVerificado = False
                LimpiarPantalla(False)
                For Each Row As DataRow In Ds.Tables(0).Rows
                    If Row!Cedula = TxtCedula.Text Then

                        If Ulthuella = TxtCedula.Text Then ' Control doble marca
                            ErrUltHuella = True
                            Exit For
                        End If
                        If Row!CodTipo = 1 Then
                            Dim HoraLimite() As DataRow = DsHorarios.Tables(0).Select(
                            String.Format("IdHorario = '{0}'", Row!IdHorario))
                            If Row!MarcaTransporte = 0 Then ' Control no Marca ingreso en transporte
                                AdverNOMarca = True
                                MensajeVisual(5)
                                If MessageBox.Show("El estudiante " & Row!Nombre & " " & Row!PrimerApellido & " NO registro marca  de ingreso. ¿Desea permitir que utilice el Servicio de Comedor?", "Advertencia", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) = DialogResult.No Then
                                    Exit For
                                End If
                            ElseIf Row!HoraMarca.TimeOfDay > HoraLimite(0).ItemArray(1) Then ' Control marca tarde en trsnporte, Hora limite en la tabla Horarios
                                AdverMarcaTardia = True
                                MensajeVisual(6)
                                If MessageBox.Show("El estudiante " & Row!Nombre & " " & Row!PrimerApellido & " marco fuera de la HORA límite para el control de asistencia. ¿Desea permitir que utilice el Servicio de Comedor?", "Advertencia", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) = DialogResult.No Then
                                    Exit For
                                End If
                            End If
                        End If
                        ProcesarMarca(Row)
                        Picture.Image = My.Resources.huella_dactilar
                        Exit For
                    End If
                Next
                If (ErrTiquetes) Then
                    MensajeVisual(4)
                ElseIf ErrUltHuella Then
                    MensajeVisual(3)
                ElseIf Not (EstadoVerificado) Then
                    MensajeVisual(1)
                Else
                    MensajeVisual(0)
                End If
            Else
                MensajeVisual(1)
            End If
            TxtCedula.Focus()
            TxtCedula.SelectAll()
        End If

    End Sub

    Private Sub lblProcesando_Click(sender As Object, e As EventArgs) Handles lblProcesando.Click

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

                Case 2
                    lblProcesando.Text = "Procesando, por favor espere"
                    lblProcesando.ForeColor = Color.Black
                    LblTitulo.ForeColor = Color.Black
                    Imgprocess.Image = My.Resources.Gif_cargando
                    PanelResult.BackColor = Color.Gainsboro
                    ''PictResult.Image = My.Resources.Procesado
                Case 3
                    lblProcesando.Text = "Se detecto doble verificación."
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
                Case 5
                    Imgprocess.Image = My.Resources.Error2
                    lblProcesando.Text = "El estudiante no registro marca de ingreso."
                    lblProcesando.ForeColor = Color.White
                    LblTitulo.ForeColor = Color.White
                    PanelResult.BackColor = Color.Coral
                Case 6
                    Imgprocess.Image = My.Resources.Error2
                    lblProcesando.Text = "El estudiante marco fuera de la hora límite para el control de asistencia."
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
    '                GbDatos.Text = "Usuario verificado correctamente"
    '                lblProcesando.ForeColor = Color.Green
    '                Imgprocess.Image = My.Resources.Verificado2
    '                '   PictResult.Image = My.Resources.Verificado

    '            Case 1
    '                'PictResult.Image = My.Resources.ErrorAcceso
    '                lblProcesando.Text = "Error, al verificar el usuario"
    '                lblProcesando.ForeColor = Color.Red
    '                Imgprocess.Image = My.Resources.Error2


    '            Case 2
    '                lblProcesando.Text = "Procesando, por favor espere"
    '                lblProcesando.ForeColor = Color.Blue
    '                Imgprocess.Image = My.Resources.Gif_cargando


    '                ''PictResult.Image = My.Resources.Procesado
    '            Case 3
    '                lblProcesando.Text = "Se detecto doble verificación"
    '                lblProcesando.ForeColor = Color.Blue
    '                Imgprocess.Image = My.Resources.Double_check
    '            Case 4

    '                Imgprocess.Image = My.Resources.Error2
    '                lblProcesando.Text = "No tiene tiquetes disponibles."
    '                lblProcesando.ForeColor = Color.Red
    '            Case Else
    '                lblProcesando.Text = ""
    '                Imgprocess.Image = My.Resources.Gif_cargando
    '        End Select

    '    Catch ex As Exception
    '        MsgBox("Error al cargar el Formulario: " & ex.Message, MsgBoxStyle.Critical)
    '    End Try
    'End Sub


End Class
