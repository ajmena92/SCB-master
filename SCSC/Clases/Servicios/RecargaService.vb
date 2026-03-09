Option Strict On
Option Explicit On

Imports System.Data
Imports System.Data.SqlClient

Public Class RecargaService
    Public NotInheritable Class UsuarioRecargaInfo
        Public Property IdUsuario As Integer
        Public Property Nombre As String
        Public Property PrimerApellido As String
        Public Property SegundoApellido As String
        Public Property CantidadTiquetes As Integer
        Public Property CodTipo As Integer
        Public Property TipoBeca As Integer
        Public Property Activo As Boolean
    End Class

    Public Function CargarBecas(ByVal cn As SqlConnection) As DataTable
        Const sql As String = "SELECT IdBeca, DiasBeca, Descripcion FROM TipoBeca ORDER BY IdBeca;"
        Using cmd As New SqlCommand(sql, cn), da As New SqlDataAdapter(cmd)
            Dim table As New DataTable("Becas")
            da.Fill(table)
            Return table
        End Using
    End Function

    Public Function ObtenerUsuarioPorCedula(ByVal cn As SqlConnection, ByVal cedula As String) As UsuarioRecargaInfo
        Const sql As String = "SELECT TOP 1 IdUsuario, Nombre, PrimerApellido, SegundoApellido, CantidadTiquetes, CodTipo, TipoBeca, Activo FROM Usuario WHERE Cedula = @Cedula;"
        Using cmd As New SqlCommand(sql, cn)
            cmd.Parameters.Add("@Cedula", SqlDbType.NVarChar, 50).Value = cedula.Trim()
            Using rd As SqlDataReader = cmd.ExecuteReader()
                If Not rd.Read() Then
                    Return Nothing
                End If

                Dim usuario As New UsuarioRecargaInfo()
                usuario.IdUsuario = CInt(rd("IdUsuario"))
                usuario.Nombre = Convert.ToString(rd("Nombre"))
                usuario.PrimerApellido = Convert.ToString(rd("PrimerApellido"))
                usuario.SegundoApellido = Convert.ToString(rd("SegundoApellido"))
                usuario.CantidadTiquetes = CInt(rd("CantidadTiquetes"))
                usuario.CodTipo = CInt(rd("CodTipo"))
                usuario.TipoBeca = CInt(rd("TipoBeca"))
                usuario.Activo = CBool(rd("Activo"))
                Return usuario
            End Using
        End Using
    End Function

    Public Function AplicarRecarga(ByVal cn As SqlConnection,
                                   ByVal idUsuario As Integer,
                                   ByVal precio As Decimal,
                                   ByVal tipoUsuarioCod As Integer,
                                   ByVal cantidad As Integer) As Integer
        Using tx As SqlTransaction = cn.BeginTransaction()
            Try
                Using cmdUpdate As New SqlCommand("UPDATE Usuario SET CantidadTiquetes = CantidadTiquetes + @Cantidad WHERE IdUsuario = @IdUsuario;", cn, tx)
                    cmdUpdate.Parameters.Add("@Cantidad", SqlDbType.Int).Value = cantidad
                    cmdUpdate.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario
                    cmdUpdate.ExecuteNonQuery()
                End Using

                Using cmdInsert As New SqlCommand("INSERT INTO RegistroComedor (IdUsuario, TipoPago, Beca, Precio, TipoUsuario, Cantidad) VALUES (@IdUsuario, @TipoPago, @Beca, @Precio, @TipoUsuario, @Cantidad);", cn, tx)
                    cmdInsert.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario
                    cmdInsert.Parameters.Add("@TipoPago", SqlDbType.Int).Value = 1
                    cmdInsert.Parameters.Add("@Beca", SqlDbType.Int).Value = 9
                    cmdInsert.Parameters.Add("@Precio", SqlDbType.Decimal).Value = precio
                    cmdInsert.Parameters.Add("@TipoUsuario", SqlDbType.Int).Value = tipoUsuarioCod
                    cmdInsert.Parameters.Add("@Cantidad", SqlDbType.Int).Value = cantidad
                    cmdInsert.ExecuteNonQuery()
                End Using

                Using cmdSelect As New SqlCommand("SELECT CantidadTiquetes FROM Usuario WHERE IdUsuario = @IdUsuario;", cn, tx)
                    cmdSelect.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario
                    Dim result As Object = cmdSelect.ExecuteScalar()
                    Dim cantidadActualizada As Integer = If(result Is Nothing OrElse result Is DBNull.Value, 0, CInt(result))
                    tx.Commit()
                    Return cantidadActualizada
                End Using
            Catch
                Try
                    tx.Rollback()
                Catch
                End Try
                Throw
            End Try
        End Using
    End Function
End Class
