Option Strict On
Option Explicit On

Public NotInheritable Class SearchRequest
    Public Property Title As String
    Public Property TableName As String
    Public Property OrderBy As String
    Public Property ReturnFieldsCsv As String
    Public Property DefaultFilterField As String
    Public Property Values() As FuncionesDB.Campos
    Public Property Keys() As FuncionesDB.Campos
End Class
