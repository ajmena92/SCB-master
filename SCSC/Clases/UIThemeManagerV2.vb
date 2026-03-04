Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Windows.Forms

Public Module UIThemeManagerV2
    Public Sub Apply(ByVal form As Form, Optional ByVal role As String = "dialogo")
        If form Is Nothing Then
            Exit Sub
        End If

        Dim normalizedRole As String = NormalizeRole(role)
        TryEnableRoundedCorners(form)
        form.SuspendLayout()
        Try
            form.BackColor = UIConstants.AppBackground
            form.ForeColor = UIConstants.TextPrimary
            If normalizedRole <> "login" Then
                form.Font = UIConstants.FontBody()
            End If
            ApplyByRole(form, normalizedRole)
            If normalizedRole = "login" Then
                ApplyLoginVisualSafe(form)
            Else
                ApplyControlTree(form)
            End If
            EnableDoubleBuffer(form)
        Finally
            form.ResumeLayout(True)
        End Try
    End Sub

    Private Function NormalizeRole(ByVal role As String) As String
        If String.IsNullOrWhiteSpace(role) Then
            Return "dialogo"
        End If
        Return role.Trim().ToLowerInvariant()
    End Function

    Private Sub ApplyByRole(ByVal form As Form, ByVal normalizedRole As String)
        Select Case normalizedRole
            Case "login"
                ' Designer-first for login: preserve form background defined in designer.
            Case "shell"
                form.BackColor = UIConstants.AppBackground
            Case "operativo"
                form.BackColor = UIConstants.AppBackground
            Case "reporte"
                form.BackColor = UIConstants.Surface
            Case Else
                form.BackColor = UIConstants.AppBackground
            End Select
    End Sub

    ' Login is designer-first: do not alter geometry, typography, borders, or control styles.
    Private Sub ApplyLoginVisualSafe(ByVal form As Form)
        For Each ctrl As Control In form.Controls
            If IsExternalBunifu(ctrl) Then
                Continue For
            End If

            If TypeOf ctrl Is MenuStrip Then
                Dim menu As MenuStrip = DirectCast(ctrl, MenuStrip)
                menu.BackColor = UIConstants.Surface
                menu.RenderMode = ToolStripRenderMode.Professional
                menu.Renderer = New ToolStripProfessionalRenderer(New ModernMenuColorTable())
            ElseIf TypeOf ctrl Is DataGridView Then
                ApplyGridTheme(DirectCast(ctrl, DataGridView))
            Else
                If ctrl.ForeColor = SystemColors.ControlText Then
                    ctrl.ForeColor = UIConstants.TextPrimary
                End If
            End If

            If ctrl.HasChildren Then
                ApplyLoginVisualSafeChildren(ctrl)
            End If
        Next
    End Sub

    Private Sub ApplyLoginVisualSafeChildren(ByVal parent As Control)
        For Each ctrl As Control In parent.Controls
            If IsExternalBunifu(ctrl) Then
                Continue For
            End If

            If TypeOf ctrl Is MenuStrip Then
                Dim menu As MenuStrip = DirectCast(ctrl, MenuStrip)
                menu.BackColor = UIConstants.Surface
                menu.RenderMode = ToolStripRenderMode.Professional
                menu.Renderer = New ToolStripProfessionalRenderer(New ModernMenuColorTable())
            ElseIf TypeOf ctrl Is DataGridView Then
                ApplyGridTheme(DirectCast(ctrl, DataGridView))
            Else
                If ctrl.ForeColor = SystemColors.ControlText Then
                    ctrl.ForeColor = UIConstants.TextPrimary
                End If
            End If

            If ctrl.HasChildren Then
                ApplyLoginVisualSafeChildren(ctrl)
            End If
        Next
    End Sub

    Public Sub ApplyControlTree(ByVal parent As Control)
        For Each ctrl As Control In parent.Controls
            If IsExternalBunifu(ctrl) Then
                Continue For
            End If

            ctrl.Font = parent.Font
            ctrl.ForeColor = UIConstants.TextPrimary

            If TypeOf ctrl Is Button Then
                Dim btn As Button = DirectCast(ctrl, Button)
                If btn.FlatStyle = FlatStyle.Standard Then
                    btn.FlatStyle = FlatStyle.Flat
                    btn.FlatAppearance.BorderColor = UIConstants.Border
                    btn.FlatAppearance.BorderSize = 1
                End If
                If btn.BackColor = Color.Transparent Then
                    btn.BackColor = UIConstants.Surface
                End If
            ElseIf TypeOf ctrl Is TextBox Then
                Dim tb As TextBox = DirectCast(ctrl, TextBox)
                tb.BackColor = UIConstants.Surface
                tb.BorderStyle = BorderStyle.FixedSingle
            ElseIf TypeOf ctrl Is ComboBox Then
                Dim cb As ComboBox = DirectCast(ctrl, ComboBox)
                cb.BackColor = UIConstants.Surface
                cb.FlatStyle = FlatStyle.Flat
            ElseIf TypeOf ctrl Is GroupBox Then
                ctrl.BackColor = UIConstants.Surface
            ElseIf TypeOf ctrl Is Panel Then
                If ctrl.BackColor = Color.Transparent Then
                    ctrl.BackColor = UIConstants.AppBackground
                End If
            ElseIf TypeOf ctrl Is DataGridView Then
                ApplyGridTheme(DirectCast(ctrl, DataGridView))
            ElseIf TypeOf ctrl Is MenuStrip Then
                Dim menu As MenuStrip = DirectCast(ctrl, MenuStrip)
                menu.BackColor = UIConstants.Surface
                menu.RenderMode = ToolStripRenderMode.Professional
                menu.Renderer = New ToolStripProfessionalRenderer(New ModernMenuColorTable())
            End If

            If ctrl.HasChildren Then
                ApplyControlTree(ctrl)
            End If
        Next
    End Sub

    Private Sub ApplyGridTheme(ByVal dgv As DataGridView)
        dgv.BackgroundColor = UIConstants.Surface
        dgv.BorderStyle = BorderStyle.None
        dgv.GridColor = UIConstants.Border
        dgv.EnableHeadersVisualStyles = False
        dgv.ColumnHeadersDefaultCellStyle.BackColor = UIConstants.SurfaceAlt
        dgv.ColumnHeadersDefaultCellStyle.ForeColor = UIConstants.TextPrimary
        dgv.DefaultCellStyle.BackColor = UIConstants.Surface
        dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(227, 238, 253)
        dgv.DefaultCellStyle.SelectionForeColor = UIConstants.TextPrimary
        dgv.RowHeadersVisible = False
        EnableDoubleBuffer(dgv)
    End Sub

    Private Function IsExternalBunifu(ByVal ctrl As Control) As Boolean
        Dim fullName As String = ctrl.GetType().FullName
        If String.IsNullOrEmpty(fullName) Then
            Return False
        End If

        If fullName.IndexOf("Bunifu", StringComparison.OrdinalIgnoreCase) < 0 Then
            Return False
        End If

        Dim asmName As String = ctrl.GetType().Assembly.GetName().Name
        Return asmName.IndexOf("Bunifu", StringComparison.OrdinalIgnoreCase) >= 0
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
            Dim attr As Integer = 33
            Dim pref As Integer = 2
            DwmSetWindowAttribute(form.Handle, attr, pref, Marshal.SizeOf(pref))
        Catch
        End Try
    End Sub

    <DllImport("dwmapi.dll", PreserveSig:=True)>
    Private Function DwmSetWindowAttribute(ByVal hwnd As IntPtr, ByVal dwAttribute As Integer, ByRef pvAttribute As Integer, ByVal cbAttribute As Integer) As Integer
    End Function

    Private Class ModernMenuColorTable
        Inherits ProfessionalColorTable

        Public Overrides ReadOnly Property MenuItemSelected() As Color
            Get
                Return Color.FromArgb(232, 242, 255)
            End Get
        End Property

        Public Overrides ReadOnly Property MenuItemBorder() As Color
            Get
                Return UIConstants.Accent
            End Get
        End Property
    End Class
End Module
