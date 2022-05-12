Public Module CodigoGeneral

    Public Sub LimpiarSession()
        Dim cls As New FuncionesDB
        gSession.Valor1 = ""
        gSession.Valor2 = ""
        gSession.Valor3 = ""
        gSession.Valor5 = ""
        gSession.Titulo = ""
        gSession.Criterio = ""
        gSession.RangoDeFecha = ""
        gSession.TipoReporte = ""
        gSession.Imprimir = False
        gSession.Reporte = ""
        gSession.Valores = cls.InicializarArray
        gSession.Llave = cls.InicializarArray
        ReDim gSession.Resultado(-1)
        gSession.ResultadoDset = Nothing
    End Sub

    Public Structure stSession
        Dim Valor1 As String
        Dim Valor2 As String
        Dim Valor3 As String
        Dim Valor5 As String
        Dim Titulo As String
        Dim Criterio As String
        Dim Valor6 As String
        Dim TipoReporte As String
        Dim Imprimir As Boolean
        Dim Reporte As String
        Dim RangoDeFecha As String
        'Dim Fecha1 As Date
        'Dim Fecha2 As Date
        Dim Valores() As FuncionesDB.Campos
        Dim Llave() As FuncionesDB.Campos
        Dim Resultado() As String     ' array dinamico, se pueden devolver 1 o n valores.
        Dim ResultadoDset As DataSet

    End Structure

    Public gSession As stSession

    Public Function KeyAscii(ByVal UserKeyArgument As KeyPressEventArgs) As Short
        KeyAscii = Asc(UserKeyArgument.KeyChar)
    End Function

    ' ************************************************
    ' Desarrollo CTP Guaycara, usar app. config
    ' ************************************************

    ''' <summary>
    ''' Recupera la configuración especificada en el archivo app.config
    ''' </summary>
    ''' <param name="NombreConfiguracion">Nombre del parámetro a buscar en el archivo app.config</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function GetAppConfig(Optional ByVal NombreConfiguracion As String = "Conexion") As String
        Dim strConn As String       
        Dim configurationAppSettings As New System.Configuration.AppSettingsReader
        strConn = CStr(configurationAppSettings.GetValue(NombreConfiguracion, GetType(String)))
        Return strConn
    End Function


    Function ObtenerValorParametroConexion(ByVal pBuscar As String, Optional ByVal pBuscarAux As String = ".") As String

        Dim CadenaConexion As String = GetAppConfig("Conexion")
        Dim Resultado As String = ""

        pBuscar = pBuscar & "="

        Dim Pos As Integer = InStr(CadenaConexion.ToLower, pBuscar.ToLower) + Len(pBuscar)
        Dim PosFin As Integer = InStr(Pos, CadenaConexion.ToLower, ";")

        If Pos > 0 Then
            ' Usuario/cadena encontrada.
            Resultado = Mid(CadenaConexion, Pos, PosFin - Pos)
        Else
            Pos = InStr(CadenaConexion.ToLower, pBuscarAux.ToLower) + Len(pBuscarAux)
            PosFin = InStr(Pos, CadenaConexion.ToLower, ";")
        End If

        Return Resultado
    End Function

    Sub SeleccionaComboItem(ByRef Combo As ComboBox, ByVal Valor As String)
        Try
            Dim Texto As String = ""
            For I As Integer = 0 To Combo.Items.Count - 1
                Texto = Combo.Items(I).ToString
                If InStr(Texto, Valor) > 0 Then
                    ' COINCIDENCIA DE TEXTO 
                    Combo.SelectedIndex = I
                    Exit For
                End If
            Next
        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Public Function SoloNumerosFN(ByVal Keyascii As Short) As Short
        If InStr("1234567890", Chr(Keyascii)) = 0 Then
            ' no acepta los caracteres
            SoloNumerosFN = 0
        Else
            ' aceptando solo numeros y lo que contenga la propiedad pPermitirCaracteres
            SoloNumerosFN = Keyascii ' permitido

        End If
        Select Case Keyascii ' permite teclas de borrado
            Case 8
                SoloNumerosFN = Keyascii
            Case 13
                SoloNumerosFN = Keyascii
        End Select
    End Function

    Function SCM(ByVal valor As String) As String
        Return "'" & valor & "'"
    End Function
    Public Function ArmaFechaReporte(ByRef Campo As String, ByRef FechaInicial As Date, ByRef FechaFinal As Date) As String
        'ArmaFechaReporte = Campo & ">=date(" & VB6.Format(FechaInicial, "yyyy,mm,dd") & ") and " & Campo & "<=date(" & VB6.Format(FechaFinal, "yyyy,mm,dd") & ")"
        ArmaFechaReporte = Campo & ">=date(" & FechaInicial.Year & "," & FechaInicial.Month & "," & FechaInicial.Day & ") and " & _
            Campo & "<=date(" & FechaFinal.Year & "," & FechaFinal.Month & "," & FechaFinal.Day & ")"
    End Function
    Function sen(ByVal Valor As String) As String
        Dim largo As Integer = 0
        Dim a As Integer = 0
        'Super Evaluador Numérico
        Dim pto As Short
        If Len(Valor) Then
            'Le Quita el Signo de Colones
            If InStr(Valor, "¢") Then
                a = InStr(Valor, "¢")
                If a = 1 Then
                    Valor = Mid(Valor, 2)
                Else
                    Valor = Mid(Valor, 1, a - 1) & Mid(Valor, a + 1)
                End If
            End If

            largo = Len(Valor)
            Do While 1 < 2
                pto = InStr(Valor, ",")
                'Elimina las Comas
                If pto > 0 Then
                    Valor = Mid(Valor, 1, pto - 1) & Mid(Valor, pto + 1, largo)
                Else
                    Exit Do
                End If
            Loop
            sen = Valor
        Else
            sen = "0"
        End If
    End Function
    Function ArmaFechaQueryHora(ByVal Campo As String, ByVal FechaInicial As Date, ByVal FechaFinal As Date)
        ArmaFechaQueryHora = Campo & ">=CONVERT(DATETIME," & SCM(Format(FechaInicial, "yyyy/MM/dd HH:mm:ss")) & " , 102) And " & Campo & "<=CONVERT(DATETIME," & SCM(Format(FechaFinal, "yyyy/MM/dd") & " 23:59:59") & ", 102)"
    End Function
    Function ArmaFechaQuery(ByVal Campo As String, ByVal FechaInicial As Date, ByVal FechaFinal As Date)
        ArmaFechaQuery = Campo & ">=CONVERT(DATETIME," & SCM(Format(FechaInicial, "yyyy/MM/dd")) & " , 102) And " & Campo & "<=CONVERT(DATETIME," & SCM(Format(FechaFinal, "yyyy/MM/dd")) & ", 102)"
    End Function
    Function ConvertDate(ByVal Campo As String, ByVal signo As String, ByVal Fecha As Date)
        ConvertDate = Campo & signo & "CONVERT(DATETIME," & SCM(Format(Fecha, "yyyy/MM/dd")) & " , 102)"
    End Function

    Function ObtenerParametroConexion(ByVal pBuscar As String, Optional ByVal pBuscarAuxiliar As String = ".") As String
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
            Resultado = Mid(CadenaConexion, Pos + Len(pBuscar), PosFin - Len(pBuscar) - 1)
        Else
            ' No se encuentra datode busqueda "parametro pbuscar, se utiliza pbuacarAuxiliar"
            Pos = InStr(CadenaConexion.ToLower, pBuscarAuxiliar.ToLower)

            If Pos > 0 Then
                PosFin = InStr(Pos, CadenaConexion.ToLower, ";")
                ' Usuario encontrado.
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
End Module