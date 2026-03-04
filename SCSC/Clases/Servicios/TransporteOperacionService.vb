Option Explicit On
Option Strict On

Imports System.Data
Imports System.Data.SqlClient

Public Class TransporteOperacionService

    Public Sub AsegurarEsquema(ByVal cn As SqlConnection)
        Const sql As String =
"IF OBJECT_ID(N'dbo.OperacionTransporteEvento', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OperacionTransporteEvento
    (
        IdOperacionTransporteEvento BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        FechaEvento DATETIME2(0) NOT NULL CONSTRAINT DF_OperacionTransporteEvento_FechaEvento DEFAULT (SYSUTCDATETIME()),
        Cedula NVARCHAR(50) NULL,
        Estado NVARCHAR(40) NOT NULL,
        Motivo NVARCHAR(300) NULL,
        TiempoAtencionMs INT NULL,
        EsDuplicado BIT NOT NULL CONSTRAINT DF_OperacionTransporteEvento_EsDuplicado DEFAULT (0),
        TieneAdvertencia BIT NOT NULL CONSTRAINT DF_OperacionTransporteEvento_TieneAdvertencia DEFAULT (0),
        TieneError BIT NOT NULL CONSTRAINT DF_OperacionTransporteEvento_TieneError DEFAULT (0),
        EsIncidenciaManual BIT NOT NULL CONSTRAINT DF_OperacionTransporteEvento_EsIncidenciaManual DEFAULT (0)
    );
    CREATE INDEX IX_OperacionTransporteEvento_FechaEvento ON dbo.OperacionTransporteEvento(FechaEvento DESC);
    CREATE INDEX IX_OperacionTransporteEvento_Estado ON dbo.OperacionTransporteEvento(Estado, FechaEvento DESC);
END"

        Using cmd As New SqlCommand(sql, cn)
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Public Sub RegistrarEvento(ByVal cn As SqlConnection,
                               ByVal fechaEvento As DateTime,
                               ByVal cedula As String,
                               ByVal estado As String,
                               ByVal motivo As String,
                               ByVal tiempoAtencionMs As Integer?,
                               ByVal esDuplicado As Boolean,
                               ByVal tieneAdvertencia As Boolean,
                               ByVal tieneError As Boolean,
                               Optional ByVal esIncidenciaManual As Boolean = False,
                               Optional ByVal tx As SqlTransaction = Nothing)
        Const sql As String =
"INSERT INTO dbo.OperacionTransporteEvento
(FechaEvento, Cedula, Estado, Motivo, TiempoAtencionMs, EsDuplicado, TieneAdvertencia, TieneError, EsIncidenciaManual)
VALUES
(@FechaEvento, @Cedula, @Estado, @Motivo, @TiempoAtencionMs, @EsDuplicado, @TieneAdvertencia, @TieneError, @EsIncidenciaManual);"

        Using cmd As New SqlCommand(sql, cn)
            If tx IsNot Nothing Then
                cmd.Transaction = tx
            End If
            cmd.Parameters.AddWithValue("@FechaEvento", fechaEvento)
            cmd.Parameters.AddWithValue("@Cedula", If(String.IsNullOrWhiteSpace(cedula), CType(DBNull.Value, Object), cedula.Trim()))
            cmd.Parameters.AddWithValue("@Estado", If(String.IsNullOrWhiteSpace(estado), "DESCONOCIDO", estado.Trim()))
            cmd.Parameters.AddWithValue("@Motivo", If(String.IsNullOrWhiteSpace(motivo), CType(DBNull.Value, Object), motivo.Trim()))
            If tiempoAtencionMs.HasValue Then
                cmd.Parameters.AddWithValue("@TiempoAtencionMs", tiempoAtencionMs.Value)
            Else
                cmd.Parameters.AddWithValue("@TiempoAtencionMs", DBNull.Value)
            End If
            cmd.Parameters.AddWithValue("@EsDuplicado", esDuplicado)
            cmd.Parameters.AddWithValue("@TieneAdvertencia", tieneAdvertencia)
            cmd.Parameters.AddWithValue("@TieneError", tieneError)
            cmd.Parameters.AddWithValue("@EsIncidenciaManual", esIncidenciaManual)
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Public Function ListarUltimosEventos(ByVal cn As SqlConnection, ByVal cantidad As Integer) As DataTable
        Const sql As String =
"SELECT TOP (@Cantidad)
    FechaEvento,
    Estado,
    ISNULL(Cedula, N'') AS Cedula,
    ISNULL(Motivo, N'') AS Motivo
FROM dbo.OperacionTransporteEvento
ORDER BY IdOperacionTransporteEvento DESC;"

        Dim dt As New DataTable()
        Using cmd As New SqlCommand(sql, cn)
            cmd.Parameters.AddWithValue("@Cantidad", Math.Max(1, cantidad))
            Using da As New SqlDataAdapter(cmd)
                da.Fill(dt)
            End Using
        End Using
        Return dt
    End Function
End Class

