Imports System
Imports System.Windows.Forms

Public NotInheritable Class CrudOperationHelper
    Private Sub New()
    End Sub

    Public Shared Function ConfirmarEliminacion(ByVal entidad As String) As Boolean
        Dim nombreEntidad As String = If(String.IsNullOrWhiteSpace(entidad), "el registro", entidad)
        Dim pregunta As String = "Desea eliminar " & nombreEntidad & "?"
        Return MsgBox(pregunta, MsgBoxStyle.OkCancel Or MsgBoxStyle.Question, "Confirmar") = MsgBoxResult.Ok
    End Function

    Public Shared Sub EjecutarConManejo(ByVal contexto As String, ByVal accion As Action)
        If accion Is Nothing Then
            Exit Sub
        End If

        Try
            accion()
        Catch ex As Exception
            ErrorLogger.LogException(contexto, ex)
            Throw
        End Try
    End Sub

    Public Shared Function ParseEnteroSeguro(ByVal raw As Object, Optional ByVal defaultValue As Integer = 0) As Integer
        If raw Is Nothing OrElse IsDBNull(raw) Then
            Return defaultValue
        End If

        Dim parsed As Integer
        If Integer.TryParse(Convert.ToString(raw), parsed) Then
            Return parsed
        End If

        Return defaultValue
    End Function
End Class
