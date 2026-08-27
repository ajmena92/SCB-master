Option Explicit On
Option Strict On

Imports System.Data
Imports System.Data.SqlClient

Public Class ComedorOperacionService

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
            cmd.Parameters.Add("@FechaEvento", SqlDbType.DateTime2).Value = fechaEvento
            cmd.Parameters.Add("@Cedula", SqlDbType.NVarChar, 50).Value = If(String.IsNullOrWhiteSpace(cedula), CType(DBNull.Value, Object), cedula.Trim())
            cmd.Parameters.Add("@Estado", SqlDbType.NVarChar, 40).Value = If(String.IsNullOrWhiteSpace(estado), "DESCONOCIDO", estado.Trim())
            cmd.Parameters.Add("@Motivo", SqlDbType.NVarChar, 300).Value = If(String.IsNullOrWhiteSpace(motivo), CType(DBNull.Value, Object), motivo.Trim())
            If tiempoAtencionMs.HasValue Then
                cmd.Parameters.Add("@TiempoAtencionMs", SqlDbType.Int).Value = tiempoAtencionMs.Value
            Else
                cmd.Parameters.Add("@TiempoAtencionMs", SqlDbType.Int).Value = DBNull.Value
            End If
            cmd.Parameters.Add("@EsDuplicado", SqlDbType.Bit).Value = esDuplicado
            cmd.Parameters.Add("@TieneAdvertencia", SqlDbType.Bit).Value = tieneAdvertencia
            cmd.Parameters.Add("@TieneError", SqlDbType.Bit).Value = tieneError
            cmd.Parameters.Add("@EsIncidenciaManual", SqlDbType.Bit).Value = esIncidenciaManual
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
            cmd.Parameters.Add("@Cantidad", SqlDbType.Int).Value = Math.Max(1, cantidad)
            Using da As New SqlDataAdapter(cmd)
                da.Fill(dt)
            End Using
        End Using
        Return dt
    End Function
End Class
