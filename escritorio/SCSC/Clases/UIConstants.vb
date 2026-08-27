Imports System.Drawing

Public NotInheritable Class UIConstants
    Private Sub New()
    End Sub

    Public Shared ReadOnly AppBackground As Color = Color.FromArgb(246, 248, 251)
    Public Shared ReadOnly Surface As Color = Color.White
    Public Shared ReadOnly SurfaceAlt As Color = Color.FromArgb(240, 244, 248)
    Public Shared ReadOnly TextPrimary As Color = Color.FromArgb(17, 24, 39)
    Public Shared ReadOnly TextSecondary As Color = Color.FromArgb(107, 114, 128)
    Public Shared ReadOnly Border As Color = Color.FromArgb(229, 231, 235)
    Public Shared ReadOnly Accent As Color = Color.FromArgb(10, 132, 255)
    Public Shared ReadOnly AccentHover As Color = Color.FromArgb(0, 113, 227)
    Public Shared ReadOnly Sidebar As Color = Color.FromArgb(15, 23, 42)
    Public Shared ReadOnly SidebarHover As Color = Color.FromArgb(30, 41, 59)

    Public Shared ReadOnly Success As Color = Color.FromArgb(22, 163, 74)
    Public Shared ReadOnly Warning As Color = Color.FromArgb(217, 119, 6)
    Public Shared ReadOnly Danger As Color = Color.FromArgb(220, 38, 38)

    Public Shared ReadOnly RadiusStandard As Integer = 8
    Public Shared ReadOnly RadiusCard As Integer = 12

    Public Shared ReadOnly SpaceXs As Integer = 6
    Public Shared ReadOnly SpaceSm As Integer = 10
    Public Shared ReadOnly SpaceMd As Integer = 16
    Public Shared ReadOnly SpaceLg As Integer = 24

    Public Shared Function FontBody() As Font
        Return New Font("Segoe UI", 10.0!, FontStyle.Regular, GraphicsUnit.Point)
    End Function

    Public Shared Function FontBodyStrong() As Font
        Return New Font("Segoe UI", 10.0!, FontStyle.Bold, GraphicsUnit.Point)
    End Function

    Public Shared Function FontSubtitle() As Font
        Return New Font("Segoe UI", 12.0!, FontStyle.Bold, GraphicsUnit.Point)
    End Function

    Public Shared Function FontTitle() As Font
        Return New Font("Segoe UI", 18.0!, FontStyle.Bold, GraphicsUnit.Point)
    End Function
End Class
