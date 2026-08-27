Imports System.Runtime.InteropServices
Imports System.Reflection
Imports System.Windows.Forms

Public Module UIThemeManager
    Private ReadOnly AccentColor As Color = Color.FromArgb(0, 120, 212)
    Private ReadOnly SurfaceColor As Color = Color.FromArgb(248, 249, 251)
    Private ReadOnly TextPrimary As Color = Color.FromArgb(32, 32, 32)

    Public Sub ApplyToForm(ByVal form As Form)
        If form Is Nothing Then
            Exit Sub
        End If

        TryEnableRoundedCorners(form)

        form.SuspendLayout()
        Try
            form.BackColor = SurfaceColor
            form.ForeColor = TextPrimary
            form.Font = GetWindows11Font()
            EnableDoubleBuffer(form)
            ApplyToControlTree(form)
        Finally
            form.ResumeLayout(True)
        End Try
    End Sub

    Private Sub ApplyToControlTree(ByVal parent As Control)
        For Each ctrl As Control In parent.Controls
            If IsBunifuControl(ctrl) Then
                Continue For
            End If

            ctrl.Font = parent.Font
            ctrl.ForeColor = TextPrimary

            If TypeOf ctrl Is Button Then
                Dim btn As Button = DirectCast(ctrl, Button)
                btn.FlatStyle = FlatStyle.Flat
                btn.FlatAppearance.BorderColor = Color.FromArgb(220, 220, 220)
                btn.FlatAppearance.BorderSize = 1
                btn.BackColor = Color.White
                btn.ForeColor = TextPrimary
            ElseIf TypeOf ctrl Is TextBox Then
                Dim tb As TextBox = DirectCast(ctrl, TextBox)
                tb.BackColor = Color.White
                tb.BorderStyle = BorderStyle.FixedSingle
            ElseIf TypeOf ctrl Is ComboBox Then
                Dim cb As ComboBox = DirectCast(ctrl, ComboBox)
                cb.BackColor = Color.White
                cb.FlatStyle = FlatStyle.Flat
            ElseIf TypeOf ctrl Is ListView Then
                Dim lv As ListView = DirectCast(ctrl, ListView)
                lv.BackColor = Color.White
            ElseIf TypeOf ctrl Is DataGridView Then
                Dim dgv As DataGridView = DirectCast(ctrl, DataGridView)
                ApplyGridTheme(dgv)
            ElseIf TypeOf ctrl Is Panel OrElse TypeOf ctrl Is GroupBox OrElse TypeOf ctrl Is TabPage Then
                ctrl.BackColor = SurfaceColor
            ElseIf TypeOf ctrl Is MenuStrip Then
                Dim menu As MenuStrip = DirectCast(ctrl, MenuStrip)
                menu.RenderMode = ToolStripRenderMode.Professional
                menu.Renderer = New ToolStripProfessionalRenderer(New ModernMenuColorTable())
            End If

            If ctrl.HasChildren Then
                ApplyToControlTree(ctrl)
            End If
        Next
    End Sub

    Private Sub ApplyGridTheme(ByVal dgv As DataGridView)
        dgv.BackgroundColor = Color.White
        dgv.BorderStyle = BorderStyle.None
        dgv.GridColor = Color.FromArgb(235, 235, 235)
        dgv.EnableHeadersVisualStyles = False
        dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(242, 242, 242)
        dgv.ColumnHeadersDefaultCellStyle.ForeColor = TextPrimary
        dgv.DefaultCellStyle.BackColor = Color.White
        dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(230, 241, 252)
        dgv.DefaultCellStyle.SelectionForeColor = TextPrimary
        dgv.RowHeadersVisible = False
        EnableDoubleBuffer(dgv)
    End Sub

    Private Function IsBunifuControl(ByVal ctrl As Control) As Boolean
        Dim fullName As String = ctrl.GetType().FullName
        If fullName.IndexOf("Bunifu", StringComparison.OrdinalIgnoreCase) < 0 Then
            Return False
        End If

        Dim asmName As String = ctrl.GetType().Assembly.GetName().Name
        Return asmName.IndexOf("Bunifu", StringComparison.OrdinalIgnoreCase) >= 0
    End Function

    Private Function GetWindows11Font() As Font
        Dim candidateFonts() As String = {"Segoe UI Variable Text", "Segoe UI", "Microsoft Sans Serif"}
        For Each fontName As String In candidateFonts
            If IsFontInstalled(fontName) Then
                Return New Font(fontName, 9.0F, FontStyle.Regular, GraphicsUnit.Point)
            End If
        Next
        Return SystemFonts.MessageBoxFont
    End Function

    Private Function IsFontInstalled(ByVal fontName As String) As Boolean
        For Each family As FontFamily In FontFamily.Families
            If String.Equals(family.Name, fontName, StringComparison.OrdinalIgnoreCase) Then
                Return True
            End If
        Next
        Return False
    End Function

    Private Sub EnableDoubleBuffer(ByVal control As Control)
        Dim prop As PropertyInfo = control.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance Or BindingFlags.NonPublic)
        If Not prop Is Nothing Then
            prop.SetValue(control, True, Nothing)
        End If
    End Sub

    Private Sub TryEnableRoundedCorners(ByVal form As Form)
        Try
            If Environment.OSVersion.Platform <> PlatformID.Win32NT Then
                Exit Sub
            End If
            Dim attr As Integer = 33 ' DWMWA_WINDOW_CORNER_PREFERENCE
            Dim pref As Integer = 2  ' DWMWCP_ROUND
            DwmSetWindowAttribute(form.Handle, attr, pref, Marshal.SizeOf(pref))
        Catch
            ' Ignorar, no afecta funcionalidad en sistemas no compatibles.
        End Try
    End Sub

    <DllImport("dwmapi.dll", PreserveSig:=True)>
    Private Function DwmSetWindowAttribute(ByVal hwnd As IntPtr, ByVal dwAttribute As Integer, ByRef pvAttribute As Integer, ByVal cbAttribute As Integer) As Integer
    End Function

    Private Class ModernMenuColorTable
        Inherits ProfessionalColorTable

        Public Overrides ReadOnly Property MenuItemSelected() As Color
            Get
                Return Color.FromArgb(230, 241, 252)
            End Get
        End Property

        Public Overrides ReadOnly Property MenuItemBorder() As Color
            Get
                Return AccentColor
            End Get
        End Property
    End Class
End Module
