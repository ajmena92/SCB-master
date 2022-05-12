Public Class LBItem
    ' Sirve para agregar objetos tipados con varios campos a un list box o combo box.
    Public Descripcion As String
    Public Valor As String
    Public Valor2 As Object

    Public Sub New(ByVal pValor As String, ByVal pDescripcion As String, Optional ByVal pValor2 As Object = Nothing)
        Descripcion = pDescripcion
        Valor = pValor
        If pValor2 Is Nothing Then
        Else
            Valor2 = pValor2
        End If

    End Sub

    Public Overrides Function ToString() As String
        Return Valor & " - " & Descripcion
    End Function

    Public Overloads Function ToString(ByVal pFormato As Short) As String
        Select Case pFormato
            Case 1
                Return Valor & " - " & Descripcion
            Case 2
                Return Valor2 & " - " & Descripcion
            Case 3
                Return Valor
            Case 4
                Return Descripcion
            Case 5
                Return Valor2
            Case 6
                Return Valor & " - " & Descripcion
            Case 7
                Return Descripcion & Valor & " - "
            Case Else
                Return Valor & " - " & Descripcion
        End Select

    End Function

End Class


