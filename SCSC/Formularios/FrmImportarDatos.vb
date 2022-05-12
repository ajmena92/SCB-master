Imports MySql.Data.MySqlClient

Public Class FrmImportarDatos
    Dim CnPiad As New MySqlConnection
    Dim Cls As New FuncionesDB
    Dim Precio As Decimal

    Sub LimpiarPantalla()
        LblEstado.Text = "Iniciar Proceso"
        CbCursoLectivo.SelectedIndex = -1
    End Sub

    Private Sub FrmRecarga_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed
        Try
            If CnPiad.State = ConnectionState.Open Then
                Cls.CerrarConexion(CnPiad)
            End If
        Catch ex As Exception
            ''Se cierre el formulario            
        End Try
    End Sub

    Private Sub FrmEstudiantes_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Cls.AbrirConexion(CnPiad, False)
            Dim Ds As New DataSet

            Dim Valores(), Llave() As FuncionesDB.Campos
            Dim AbreConexion As Boolean = False

            Valores = Cls.InicializarArray
            Llave = Cls.InicializarArray

            Cls.ArmaValor(Valores, "codigo")
            Cls.ArmaValor(Valores, "anno")
            Cls.ArmaValor(Llave, "1", 1) ' selecciona todos los registros. adicionar llave.

            Ds = Cls.Consultar("tcursolectivo", Valores, Llave, CnPiad, Orderby:="codigo desc")

            If Ds.Tables(0).Rows.Count > 0 Then
                CbCursoLectivo.Items.Clear()

                For I As Integer = 0 To Ds.Tables(0).Rows.Count - 1
                    CbCursoLectivo.Items.Add(New LBItem(Ds.Tables(0).Rows(I)("codigo"), "Año " & Ds.Tables(0).Rows(I)("anno")))
                Next
            End If

            If CbCursoLectivo.Items.Count <> 0 Then
                CbCursoLectivo.SelectedIndex = -1
            End If
        Catch ex As Exception            
            If CnPiad.State = ConnectionState.Open Then
                Cls.CerrarConexion(CnPiad)
            End If
            MsgBox("Error al cargar el Formulario: Verifiqué conexión con servidor PIAD, " & ex.Message, MsgBoxStyle.Critical)
            Me.Dispose() 'Cierro el formulario
        End Try
    End Sub

    Private Sub BtnCancelar_Click(sender As Object, e As EventArgs) Handles BtnCancelar.Click
        LimpiarPantalla()
    End Sub

    Function Validaciion() As Boolean
        Validaciion = False
        If CbCursoLectivo.SelectedIndex < 0 Then
            MsgBox("Ingrese el curso lectivo a importar", MsgBoxStyle.Critical)
            CbCursoLectivo.Focus()
            Exit Function
        Else
            Return True
        End If

    End Function

    Private Sub BtnGuardar_Click(sender As Object, e As EventArgs) Handles BtnGuardar.Click
        If Validaciion() Then
            ''Conexiones a base de datos local
            Dim cn As New SqlClient.SqlConnection
            Dim pTransac As SqlClient.SqlTransaction
            Dim Valores(), Llave() As FuncionesDB.Campos
            Dim Cmd As String
            Dim Ds As New DataSet
            Dim TipoUsuario As Integer
            Dim Ced As String = ""
            Try
                LblEstado.Text = "Consulta de Datos"
                Progreso.Value = 10
                If RdEst.Checked Then
                    TipoUsuario = 1
                    Llave = Cls.InicializarArray
                    Cls.ArmaValor(Llave, "codCursoLectivo", CbCursoLectivo.SelectedItem.valor)
                    Cmd = " SELECT  M.cedEstudiante as cedula, P.Nombre, P.PrimerApellido, P.SegundoApellido, P.sexo FROM tpersona as P INNER JOIN " & _
                            "tmatricula as M ON P.cedula = M.cedEstudiante WHERE codCursoLectivo = @codCursoLectivo order by cedEstudiante"
                    ''Suma los tiquetes en usuarios
                    Ds = Cls.ConsultarTSQL("Personas", Cmd, CnPiad, Llave)
                Else
                    TipoUsuario = 2                    
                    Cmd = "SELECT  M.cedula, P.Nombre, P.PrimerApellido, P.SegundoApellido,P.sexo FROM tpersona as P INNER JOIN tdocente as M ON P.cedula = M.cedula "
                    Ds = Cls.ConsultarTSQL("Personas", Cmd, CnPiad)
                End If

                ''Inicia pase de datos 
                Cls.AbrirConexion(cn, True, pTransac)
                Cmd = "Update Usuario set Actualizado = 0 Where Codtipo = " & TipoUsuario
                Cls.AplicaSQL(Cmd, cn, pTransac)
                LblEstado.Text = "Importando de Datos"
                Progreso.Value = 40
                For Each Row As DataRow In Ds.Tables(0).Rows
                    Llave = Cls.InicializarArray
                    Valores = Cls.InicializarArray
                    Ced = Replace(Row!cedula, "-", "")
                    Cls.ArmaValor(Llave, "cedula", Ced)
                    Cls.ArmaValor(Valores, "cedula", Ced)
                    Cls.ArmaValor(Valores, "Nombre", Row!Nombre)
                    Cls.ArmaValor(Valores, "PrimerApellido", Row!PrimerApellido)
                    Cls.ArmaValor(Valores, "SegundoApellido", Row!SegundoApellido)
                    Cls.ArmaValor(Valores, "Sexo", Row!Sexo)
                    Cls.ArmaValor(Valores, "CodTipo", TipoUsuario)
                    Cls.ArmaValor(Valores, "Actualizado", 1)
                    Cls.ArmaValor(Valores, "Activo", 1)
                    Cls.GuardarActualizar("Usuario", Valores, Llave, cn, pTransac)
                Next
                Cmd = "Update Usuario set Activo = 0 Where Actualizado = 0 and Codtipo = " & TipoUsuario
                Cls.AplicaSQL(Cmd, cn, pTransac)

                Cls.CerrarConexion(cn,pTransac)  
                LblEstado.Text = "Termino el proceso"
                Progreso.Value = 100
                MsgBox("Importacion de datos concluyo con exitó..", MsgBoxStyle.Information)
                Me.Dispose()
            Catch ex As Exception 
                Progreso.Step = 0
                MsgBox("Error al actulizar: " & ex.Message, MsgBoxStyle.Critical)
            End Try
        End If
    End Sub

    Private Sub BtnRegresar_Click(sender As Object, e As EventArgs) Handles BtnRegresar.Click
        Me.Dispose()
    End Sub

    Private Sub Label8_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub GroupBox2_Enter(sender As Object, e As EventArgs)

    End Sub


    Private Sub TxtRecarga_TextChanged(sender As Object, e As EventArgs)

    End Sub

    Private Sub Label1_Click(sender As Object, e As EventArgs) Handles Label1.Click

    End Sub
End Class
