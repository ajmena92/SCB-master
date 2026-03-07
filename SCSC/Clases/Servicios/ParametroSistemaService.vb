Imports System.Configuration
Imports System.Data.SqlClient
Imports System.Globalization

Public Class ParametroSistemaService
    Private Const ParametroFilaUnicaId As Integer = 0

    Public Class ParametroSistemaConfig
        Public Property Id As Integer
        Public Property Institucion As String
        Public Property CodPresupuestario As String
        Public Property Ubicacion As String
        Public Property Leyenda As String
        Public Property ControlCarnet As String
        Public Property PrecioDocente As Decimal
        Public Property PrecioEstudiante As Decimal
        Public Property PermitirSinMarcaTransporte As Boolean
        Public Property DesactivarNoImportadosExcel As Boolean
        Public Property AuditarImportacionExcel As Boolean
    End Class

    Public Sub AsegurarEsquema(ByVal cn As SqlConnection)
        Dim script As String =
"IF OBJECT_ID(N'dbo.Parametro', N'U') IS NULL RETURN;" &
"IF COL_LENGTH('dbo.Parametro', 'PermitirSinMarcaTransporte') IS NULL " &
"BEGIN " &
"    ALTER TABLE dbo.Parametro ADD PermitirSinMarcaTransporte BIT NULL; " &
"    EXEC sp_executesql N'UPDATE dbo.Parametro SET PermitirSinMarcaTransporte = 0 WHERE PermitirSinMarcaTransporte IS NULL;'; " &
"END;" &
"IF COL_LENGTH('dbo.Parametro', 'DesactivarNoImportadosExcel') IS NULL " &
"BEGIN " &
"    ALTER TABLE dbo.Parametro ADD DesactivarNoImportadosExcel BIT NULL; " &
"    EXEC sp_executesql N'UPDATE dbo.Parametro SET DesactivarNoImportadosExcel = 1 WHERE DesactivarNoImportadosExcel IS NULL;'; " &
"END;" &
"IF COL_LENGTH('dbo.Parametro', 'DesactivarNoImportatExcel') IS NOT NULL " &
"BEGIN " &
"    EXEC sp_executesql N'UPDATE dbo.Parametro SET DesactivarNoImportadosExcel = ISNULL(DesactivarNoImportadosExcel, 1) WHERE DesactivarNoImportadosExcel IS NULL;'; " &
"    EXEC sp_executesql N'UPDATE dbo.Parametro SET DesactivarNoImportadosExcel = CAST(DesactivarNoImportatExcel AS BIT) WHERE DesactivarNoImportatExcel IS NOT NULL;'; " &
"END;" &
"IF COL_LENGTH('dbo.Parametro', 'AuditarImportacionExcel') IS NULL " &
"BEGIN " &
"    ALTER TABLE dbo.Parametro ADD AuditarImportacionExcel BIT NULL; " &
"    EXEC sp_executesql N'UPDATE dbo.Parametro SET AuditarImportacionExcel = 1 WHERE AuditarImportacionExcel IS NULL;'; " &
"END"

        Using cmd As New SqlCommand(script, cn)
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Public Sub MigrarDesdeAppConfigSiCorresponde(ByVal cn As SqlConnection)
        Dim cfg As ParametroSistemaConfig = ObtenerFila1(cn)
        If cfg Is Nothing Then
            Exit Sub
        End If

        Dim huboCambio As Boolean = False

        If String.IsNullOrWhiteSpace(cfg.Institucion) Then
            Dim valor As String = LeerApp("Institucion")
            If valor.Length > 0 Then
                cfg.Institucion = valor
                huboCambio = True
            End If
        End If

        If String.IsNullOrWhiteSpace(cfg.CodPresupuestario) Then
            Dim valor As String = LeerApp("CodPresupuestario")
            If valor.Length > 0 Then
                cfg.CodPresupuestario = valor
                huboCambio = True
            End If
        End If

        If String.IsNullOrWhiteSpace(cfg.Ubicacion) Then
            Dim valor As String = LeerApp("Ubicacion")
            If valor.Length > 0 Then
                cfg.Ubicacion = valor
                huboCambio = True
            End If
        End If

        If String.IsNullOrWhiteSpace(cfg.Leyenda) Then
            Dim valor As String = LeerApp("Leyenda")
            If valor.Length > 0 Then
                cfg.Leyenda = valor
                huboCambio = True
            End If
        End If

        If String.IsNullOrWhiteSpace(cfg.ControlCarnet) Then
            Dim valor As String = LeerApp("ControlCarnet")
            If valor.Length > 0 Then
                cfg.ControlCarnet = valor
                huboCambio = True
            End If
        End If

        If cfg.PrecioDocente <= 0D Then
            Dim valor As Decimal = LeerAppDecimal("PrecioDocente", 0D)
            If valor > 0D Then
                cfg.PrecioDocente = valor
                huboCambio = True
            End If
        End If

        If cfg.PrecioEstudiante <= 0D Then
            Dim valor As Decimal = LeerAppDecimal("PrecioEstudiante", 0D)
            If valor > 0D Then
                cfg.PrecioEstudiante = valor
                huboCambio = True
            End If
        End If

        If Not cfg.PermitirSinMarcaTransporte Then
            Dim permitir As Boolean = LeerAppBool("PermitirSinMarcaTransporte", False)
            If permitir Then
                cfg.PermitirSinMarcaTransporte = True
                huboCambio = True
            End If
        End If

        If Not cfg.DesactivarNoImportadosExcel Then
            Dim activar As Boolean = LeerAppBool("DesactivarNoImportadosExcel", False)
            If Not activar Then
                activar = LeerAppBool("DesactivarNoImportatExcel", False)
            End If
            If activar Then
                cfg.DesactivarNoImportadosExcel = True
                huboCambio = True
            End If
        End If

        If Not cfg.AuditarImportacionExcel Then
            Dim auditar As Boolean = LeerAppBool("AuditarImportacionExcel", False)
            If auditar Then
                cfg.AuditarImportacionExcel = True
                huboCambio = True
            End If
        End If

        If huboCambio Then
            GuardarFila1(cn, cfg)
        End If
    End Sub

    Public Function ObtenerFila1(ByVal cn As SqlConnection) As ParametroSistemaConfig
        Dim sql As String = "SELECT TOP 1 * FROM Parametro WHERE Id = @Id;"
        Using cmd As New SqlCommand(sql, cn)
            cmd.Parameters.AddWithValue("@Id", ParametroFilaUnicaId)
            Using rd As SqlDataReader = cmd.ExecuteReader()
                If Not rd.Read() Then
                    Return Nothing
                End If

                Dim cfg As New ParametroSistemaConfig()
                cfg.Id = ParametroFilaUnicaId
                cfg.Institucion = LeerStr(rd, "Institucion")
                cfg.CodPresupuestario = LeerStr(rd, "CodPresupuestario")
                cfg.Ubicacion = LeerStr(rd, "Ubicacion")
                cfg.Leyenda = LeerStr(rd, "Leyenda")
                cfg.ControlCarnet = LeerStr(rd, "ControlCarnet")
                cfg.PrecioDocente = LeerDec(rd, "PrecioDocente")
                cfg.PrecioEstudiante = LeerDec(rd, "PrecioEstudiante")
                cfg.PermitirSinMarcaTransporte = LeerBool(rd, "PermitirSinMarcaTransporte", False)
                cfg.DesactivarNoImportadosExcel = LeerBool(rd, "DesactivarNoImportadosExcel", True)
                cfg.AuditarImportacionExcel = LeerBool(rd, "AuditarImportacionExcel", True)
                Return cfg
            End Using
        End Using
    End Function

    Public Function ContarFilasIgnoradas(ByVal cn As SqlConnection) As Integer
        Dim sql As String = "SELECT COUNT(1) FROM Parametro WHERE Id <> @Id;"
        Using cmd As New SqlCommand(sql, cn)
            cmd.Parameters.AddWithValue("@Id", ParametroFilaUnicaId)
            Return Convert.ToInt32(cmd.ExecuteScalar())
        End Using
    End Function

    Public Sub CrearFila1(ByVal cn As SqlConnection)
        Dim sql As String =
"IF EXISTS (SELECT 1 FROM Parametro WHERE Id = @Id) RETURN;" & vbCrLf &
"BEGIN TRY" & vbCrLf &
"    INSERT INTO Parametro (Id, Institucion, CodPresupuestario, Ubicacion, Leyenda, ControlCarnet, PrecioDocente, PrecioEstudiante, PermitirSinMarcaTransporte, DesactivarNoImportadosExcel, AuditarImportacionExcel)" & vbCrLf &
"    VALUES (@Id, N'', N'', N'', N'', N'', 0, 0, 0, 1, 1);" & vbCrLf &
"END TRY" & vbCrLf &
"BEGIN CATCH" & vbCrLf &
"    BEGIN TRY" & vbCrLf &
"        SET IDENTITY_INSERT Parametro ON;" & vbCrLf &
"        INSERT INTO Parametro (Id, Institucion, CodPresupuestario, Ubicacion, Leyenda, ControlCarnet, PrecioDocente, PrecioEstudiante, PermitirSinMarcaTransporte, DesactivarNoImportadosExcel, AuditarImportacionExcel)" & vbCrLf &
"        VALUES (@Id, N'', N'', N'', N'', N'', 0, 0, 0, 1, 1);" & vbCrLf &
"        SET IDENTITY_INSERT Parametro OFF;" & vbCrLf &
"    END TRY" & vbCrLf &
"    BEGIN CATCH" & vbCrLf &
"        BEGIN TRY SET IDENTITY_INSERT Parametro OFF; END TRY BEGIN CATCH END CATCH;" & vbCrLf &
"        THROW;" & vbCrLf &
"    END CATCH" & vbCrLf &
"END CATCH"

        Using cmd As New SqlCommand(sql, cn)
            cmd.Parameters.AddWithValue("@Id", ParametroFilaUnicaId)
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Public Sub GuardarFila1(ByVal cn As SqlConnection, ByVal cfg As ParametroSistemaConfig)
        Dim sql As String =
"UPDATE Parametro SET" & vbCrLf &
" Institucion=@Institucion," & vbCrLf &
" CodPresupuestario=@CodPresupuestario," & vbCrLf &
" Ubicacion=@Ubicacion," & vbCrLf &
" Leyenda=@Leyenda," & vbCrLf &
" ControlCarnet=@ControlCarnet," & vbCrLf &
" PrecioDocente=@PrecioDocente," & vbCrLf &
" PrecioEstudiante=@PrecioEstudiante," & vbCrLf &
" PermitirSinMarcaTransporte=@PermitirSinMarcaTransporte," & vbCrLf &
" DesactivarNoImportadosExcel=@DesactivarNoImportadosExcel," & vbCrLf &
" AuditarImportacionExcel=@AuditarImportacionExcel" & vbCrLf &
"WHERE Id=@Id;"

        Using cmd As New SqlCommand(sql, cn)
            cmd.Parameters.AddWithValue("@Id", ParametroFilaUnicaId)
            cmd.Parameters.AddWithValue("@Institucion", If(cfg.Institucion, String.Empty))
            cmd.Parameters.AddWithValue("@CodPresupuestario", If(cfg.CodPresupuestario, String.Empty))
            cmd.Parameters.AddWithValue("@Ubicacion", If(cfg.Ubicacion, String.Empty))
            cmd.Parameters.AddWithValue("@Leyenda", If(cfg.Leyenda, String.Empty))
            cmd.Parameters.AddWithValue("@ControlCarnet", If(cfg.ControlCarnet, String.Empty))
            cmd.Parameters.AddWithValue("@PrecioDocente", cfg.PrecioDocente)
            cmd.Parameters.AddWithValue("@PrecioEstudiante", cfg.PrecioEstudiante)
            cmd.Parameters.AddWithValue("@PermitirSinMarcaTransporte", cfg.PermitirSinMarcaTransporte)
            cmd.Parameters.AddWithValue("@DesactivarNoImportadosExcel", cfg.DesactivarNoImportadosExcel)
            cmd.Parameters.AddWithValue("@AuditarImportacionExcel", cfg.AuditarImportacionExcel)
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Public Sub RestablecerFila1(ByVal cn As SqlConnection)
        Dim sql As String =
"UPDATE Parametro SET" & vbCrLf &
" Institucion=N''," & vbCrLf &
" CodPresupuestario=N''," & vbCrLf &
" Ubicacion=N''," & vbCrLf &
" Leyenda=N''," & vbCrLf &
" ControlCarnet=N''," & vbCrLf &
" PrecioDocente=0," & vbCrLf &
" PrecioEstudiante=0," & vbCrLf &
" PermitirSinMarcaTransporte=0," & vbCrLf &
" DesactivarNoImportadosExcel=1," & vbCrLf &
" AuditarImportacionExcel=1" & vbCrLf &
"WHERE Id=@Id;"

        Using cmd As New SqlCommand(sql, cn)
            cmd.Parameters.AddWithValue("@Id", ParametroFilaUnicaId)
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Private Function LeerApp(ByVal key As String) As String
        Return Convert.ToString(ConfigurationManager.AppSettings(key)).Trim()
    End Function

    Private Function LeerAppBool(ByVal key As String, ByVal defaultValue As Boolean) As Boolean
        Dim raw As String = LeerApp(key)
        If String.IsNullOrWhiteSpace(raw) Then Return defaultValue

        If raw = "1" Then Return True
        If raw = "0" Then Return False

        Dim parsed As Boolean
        If Boolean.TryParse(raw, parsed) Then
            Return parsed
        End If

        Return defaultValue
    End Function

    Private Function LeerAppDecimal(ByVal key As String, ByVal defaultValue As Decimal) As Decimal
        Dim raw As String = LeerApp(key)
        If String.IsNullOrWhiteSpace(raw) Then Return defaultValue

        Dim parsed As Decimal
        If Decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, parsed) Then
            Return parsed
        End If

        If Decimal.TryParse(raw, NumberStyles.Any, CultureInfo.CurrentCulture, parsed) Then
            Return parsed
        End If

        Return defaultValue
    End Function

    Private Function LeerStr(ByVal rd As SqlDataReader, ByVal col As String, Optional ByVal [default] As String = "") As String
        Dim i As Integer = GetOrdinalSeguro(rd, col)
        If i < 0 OrElse rd.IsDBNull(i) Then Return [default]
        Return Convert.ToString(rd.GetValue(i)).Trim()
    End Function

    Private Function LeerDec(ByVal rd As SqlDataReader, ByVal col As String, Optional ByVal [default] As Decimal = 0D) As Decimal
        Dim i As Integer = GetOrdinalSeguro(rd, col)
        If i < 0 OrElse rd.IsDBNull(i) Then Return [default]

        Dim parsed As Decimal
        If Decimal.TryParse(Convert.ToString(rd.GetValue(i)), NumberStyles.Any, CultureInfo.InvariantCulture, parsed) Then
            Return parsed
        End If

        If Decimal.TryParse(Convert.ToString(rd.GetValue(i)), NumberStyles.Any, CultureInfo.CurrentCulture, parsed) Then
            Return parsed
        End If

        Return [default]
    End Function

    Private Function LeerBool(ByVal rd As SqlDataReader, ByVal col As String, ByVal [default] As Boolean) As Boolean
        Dim i As Integer = GetOrdinalSeguro(rd, col)
        If i < 0 OrElse rd.IsDBNull(i) Then Return [default]

        Dim raw As Object = rd.GetValue(i)
        If TypeOf raw Is Boolean Then Return CBool(raw)

        Dim s As String = Convert.ToString(raw).Trim()
        If s = "1" Then Return True
        If s = "0" Then Return False

        Dim parsed As Boolean
        If Boolean.TryParse(s, parsed) Then
            Return parsed
        End If

        Return [default]
    End Function

    Private Function GetOrdinalSeguro(ByVal rd As SqlDataReader, ByVal nombreColumna As String) As Integer
        Try
            Return rd.GetOrdinal(nombreColumna)
        Catch
            Return -1
        End Try
    End Function
End Class
