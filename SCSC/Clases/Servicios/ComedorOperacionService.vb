Option Explicit On
Option Strict On

Imports System.Data
Imports System.Data.SqlClient

Public Class ComedorOperacionService

    Public Sub AsegurarEsquema(ByVal cn As SqlConnection)
        Const sql As String =
"IF OBJECT_ID(N'dbo.OperacionComedorEvento', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OperacionComedorEvento
    (
        IdOperacionComedorEvento BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        FechaEvento DATETIME2(0) NOT NULL CONSTRAINT DF_OperacionComedorEvento_FechaEvento DEFAULT (SYSUTCDATETIME()),
        Cedula NVARCHAR(50) NULL,
        Estado NVARCHAR(40) NOT NULL,
        Motivo NVARCHAR(300) NULL,
        TiempoAtencionMs INT NULL,
        EsDuplicado BIT NOT NULL CONSTRAINT DF_OperacionComedorEvento_EsDuplicado DEFAULT (0),
        TieneAdvertencia BIT NOT NULL CONSTRAINT DF_OperacionComedorEvento_TieneAdvertencia DEFAULT (0),
        TieneError BIT NOT NULL CONSTRAINT DF_OperacionComedorEvento_TieneError DEFAULT (0),
        EsIncidenciaManual BIT NOT NULL CONSTRAINT DF_OperacionComedorEvento_EsIncidenciaManual DEFAULT (0)
    );
    CREATE INDEX IX_OperacionComedorEvento_FechaEvento ON dbo.OperacionComedorEvento(FechaEvento DESC);
    CREATE INDEX IX_OperacionComedorEvento_Estado ON dbo.OperacionComedorEvento(Estado, FechaEvento DESC);
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
                               Optional ByVal esIncidenciaManual As Boolean = False)
        Const sql As String =
"INSERT INTO dbo.OperacionComedorEvento
(FechaEvento, Cedula, Estado, Motivo, TiempoAtencionMs, EsDuplicado, TieneAdvertencia, TieneError, EsIncidenciaManual)
VALUES
(@FechaEvento, @Cedula, @Estado, @Motivo, @TiempoAtencionMs, @EsDuplicado, @TieneAdvertencia, @TieneError, @EsIncidenciaManual);"

        Using cmd As New SqlCommand(sql, cn)
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
FROM dbo.OperacionComedorEvento
ORDER BY IdOperacionComedorEvento DESC;"

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
