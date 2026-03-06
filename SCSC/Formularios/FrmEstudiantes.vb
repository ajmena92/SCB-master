Option Strict On
Option Explicit On

Imports System.IO
Imports System.Linq

Public Class FrmEstudiantes
    Dim DsRutas As New DataSet
    Dim DsBecas As New DataSet
    Dim ActivaEdicion As Boolean = False
    Dim Cn As New SqlClient.SqlConnection
    Dim Cls As New FuncionesDB
    Private Sub FrmEstudiantes_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If CrudVisualHelper.IsInDesignMode(Me) Then
            Return
        End If
        Try
            CrudVisualHelper.ApplyCrudStandard(Me, "dialogo")
            Cls.AbrirConexion(Cn, False)
            CargaRutas(CBRuta)
            CargaBecas(CBBeca)
            CargaGenero(CBGenero)
            CargaPermiso(CBPermisoSalida)
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
        Combo.Items.Add(New LBItem("0", "NO INGRESADO"))
        Combo.Items.Add(New LBItem("1", "MASCULINO"))
        Combo.Items.Add(New LBItem("2", "FEMENINO"))
    End Sub
    Sub CargaPermiso(ByRef Combo As ComboBox)
        Combo.Items.Add(New LBItem("0", "NO Autorizado"))
        Combo.Items.Add(New LBItem("1", "SI Autorizado"))
        Combo.SelectedIndex = 0
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
            DsRutas = Cls.Consultar("Ruta", Valores, Llave, Cn, Orderby:="IdRuta")
            If DsRutas.Tables(0).Rows.Count > 0 Then

                Combo.DataSource = DsRutas.Tables(0)
                Combo.DisplayMember = "Descripcion"
                Combo.ValueMember = "IdRutA"
            End If
            Combo.SelectedIndex = 0
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Sub CargaBecas(ByRef Combo As ComboBox)
        Try
            Dim Valores(), Llave() As FuncionesDB.Campos
            Valores = Cls.InicializarArray
            Llave = Cls.InicializarArray
            Cls.ArmaValor(Valores, "IdBeca")
            Cls.ArmaValor(Valores, "Descripcion")
            Cls.ArmaValor(Valores, "DiasBeca")
            Cls.ArmaValor(Valores, "Activo")
            Cls.ArmaValor(Llave, "Activo", 1)
            DsBecas = Cls.Consultar("TipoBeca", Valores, Llave, Cn)
            If DsBecas.Tables(0).Rows.Count > 0 Then
                Combo.DataSource = DsBecas.Tables(0)
                Combo.DisplayMember = "Descripcion"
                Combo.ValueMember = "IdBeca"
                'For i As Integer = 0 To DsBecas.Tables(0).Rows.Count - 1
                '    Combo.Items.Add(New LBItem(CType((DsBecas.Tables(0).Rows(i)!IdBeca), String), CType((DsBecas.Tables(0).Rows(i)!Descripcion), String)))
                'Next
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
        CBBeca.SelectedIndex = 0
        LblCantTiques.Text = "0 Tiquetes"
        If pFocus Then
            TxtCedula.Focus()
        End If
        Picture.Image = Nothing
        Picture.BackgroundImage = Nothing
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
            Using frm As New Global.SCSC.Busqueda()
                frm.ShowDialog(Me)
            End Using
            TxtCedula.Text = CStr(gSession.Resultado(0))
            TxtCedula_Validated(sender, e)
            BtnGuardar.Select()
        Catch ex As Exception
            'MsgBox(MSJ.Mensajes.ErrorBusqueda)
        End Try

    End Sub

    Private Sub TxtCedula_Validated(sender As Object, e As EventArgs) Handles TxtCedula.Validated
        LimpiarPantalla(False)
        Dim Cedula As String = Replace(TxtCedula.Text.Trim, ControlCarnet, "")
        Dim Ds As New DataSet
        Dim Valores(), Llave() As FuncionesDB.Campos
        If Cedula.Trim().Length > 0 Then
            Try
                Valores = Cls.InicializarArray
                Llave = Cls.InicializarArray
                Cls.ArmaValor(Llave, "Cedula", Cedula)
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
                Cls.ArmaValor(Valores, "TipoBeca")
                Cls.ArmaValor(Valores, "HuellaDactilar")
                Cls.ArmaValor(Valores, "PendienteBecaTransporte")
                Cls.ArmaValor(Valores, "PermisoSalida")
                Cls.ArmaValor(Valores, "Activo")

                Ds = Cls.Consultar("Usuario", Valores, Llave, Cn)
                If Ds.Tables(0).Rows.Count > 0 Then
                    Dim row As DataRow = Ds.Tables(0).Rows(0)
                    TxtNombre.Text = CStr(row("Nombre"))
                    TxtApe1.Text = CStr(row("PrimerApellido"))
                    TxtApe2.Text = CStr(row("SegundoApellido"))
                    TxtFecNac.Text = Convert.ToString(row("FechaNac"))
                    TxtTelefono.Text = Convert.ToString(row("Telefono"))
                    TxtSeccion.Text = Convert.ToString(row("Seccion"))

                    If IsDBNull(row("PendienteBecaTransporte")) Then
                        CBRutaPendiente.Checked = False
                    Else
                        CBRutaPendiente.Checked = CBool(row("PendienteBecaTransporte"))
                    End If

                    If IsDBNull(row("PermisoSalida")) Then
                        CBPermisoSalida.SelectedIndex = 0
                    Else
                        If CBool(row("PermisoSalida")) Then
                            CBPermisoSalida.SelectedIndex = 1
                        Else
                            CBPermisoSalida.SelectedIndex = 0
                        End If
                    End If

                    If IsDBNull(row("Sexo")) Then
                        CBGenero.SelectedIndex = 0
                    Else
                        If CInt(row("Sexo")) = 1 Then
                            CBGenero.SelectedIndex = 1
                        ElseIf CInt(row("Sexo")) = 2 Then
                            CBGenero.SelectedIndex = 2
                        Else
                            CBGenero.SelectedIndex = 0
                        End If
                    End If

                    If IsDBNull(row("IdRuta")) Then
                        CBRuta.SelectedIndex = 0
                    Else
                        CBRuta.SelectedValue = CInt(row("IdRuta"))
                    End If

                    If IsDBNull(row("TipoBeca")) Then
                        CBBeca.SelectedIndex = 0
                    Else
                        CBBeca.SelectedValue = CInt(row("TipoBeca"))
                    End If

                    If CInt(row("CodTipo")) = 1 Then
                        TxtTipoUsuario.Text = "ESTUDIANTE"
                    Else
                        TxtTipoUsuario.Text = "PROFESOR"
                    End If
                    If IsDBNull(row("HuellaDactilar")) Then
                        Picture.BackgroundImage = Nothing
                    Else
                        Picture.BackgroundImage = My.Resources.huella_dactilar
                    End If
                    TxtCedula.Tag = CInt(row("IdUsuario"))
                    LblCantTiques.Text = CInt(row("CantidadTiquetes")).ToString() & " Tiquetes"
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
            CBBeca.Focus()
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
            Dim tipoBeca As Integer = If(CBBeca.SelectedValue Is Nothing, 0, CInt(CBBeca.SelectedValue))
            Dim idRuta As Integer = If(CBRuta.SelectedValue Is Nothing, 0, CInt(CBRuta.SelectedValue))
            If (TxtTipoUsuario.Text = "PROFESOR" And CBBeca.SelectedIndex <> 0) Then
                MsgBox("Al usuario tipo (PROFESOR) no se puede activar el beneficio de la BECA.", MsgBoxStyle.Exclamation)
            Else
                Valores = Cls.InicializarArray
                Llave = Cls.InicializarArray
                Cls.ArmaValor(Llave, "IdUsuario", TxtCedula.Tag)
                Cls.ArmaValor(Valores, "TipoBeca", tipoBeca)
                Cls.ArmaValor(Valores, "IdRuta", idRuta)
                Cls.ArmaValor(Valores, "Sexo", CBGenero.SelectedIndex)
                Cls.ArmaValor(Valores, "PendienteBecaTransporte", CBRutaPendiente.Checked)
                Cls.ArmaValor(Valores, "PermisoSalida", CBPermisoSalida.SelectedIndex)
                Cls.ArmaValor(Valores, "Activo", True)
                Cls.Update("Usuario", Valores, Llave, Cn)
                BtnCancelar_Click(sender, e)
            End If
        Catch ex As Exception
            MsgBox("Error al actulizar: " & ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnCancelar_Click(sender As Object, e As EventArgs) Handles BtnCancelar.Click
        ActivaEdicion = False
        LimpiarPantalla()
        TxtCedula.Clear()
        TxtCedula.Focus()
    End Sub

    Private Sub CBRuta_SelectedIndexChanged(sender As Object, e As EventArgs) Handles CBRuta.SelectedIndexChanged
        Dim pIdRuta As Integer
        If CBRuta.SelectedIndex = 0 Then
            pIdRuta = 1
        Else
            pIdRuta = CInt(CBRuta.SelectedValue)
        End If
        Dim result As DataRow = (From cust As DataRow In DsRutas.Tables(0).Select("IdRuta = " & pIdRuta.ToString())).SingleOrDefault()
        If result IsNot Nothing Then
            LblRuta.Text = CStr(result("Codigo"))
        Else
            LblRuta.Text = String.Empty
        End If
    End Sub

    Private Sub Label16_Click(sender As Object, e As EventArgs) Handles Label16.Click

    End Sub

    Private Sub FrmEstudiantes_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        Try
            If Cn.State = ConnectionState.Open Then
                Cls.CerrarConexion(Cn)
            End If
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub
End Class
