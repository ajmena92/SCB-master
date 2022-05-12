Imports System
Imports System.IO
Imports System.Xml
Imports System.Text
Imports System.Security.Cryptography

Namespace Seguridad
    Public Class Encriptacion64
        Private key() As Byte = {}
        Private IV() As Byte = {&H12, &H34, &H56, &H78, &H90, &HAB, &HCD, &HEF}

        Function EncriptaPossic(ByVal pClave As String) As String
            Dim Encriptar As String = ""
            For i As Integer = 1 To Len(pClave)
                Encriptar = Encriptar & (Asc(Mid(pClave, i, 1)) & "173")
            Next i
            Return Encriptar
        End Function

        Private Function Encrypt(ByVal stringToEncrypt As String, ByVal SEncryptionKey As String) As String
            Try
                key = System.Text.Encoding.UTF8.GetBytes(Left(SEncryptionKey, 8))
                Dim des As New DESCryptoServiceProvider()
                Dim inputByteArray() As Byte = Encoding.UTF8.GetBytes(stringToEncrypt)
                Dim ms As New MemoryStream()
                Dim cs As New CryptoStream(ms, des.CreateEncryptor(key, IV), CryptoStreamMode.Write)
                cs.Write(inputByteArray, 0, inputByteArray.Length)
                cs.FlushFinalBlock()
                Return Convert.ToBase64String(ms.ToArray())
            Catch e As Exception
                Throw New Exception(e.Message)
            End Try
        End Function

        Public Function encryptQueryString(ByVal strQueryString As String, ByVal pkey As String) As String
            Dim oES As New Encriptacion64
            Return oES.Encrypt(strQueryString, pkey)
            oES = Nothing
        End Function


        '----------------------------------------------------
        ' ****************************************************
        '*** Encriptacion y seguridad de anti.copia. x Cesardgo.
        ' *** FACTOR DE ENCRIPTACION DEBE ESTAR EN TRE 50-70, NUMEROS MAYORES DARAN ERROR.
        Public Function M_Encriptar(ByVal Texto As String, ByVal Factor As Byte) As String

            Dim Clave1 As Byte, StrClave1 As String
            Dim Clave2 As Byte, StrClave2 As String
            Dim Clave3 As Integer, i As Integer
            Dim Encriptar As String
            Dim MiValor As Integer

            Randomize()

            'MiValor = Int((50 * Rnd()) + 10)   ' Genera un valor aleatorio entre 1 y 6.
            MiValor = Int((Factor * Rnd()) + 10)   ' Genera un valor aleatorio entre 1 y Factor.

            Clave1 = MiValor
            StrClave1 = Mid(Str(MiValor), 2, 2)

            'MiValor = Int((50 * Rnd()) + 10)   ' Genera un valor aleatorio entre 1 y 6.
            MiValor = Int((Factor * Rnd()) + 10)   ' Genera un valor aleatorio entre 1 y Factor.

            Clave2 = MiValor
            StrClave2 = Mid(Str(MiValor), 2, 2)

            If Clave1 > Clave2 Then
                Clave3 = Clave1 - Clave2
            ElseIf Clave2 > Clave1 Then
                Clave3 = Clave2 - Clave1
            Else
                Clave3 = 12
            End If

            Encriptar = Chr(Asc(Mid(StrClave1, 1, 1)) + 5) & Chr(Asc(Mid(StrClave1, 2, 1)) + 5)
            For i = 1 To Len(Texto)
                Encriptar = Encriptar & Chr(Asc(Mid(Texto, i, 1)) + Clave3)
            Next i

            Encriptar = Encriptar & Chr(Asc(Mid(StrClave2, 1, 1)) + 5) & Chr(Asc(Mid(StrClave2, 2, 1)) + 5)

            M_Encriptar = Encriptar

        End Function

        Public Function M_DesEncriptar(ByVal Texto As String) As String
            Dim Clave1 As Byte, StrClave1 As String
            Dim Clave2 As Byte, StrClave2 As String
            Dim Clave3 As Integer, i As Integer
            Dim Encriptar As String

            StrClave1 = Mid(Texto, 1, 2)
            StrClave1 = Chr(Asc(Mid(StrClave1, 1, 1)) - 5) & Chr(Asc(Mid(StrClave1, 2, 1)) - 5)
            Clave1 = Val(StrClave1)

            StrClave2 = Mid(Texto, Len(Texto) - 1, 2)
            StrClave2 = Chr(Asc(Mid(StrClave2, 1, 1)) - 5) & Chr(Asc(Mid(StrClave2, 2, 1)) - 5)
            Clave2 = Val(StrClave2)

            If Clave1 > Clave2 Then
                Clave3 = Clave1 - Clave2
            ElseIf Clave2 > Clave1 Then
                Clave3 = Clave2 - Clave1
            Else
                Clave3 = 1
            End If

            Encriptar = ""
            For i = 3 To Len(Texto) - 2
                Encriptar = Encriptar & Chr(Asc(Mid(Texto, i, 1)) - Clave3)
            Next i

            M_DesEncriptar = Encriptar
        End Function

        Structure SysFile
            Dim RazonSocial As String
            Dim DenominacionSocial As String
            Dim MachineName As String
            Dim TipoServicio As String ' 01=cxc, 02=cxp, 03= sincroniza. 00=NO permiso
            Dim TipoActivacion As String ' Mes/Indefinido
            Dim FechaVence As String
            Dim FechaActivacion As String
            Dim CantUsos As Integer
            Dim Estado As String ' Activo/Bloqueado
        End Structure

        Dim lArchivo1 As String = Environment.GetEnvironmentVariable("windir") & "\Microsoft.NET\Framework\v2.0.50727\mscotyp7.dll"
        'Dim lArchivo2 As String = Environment.GetEnvironmentVariable("LOCALAPPDATA") & "\mk1020.sts"
        Dim lArchivo2 As String = Environment.GetEnvironmentVariable("windir") & "\appletCis.etc"


        Function RecuperaFile01() As SysFile
            Try
                Dim Info As New SysFile

                Dim oSW As New StreamReader(lArchivo1)
                Dim Secur As New Seguridad.Encriptacion64
                'Dim Linea As String = "Línea de texto " & vbNewLine & "Otra linea de texto"
                Info.MachineName = Secur.M_DesEncriptar(oSW.ReadLine)
                Info.RazonSocial = Secur.M_DesEncriptar(oSW.ReadLine)
                Info.DenominacionSocial = Secur.M_DesEncriptar(oSW.ReadLine)
                Info.TipoServicio = Secur.M_DesEncriptar(oSW.ReadLine)
                Info.TipoActivacion = Secur.M_DesEncriptar(oSW.ReadLine)
                Info.FechaVence = Secur.M_DesEncriptar(oSW.ReadLine)
                Info.FechaActivacion = Secur.M_DesEncriptar(oSW.ReadLine)
                Info.CantUsos = Secur.M_DesEncriptar(oSW.ReadLine)
                Info.Estado = Secur.M_DesEncriptar(oSW.ReadLine)


                oSW.Close()
                oSW.Dispose()

                Return Info
                ' error
            Catch ex As Exception
                MsgBox("Error de Acceso.", MsgBoxStyle.Critical)
                Dim InfoError As New SysFile
                InfoError.RazonSocial = "err1"

                Return InfoError
            End Try
        End Function

        Function RecuperaFile02() As SysFile
            Try
                Dim Info As New SysFile
                '*************************************************************
                '*** 2do archivo.
                '*************************************************************
                Dim oSW2 As New StreamReader(lArchivo2)
                Dim Secur2 As New Seguridad.Encriptacion64
                'Dim Linea As String = "Línea de texto " & vbNewLine & "Otra linea de texto"

                Info.Estado = Secur2.M_DesEncriptar(oSW2.ReadLine)
                Info.CantUsos = Secur2.M_DesEncriptar(oSW2.ReadLine)
                Info.FechaActivacion = Secur2.M_DesEncriptar(oSW2.ReadLine)
                Info.RazonSocial = Secur2.M_DesEncriptar(oSW2.ReadLine)
                Info.DenominacionSocial = Secur2.M_DesEncriptar(oSW2.ReadLine)
                Info.TipoServicio = Secur2.M_DesEncriptar(oSW2.ReadLine)
                Info.FechaVence = Secur2.M_DesEncriptar(oSW2.ReadLine)
                Info.TipoActivacion = Secur2.M_DesEncriptar(oSW2.ReadLine)
                Info.MachineName = Secur2.M_DesEncriptar(oSW2.ReadLine)

                oSW2.Close()
                oSW2.Dispose()

                Return Info
                ' error
            Catch ex As Exception
                MsgBox("Error de Acceso.", MsgBoxStyle.Critical)
                Dim InfoError As New SysFile
                InfoError.RazonSocial = "err2."

                Return InfoError
            End Try
        End Function

        Sub CreaArchivo1(ByVal Info As SysFile)

            ' crear
            'Dim Archivo As System.IO.FileStream
            ' crea un archivo vacio prueba.txt
            ' Archivo = System.IO.File.Create(Environment.SystemDirectory.ToString() & "\cisFont.seg") ' crea archivo en system32

            'Dim oSW As New StreamWriter(Environment.SystemDirectory.ToString() & "\cisFont.seg")

            Dim Secur As New Seguridad.Encriptacion64
            Dim WinDir As String = Environment.SystemDirectory ' ES system32.

            'WinDir = Environment.GetEnvironmentVariable("LOCALAPPDATA")
            WinDir = Environment.GetEnvironmentVariable("windir")
            WinDir &= "\Microsoft.NET"

            If System.IO.File.Exists(lArchivo1) Then
                System.IO.File.Delete(lArchivo1)
            End If

            Dim oSW As New StreamWriter(lArchivo1)

            'Dim Linea As String = "Línea de texto " & vbNewLine & "Otra linea de texto"

            oSW.WriteLine(Secur.M_Encriptar(Info.MachineName, 30))
            oSW.WriteLine(Secur.M_Encriptar(Info.RazonSocial, 30))
            oSW.WriteLine(Secur.M_Encriptar(Info.DenominacionSocial, 30))
            oSW.WriteLine(Secur.M_Encriptar(Info.TipoServicio, 30))
            oSW.WriteLine(Secur.M_Encriptar(Info.TipoActivacion, 30))
            oSW.WriteLine(Secur.M_Encriptar(Info.FechaVence, 30))
            oSW.WriteLine(Secur.M_Encriptar(Info.FechaActivacion, 30))
            oSW.WriteLine(Secur.M_Encriptar(Info.CantUsos, 30))
            oSW.WriteLine(Secur.M_Encriptar(Info.Estado, 30))
            oSW.Flush()
            oSW.Close()

        End Sub

        Sub CreaArchivo2(ByVal Info As SysFile)
            ' crear
            'Dim Archivo As System.IO.FileStream
            ' crea un archivo vacio prueba.txt
            ' Archivo = System.IO.File.Create(Environment.SystemDirectory.ToString() & "\cisFont.seg") ' crea archivo en system32

            'Dim oSW As New StreamWriter(Environment.SystemDirectory.ToString() & "\cisFont.seg")
            Dim Secur As New Seguridad.Encriptacion64
            'Dim WinDir As String = lArchivo2

            If System.IO.File.Exists(lArchivo2) Then
                System.IO.File.Delete(lArchivo2)
            End If

            Dim oSW As New StreamWriter(lArchivo2)

            'Dim Linea As String = "Línea de texto " & vbNewLine & "Otra linea de texto"

            oSW.WriteLine(Secur.M_Encriptar(Info.Estado, 30))
            oSW.WriteLine(Secur.M_Encriptar(Info.CantUsos, 30))
            oSW.WriteLine(Secur.M_Encriptar(Info.FechaActivacion, 30))
            oSW.WriteLine(Secur.M_Encriptar(Info.RazonSocial, 30))
            oSW.WriteLine(Secur.M_Encriptar(Info.DenominacionSocial, 30))
            oSW.WriteLine(Secur.M_Encriptar(Info.TipoServicio, 30))
            oSW.WriteLine(Secur.M_Encriptar(Info.FechaVence, 30))
            oSW.WriteLine(Secur.M_Encriptar(Info.TipoActivacion, 30))
            oSW.WriteLine(Secur.M_Encriptar(Info.MachineName, 30))

            oSW.Flush()
            oSW.Close()

        End Sub

        Function CreaArchivos(ByVal Info As SysFile) As Boolean
            Try
                Dim lTrue As Boolean = True
                Dim Info1, Info2 As Seguridad.Encriptacion64.SysFile
                Dim Sec As New Seguridad.Encriptacion64
                While lTrue
                    CreaArchivo1(Info)
                    CreaArchivo2(Info)

                    Info1 = Sec.RecuperaFile01
                    Info2 = Sec.RecuperaFile02

                    If Info1.Equals(Info2) Then
                        lTrue = False
                    End If

                End While

                Return True
            Catch ex As Exception
                MsgBox("Error en Escritura de Disco Duro o Archivo en Uso. Ejecute la aplicación como Administrador o Vuelva a Ejectuar la Seguridad.", MsgBoxStyle.Critical)
                Return False
            End Try
        End Function

        Function PinGenera(ByVal Info As SysFile) As String
            Dim R As String = ""

            Randomize()
            Dim NumRango As Integer = CInt(30 * Rnd() + 1)

            If NumRango = 32 Then NumRango = CInt(28 * Rnd() + 1)

            Dim NumRandoSTR As String = NumRango.ToString.PadLeft(2, "0")

            ' se suma minimo 17, para que 0 sea = A. Solo en codigo de descifrado.
            Info.RazonSocial = Info.RazonSocial.ToUpper

            For I As Integer = 0 To 1
                R &= Chr(Asc(NumRandoSTR.Substring(I, 1)) + 17)
            Next

            For I As Integer = 0 To Info.RazonSocial.Length - 1
                R &= Chr(Asc(Info.RazonSocial.Substring(I, 1)) + NumRango)
            Next

            Return R

        End Function

        Function PinDesEncripta(ByVal lPIN As String) As String
            Dim R As String = ""

            Dim lst1 As String = Chr(Asc(lPIN.Substring(0, 1)) - 17)
            Dim lst2 As String = Chr(Asc(lPIN.Substring(1, 1)) - 17)

            Dim lNumEnc As Integer = (CInt(lst1) * 10) + CInt(lst2)

            For I As Integer = 2 To lPIN.Length - 1
                R &= Chr(Asc(lPIN.Substring(I, 1)) - lNumEnc)
            Next

            Return R
        End Function

    End Class
End Namespace

