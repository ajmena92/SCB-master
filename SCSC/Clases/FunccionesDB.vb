Option Explicit On

Imports System.Data.SqlClient


' Dar formato a celdas de datagridview en windows.

'Dset = Cls.Consultar(Cn, "V_Transacciones", Valores, Llave, "NumFactura Desc")
'DataGridPendientes.DataSource = Dset
'DataGridPendientes.DataMember = "V_Transacciones"
'DataGridPendientes.Columns("Monto").DefaultCellStyle.Format = "c"
'DataGridPendientes.Columns("Monto").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
'
'
Public Class FuncionesDB
    Public Structure Campos
        Public Nombre As String ' nombre del campo en la tabla
        Public Valor As Object ' puede ser texto o numero, date, etc.
        Public Formato As String
    End Structure
    Function Redondear(ValorNumero)
        'Función para el Redondeo de Números
        'A Colones
        ValorNumero = Val(ValorNumero)
        Dim Entero, Mult5 As Integer
        Dim Decimales As Double
        Entero = Int(ValorNumero) 'Extrae la parte entera
        Decimales = ((ValorNumero - Entero) * 100) + 2.5
        Mult5 = Int((Decimales / 5))
        Decimales = Mult5 * 5
        If Decimales >= 100 Then
            Redondear = Entero + 1
        Else
            Redondear = Entero + (Decimales * 0.01)
        End If
    End Function
    'Public Sub AbrirConexion(ByRef pCnMysql As MySqlConnection, ByVal pUsarTransaccion As Boolean, Optional ByRef pTran As MySqlTransaction = Nothing, Optional ByVal Conexion As String = "Cnpiad")
    '    Try
    '        'Dim Cn As New SqlConnection(CadenaConexion)

    '        If Conexion.ToUpper = "Cnpiad".ToUpper Then
    '            pCnMysql.ConnectionString = GetAppConfig("PIAD")
    '        Else
    '            pCnMysql.ConnectionString = Conexion
    '        End If
    '        pCnMysql.Open()
    '        If pUsarTransaccion Then
    '            ' se manejan transacciones
    '            pTran = pCnMysql.BeginTransaction
    '        Else
    '            ' Sin transacciones
    '        End If

    '    Catch ex As Exception
    '        Throw New Exception("FuncionesDB.AbrirConexion..." & ex.Message)
    '    End Try
    'End Sub
    'Function GetConnectionString(ByVal NombreConfiguracion As String) As String
    '    Dim strConn As String
    '    'Dim configurationAppSettings As New System.Configuration.AppSettingsReader
    '    'strConn = CStr(configurationAppSettings.GetValue(NombreConfiguracion, GetType(String)))
    '    strConn = ConfigurationManager.ConnectionStrings(NombreConfiguracion).ConnectionString()
    '    Return strConn
    'End Function

    'Private ConString As String
    'solo funciona en app de windows
    'Public Property ConnectionString() As String
    '    Get
    '        If ConString = "" Then
    '            Return ConfigurationManager.ConnectionStrings("Default").ConnectionString()
    '        Else
    '            Return ConfigurationManager.ConnectionStrings(ConString).ConnectionString
    '        End If
    '    End Get
    '    Set(ByVal value As String)
    '        ConString = value
    '    End Set
    'End Property

    Public Sub AbrirConexion(ByRef pCn As SqlConnection, ByVal pUsarTransaccion As Boolean, Optional ByRef pTran As SqlTransaction = Nothing, Optional ByVal Conexion As String = "Conexion")
        Try
            'Dim Cn As New SqlConnection(CadenaConexion)

            If Conexion.ToUpper = "Conexion".ToUpper Then
                pCn.ConnectionString = GetAppConfig(Conexion)
            Else
                pCn.ConnectionString = Conexion
            End If
            'pCn.ConnectionString = "server=DANIELFONSECA\SQLDANIEL; database=SCSC; uid=Daniel; pwd=esquivel07"

            pCn.Open()
            If pUsarTransaccion Then
                ' se manejan transacciones
                pTran = pCn.BeginTransaction
            Else
                ' Sin transacciones
            End If

        Catch ex As Exception
            Throw New Exception("FuncionesDB.AbrirConexion..." & ex.Message)
        End Try
    End Sub

    Public Sub CerrarConexion(ByRef pCn As SqlConnection, Optional ByRef pTran As SqlTransaction = Nothing)
        Try
            If Not pTran Is Nothing Then
                ' se manejan transacciones
                pTran.Commit() ' guarda la transaccion.
            Else
                ' Sin transacciones
            End If

            If pCn.State = ConnectionState.Open Then
                pCn.Close()
            End If
        Catch ex As Exception
            Throw New Exception("FuncionesDB.CerrarConexion..." & ex.Message)
        End Try
    End Sub

    'Public Sub CerrarConexion(ByRef pCn As MySqlConnection, Optional ByRef pTran As MySqlTransaction = Nothing)
    '    Try
    '        If Not pTran Is Nothing Then
    '            ' se manejan transacciones
    '            pTran.Commit() ' guarda la transaccion.
    '        Else
    '            ' Sin transacciones
    '        End If
    '        If pCn.State = ConnectionState.Open Then
    '            pCn.Close()
    '        End If
    '    Catch ex As Exception
    '        Throw New Exception("FuncionesDB.CerrarConexion..." & ex.Message)
    '    End Try
    'End Sub

    Function FechaServer(Optional ByVal Conexion As String = "Conexion") As Date
        Try
            Dim Cls As New FuncionesDB

            Dim SQL As String = ("select convert(varchar,getdate(),103)+ ' ' + convert(varchar,getdate(),108) as FechaServer")
            Dim Dset As New DataSet

            Dset = Cls.ConsultarTSQL("Hora", SQL, , , , Conexion)

            If Dset.Tables(0).Rows.Count > 0 Then
                FechaServer = Dset.Tables(0).Rows(0)!FechaServer
            Else
                Throw New Exception("Servidor No ha Devuelto ninguna FECHA.")
            End If

        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Function FechaSistema(Optional ByVal Conexion As String = "Conexion") As Date
        Try
            Dim Cls As New FuncionesDB

            Dim SQL As String = ("select FechaSistema From Parametros")
            Dim Dset As New DataSet

            Dset = Cls.ConsultarTSQL("FechaSistema", SQL, , , , Conexion)

            If Dset.Tables(0).Rows.Count > 0 Then
                Return Dset.Tables(0).Rows(0)!FechaSistema
            Else
                Throw New Exception("ERROR, Servidor No Posee los Parametros de configuración.")
            End If

        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Function FechaCierre(Optional ByVal Conexion As String = "Conexion") As Date
        Try
            Dim Cls As New FuncionesDB

            Dim SQL As String = ("select FechaCierrePeriodo From ParametrosInventar")
            Dim Dset As New DataSet

            Dset = Cls.ConsultarTSQL("FechaCierrePeriodo", SQL, , , , Conexion)

            If Dset.Tables(0).Rows.Count > 0 Then
                Return Dset.Tables(0).Rows(0)!FechaCierrePeriodo
            Else
                Throw New Exception("ERROR, Servidor No Posee los Parametros de configuración.")
            End If

        Catch ex As Exception
            Throw ex
        End Try
    End Function


    ''' <summary>
    ''' Borra un registro de la base de datos indicada
    ''' </summary>
    ''' <param name="Tabla">Nombre de la tabla donde se encuentra el registro a borrar</param>
    ''' <param name="Llave">Campo llave para ubicar el dato a borrar</param>
    ''' <param name="Valor">Codigo o valor para borrar en la tabla indicada</param>
    ''' <returns>Retorna verdadero en caso de exito, caso de error, lanza una excepcion con el error capturado.
    '''          Retorna Falso, en caso de no lograr el borrado</returns>
    ''' <remarks></remarks>
    Public Function Delete(ByVal Tabla As String, ByVal Llave As String, _
                           ByVal Valor As Object, Optional ByRef Cn As SqlConnection = Nothing, Optional ByRef PTransac As SqlTransaction = Nothing, Optional ByVal Conexion As String = "Conexion", Optional TimeOut As Integer = 45) As Boolean
        'Función para el Borrado de Datos en las Tablas
        Dim LocalCN As Boolean = False
        Try
            'Dim PTransac As SqlTransaction = Nothing
            If Cn Is Nothing Or IsNothing(Cn) Then
                Cn = New SqlClient.SqlConnection()
                LocalCN = True
                If IsNothing(PTransac) Then
                    AbrirConexion(Cn, False, , Conexion)
                Else
                    AbrirConexion(Cn, True, PTransac, Conexion)
                End If
            End If

            Dim SQL As String = "Delete From " & Tabla & " Where " & Llave & "=@Valor"
            Dim Cmd As New SqlCommand(SQL, Cn)
            Cmd.CommandTimeout = TimeOut
            Cmd.Parameters.Clear()
            Cmd.Parameters.AddWithValue("@Valor", Valor)

            'AbrirConexion(Cn, True, PTransac)

            If IsNothing(PTransac) Then
            Else
                Cmd.Transaction = PTransac
            End If
            Cmd.ExecuteNonQuery()

            '*** si NO se manda CN como parametro, se crea localmente, y se cierra.
            If LocalCN Then
                If IsNothing(PTransac) Then
                    CerrarConexion(Cn)
                Else
                    CerrarConexion(Cn, PTransac)
                End If
            End If

            Return True

        Catch ex As Exception
            If Not PTransac Is Nothing Then
                PTransac.Rollback()
            End If

            Dim Mensaje As String = "FuncionesDB.Delete (Simple) ERROR En El Borrado de Datos en la Tabla: " & Tabla & ".!!!" & _
                ". Error devuelto: " & ex.Message
            Throw New Exception(Mensaje)
        End Try
    End Function

    '<WebMethod(messagename:="DeleteArray")> _
    Public Function Delete(ByVal Tabla As String, ByVal Llave As Campos(), Optional ByRef Cn As SqlConnection = Nothing, Optional ByRef PTransac As SqlTransaction = Nothing, Optional ByVal Conexion As String = "Conexion", Optional TimeOut As Integer = 45) As Boolean
        'Función para el Borrado de Datos en las Tablas
        Dim LocalCN As Boolean = False
        Try
            If Cn Is Nothing Or IsNothing(Cn) Then
                Cn = New SqlClient.SqlConnection()
                LocalCN = True
                If IsNothing(PTransac) Then
                    AbrirConexion(Cn, False, , Conexion)
                Else
                    AbrirConexion(Cn, True, PTransac, Conexion)
                End If
            End If


            ' ************ armar sql
            Dim SQL As String = "Delete From " & Tabla & " Where " '& Campo & "=@Valor"

            For I As Integer = 0 To UBound(Llave)
                SQL &= Llave(I).Nombre & "=@Valor" & I & " And "
            Next

            SQL = SQL.Remove(SQL.Length - 5, 5) ' quita el ultimo And. para corregir sintaxis SQL

            Dim Cmd As New SqlCommand(SQL, Cn)
            Cmd.CommandTimeout = TimeOut
            Cmd.Parameters.Clear()

            For I As Integer = 0 To UBound(Llave)
                Cmd.Parameters.AddWithValue("@Valor" & I, Llave(I).Valor)
            Next


            'AbrirConexion(Cn, True, PTransac) ' debe ir fuera del delete
            If IsNothing(PTransac) Then
            Else
                Cmd.Transaction = PTransac
            End If
            Cmd.ExecuteNonQuery()

            '*** si NO se manda CN como parametro, se crea localmente, y se cierra.
            If LocalCN Then
                If IsNothing(PTransac) Then
                    CerrarConexion(Cn)
                Else
                    CerrarConexion(Cn, PTransac)
                End If
            End If
            'CerrarConexion(Cn, PTransac)  ' debe ir fuera del delete.

            Return True

        Catch ex As Exception
            If Not PTransac Is Nothing Then
                PTransac.Rollback()
            End If
            Dim Mensaje As String = "FuncionesDB.Delete(Con array). ERROR En El Borrado de Datos en la Tabla: " & Tabla & ".!!!" & _
                ". Error devuelto: " & ex.Message
            Throw New Exception(Mensaje)
        End Try
    End Function

    Public Function Insert(ByVal Tabla As String, ByVal Campo As Campos(), Optional ByRef Cn As SqlConnection = Nothing, Optional ByRef PTransac As SqlTransaction = Nothing, Optional ByVal Conexion As String = "Conexion", Optional TimeOut As Integer = 45) As Boolean
        'Función para el Borrado de Datos en las Tablas
        ' *** puede dar problemas al insertar una fecha,
        Dim LocalCN As Boolean = False
        Try
            If Cn Is Nothing Or IsNothing(Cn) Then

                LocalCN = True
                If IsNothing(PTransac) Then
                    AbrirConexion(Cn, False, , Conexion)
                Else
                    AbrirConexion(Cn, True, PTransac, Conexion)
                End If
            End If
            ' ************ armar sql
            ' Insert into tabla (campo1, campo2) Values (@valor1,@valor2)
            Dim SQL As String = "Insert into " & Tabla & " ( " '& Campo & "=@Valor"

            For I As Integer = 0 To UBound(Campo)
                If Campo(I).Valor.ToString.ToUpper = "NULL" Then
                Else
                    SQL &= Campo(I).Nombre & ","
                End If
            Next

            SQL = SQL.Remove(SQL.Length - 1, 1) & ") Values ("

            For I As Integer = 0 To UBound(Campo)
                If Campo(I).Valor.ToString.ToUpper = "NULL" Then
                    ' no se incluyen campos nulos.
                Else
                    SQL &= "@Valor" & I & ","
                End If

            Next
            SQL = SQL.Remove(SQL.Length - 1, 1) & ") "

            Dim Cmd As New SqlCommand(SQL, Cn)
            Cmd.CommandTimeout = TimeOut
            Cmd.Parameters.Clear()

            For I As Integer = 0 To UBound(Campo)
                If Campo(I).Valor.ToString.ToUpper = "NULL" Then
                    ' Cmd.Parameters.AddWithValue("@Valor" & I, DBNull.Value)
                Else
                    Cmd.Parameters.AddWithValue("@Valor" & I, Campo(I).Valor)
                End If

            Next

            'AbrirConexion(Cn, True, PTransac) ' debe ir fuera del delete

            If IsNothing(PTransac) Then
            Else
                Cmd.Transaction = PTransac
            End If

            Cmd.ExecuteNonQuery()

            '*** si NO se manda CN como parametro, se crea localmente, y se cierra.
            If LocalCN Then
                If IsNothing(PTransac) Then
                    CerrarConexion(Cn)
                Else
                    CerrarConexion(Cn, PTransac)
                End If
            End If

            'CerrarConexion(Cn, PTransac)  ' debe ir fuera del delete.

            Return True
        Catch ex As Exception
            If Not PTransac Is Nothing Then
                PTransac.Rollback()
            End If
            Dim Mensaje As String = "FuncionesDB.Insert...ERROR EN La insersión de Datos en la Tabla: " & Tabla & ". !!!" & _
                "F. Error devuelto: " & ex.Message
            Throw New Exception(Mensaje)
        End Try
    End Function

    ''' <summary>
    ''' Inserta Registros en tablas que tengan campo Identity, devuelve el codigo del dato identity insertado.
    ''' </summary>
    ''' <param name="Cn">conexion utilizada</param>
    ''' <param name="Tabla">Tabla que sera guardada</param>
    ''' <param name="Campo">Campos que seran guardados con su respectivo valor, se accesan x indice: campos(I).valor , etc</param>
    ''' <param name="PTransac">Para utilizar transacciones, en caso de no necesitar transacciones, dejar en blanco.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function InsertScalar(ByVal Tabla As String, ByVal Campo As Campos(), Optional ByRef Cn As SqlConnection = Nothing, Optional ByRef PTransac As SqlTransaction = Nothing, Optional ByVal Conexion As String = "Conexion", Optional TimeOut As Integer = 45) As Integer
        Dim LocalCN As Boolean = False
        Dim Resultado As Integer = 0
        Try
            If Cn Is Nothing Or IsNothing(Cn) Then
                Cn = New SqlClient.SqlConnection()
                LocalCN = True
                If IsNothing(PTransac) Then
                    AbrirConexion(Cn, False, , Conexion)
                Else
                    AbrirConexion(Cn, True, PTransac, Conexion)
                End If
            End If
            ' ************ armar sql
            ' Insert into tabla (campo1, campo2) Values (@valor1,@valor2)
            Dim SQL As String = "Insert into " & Tabla & "( " '& Campo & "=@Valor"

            For I As Integer = 0 To UBound(Campo)
                SQL &= Campo(I).Nombre & ","
            Next

            SQL = SQL.Remove(SQL.Length - 1, 1) & ") Values ("

            For I As Integer = 0 To UBound(Campo)
                SQL &= "@Valor" & I & ","
            Next
            SQL = SQL.Remove(SQL.Length - 1, 1) & ") "

            SQL &= ";SELECT  Scope_Identity();"

            Dim Cmd As New SqlCommand(SQL, Cn)
            Cmd.CommandTimeout = TimeOut
            Cmd.Parameters.Clear()

            For I As Integer = 0 To UBound(Campo)
                Cmd.Parameters.AddWithValue("@Valor" & I, Campo(I).Valor)
            Next

            If IsNothing(PTransac) Then
            Else
                Cmd.Transaction = PTransac
            End If

            Resultado = Cmd.ExecuteScalar

            '*** si NO se manda CN como parametro, se crea localmente, y se cierra.
            If LocalCN Then
                If IsNothing(PTransac) Then
                    CerrarConexion(Cn)
                Else
                    CerrarConexion(Cn, PTransac)
                End If
            End If

            Return Resultado
        Catch ex As Exception
            If Not PTransac Is Nothing Then
                PTransac.Rollback()
            End If
            Dim Mensaje As String = "FuncionesDB.InsertSCALAR ...ERROR EN La insersión de Datos en la Tabla: " & Tabla & ". !!!" & _
                "F. Error devuelto: " & ex.Message
            Throw New Exception(Mensaje)
        End Try
    End Function

    Public Function Update(ByVal Tabla As String, ByVal Campo As Campos(), ByRef LlavePrimaria As Campos(), Optional ByRef Cn As SqlConnection = Nothing, Optional ByRef PTransac As SqlTransaction = Nothing, Optional ByVal Conexion As String = "Conexion", Optional TimeOut As Integer = 45) As Boolean
        'Función para el Borrado de Datos en las Tablas
        Dim LocalCN As Boolean = False
        Try
            If Cn Is Nothing Or IsNothing(Cn) Then
                Cn = New SqlClient.SqlConnection
                LocalCN = True
                If IsNothing(PTransac) Then
                    AbrirConexion(Cn, False, , Conexion)
                Else
                    AbrirConexion(Cn, True, PTransac, Conexion)
                End If
            End If
            ' ************ armar sql
            ' Update Tabla003 set campo1=@Valor1, campo2=@valor2 Where Llave1=@Valor1 and Llave2=@Valor2
            Dim SQL As String = "Update " & Tabla & " set "

            For I As Integer = 0 To UBound(Campo)
                If Campo(I).Valor.ToString.ToUpper = "NULL" Then
                Else
                    SQL &= Campo(I).Nombre & "=@Valor" & I & ","
                End If

            Next

            SQL = SQL.Remove(SQL.Length - 1, 1) & " Where "

            For I As Integer = 0 To UBound(LlavePrimaria)
                SQL &= LlavePrimaria(I).Nombre & "=@ValorLlave" & I & " and "
            Next
            SQL = SQL.Remove(SQL.Length - 5, 5)

            Dim Cmd As New SqlCommand(SQL, Cn)
            Cmd.CommandTimeout = TimeOut
            Cmd.Parameters.Clear()

            For I As Integer = 0 To UBound(Campo)

                If Campo(I).Valor.ToString.ToUpper = "NULL" Then
                Else
                    Cmd.Parameters.AddWithValue("@Valor" & I, Campo(I).Valor)
                End If
            Next

            For I As Integer = 0 To UBound(LlavePrimaria)
                Cmd.Parameters.AddWithValue("@ValorLlave" & I, LlavePrimaria(I).Valor)
            Next

            'AbrirConexion(Cn, True, PTransac) ' debe ir fuera del delete
            If IsNothing(PTransac) Then
            Else
                Cmd.Transaction = PTransac
            End If

            Cmd.ExecuteNonQuery()

            'CerrarConexion(Cn, PTransac)  ' debe ir fuera del delete.

            If LocalCN Then
                If IsNothing(PTransac) Then
                    CerrarConexion(Cn)
                Else
                    CerrarConexion(Cn, PTransac)
                End If
            End If

            Return True

        Catch ex As Exception
            If Not PTransac Is Nothing Then
                PTransac.Rollback()
            End If

            Dim Mensaje As String = "FuncionesDB.Update...ERROR En El Actualizado de datos en la Tabla: " & Tabla & ".!!!" & _
                ". Error devuelto: " & ex.Message
            Throw New Exception(Mensaje)
        End Try
    End Function

    Public Function ConsultarTSQL(ByVal Tabla As String, ByVal pSQL As String, Optional ByVal LlavePrimaria As Campos() = Nothing, _
            Optional ByRef Cn As SqlConnection = Nothing, Optional ByRef Ptransac As SqlTransaction = Nothing, Optional ByVal Conexion As String = "Conexion", Optional TimeOut As Integer = 45) As System.Data.DataSet
        ' esta consulta aplica para los SQL que requieren de un group by. y seleccion de registros x Having
        Dim LocalCN As Boolean = False
        Try
            Dim Resultado As New System.Data.DataSet

            If Cn Is Nothing Or IsNothing(Cn) Then
                Cn = New SqlClient.SqlConnection
                LocalCN = True
                If IsNothing(Ptransac) Then
                    AbrirConexion(Cn, False, , Conexion)
                Else
                    AbrirConexion(Cn, True, Ptransac, Conexion)
                End If
            End If

            Dim Cmd As New SqlCommand(pSQL, Cn)
            Cmd.CommandTimeout = TimeOut
            Cmd.Parameters.Clear()

            If Not (LlavePrimaria Is Nothing) Then
                For I As Integer = 0 To UBound(LlavePrimaria)
                    Cmd.Parameters.AddWithValue(LlavePrimaria(I).Nombre, LlavePrimaria(I).Valor)
                Next
            End If

            If IsNothing(Ptransac) Then
            Else
                Cmd.Transaction = Ptransac
            End If

            Dim Da As New SqlDataAdapter(Cmd)
            If Tabla = "" Then
                Da.Fill(Resultado)
            Else
                Da.Fill(Resultado, Tabla)
            End If

            If LocalCN Then
                If IsNothing(Ptransac) Then
                    CerrarConexion(Cn)
                Else
                    CerrarConexion(Cn, Ptransac)
                End If
            End If

            Return Resultado
        Catch ex As Exception
            If Not Ptransac Is Nothing Then
                Ptransac.Rollback()
            End If
            Throw New Exception("FuncionesDB.Consultar  " & ex.Message)
        End Try

    End Function

    'Public Function ConsultarTSQL(ByVal Tabla As String, ByVal pSQL As String, ByRef Cn As MySqlConnection, Optional ByVal LlavePrimaria As Campos() = Nothing, Optional ByRef Ptransac As MySqlTransaction = Nothing, Optional ByVal Conexion As String = "Conexion", Optional TimeOut As Integer = 45) As System.Data.DataSet
    '    ' esta consulta aplica para los SQL que requieren de un group by. y seleccion de registros x Having
    '    Dim LocalCN As Boolean = False
    '    Try
    '        Dim Resultado As New System.Data.DataSet
    '        Dim Cmd As New MySqlCommand(pSQL, Cn)
    '        Cmd.CommandTimeout = TimeOut
    '        Cmd.Parameters.Clear()

    '        If Not (LlavePrimaria Is Nothing) Then
    '            For I As Integer = 0 To UBound(LlavePrimaria)
    '                Cmd.Parameters.AddWithValue(LlavePrimaria(I).Nombre, LlavePrimaria(I).Valor)
    '            Next
    '        End If

    '        If IsNothing(Ptransac) Then
    '        Else
    '            Cmd.Transaction = Ptransac
    '        End If

    '        Dim Da As New MySqlDataAdapter(Cmd)
    '        If Tabla = "" Then
    '            Da.Fill(Resultado)
    '        Else
    '            Da.Fill(Resultado, Tabla)
    '        End If
    '        Return Resultado
    '    Catch ex As Exception
    '        If Not Ptransac Is Nothing Then
    '            Ptransac.Rollback()
    '        End If
    '        Throw New Exception("FuncionesDB.Consultar  " & ex.Message)
    '    End Try

    'End Function

    Public Function ConsultarTSQLGroupBy(ByVal Tabla As String, ByVal pSQL As String, _
            Optional ByRef Cn As SqlConnection = Nothing, Optional ByRef Ptransac As SqlTransaction = Nothing, Optional ByVal LlavePrimaria As Campos() = Nothing, Optional ByVal pOperador As String = " And ", Optional ByVal Conexion As String = "Conexion", Optional TimeOut As Integer = 45) As System.Data.DataSet
        ' esta consulta aplica para los SQL que requieren de un group by. y seleccion de registros x Having
        Dim LocalCN As Boolean = False
        Try
            Dim Resultado As New System.Data.DataSet

            If Not (LlavePrimaria Is Nothing) Then
                pSQL += " Having "
                For I As Integer = 0 To UBound(LlavePrimaria)
                    ' manejar uso de like
                    If InStr(LlavePrimaria(I).Valor, "%") > 0 Then
                        ' uso de like
                        pSQL &= LlavePrimaria(I).Nombre & " Like @ValorLlave" & I & pOperador ' " And "
                    Else
                        'uso de igual =
                        pSQL &= LlavePrimaria(I).Nombre & "=@ValorLlave" & I & pOperador ' " And "
                    End If

                Next
                pSQL = pSQL.Remove(pSQL.Length - 5, 5)

            End If

            'If Orderby = "" Then
            'Else
            '    pSQL += " Order by " & Orderby
            'End If

            If Cn Is Nothing Or IsNothing(Cn) Then
                Cn = New SqlClient.SqlConnection()
                LocalCN = True
                If IsNothing(Ptransac) Then
                    AbrirConexion(Cn, False, , Conexion)
                Else
                    AbrirConexion(Cn, True, Ptransac, Conexion)
                End If
            End If

            Dim Cmd As New SqlCommand(pSQL, Cn)
            Cmd.CommandTimeout = TimeOut
            Cmd.Parameters.Clear()

            If Not (LlavePrimaria Is Nothing) Then
                For I As Integer = 0 To UBound(LlavePrimaria)
                    Cmd.Parameters.AddWithValue("@ValorLlave" & I, LlavePrimaria(I).Valor)
                Next
            End If

            If IsNothing(Ptransac) Then
            Else
                Cmd.Transaction = Ptransac
            End If

            Dim Da As New SqlDataAdapter(Cmd)
            Da.Fill(Resultado, Tabla)

            If LocalCN Then
                If IsNothing(Ptransac) Then
                    CerrarConexion(Cn)
                Else
                    CerrarConexion(Cn, Ptransac)
                End If
            End If

            Return Resultado
        Catch ex As Exception
            If Not Ptransac Is Nothing Then
                Ptransac.Rollback()
            End If
            Throw New Exception("FuncionesDB.Consultar  " & ex.Message)
        End Try

    End Function

    Public Function Consultar(ByVal Tabla As String, ByVal Campo As Campos(), _
            ByVal LlavePrimaria As Campos(), Optional ByRef Cn As SqlConnection = Nothing, _
            Optional ByRef Ptransac As SqlTransaction = Nothing, Optional ByVal Orderby As String = "", Optional ByVal pJoin As String = "", Optional ByVal pOperador As String = " And ", Optional ByVal Conexion As String = "Conexion", Optional TimeOut As Integer = 45) As System.Data.DataSet
        Try
            Dim LocalCN As Boolean = False

            If Cn Is Nothing Or IsNothing(Cn) Then
                Cn = New SqlClient.SqlConnection()
                LocalCN = True
                If IsNothing(Ptransac) Then
                    AbrirConexion(Cn, False, , Conexion)
                Else
                    AbrirConexion(Cn, True, Ptransac, Conexion)
                End If
            End If

            Dim Resultado As New System.Data.DataSet
            Dim SQL As String = "Select "

            For I As Integer = 0 To UBound(Campo)
                SQL &= Campo(I).Nombre & ","
            Next

            If LlavePrimaria Is Nothing Then
                SQL = SQL.Remove(SQL.Length - 1, 1)
                If pJoin = "" Then
                    ' no viene JOIN, solo se usa 1 tabla
                    SQL &= " From " & Tabla & " "
                Else
                    SQL &= " From " & Tabla & " " & pJoin
                End If
            Else ' viene llave primara o de seleccion
                SQL = SQL.Remove(SQL.Length - 1, 1)
                If pJoin = "" Then
                    ' no viene JOIN, solo se usa 1 tabla
                    SQL &= " From " & Tabla & " Where "
                Else
                    SQL &= " From " & Tabla & " " & pJoin & " Where "
                End If
            End If

            For I As Integer = 0 To UBound(LlavePrimaria)
                ' manejar uso de like
                If InStr(LlavePrimaria(I).Valor, "%") > 0 Then
                    ' uso de like
                    SQL &= LlavePrimaria(I).Nombre & " Like @ValorLlave" & I & pOperador ' " And "
                Else
                    'uso de igual =
                    If LlavePrimaria(I).Valor.ToString.ToUpper.Trim = "IS NOT NULL" Then ' Nuevo agregado el 30 nov 2010
                        SQL &= LlavePrimaria(I).Nombre & " " & LlavePrimaria(I).Valor & pOperador ' " And "
                    Else
                        SQL &= LlavePrimaria(I).Nombre & "=@ValorLlave" & I & pOperador ' " And "
                    End If

                End If

            Next
            SQL = SQL.Remove(SQL.Length - 5, 5)

            If Orderby = "" Then
            Else
                SQL += " Order by " & Orderby
            End If

            Dim Cmd As New SqlCommand(SQL, Cn)
            Cmd.CommandTimeout = TimeOut
            Cmd.Parameters.Clear()

            For I As Integer = 0 To UBound(LlavePrimaria)
                Cmd.Parameters.AddWithValue("@ValorLlave" & I, LlavePrimaria(I).Valor)
            Next

            If IsNothing(Ptransac) Then
            Else
                Cmd.Transaction = Ptransac
            End If

            Dim Da As New SqlDataAdapter(Cmd)
            Da.Fill(Resultado, Tabla)

            '*** si NO se manda CN como parametro, se crea localmente, y se cierra.
            If LocalCN Then
                If IsNothing(Ptransac) Then
                    CerrarConexion(Cn)
                Else
                    CerrarConexion(Cn, Ptransac)
                End If
            End If

            Return Resultado
        Catch ex As Exception
            If Not Ptransac Is Nothing Then
                Ptransac.Rollback()
            End If
            Throw New Exception("FuncionesDB.Consultar  " & ex.Message)
            'MsgBox(ex.Message, MsgBoxStyle.Critical)
            Return Nothing
        End Try

    End Function


    'Public Function Consultar(ByVal Tabla As String, ByVal Campo As Campos(), _
    '        ByVal LlavePrimaria As Campos(), ByRef Cn As MySqlConnection, _
    '        Optional ByRef Ptransac As MySqlTransaction = Nothing, Optional ByVal Orderby As String = "", Optional ByVal pJoin As String = "", Optional ByVal pOperador As String = " And ", Optional ByVal Conexion As String = "Cnpiad") As System.Data.DataSet
    '    Try
    '        Dim Resultado As New System.Data.DataSet
    '        Dim SQL As String = "Select "

    '        For I As Integer = 0 To UBound(Campo)
    '            SQL &= Campo(I).Nombre & ","
    '        Next

    '        If LlavePrimaria Is Nothing Then
    '            SQL = SQL.Remove(SQL.Length - 1, 1)
    '            If pJoin = "" Then
    '                ' no viene JOIN, solo se usa 1 tabla
    '                SQL &= " From " & Tabla & " "
    '            Else
    '                SQL &= " From " & Tabla & " " & pJoin
    '            End If
    '        Else ' viene llave primara o de seleccion
    '            SQL = SQL.Remove(SQL.Length - 1, 1)
    '            If pJoin = "" Then
    '                ' no viene JOIN, solo se usa 1 tabla
    '                SQL &= " From " & Tabla & " Where "
    '            Else
    '                SQL &= " From " & Tabla & " " & pJoin & " Where "
    '            End If
    '        End If

    '        For I As Integer = 0 To UBound(LlavePrimaria)
    '            ' manejar uso de like
    '            If InStr(LlavePrimaria(I).Valor, "%") > 0 Then
    '                ' uso de like
    '                SQL &= LlavePrimaria(I).Nombre & " Like @ValorLlave" & I & pOperador ' " And "
    '            Else
    '                'uso de igual =
    '                If LlavePrimaria(I).Valor.ToString.ToUpper.Trim = "IS NOT NULL" Then ' Nuevo agregado el 30 nov 2010
    '                    SQL &= LlavePrimaria(I).Nombre & " " & LlavePrimaria(I).Valor & pOperador ' " And "
    '                Else
    '                    SQL &= LlavePrimaria(I).Nombre & "=@ValorLlave" & I & pOperador ' " And "
    '                End If

    '            End If

    '        Next
    '        SQL = SQL.Remove(SQL.Length - 5, 5)

    '        If Orderby = "" Then
    '        Else
    '            SQL += " Order by " & Orderby
    '        End If

    '        Dim Cmd As New MySqlCommand(SQL, Cn)
    '        Cmd.Parameters.Clear()

    '        For I As Integer = 0 To UBound(LlavePrimaria)
    '            Cmd.Parameters.AddWithValue("@ValorLlave" & I, LlavePrimaria(I).Valor)
    '        Next

    '        If IsNothing(Ptransac) Then
    '        Else
    '            Cmd.Transaction = Ptransac
    '        End If

    '        Dim Da As New MySqlDataAdapter(Cmd)
    '        Da.Fill(Resultado, Tabla)
    '        Return Resultado
    '    Catch ex As Exception
    '        If Not Ptransac Is Nothing Then
    '            Ptransac.Rollback()
    '        End If
    '        Throw New Exception("FuncionesDB.Consultar  " & ex.Message)
    '        'MsgBox(ex.Message, MsgBoxStyle.Critical)
    '        Return Nothing
    '    End Try

    'End Function


    Public Function GuardarActualizar(ByVal Tabla As String, ByVal Campo As Campos(), Optional ByRef LlavePrimaria As Campos() = Nothing, Optional ByRef Cn As SqlConnection = Nothing, Optional ByRef PTransac As SqlTransaction = Nothing, Optional ByVal Conexion As String = "Conexion") As Boolean
        Dim LocalCN As Boolean = False
        Dim Resultado As Boolean = False
        Try
            ' ************ armar sql
            ' Consultar si registro existe.
            If Cn Is Nothing Or IsNothing(Cn) Then
                Cn = New SqlClient.SqlConnection()
                LocalCN = True
                If IsNothing(PTransac) Then
                    AbrirConexion(Cn, False, , Conexion)
                Else
                    AbrirConexion(Cn, True, PTransac, Conexion)
                End If
            End If

            If Not LlavePrimaria Is Nothing Then
                Dim Dset As DataSet = Consultar(Tabla, Campo, LlavePrimaria, Cn, PTransac, , , , Conexion)
                If Dset.Tables(Tabla).Rows.Count > 0 Then
                    'Dato existe, se actualiza
                    'update

                    If Update(Tabla, Campo, LlavePrimaria, Cn, PTransac, Conexion) Then
                        Resultado = True
                    End If
                Else

                    If Insert(Tabla, Campo, Cn, PTransac, Conexion) Then
                        Resultado = True
                    End If
                End If
            Else ' no viene llave primaria, se asume dato nuevo.
                ' Registro Nuevo, se ingresan los datos.
                'insert

                If Insert(Tabla, Campo, Cn, PTransac, Conexion) Then
                    Resultado = True
                End If
            End If

            If LocalCN Then
                If IsNothing(PTransac) Then
                    CerrarConexion(Cn)
                Else
                    CerrarConexion(Cn, PTransac)
                End If
            End If

            Return Resultado
        Catch ex As Exception
            Dim Mensaje As String = "FuncionesDB.GuardarActualizar...El Actualizado de datos en la Tabla: " & Tabla & ". Error al Guardar.!!!" & _
                ". Error devuelto: " & ex.Message
            Throw New Exception(Mensaje)
        End Try
    End Function

    Public Function AplicaSQL(ByVal Sql As String, Optional ByRef Cn As SqlConnection = Nothing, Optional ByRef PTransac As SqlTransaction = Nothing, Optional ByVal Llave As Campos() = Nothing, Optional ByVal pOperadorLogico As String = " and ", Optional ByVal Conexion As String = "Conexion", Optional TimeOut As Integer = 45) As Boolean
        'Función para el Borrado de Datos en las Tablas
        Dim LocalCN As Boolean = False
        Try
            If Cn Is Nothing Or IsNothing(Cn) Then
                Cn = New SqlClient.SqlConnection()
                LocalCN = True
                If IsNothing(PTransac) Then
                    AbrirConexion(Cn, False, , Conexion)
                Else
                    AbrirConexion(Cn, True, PTransac, Conexion)
                End If
            End If

            pOperadorLogico = pOperadorLogico.PadLeft(5, " ")
            Dim Cmd As New SqlCommand(Sql, Cn)
            Cmd.CommandTimeout = TimeOut

            'AbrirConexion(Cn, True, PTransac) ' debe ir fuera del delete
            If IsNothing(PTransac) Then
            Else
                Cmd.Transaction = PTransac
            End If

            '*** agregado 28 Julio 2011
            ' *** compatibilidad con parametros llave, clausula where
            '*********************************************************
            '* arma el where.
            If IsNothing(Llave) Then
                ' no viene llave.
            Else
                ' viene llave, se agrega where al sql
                For I As Integer = 0 To UBound(Llave)
                    Cmd.Parameters.AddWithValue(Llave(I).Nombre, Llave(I).Valor)
                    'Sql &= Llave(I).Nombre & "=" & "@ValorLlave" & I & pOperadorLogico
                Next

                ' Sql = Mid(Sql, 1, Len(Sql) - 5)

                'For I As Integer = 0 To UBound(Llave)
                '    Cmd.Parameters.AddWithValue("@ValorLlave" & I, Llave(I).Valor)
                'Next
            End If

            '*********************************************************
            Cmd.ExecuteNonQuery()

            'CerrarConexion(Cn, PTransac)  ' debe ir fuera del delete.
            If LocalCN Then
                If IsNothing(PTransac) Then
                    CerrarConexion(Cn)
                Else
                    CerrarConexion(Cn, PTransac)
                End If
            End If

            Return True

        Catch ex As Exception
            If Not PTransac Is Nothing Then
                PTransac.Rollback()
            End If

            Dim Mensaje As String = "FuncionesDB.AplicaSQL...ERROR En El SQL: " & Sql & ".!!!" & _
                ". Error devuelto: " & ex.Message
            Throw New Exception(Mensaje)
        End Try
    End Function

    Sub ArmaValor(ByRef Array() As FuncionesDB.Campos, ByVal NombreCampo As String, Optional ByVal Valor As String = "", Optional ByVal Formato As String = "")
        Try
            If UBound(Array) = -1 Then
                ' primer registro, el array debe venir con valor -1
                ' Ejemplo:  Dim Array(-1) as tipodato
                ReDim Preserve Array(0)
            Else
                ReDim Preserve Array(UBound(Array) + 1)
            End If

            Array(UBound(Array)).Nombre = NombreCampo
            Array(UBound(Array)).Valor = Valor
            Array(UBound(Array)).Formato = Formato

        Catch ex As Exception
            Throw New Exception("FuncionesDB.ArmaValor: " & ex.Message)
        End Try

    End Sub

    Sub ArmaValor(ByRef Array() As FuncionesDB.Campos, ByVal NombreCampo As String, ByVal Valor As Object, Optional ByVal Formato As String = "")
        Try
            If UBound(Array) = -1 Then
                ' primer registro, el array debe venir con valor -1
                ' Ejemplo:  Dim Array(-1) as tipodato
                ReDim Preserve Array(0)
            Else
                ReDim Preserve Array(UBound(Array) + 1)
            End If

            Array(UBound(Array)).Nombre = NombreCampo
            If Valor Is Nothing Then
            Else
                Array(UBound(Array)).Valor = Valor
            End If

            Array(UBound(Array)).Formato = Formato

        Catch ex As Exception
            Throw New Exception("FuncionesDB.ArmaValor: " & ex.Message)
        End Try

    End Sub

    Function InicializarArray() As FuncionesDB.Campos()
        Dim Respuesta() As Campos
        ReDim Respuesta(-1)
        Return Respuesta
    End Function

    Function ObtenerUsuarioBaseDatos(ByVal pBuscar As String, Optional ByVal pBuscarAuxiliar As String = ".", Optional ByVal Conexion As String = "Conexion") As String
        Dim Usr As String = ""
        Dim CadenaConexion As String = GetAppConfig(Conexion)
        Dim Resultado As String = ""

        pBuscar = pBuscar & "="
        pBuscarAuxiliar = pBuscarAuxiliar & "="

        Dim Pos As Integer = InStr(CadenaConexion.ToLower, pBuscar.ToLower)
        Dim PosFin As Integer = 0

        If Pos > 0 Then
            PosFin = InStr(Pos, CadenaConexion.ToLower, ";")
            ' Usuario/cadena encontrada.
            If PosFin = 0 Then
                ' se asume que no viene separador en el ultimo parametro
                PosFin = CadenaConexion.Length + 1
            End If

            Resultado = Mid(CadenaConexion, Pos + Len(pBuscar), PosFin - Pos - Len(pBuscar))
        Else
            ' No se encuentra datode busqueda "parametro pbuscar, se utiliza pbuacarAuxiliar"
            Pos = InStr(CadenaConexion.ToLower, pBuscarAuxiliar.ToLower)

            If Pos > 0 Then
                PosFin = InStr(Pos, CadenaConexion.ToLower, ";")
                ' Usuario encontrado.
                If PosFin = 0 Then
                    ' se asume que no viene separador en el ultimo parametro
                    PosFin = CadenaConexion.Length + 1
                End If
                Resultado = Mid(CadenaConexion, Pos + Len(pBuscarAuxiliar), PosFin - Pos - Len(pBuscarAuxiliar))

            Else
                ' No se encuentra usuario, buscar "usr", buscar Trusted_Connection.
                Pos = InStr(CadenaConexion.ToLower, "trusted")

                If Pos > 0 Then
                    Resultado = "Conexion de Confianza"
                Else
                    ' usuario es el ultimo parametro
                    Resultado = "No hay parámetro de busqueda definido." ' esto no deberia suceder nunca.
                End If
            End If
        End If

        Resultado = Resultado.Replace(";", "")
        Return Resultado
    End Function

    Function ObtenerUsuarioBaseDatos(ByVal pBuscar As String, Optional ByVal pBuscarAuxiliar As String = ".") As String
        Dim Usr As String = ""
        Dim CadenaConexion As String = GetAppConfig("Conexion")
        Dim Resultado As String = ""

        pBuscar = pBuscar & "="
        pBuscarAuxiliar = pBuscarAuxiliar & "="

        Dim Pos As Integer = InStr(CadenaConexion.ToLower, pBuscar.ToLower)
        Dim PosFin As Integer = 0

        If Pos > 0 Then
            PosFin = InStr(Pos, CadenaConexion.ToLower, ";")
            ' Usuario/cadena encontrada.
            If PosFin = 0 Then
                ' se asume que no viene separador en el ultimo parametro
                PosFin = CadenaConexion.Length + 1
            End If

            Resultado = Mid(CadenaConexion, Pos + Len(pBuscar), PosFin - Pos - Len(pBuscar))
        Else
            ' No se encuentra datode busqueda "parametro pbuscar, se utiliza pbuacarAuxiliar"
            Pos = InStr(CadenaConexion.ToLower, pBuscarAuxiliar.ToLower)

            If Pos > 0 Then
                PosFin = InStr(Pos, CadenaConexion.ToLower, ";")
                ' Usuario encontrado.
                If PosFin = 0 Then
                    ' se asume que no viene separador en el ultimo parametro
                    PosFin = CadenaConexion.Length + 1
                End If
                Resultado = Mid(CadenaConexion, Pos + Len(pBuscarAuxiliar), PosFin - Pos - Len(pBuscarAuxiliar))

            Else
                ' No se encuentra usuario, buscar "usr", buscar Trusted_Connection.
                Pos = InStr(CadenaConexion.ToLower, "trusted")

                If Pos > 0 Then
                    Resultado = "Conexion de Confianza"
                Else
                    ' usuario es el ultimo parametro
                    Resultado = "No hay parámetro de busqueda definido." ' esto no deberia suceder nunca.
                End If
            End If
        End If

        Resultado = Resultado.Replace(";", "")
        Return Resultado
    End Function

    Function ObtenerParametroConexion(ByVal pBuscar As String, Optional ByVal pBuscarAuxiliar As String = ".", Optional ByVal CadenaConexion As String = "") As String
        Dim Usr As String = ""
        'Dim CadenaConexion As String = GetAppConfig("Conexion")
        Dim Resultado As String = ""

        pBuscar = pBuscar & "="
        pBuscarAuxiliar = pBuscarAuxiliar & "="

        Dim Pos As Integer = InStr(CadenaConexion.ToLower, pBuscar.ToLower)
        Dim PosFin As Integer = 0

        If Pos > 0 Then
            PosFin = InStr(Pos, CadenaConexion.ToLower, ";")
            ' Usuario/cadena encontrada.
            If PosFin = 0 Then
                ' se asume que no viene separador en el ultimo parametro
                PosFin = CadenaConexion.Length + 1
            End If

            Resultado = Mid(CadenaConexion, Pos + Len(pBuscar), PosFin - Pos - Len(pBuscar))
        Else
            ' No se encuentra datode busqueda "parametro pbuscar, se utiliza pbuacarAuxiliar"
            Pos = InStr(CadenaConexion.ToLower, pBuscarAuxiliar.ToLower)

            If Pos > 0 Then
                PosFin = InStr(Pos, CadenaConexion.ToLower, ";")
                ' Usuario encontrado.
                If PosFin = 0 Then
                    ' se asume que no viene separador en el ultimo parametro
                    PosFin = CadenaConexion.Length + 1
                End If
                Resultado = Mid(CadenaConexion, Pos + Len(pBuscarAuxiliar), PosFin - Pos - Len(pBuscarAuxiliar))

            Else
                ' No se encuentra usuario, buscar "usr", buscar Trusted_Connection.
                Pos = InStr(CadenaConexion.ToLower, "trusted")

                If Pos > 0 Then
                    Resultado = "Conexion de Confianza"
                Else
                    ' usuario es el ultimo parametro
                    Resultado = "No hay parámetro de busqueda definido." ' esto no deberia suceder nunca.
                End If
            End If
        End If

        Resultado = Resultado.Replace(";", "")
        Return Resultado
    End Function

    Sub CargaCombo(ByRef Combo As ComboBox, ByVal pTabla As String, ByVal pCodigo As String, ByVal pNombre As String, Optional Cn As SqlClient.SqlConnection = Nothing, Optional Transac As SqlClient.SqlTransaction = Nothing)
        Dim Cls As New FuncionesDB
        Dim Ds As New DataSet
        Try
            Dim Valores(), Llave() As FuncionesDB.Campos
            Dim AbreConexion As Boolean = False

            If Cn Is Nothing Then
                Cls.AbrirConexion(Cn, False)
                AbreConexion = True
            End If

            Valores = Cls.InicializarArray
            Llave = Cls.InicializarArray

            Cls.ArmaValor(Valores, pCodigo)
            Cls.ArmaValor(Valores, pNombre)
            Cls.ArmaValor(Llave, "1", 1) ' selecciona todos los registros. adicionar llave.

            Ds = Cls.Consultar(pTabla, Valores, Llave, Cn, Transac)

            If Ds.Tables(0).Rows.Count > 0 Then
                Combo.Items.Clear()

                For I As Integer = 0 To Ds.Tables(0).Rows.Count - 1
                    Combo.Items.Add(New LBItem(Ds.Tables(0).Rows(I)(pCodigo), Ds.Tables(0).Rows(I)(pNombre).ToUpper()))
                Next
            End If

            If AbreConexion Then
                Cls.CerrarConexion(Cn)
            End If

            If Combo.Items.Count <> 0 Then
                Combo.SelectedIndex = -1
            End If

        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    ' **********************************************************************************************************************************
    ' Este cargar combo colo funciona para WEB, en windows hay que utilizar LBITEM en lugar de ListItem
    'Sub CargaCombo(ByRef Combo As DropDownList, ByVal pTabla As String, ByVal pCodigo As String, ByVal pNombre As String, Optional ByVal pNingunoTodosDescription As String = "-- Ninguno --", Optional ByVal Conexion As String = "Conexion")
    '    ' *** Carga y devuelve un Combo, basandose en una tablaX, asumiento los campos de codigo y nombre respectivos.
    '    Try
    '        Dim Cls As New FuncionesDB

    '        Dim Ds As New DataSet

    '        Dim Valores(), Llave() As FuncionesDB.Campos
    '        Valores = Cls.InicializarArray
    '        Llave = Cls.InicializarArray

    '        Cls.ArmaValor(Valores, pCodigo)
    '        Cls.ArmaValor(Valores, pNombre)
    '        Cls.ArmaValor(Llave, "1", 1)

    '        Ds = Cls.Consultar(pTabla, Valores, Llave, Conexion:=Conexion)

    '        Dim MiItem As New ListItem
    '        MiItem.Text = pNingunoTodosDescription ' "-- Ninguno --"
    '        MiItem.Value = "-1"
    '        Combo.Items.Add(MiItem)

    '        For I As Integer = 0 To Ds.Tables(0).Rows.Count - 1
    '            MiItem = New ListItem
    '            MiItem.Value = Ds.Tables(0).Rows(I)(pCodigo)
    '            MiItem.Text = Ds.Tables(0).Rows(I)(pCodigo) & " - " & Ds.Tables(0).Rows(I)(pNombre)
    '            Combo.Items.Add(MiItem)
    '        Next

    '        If Combo.Items.Count <> 0 Then
    '            Combo.SelectedIndex = -1
    '        End If
    '    Catch ex As Exception
    '        Throw ex
    '    End Try
    'End Sub

    ''' <summary>
    ''' Inicializa una Transaccion ligada a la conexion que se envia. Si la conexion no ha sido habierta, la misma se abre y se inicia la conexion.
    ''' </summary>
    ''' <param name="pCn">Conexion a trabajar.</param>
    ''' <param name="pTran">Transaccion a ligar.</param>
    ''' <remarks></remarks>
    Public Sub IniciaSQL(ByRef pCn As SqlConnection, ByRef pTran As SqlTransaction)
        Try
            'Dim Cn As New SqlConnection(CadenaConexion)

            If pCn.State = ConnectionState.Closed Then
                pCn.Open()
            End If
            ' se manejan transacciones
            pTran = pCn.BeginTransaction
        Catch ex As Exception
            Throw New Exception("FuncionesDB.AbrirConexion..." & ex.Message)
        End Try
    End Sub

    Public Sub FinalSQL(ByRef pTran As SqlTransaction)
        Try
            ' se manejan transacciones
            pTran.Commit()
        Catch ex As Exception
            Throw New Exception("FuncionesDB.AbrirConexion..." & ex.Message)
        End Try
    End Sub

    Public Sub RollSQL(ByRef pTran As SqlTransaction)
        Try
            If Not pTran Is Nothing Then
                pTran.Rollback()
            End If

        Catch ex As Exception

        End Try

    End Sub
End Class




