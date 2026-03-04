Option Strict Off
Option Explicit On

Friend Class Login
    Inherits System.Windows.Forms.Form
    Private Const DefaultAdminUser As String = "admin"
    Private Const DefaultAdminSupportPassword As String = "S0p0rt3CTP."
    Dim ClaveUsuario As String
    Dim Cls As New FuncionesDB
    Dim Cn As New SqlClient.SqlConnection
    Dim DsParametro As New DataSet
    Dim Verificado As Boolean = False
    ' *** Pendiente, revision: GrabaBitacora NO se utiliza en el formulario.
    Private Sub ClavePaso_Enter(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles ClavePaso.Enter
        ClavePaso.SelectAll()
    End Sub



    Private Sub ClavePaso_Leave(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles ClavePaso.Validated
        ClavePaso.Text = ClavePaso.Text.Trim
        If Len(ClavePaso.Text) <> 0 Then
            Dim Seguridad As New Seguridad.Encriptacion64
            ''Esto para soporte del Administrdor
            If String.Equals(CodUsuario.Text.Trim(), ObtenerUsuarioAdmin(), StringComparison.OrdinalIgnoreCase) Then
                If ClavePaso.Text.Trim <> ObtenerClaveSoporteAdmin() Then
                    MsgBox("Clave de Paso Inválida", 16)
                    ClavePaso.Text = ""
                    ClavePaso.Focus()
                Else
                    Verificado = True
                End If
            Else
                'Pendiente encriptar claves de usuario
                'If RTrim(Seguridad.encryptQueryString(ClavePaso.Text, LlaveIncriptacion)) <> RTrim(ClaveUsuario) Then
                '    MsgBox("Clave de Paso Inválida", 16)
                '    ClavePaso.Text = ""
                '    ClavePaso.Focus()
                'End If
                If RTrim(ClavePaso.Text) <> RTrim(ClaveUsuario) Then
                    MsgBox("Usuario o contraseña invalida !", 16)
                    ClavePaso.Text = ""
                    ClavePaso.Focus()
                Else
                    Verificado = True
                    BtnAceptar_Click(eventSender, eventArgs)
                End If

            End If
        End If
    End Sub

    Private Sub CodUsuario_Enter(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs)
        CodUsuario.SelectAll()
    End Sub
    Private Sub CodUsuario_Leave(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles CodUsuario.Validated
        CodUsuario.Text = CodUsuario.Text.Trim
        If Len(CodUsuario.Text.Trim) <> 0 Then
            CodUsuario.Text = CodUsuario.Text.PadLeft(2, "0")
            Try
                Dim Ds As DataSet
                Dim Cls As New FuncionesDB
                Dim Valores(), Llave() As FuncionesDB.Campos

                Valores = Cls.InicializarArray
                Llave = Cls.InicializarArray

                Cls.ArmaValor(Valores, " * ")
                Cls.ArmaValor(Valores, "Getdate() as Fecha")
                Cls.ArmaValor(Llave, "Nombre", CodUsuario.Text)
                Cls.ArmaValor(Llave, "Activo", True)

                Ds = Cls.Consultar("TablaSeguridad", Valores, Llave)

                If Ds.Tables(0).Rows.Count > 0 Then
                    'LblNombreUsuario.Text = Ds.Tables(0).Rows(0)!Nombre
                    If IsDBNull(Ds.Tables(0).Rows(0)!Contrasena) Then
                        Throw New Exception("Por favor solicite al administrador del Sistema que ingrese una clave valida para este usuario.")
                    Else
                        ClaveUsuario = Ds.Tables(0).Rows(0)!Contrasena
                        FechaServer = CDate(Ds.Tables(0).Rows(0)!Fecha).Date
                    End If
                Else
                    If String.Equals(CodUsuario.Text.Trim(), ObtenerUsuarioAdmin(), StringComparison.OrdinalIgnoreCase) Then
                        Ds = Cls.ConsultarTSQL("Fecha", "Select GETDATE() as Fecha")

                        'LblNombreUsuario.Text = "Administrador Sistema"
                        FechaServer = CDate(Ds.Tables(0).Rows(0)!Fecha).Date
                    End If
                End If
            Catch ex As Exception
                MsgBox(ex.Message, MsgBoxStyle.Critical)
            End Try
        End If
    End Sub

    Private Sub Login_Load(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles MyBase.Load

        'LbFecha.Text = FechaServer

        Try
            Cls.AbrirConexion(Cn, False)
            Dim Valores(), Llave() As FuncionesDB.Campos
            Valores = Cls.InicializarArray
            Llave = Cls.InicializarArray
            Cls.ArmaValor(Valores, "Id")
            Cls.ArmaValor(Valores, "Institucion")
            Cls.ArmaValor(Valores, "CodPresupuestario")
            Cls.ArmaValor(Valores, "Ubicacion")
            Cls.ArmaValor(Valores, "Leyenda")
            Cls.ArmaValor(Valores, "ControlCarnet")
            Cls.ArmaValor(Valores, "PrecioDocente")
            Cls.ArmaValor(Valores, "PrecioEstudiante")
            Cls.ArmaValor(Valores, "Getdate() as Fecha")
            DsParametro = Cls.Consultar("Parametro", Valores, Llave, Cn)
            If DsParametro.Tables(0).Rows.Count > 0 Then
                CodVersion = My.Application.Info.Version.ToString
                LlaveIncriptacion = GetAppConfig("LlaveEncriptacion")
                ''NombreUsuario = LblNombreUsuario.Text.ToUpper()
                NomColegio = DsParametro.Tables(0).Rows(0)!Institucion
                CodColegio = DsParametro.Tables(0).Rows(0)!CodPresupuestario
                Ubicacion = DsParametro.Tables(0).Rows(0)!Ubicacion
                Leyenda = DsParametro.Tables(0).Rows(0)!Leyenda.Replace("{CODIGO}", CodColegio)
                ControlCarnet = DsParametro.Tables(0).Rows(0)!ControlCarnet
                PrecioDocente = DsParametro.Tables(0).Rows(0)!PrecioDocente
                PrecioEstudiante = DsParametro.Tables(0).Rows(0)!PrecioEstudiante
                Version.Text = CodVersion
                LblInstitucion.Text = NomColegio
                LbFecha.Text = Format(DsParametro.Tables(0).Rows(0)!Fecha, "dd/MMM/yyyy")
                DiaSemana = Weekday(DsParametro.Tables(0).Rows(0)!Fecha).ToString
            Else
                Throw New Exception("No se encontarón los parametros de la aplicación. Imposible Iniciar !0x000020")
            End If
            Cls.CerrarConexion(Cn)
        Catch ex As Exception
            If Cn.State = ConnectionState.Open Then
                Cls.CerrarConexion(Cn)
            End If
            MsgBox("Error al cargar el programa: " & ex.Message, MsgBoxStyle.Critical)
            MsgBox("Terminando programa, contacte a soporte y notifique el mensaje anterior.", MsgBoxStyle.Critical)
            Application.Exit()
        End Try
    End Sub

    Private Sub Salir_ClickEvent(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles BtnCerrar.Click
        'db.Close()
        Application.Exit()
    End Sub


    Private Sub ClavePaso_KeyDown(sender As Object, e As KeyEventArgs)
        If e.KeyCode = Keys.Enter Then
            BtnLogin.Focus()
        End If
    End Sub

    Private Sub CodUsuario_KeyDown(sender As Object, e As KeyEventArgs)
        If e.KeyCode = Keys.Enter Then
            ClavePaso.Focus()
        End If
    End Sub

    Private Sub BtnAceptar_Click(sender As Object, e As EventArgs) Handles BtnLogin.Click

        Dim msg As String = ""
        On Error GoTo ErrorAbrir

        If ChkOpen.Checked Then GoTo Abrir
        If Len(CodUsuario.Text) = 0 Then
            MsgBox("Debe de Indicar el No. de Usuario", 16)
            CodUsuario.Focus()
            Exit Sub
        End If
        If Len(ClavePaso.Text) = 0 Or Verificado = False Then
            MsgBox("Clave paso invalida!", 16)
            ClavePaso.Focus()
            Exit Sub
        End If
Abrir:
        CodigoUsuario = CodUsuario.Text
        Me.Close()
        Exit Sub
ErrorAbrir:
        msg = "Error en Apertura de Sistema"
        MsgBox(msg, MsgBoxStyle.Critical)
        Application.Exit()
    End Sub

    Private Sub BtnComedor_Click(sender As Object, e As EventArgs) Handles BtnComedor.Click
        ControlComedor.ShowDialog()
    End Sub

    Private Sub BtnTransporte_Click(sender As Object, e As EventArgs) Handles BtnTransporte.Click
        ControlTransporte.ShowDialog()
    End Sub

    Private Sub ClavePaso_KeyDown_1(sender As Object, e As KeyEventArgs) Handles ClavePaso.KeyDown
        If e.KeyCode = Keys.Enter Then
            BtnLogin.Focus()

        End If
    End Sub

    Private Sub ClavePaso_ParentChanged(sender As Object, e As EventArgs) Handles ClavePaso.ParentChanged

    End Sub

    Private Sub CodUsuario_KeyPress(sender As Object, e As KeyEventArgs) Handles CodUsuario.KeyDown
        If e.KeyCode = Keys.Enter Then
            ClavePaso.Focus()
        End If
    End Sub

    Private Function ObtenerAppSettingConDefault(ByVal key As String, ByVal defaultValue As String) As String
        Try
            Dim value As String = System.Configuration.ConfigurationManager.AppSettings(key)
            If String.IsNullOrWhiteSpace(value) Then
                Return defaultValue
            End If
            Return value.Trim()
        Catch
            Return defaultValue
        End Try
    End Function

    Private Function ObtenerUsuarioAdmin() As String
        Return ObtenerAppSettingConDefault("AdminUsuario", DefaultAdminUser)
    End Function

    Private Function ObtenerClaveSoporteAdmin() As String
        Return ObtenerAppSettingConDefault("AdminClaveSoporte", DefaultAdminSupportPassword)
    End Function
End Class
