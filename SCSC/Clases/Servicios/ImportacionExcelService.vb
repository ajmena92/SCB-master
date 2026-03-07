Imports System.Data
Imports System.Data.SqlClient

Public Class ImportacionExcelService
    Private ReadOnly _cls As FuncionesDB

    Public Class ImportacionAuditoria
        Public Property ArchivoOrigen As String
        Public Property TipoUsuario As Integer
        Public Property IdHorario As Integer
        Public Property FilasOrigen As Integer
        Public Property FilasValidas As Integer
        Public Property FilasOmitidasEstado As Integer
        Public Property FilasOmitidasCedula As Integer
        Public Property FilasDuplicadas As Integer
        Public Property FilasSinFechaNac As Integer
        Public Property DesactivarNoImportados As Boolean
        Public Property Exito As Boolean
        Public Property Mensaje As String
        Public Property UsuarioEjecucion As String
    End Class

    Public Sub New(Optional ByVal cls As FuncionesDB = Nothing)
        If cls Is Nothing Then
            _cls = New FuncionesDB()
        Else
            _cls = cls
        End If
    End Sub

    Public Function ObtenerHorariosActivos(ByVal cn As SqlConnection) As DataSet
        Dim valores() As FuncionesDB.Campos = _cls.InicializarArray
        Dim llave() As FuncionesDB.Campos = _cls.InicializarArray

        _cls.ArmaValor(valores, "IdHorario")
        _cls.ArmaValor(valores, "Descripcion")
        _cls.ArmaValor(valores, "Activo")
        _cls.ArmaValor(llave, "Activo", 1)

        Return _cls.Consultar("Horario", valores, llave, cn)
    End Function

    Public Sub MarcarUsuariosComoNoActualizados(ByVal cn As SqlConnection,
                                                ByVal pTransac As SqlTransaction,
                                                ByVal tipoUsuario As Integer,
                                                ByVal idHorario As Integer)
        Using cmd As New SqlCommand("UPDATE Usuario SET Actualizado = 0 WHERE CodTipo = @CodTipo AND IdHorario = @IdHorario;", cn, pTransac)
            cmd.Parameters.Add("@CodTipo", SqlDbType.Int).Value = tipoUsuario
            cmd.Parameters.Add("@IdHorario", SqlDbType.Int).Value = idHorario
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Public Sub DesactivarNoActualizados(ByVal cn As SqlConnection,
                                        ByVal pTransac As SqlTransaction,
                                        ByVal tipoUsuario As Integer,
                                        ByVal idHorario As Integer)
        Using cmd As New SqlCommand("UPDATE Usuario SET Activo = 0 WHERE Actualizado = 0 AND CodTipo = @CodTipo AND IdHorario = @IdHorario;", cn, pTransac)
            cmd.Parameters.Add("@CodTipo", SqlDbType.Int).Value = tipoUsuario
            cmd.Parameters.Add("@IdHorario", SqlDbType.Int).Value = idHorario
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Public Sub ImportarUsuariosNormalizadosEnLote(ByVal tabla As DataTable,
                                                   ByVal tipoUsuario As Integer,
                                                   ByVal idHorario As Integer,
                                                   ByVal cn As SqlConnection,
                                                   ByVal pTransac As SqlTransaction)
        If tabla Is Nothing OrElse tabla.Rows.Count = 0 Then
            Return
        End If

        CrearTablaTemporalImportacion(cn, pTransac)
        CargarTablaTemporal(tabla, cn, pTransac)
        ActualizarUsuariosExistentes(tipoUsuario, idHorario, cn, pTransac)
        InsertarUsuariosNuevos(tipoUsuario, idHorario, cn, pTransac)
    End Sub

    Private Sub CrearTablaTemporalImportacion(ByVal cn As SqlConnection, ByVal pTransac As SqlTransaction)
        Dim sql As String =
            "IF OBJECT_ID('tempdb..#TmpUsuarioImportacion') IS NOT NULL DROP TABLE #TmpUsuarioImportacion;" &
            " CREATE TABLE #TmpUsuarioImportacion (" &
            " Cedula NVARCHAR(20) COLLATE DATABASE_DEFAULT NOT NULL," &
            " PrimerApellido NVARCHAR(100) COLLATE DATABASE_DEFAULT NULL," &
            " SegundoApellido NVARCHAR(100) COLLATE DATABASE_DEFAULT NULL," &
            " Nombre NVARCHAR(100) COLLATE DATABASE_DEFAULT NULL," &
            " Seccion NVARCHAR(30) COLLATE DATABASE_DEFAULT NULL," &
            " Especialidad NVARCHAR(100) COLLATE DATABASE_DEFAULT NULL," &
            " FechaNac DATE NULL," &
            " Telefono NVARCHAR(50) COLLATE DATABASE_DEFAULT NULL," &
            " Sexo INT NULL" &
            " );" &
            " CREATE INDEX IX_TmpUsuarioImportacion_Cedula ON #TmpUsuarioImportacion(Cedula);"

        Using cmd As New SqlCommand(sql, cn, pTransac)
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Private Sub CargarTablaTemporal(ByVal tabla As DataTable, ByVal cn As SqlConnection, ByVal pTransac As SqlTransaction)
        Using bulk As New SqlBulkCopy(cn, SqlBulkCopyOptions.CheckConstraints Or SqlBulkCopyOptions.KeepNulls, pTransac)
            bulk.DestinationTableName = "#TmpUsuarioImportacion"
            bulk.BatchSize = 500
            bulk.BulkCopyTimeout = 120
            bulk.ColumnMappings.Add("Cedula", "Cedula")
            bulk.ColumnMappings.Add("PrimerApellido", "PrimerApellido")
            bulk.ColumnMappings.Add("SegundoApellido", "SegundoApellido")
            bulk.ColumnMappings.Add("Nombre", "Nombre")
            bulk.ColumnMappings.Add("Seccion", "Seccion")
            bulk.ColumnMappings.Add("Especialidad", "Especialidad")
            bulk.ColumnMappings.Add("FechaNac", "FechaNac")
            bulk.ColumnMappings.Add("Telefono", "Telefono")
            bulk.ColumnMappings.Add("Sexo", "Sexo")
            bulk.WriteToServer(tabla)
        End Using
    End Sub

    Private Sub ActualizarUsuariosExistentes(ByVal tipoUsuario As Integer,
                                             ByVal idHorario As Integer,
                                             ByVal cn As SqlConnection,
                                             ByVal pTransac As SqlTransaction)
        Dim sql As String =
            "UPDATE U" &
            " SET U.PrimerApellido = LEFT(ISNULL(T.PrimerApellido, ''), 100)," &
            "     U.SegundoApellido = LEFT(ISNULL(T.SegundoApellido, ''), 100)," &
            "     U.Nombre = LEFT(ISNULL(T.Nombre, ''), 100)," &
            "     U.Seccion = CASE WHEN @CodTipo = 1 THEN LEFT(ISNULL(NULLIF(T.Seccion, ''), 'NA'), 30) ELSE 'NA' END," &
            "     U.Especialidad = CASE WHEN @CodTipo = 1 THEN UPPER(LEFT(ISNULL(NULLIF(T.Especialidad, ''), 'III CICLO'), 100)) ELSE 'NA' END," &
            "     U.FechaNac = CASE WHEN @CodTipo = 1 THEN ISNULL(T.FechaNac, U.FechaNac) ELSE U.FechaNac END," &
            "     U.Telefono = CASE WHEN @CodTipo = 1 THEN LEFT(ISNULL(T.Telefono, ''), 50) ELSE U.Telefono END," &
            "     U.Sexo = CASE WHEN @CodTipo = 1 THEN ISNULL(T.Sexo, U.Sexo) ELSE U.Sexo END," &
            "     U.IdHorario = @IdHorario," &
            "     U.CodTipo = @CodTipo," &
            "     U.Actualizado = 1," &
            "     U.Activo = 1" &
            " FROM Usuario U" &
            " INNER JOIN #TmpUsuarioImportacion T ON U.Cedula COLLATE DATABASE_DEFAULT = T.Cedula COLLATE DATABASE_DEFAULT;"

        Using cmd As New SqlCommand(sql, cn, pTransac)
            cmd.Parameters.Add("@CodTipo", SqlDbType.Int).Value = tipoUsuario
            cmd.Parameters.Add("@IdHorario", SqlDbType.Int).Value = idHorario
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Private Sub InsertarUsuariosNuevos(ByVal tipoUsuario As Integer,
                                       ByVal idHorario As Integer,
                                       ByVal cn As SqlConnection,
                                       ByVal pTransac As SqlTransaction)
        Dim sql As String =
            "INSERT INTO Usuario (" &
            " Cedula, PrimerApellido, SegundoApellido, Nombre, Seccion, Especialidad, FechaNac, Telefono, Sexo, IdHorario, CodTipo, Actualizado, Activo" &
            " )" &
            " SELECT" &
            " LEFT(T.Cedula, 20) AS Cedula," &
            " LEFT(ISNULL(T.PrimerApellido, ''), 100) AS PrimerApellido," &
            " LEFT(ISNULL(T.SegundoApellido, ''), 100) AS SegundoApellido," &
            " LEFT(ISNULL(T.Nombre, ''), 100) AS Nombre," &
            " CASE WHEN @CodTipo = 1 THEN LEFT(ISNULL(NULLIF(T.Seccion, ''), 'NA'), 30) ELSE 'NA' END AS Seccion," &
            " CASE WHEN @CodTipo = 1 THEN UPPER(LEFT(ISNULL(NULLIF(T.Especialidad, ''), 'III CICLO'), 100)) ELSE 'NA' END AS Especialidad," &
            " CASE WHEN @CodTipo = 1 THEN ISNULL(T.FechaNac, CONVERT(date, GETDATE())) ELSE CONVERT(date, GETDATE()) END AS FechaNac," &
            " CASE WHEN @CodTipo = 1 THEN LEFT(ISNULL(T.Telefono, ''), 50) ELSE '' END AS Telefono," &
            " CASE WHEN @CodTipo = 1 THEN ISNULL(T.Sexo, 0) ELSE 0 END AS Sexo," &
            " @IdHorario AS IdHorario," &
            " @CodTipo AS CodTipo," &
            " 1 AS Actualizado," &
            " 1 AS Activo" &
            " FROM #TmpUsuarioImportacion T" &
            " WHERE NOT EXISTS (SELECT 1 FROM Usuario U WHERE U.Cedula COLLATE DATABASE_DEFAULT = T.Cedula COLLATE DATABASE_DEFAULT);"

        Using cmd As New SqlCommand(sql, cn, pTransac)
            cmd.Parameters.Add("@CodTipo", SqlDbType.Int).Value = tipoUsuario
            cmd.Parameters.Add("@IdHorario", SqlDbType.Int).Value = idHorario
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Public Sub RegistrarAuditoriaImportacion(ByVal cn As SqlConnection,
                                             ByVal pTransac As SqlTransaction,
                                             ByVal auditoria As ImportacionAuditoria)
        If auditoria Is Nothing Then
            Exit Sub
        End If

        AsegurarTablaAuditoria(cn, pTransac)

        Dim sql As String =
"INSERT INTO dbo.ImportacionExcelAuditoria (" &
" ArchivoOrigen, TipoUsuario, IdHorario, FilasOrigen, FilasValidas, FilasOmitidasEstado, FilasOmitidasCedula, FilasDuplicadas, FilasSinFechaNac, DesactivarNoImportados, Exito, Mensaje, UsuarioEjecucion)" &
" VALUES (" &
" @ArchivoOrigen, @TipoUsuario, @IdHorario, @FilasOrigen, @FilasValidas, @FilasOmitidasEstado, @FilasOmitidasCedula, @FilasDuplicadas, @FilasSinFechaNac, @DesactivarNoImportados, @Exito, @Mensaje, @UsuarioEjecucion);"

        Using cmd As New SqlCommand(sql, cn, pTransac)
            cmd.Parameters.Add("@ArchivoOrigen", SqlDbType.NVarChar, 260).Value = If(auditoria.ArchivoOrigen, String.Empty)
            cmd.Parameters.Add("@TipoUsuario", SqlDbType.Int).Value = auditoria.TipoUsuario
            cmd.Parameters.Add("@IdHorario", SqlDbType.Int).Value = auditoria.IdHorario
            cmd.Parameters.Add("@FilasOrigen", SqlDbType.Int).Value = auditoria.FilasOrigen
            cmd.Parameters.Add("@FilasValidas", SqlDbType.Int).Value = auditoria.FilasValidas
            cmd.Parameters.Add("@FilasOmitidasEstado", SqlDbType.Int).Value = auditoria.FilasOmitidasEstado
            cmd.Parameters.Add("@FilasOmitidasCedula", SqlDbType.Int).Value = auditoria.FilasOmitidasCedula
            cmd.Parameters.Add("@FilasDuplicadas", SqlDbType.Int).Value = auditoria.FilasDuplicadas
            cmd.Parameters.Add("@FilasSinFechaNac", SqlDbType.Int).Value = auditoria.FilasSinFechaNac
            cmd.Parameters.Add("@DesactivarNoImportados", SqlDbType.Bit).Value = auditoria.DesactivarNoImportados
            cmd.Parameters.Add("@Exito", SqlDbType.Bit).Value = auditoria.Exito
            cmd.Parameters.Add("@Mensaje", SqlDbType.NVarChar, 2000).Value = If(auditoria.Mensaje, String.Empty)
            cmd.Parameters.Add("@UsuarioEjecucion", SqlDbType.NVarChar, 100).Value = If(auditoria.UsuarioEjecucion, String.Empty)
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Private Sub AsegurarTablaAuditoria(ByVal cn As SqlConnection, ByVal pTransac As SqlTransaction)
        Dim sql As String =
"IF OBJECT_ID('dbo.ImportacionExcelAuditoria', 'U') IS NULL " &
"BEGIN " &
"    CREATE TABLE dbo.ImportacionExcelAuditoria ( " &
"        IdAuditoria INT IDENTITY(1,1) NOT NULL PRIMARY KEY, " &
"        FechaEvento DATETIME2(0) NOT NULL CONSTRAINT DF_ImportacionExcelAuditoria_FechaEvento DEFAULT SYSDATETIME(), " &
"        ArchivoOrigen NVARCHAR(260) NULL, " &
"        TipoUsuario INT NOT NULL, " &
"        IdHorario INT NOT NULL, " &
"        FilasOrigen INT NOT NULL, " &
"        FilasValidas INT NOT NULL, " &
"        FilasOmitidasEstado INT NOT NULL, " &
"        FilasOmitidasCedula INT NOT NULL, " &
"        FilasDuplicadas INT NOT NULL, " &
"        FilasSinFechaNac INT NOT NULL CONSTRAINT DF_ImportacionExcelAuditoria_FilasSinFechaNac DEFAULT(0), " &
"        DesactivarNoImportados BIT NOT NULL CONSTRAINT DF_ImportacionExcelAuditoria_Desactivar DEFAULT(1), " &
"        Exito BIT NOT NULL, " &
"        Mensaje NVARCHAR(2000) NULL, " &
"        UsuarioEjecucion NVARCHAR(100) NULL " &
"    ); " &
"    CREATE INDEX IX_ImportacionExcelAuditoria_FechaEvento ON dbo.ImportacionExcelAuditoria(FechaEvento); " &
"END " &
"ELSE " &
"BEGIN " &
"    IF COL_LENGTH('dbo.ImportacionExcelAuditoria', 'FilasSinFechaNac') IS NULL " &
"        ALTER TABLE dbo.ImportacionExcelAuditoria ADD FilasSinFechaNac INT NOT NULL CONSTRAINT DF_ImportacionExcelAuditoria_FilasSinFechaNac DEFAULT(0); " &
"    IF COL_LENGTH('dbo.ImportacionExcelAuditoria', 'DesactivarNoImportados') IS NULL " &
"        ALTER TABLE dbo.ImportacionExcelAuditoria ADD DesactivarNoImportados BIT NOT NULL CONSTRAINT DF_ImportacionExcelAuditoria_Desactivar DEFAULT(1); " &
"    IF COL_LENGTH('dbo.ImportacionExcelAuditoria', 'UsuarioEjecucion') IS NULL " &
"        ALTER TABLE dbo.ImportacionExcelAuditoria ADD UsuarioEjecucion NVARCHAR(100) NULL; " &
"END;"

        Using cmd As New SqlCommand(sql, cn, pTransac)
            cmd.ExecuteNonQuery()
        End Using
    End Sub

End Class
