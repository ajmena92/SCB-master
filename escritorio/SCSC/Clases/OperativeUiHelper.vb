Imports System.Drawing
Imports System.Windows.Forms

Public NotInheritable Class OperativeUiHelper
    Public Enum LayoutBand
        Narrow1366 = 0
        Compact = 1
        Standard = 2
        Wide = 3
    End Enum

    Private Sub New()
    End Sub

    Public Shared Function ResolveLayoutBand(ByVal clientSize As Size) As LayoutBand
        If clientSize.Width <= 1366 OrElse clientSize.Height <= 768 Then
            Return LayoutBand.Narrow1366
        End If

        If clientSize.Width < 1600 OrElse clientSize.Height < 900 Then
            Return LayoutBand.Compact
        End If

        If clientSize.Width >= 1900 AndAlso clientSize.Height >= 1000 Then
            Return LayoutBand.Wide
        End If

        Return LayoutBand.Standard
    End Function

    Public Shared Sub ApplyReadOnlyDisplayField(ByVal tb As TextBox, ByVal backColor As Color, ByVal foreColor As Color)
        If tb Is Nothing Then
            Exit Sub
        End If

        tb.ReadOnly = True
        tb.TabStop = False
        tb.ShortcutsEnabled = False
        tb.BackColor = backColor
        tb.ForeColor = foreColor
        tb.BorderStyle = BorderStyle.FixedSingle
        tb.Cursor = Cursors.Default
    End Sub

    Public Shared Sub DisableFocus(ParamArray controls() As Control)
        If controls Is Nothing Then
            Exit Sub
        End If

        For Each ctrl As Control In controls
            If ctrl Is Nothing Then
                Continue For
            End If

            ctrl.TabStop = False
        Next
    End Sub

    Public Shared Sub RecoverScannerFocusAfterClick(ByVal owner As Control,
                                                    ByVal sender As Object,
                                                    ByVal scanner As TextBox,
                                                    ByVal manualEntryVisible As Boolean,
                                                    ByVal ensureFocus As Action(Of Boolean))
        If manualEntryVisible OrElse owner Is Nothing OrElse scanner Is Nothing OrElse ensureFocus Is Nothing Then
            Exit Sub
        End If

        If Object.ReferenceEquals(sender, scanner) Then
            Exit Sub
        End If

        owner.BeginInvoke(New Action(Sub() ensureFocus(False)))
    End Sub

    Public Shared Function ConvertToBoolean(ByVal raw As Object) As Boolean
        If raw Is Nothing OrElse IsDBNull(raw) Then
            Return False
        End If

        If TypeOf raw Is Boolean Then
            Return CBool(raw)
        End If

        If IsNumeric(raw) Then
            Return CInt(raw) <> 0
        End If

        Dim texto As String = CStr(raw).Trim()
        Dim parsed As Boolean
        If Boolean.TryParse(texto, parsed) Then
            Return parsed
        End If

        Return texto = "1"
    End Function
End Class
