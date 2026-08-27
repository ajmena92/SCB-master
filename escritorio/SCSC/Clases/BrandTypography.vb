Option Strict On
Option Explicit On

Imports System.Drawing
Imports System.Drawing.Text
Imports System.IO

Public NotInheritable Class BrandTypography
    Private Shared ReadOnly FontCollection As New PrivateFontCollection()
    Private Shared _fontFamily As FontFamily
    Private Shared _initialized As Boolean

    Private Sub New()
    End Sub

    Public Shared Function CreateFont(ByVal size As Single, ByVal style As FontStyle) As Font
        EnsureLoaded()

        If _fontFamily Is Nothing Then
            Return New Font("Segoe UI", size, style, GraphicsUnit.Point)
        End If

        Dim safeStyle As FontStyle = If(_fontFamily.IsStyleAvailable(style), style, FontStyle.Regular)
        Return New Font(_fontFamily, size, safeStyle, GraphicsUnit.Point)
    End Function

    Private Shared Sub EnsureLoaded()
        If _initialized Then
            Exit Sub
        End If

        _initialized = True
        Try
            Dim path As String = ResolveResourcePath(Path.Combine("Fonts", "Montserrat-Variable.ttf"))
            If Not File.Exists(path) Then
                Exit Sub
            End If

            FontCollection.AddFontFile(path)
            If FontCollection.Families.Length > 0 Then
                _fontFamily = FontCollection.Families(0)
            End If
        Catch
            _fontFamily = Nothing
        End Try
    End Sub
End Class
