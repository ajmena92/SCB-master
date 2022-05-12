Option Strict Off
Option Explicit On

Friend Class Login
    Inherits System.Windows.Forms.Form
    Dim ClaveUsuario As String
    Dim Cls As New FuncionesDB
    Dim Cn As New SqlClient.SqlConnection
    ' *** Pendiente, revision: GrabaBitacora NO se utiliza en el formulario.
    Private Sub ClavePaso_Enter(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles ClavePaso.Enter
        ClavePaso.SelectAll()
    End Sub



    Private Sub ClavePaso_Leave(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles CodUsuario.Leave, ClavePaso.Leave
        ClavePaso.Text = ClavePaso.Text.Trim
        If Len(ClavePaso.Text) <> 0 Then
            Dim Seguridad As New Seguridad.Encriptacion64
            ''Esto para soporte de SicSoft Clave Paso
            If CodUsuario.Text.Trim.ToLower = "admin" Then
                If ClavePaso.Text.Trim <> "S0p0rt3" Then
                    MsgBox("Clave de Paso Inválida", 16)
                    ClavePaso.Text = ""
                    ClavePaso.Focus()
                End If
            Else
                If RTrim(Seguridad.encryptQueryString(ClavePaso.Text, LlaveIncriptacion)) <> RTrim(ClaveUsuario) Then
                    MsgBox("Clave de Paso Inválida", 16)
                    ClavePaso.Text = ""
                    ClavePaso.Focus()
                End If
            End If
        End If
    End Sub

    Private Sub CodUsuario_Enter(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs)
        CodUsuario.SelectAll()
    End Sub
    Private Sub CodUsuario_Leave(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles CodUsuario.Enter
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
                Cls.ArmaValor(Llave, "Usuario", CodUsuario.Text)

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
                    If CodUsuario.Text.ToLower = "admin" Then
                        Ds = Cls.ConsultarTSQL("Fecha", "Select GETDATE() as Fecha")

                        'LblNombreUsuario.Text = "Administrador Sistema"
                        FechaServer = CDate(Ds.Tables(0).Rows(0)!Fecha).Date
                    Else
                        MsgBox("No. de Usuario no Existe", MsgBoxStyle.Critical)
                        CodUsuario.Text = ""
                        CodUsuario.Focus()
                    End If
                End If
            Catch ex As Exception
                MsgBox(ex.Message, MsgBoxStyle.Critical)
            End Try
        End If
    End Sub

    Private Sub Login_Load(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles MyBase.Load

        'LbFecha.Text = FechaServer


        LbFecha.Text = Format(Now, "dd/MMM/yyyy")
        CodVersion = My.Application.Info.Version.ToString
        LlaveIncriptacion = GetAppConfig("LlaveEncriptacion")

        ''NombreUsuario = LblNombreUsuario.Text.ToUpper()
        NomColegio = "COLEGIO TÉCNICO PROFESIONAL PLATANARES"
        CodColegio = "2222"
        Ubicacion = "Cantón de Peréz Zeledón - Provincia San Jose"
        Leyenda = "Código: No.{CODIGO} - Circuito 07 – Platanares, Distrito Nose".Replace("{CODIGO}", CodColegio)


        Version.Text = CodVersion
        LblInstitucion.Text = NomColegio

    End Sub

    Private Sub Salir_ClickEvent(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles BtnCerrar.Click
        'db.Close()
        End
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
        If Len(ClavePaso.Text) = 0 Then
            MsgBox("Debe de Indicar la Clave de Paso", 16)
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
        End
    End Sub

    Private Sub BtnComedor_Click(sender As Object, e As EventArgs) Handles BtnComedor.Click
        ControlComedor.ShowDialog()
    End Sub

    Private Sub BtnTransporte_Click(sender As Object, e As EventArgs) Handles BtnTransporte.Click
        ControlTransporte.ShowDialog()
    End Sub
End Class