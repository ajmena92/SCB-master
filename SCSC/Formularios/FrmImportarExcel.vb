Imports System.Data.OleDb


Public Class FrmImportarExcel
    Dim Cls As New FuncionesDB
    Dim Precio As Decimal
    Dim conn As OleDbConnection
    Dim dta As OleDbDataAdapter
    Dim dts As DataSet
    Dim excel As String
    Dim OpenFileDialog As New OpenFileDialog

    Sub LimpiarPantalla()
        LblEstado.Text = "Iniciar Proceso"
    End Sub





    Private Sub BtnCancelar_Click(sender As Object, e As EventArgs) Handles BtnCancelar.Click
        LimpiarPantalla()
    End Sub

    Function Validaciion() As Boolean
        Validaciion = False
        If CBHorario.SelectedIndex <= 0 Then
            MsgBox("Ingrese el horario de los estudiantes a importar", MsgBoxStyle.Critical)
            CBHorario.Focus()
            Exit Function
        Else
            Return True
        End If

    End Function



    'Private Sub BtnGuardar_Click(sender As Object, e As EventArgs) Handles BtnGuardar.Click
    '    If Validaciion() Then
    '        ''Conexiones a base de datos local
    '        Dim cn As New SqlClient.SqlConnection
    '        Dim pTransac As SqlClient.SqlTransaction
    '        Dim Valores(), Llave() As FuncionesDB.Campos
    '        Dim Cmd As String
    '        Dim Ds As New DataSet
    '        Dim TipoUsuario As Integer
    '        Dim Ced As String = ""
    '        Try



    '            LblEstado.Text = "Consulta de Datos"
    '            Progreso.Value = 10
    '            If RdEst.Checked Then
    '                TipoUsuario = 1
    '                Llave = Cls.InicializarArray
    '                Cls.ArmaValor(Llave, "codCursoLectivo", CbCursoLectivo.SelectedItem.valor)
    '                Cmd = " SELECT  M.cedEstudiante as cedula, P.Nombre, P.PrimerApellido, P.SegundoApellido, P.sexo FROM tpersona as P INNER JOIN " &
    '                        "tmatricula as M ON P.cedula = M.cedEstudiante WHERE codCursoLectivo = @codCursoLectivo order by cedEstudiante"
    '                ''Suma los tiquetes en usuarios
    '                Ds = Cls.ConsultarTSQL("Personas", Cmd, CnPiad, Llave)
    '            Else
    '                TipoUsuario = 2
    '                Cmd = "SELECT  M.cedula, P.Nombre, P.PrimerApellido, P.SegundoApellido,P.sexo FROM tpersona as P INNER JOIN tdocente as M ON P.cedula = M.cedula "
    '                Ds = Cls.ConsultarTSQL("Personas", Cmd, CnPiad)
    '            End If

    '            ''Inicia pase de datos 
    '            Cls.AbrirConexion(cn, True, pTransac)
    '            Cmd = "Update Usuario set Actualizado = 0 Where Codtipo = " & TipoUsuario
    '            Cls.AplicaSQL(Cmd, cn, pTransac)
    '            LblEstado.Text = "Importando de Datos"
    '            Progreso.Value = 40
    '            For Each Row As DataRow In Ds.Tables(0).Rows
    '                Llave = Cls.InicializarArray
    '                Valores = Cls.InicializarArray
    '                Ced = Replace(Row!cedula, "-", "")
    '                Cls.ArmaValor(Llave, "cedula", Ced)
    '                Cls.ArmaValor(Valores, "cedula", Ced)
    '                Cls.ArmaValor(Valores, "Nombre", Row!Nombre)
    '                Cls.ArmaValor(Valores, "PrimerApellido", Row!PrimerApellido)
    '                Cls.ArmaValor(Valores, "SegundoApellido", Row!SegundoApellido)
    '                Cls.ArmaValor(Valores, "Sexo", Row!Sexo)
    '                Cls.ArmaValor(Valores, "CodTipo", TipoUsuario)
    '                Cls.ArmaValor(Valores, "Actualizado", 1)
    '                Cls.ArmaValor(Valores, "Activo", 1)
    '                Cls.GuardarActualizar("Usuario", Valores, Llave, cn, pTransac)
    '            Next
    '            Cmd = "Update Usuario set Activo = 0 Where Actualizado = 0 and Codtipo = " & TipoUsuario
    '            Cls.AplicaSQL(Cmd, cn, pTransac)

    '            Cls.CerrarConexion(cn, pTransac)
    '            LblEstado.Text = "Termino el proceso"
    '            Progreso.Value = 100
    '            MsgBox("Importacion de datos concluyo con exitó..", MsgBoxStyle.Information)
    '            Me.Dispose()
    '        Catch ex As Exception
    '            Progreso.Step = 0
    '            MsgBox("Error al actulizar: " & ex.Message, MsgBoxStyle.Critical)
    '        End Try
    '    End If
    'End Sub

    Private Sub BtnRegresar_Click(sender As Object, e As EventArgs) Handles BtnRegresar.Click
        Me.Dispose()
    End Sub


    Private Sub FrmImportarExcel_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            CargaHorarios(CBHorario)
        Catch ex As Exception
            MsgBox("Error al acarga el forumlario" + ex.Message)
            Me.Dispose()
        End Try
    End Sub

    Private Sub BtnGuardar_Click(sender As Object, e As EventArgs) Handles BtnGuardar.Click
        If Validaciion() Then
            Try
                OpenFileDialog.InitialDirectory = My.Computer.FileSystem.SpecialDirectories.MyDocuments
                OpenFileDialog.Filter = "Excel Files(.xls)|*.xls| Excel Files(.xlsx)|*.xlsx| Excel Files(*.xlsm)|*.xlsm"
                If OpenFileDialog.ShowDialog(Me) = System.Windows.Forms.DialogResult.OK Then
                    Dim Fi As New IO.FileInfo(OpenFileDialog.FileName)
                    Dim FileNmae As String = OpenFileDialog.FileName
                    excel = Fi.FullName
                    conn = New OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + excel + ";" + "Extended Properties='Excel 12.0 Xml;HDR=YES;'")
                    dta = New OleDbDataAdapter("Select * from [Lista$]", conn)
                    dts = New DataSet
                    dta.Fill(dts, "[Lista$]")
                    ''En este proceso se eliminan los primeros 6 registros con datos adminsitrativos del excel
                    For i As Integer = 0 To 4
                        dts.Tables(0).Rows(i).Delete()
                    Next
                    dts.AcceptChanges()

                    DGV1.DataSource = dts
                    DGV1.DataMember = "[Lista$]"
                    conn.Close()



                    ''Conexiones a base de datos local
                    Dim cn As New SqlClient.SqlConnection
                    Dim pTransac As SqlClient.SqlTransaction
                    Dim Valores(), Llave() As FuncionesDB.Campos
                    Dim Cmd As String
                    Dim TipoUsuario As Integer
                    Dim Ced As String = ""
                    Dim Contador As Integer = 0



                    Try
                        LblEstado.Text = "Consulta de Datos"
                        Progreso.Value = 50
                        If RdEst.Checked Then
                            TipoUsuario = 1
                        Else
                            TipoUsuario = 2
                            Throw New Exception("Funcion Profesores No Habilitada")
                        End If
                        Refresh()

                        ''Inicia pase de datos 
                        Cls.AbrirConexion(cn, True, pTransac)
                        Cmd = "Update Usuario set Actualizado = 0 Where Codtipo = " & TipoUsuario
                        Cls.AplicaSQL(Cmd, cn, pTransac)
                        LblEstado.Text = "Importando de Datos"
                        Progreso.Maximum = dts.Tables(0).Rows.Count
                        Progreso.Step = 1
                        Progreso.Value = 0
                        Refresh()
                        For Each Row As DataRow In dts.Tables(0).Rows
                            Llave = Cls.InicializarArray
                            Valores = Cls.InicializarArray
                            Ced = Replace(CType((Row(0)), String), "-", "")
                            Cls.ArmaValor(Llave, "cedula", Ced)
                            Cls.ArmaValor(Valores, "cedula", Ced)
                            Cls.ArmaValor(Valores, "PrimerApellido", Row(1))
                            Cls.ArmaValor(Valores, "SegundoApellido", Row(2))
                            Cls.ArmaValor(Valores, "Nombre", Row(3))
                            Cls.ArmaValor(Valores, "Seccion", Row(4))
                            Cls.ArmaValor(Valores, "IdHorario", CBHorario.SelectedIndex)
                            If (Row(5).ToString.Length() > 0) Then
                                Cls.ArmaValor(Valores, "Especialidad", Row(5).ToString.ToUpper())
                            Else
                                Cls.ArmaValor(Valores, "Especialidad", "III CICLO")
                            End If
                            Cls.ArmaValor(Valores, "FechaNac", Row(6))
                            Cls.ArmaValor(Valores, "Telefono", Row(8))
                            Cls.ArmaValor(Valores, "CodTipo", TipoUsuario)
                            Cls.ArmaValor(Valores, "Actualizado", 1)
                            Cls.ArmaValor(Valores, "Activo", 1)
                            Cls.GuardarActualizar("Usuario", Valores, Llave, cn, pTransac)
                            Progreso.Value = Progreso.Value + 1
                            Contador += 1
                            If Contador = 20 Then
                                Contador = 0
                                Refresh()
                            End If
                        Next
                        Cmd = "Update Usuario set Activo = 0 Where Actualizado = 0 and Codtipo = " & TipoUsuario
                        Cls.AplicaSQL(Cmd, cn, pTransac)

                        Cls.CerrarConexion(cn, pTransac)
                        LblEstado.Text = "Termino el proceso"
                        Progreso.Value = dts.Tables(0).Rows.Count
                        Refresh()
                        MsgBox("Importacion de datos concluyo con exitó..", MsgBoxStyle.Information)
                        Me.Dispose()
                    Catch ex As Exception
                        Progreso.Value = 0
                        Cls.RollSQL(pTransac)
                        MsgBox("Error al actulizar: " & ex.Message, MsgBoxStyle.Critical)
                    End Try
                End If
            Catch ex As Exception
                MsgBox("ERROR" + ex.Message)
                conn.Close()
            End Try
        End If
    End Sub

    Sub CargaHorarios(ByRef Combo As ComboBox)

        Try
            Dim Cn As New SqlClient.SqlConnection()
            'Dim Dr As SqlClient.SqlDataReader
            Dim Ds As New DataSet
            Dim Cls As New FuncionesDB
            Dim Valores(), Llave() As FuncionesDB.Campos
            Valores = Cls.InicializarArray
            Llave = Cls.InicializarArray

            Cls.AbrirConexion(Cn, False, )

            Cls.ArmaValor(Valores, "IdHorario")
            Cls.ArmaValor(Valores, "Descripcion")
            Cls.ArmaValor(Valores, "Activo")
            Cls.ArmaValor(Llave, "Activo", 1)
            Ds = Cls.Consultar("Horario", Valores, Llave, Cn)
            Combo.Items.Add(New LBItem(0, " SELECCIONE  -"))
            If Ds.Tables(0).Rows.Count > 0 Then
                For i As Integer = 0 To Ds.Tables(0).Rows.Count - 1
                    Combo.Items.Add(New LBItem(CType((Ds.Tables(0).Rows(i)!IdHorario), String), CType((Ds.Tables(0).Rows(i)!Descripcion), String)))
                Next

            End If
            Cls.CerrarConexion(Cn)
            Combo.SelectedIndex = 0
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub
End Class
