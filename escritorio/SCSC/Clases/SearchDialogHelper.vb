Option Strict On
Option Explicit On

Public NotInheritable Class SearchDialogHelper
    Private Sub New()
    End Sub

    Public Shared Function TrySelectSingleValue(ByVal owner As IWin32Window,
                                                ByVal request As SearchRequest,
                                                ByVal currentValue As String,
                                                ByRef selectedValue As String) As Boolean
        selectedValue = If(currentValue, String.Empty)

        If request Is Nothing Then
            Return False
        End If

        Using frm As New Global.SCSC.Busqueda()
            frm.Request = request
            Dim result As DialogResult = frm.ShowDialog(owner)
            If result <> DialogResult.OK OrElse frm.SelectedValues Is Nothing OrElse frm.SelectedValues.Length = 0 Then
                Return False
            End If

            Dim rawValue As String = frm.SelectedValues(0)
            If String.IsNullOrWhiteSpace(rawValue) Then
                Return False
            End If

            selectedValue = rawValue.Trim()
            Return True
        End Using
    End Function
End Class
