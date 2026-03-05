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

    Public Sub ApplyCrudModuleChrome(ByVal form As Form)
        If form Is Nothing Then
            Exit Sub
        End If

        form.BackColor = UIConstants.AppBackground
        form.ForeColor = UIConstants.TextPrimary
        form.Font = UIConstants.FontBody()
        ApplyCrudFieldVisuals(form)

        Dim header As Label = TryCast(FindControlRecursive(form, "Label1"), Label)
        If Not header Is Nothing Then
            header.Font = UIConstants.FontTitle()
            header.ForeColor = UIConstants.TextPrimary
            header.BackColor = Color.Transparent
        End If

        Dim actionPanel As Control = FindControlRecursive(form, "Panel4")
        If Not actionPanel Is Nothing Then
            actionPanel.BackColor = Color.Transparent
        End If

        StyleActionButton(form, "BtnGuardar", "Guardar", UIConstants.Accent)
        StyleActionButton(form, "BtnCancelar", "Cancelar", Color.FromArgb(100, 116, 139))
        StyleActionButton(form, "BtnEliminar", "Eliminar", UIConstants.Danger)
        StyleActionButton(form, "BtnRegresar", "Salir", Color.FromArgb(71, 85, 105))

        If Not actionPanel Is Nothing Then
            LayoutCrudActionPanel(form, actionPanel)
            AddHandler form.Resize,
                Sub(sender As Object, e As EventArgs)
                    LayoutCrudActionPanel(form, actionPanel)
                End Sub
        End If
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

    Private Sub StyleActionButton(ByVal root As Control, ByVal controlName As String, ByVal text As String, ByVal color As Color)
        Dim btn As Button = TryCast(FindControlRecursive(root, controlName), Button)
        If btn Is Nothing Then
            Exit Sub
        End If

        btn.BackgroundImage = Nothing
        btn.Text = text
        btn.AutoEllipsis = True
        btn.TextAlign = ContentAlignment.MiddleCenter
        btn.TextImageRelation = TextImageRelation.Overlay
        btn.FlatStyle = FlatStyle.Flat
        btn.FlatAppearance.BorderSize = 0
        btn.BackColor = color
        btn.ForeColor = Color.White
        btn.Font = New Font("Segoe UI", 8.25!, FontStyle.Bold, GraphicsUnit.Point)
        btn.Cursor = Cursors.Hand
    End Sub

    Private Sub ApplyCrudFieldVisuals(ByVal root As Control)
        For Each ctrl As Control In root.Controls
            If TypeOf ctrl Is GroupBox Then
                ctrl.BackColor = UIConstants.Surface
                ctrl.ForeColor = UIConstants.TextPrimary
                ctrl.Font = New Font("Segoe UI", 10.0!, FontStyle.Bold, GraphicsUnit.Point)
            ElseIf TypeOf ctrl Is Label Then
                Dim lbl As Label = DirectCast(ctrl, Label)
                If Not String.Equals(lbl.Name, "Label1", StringComparison.OrdinalIgnoreCase) Then
                    lbl.ForeColor = UIConstants.TextSecondary
                    lbl.Font = New Font("Segoe UI", 9.75!, FontStyle.Bold, GraphicsUnit.Point)
                End If
            ElseIf TypeOf ctrl Is TextBox Then
                Dim tb As TextBox = DirectCast(ctrl, TextBox)
                tb.BackColor = UIConstants.Surface
                tb.ForeColor = UIConstants.TextPrimary
                tb.BorderStyle = BorderStyle.FixedSingle
                tb.Font = New Font("Segoe UI", 10.5!, FontStyle.Regular, GraphicsUnit.Point)
            ElseIf TypeOf ctrl Is ComboBox Then
                Dim cb As ComboBox = DirectCast(ctrl, ComboBox)
                cb.BackColor = UIConstants.Surface
                cb.ForeColor = UIConstants.TextPrimary
                cb.FlatStyle = FlatStyle.Flat
                cb.Font = New Font("Segoe UI", 10.0!, FontStyle.Regular, GraphicsUnit.Point)
            ElseIf TypeOf ctrl Is CheckBox Then
                Dim chk As CheckBox = DirectCast(ctrl, CheckBox)
                chk.ForeColor = UIConstants.TextPrimary
                chk.Font = New Font("Segoe UI", 10.0!, FontStyle.Bold, GraphicsUnit.Point)
                chk.BackColor = Color.Transparent
            ElseIf TypeOf ctrl Is DateTimePicker Then
                Dim dt As DateTimePicker = DirectCast(ctrl, DateTimePicker)
                dt.CalendarMonthBackground = UIConstants.Surface
                dt.Font = New Font("Segoe UI", 10.0!, FontStyle.Regular, GraphicsUnit.Point)
            ElseIf TypeOf ctrl Is NumericUpDown Then
                Dim num As NumericUpDown = DirectCast(ctrl, NumericUpDown)
                num.BackColor = UIConstants.Surface
                num.ForeColor = UIConstants.TextPrimary
                num.Font = New Font("Segoe UI", 10.0!, FontStyle.Regular, GraphicsUnit.Point)
            End If

            If ctrl.HasChildren Then
                ApplyCrudFieldVisuals(ctrl)
            End If
        Next
    End Sub

    Private Sub LayoutCrudActionPanel(ByVal form As Form, ByVal actionPanel As Control)
        If actionPanel Is Nothing Then
            Exit Sub
        End If

        Dim buttons As New List(Of Button)()
        For Each ctrl As Control In actionPanel.Controls
            Dim btn As Button = TryCast(ctrl, Button)
            If Not btn Is Nothing AndAlso btn.Visible Then
                buttons.Add(btn)
            End If
        Next

        If buttons.Count = 0 Then
            Exit Sub
        End If

        buttons.Sort(Function(a, b) a.Left.CompareTo(b.Left))

        Dim gap As Integer = 8
        Dim panelPadding As Integer = 6
        Dim desiredButtonWidth As Integer = 88
        Dim desiredButtonHeight As Integer = 34
        Dim minButtonWidth As Integer = 62

        Dim requiredWidth As Integer = (panelPadding * 2) + (buttons.Count * desiredButtonWidth) + ((buttons.Count - 1) * gap)
        Dim availableWidth As Integer = Math.Max(120, actionPanel.Width - (panelPadding * 2) - ((buttons.Count - 1) * gap))
        Dim computedButtonWidth As Integer = Math.Max(minButtonWidth, availableWidth \ buttons.Count)

        If actionPanel.Width < requiredWidth AndAlso form.ClientSize.Width > requiredWidth + 24 Then
            actionPanel.Width = requiredWidth
            actionPanel.Left = Math.Max(8, form.ClientSize.Width - actionPanel.Width - 16)
        End If

        Dim top As Integer = Math.Max(4, (actionPanel.Height - desiredButtonHeight) \ 2)
        For i As Integer = 0 To buttons.Count - 1
            Dim left As Integer = panelPadding + (i * (computedButtonWidth + gap))
            buttons(i).SetBounds(left, top, computedButtonWidth, desiredButtonHeight)
        Next
    End Sub

    Private Function FindControlRecursive(ByVal root As Control, ByVal controlName As String) As Control
        If root Is Nothing OrElse String.IsNullOrWhiteSpace(controlName) Then
            Return Nothing
        End If

        For Each ctrl As Control In root.Controls
            If String.Equals(ctrl.Name, controlName, StringComparison.OrdinalIgnoreCase) Then
                Return ctrl
            End If
            Dim nested As Control = FindControlRecursive(ctrl, controlName)
            If Not nested Is Nothing Then
                Return nested
            End If
        Next

        Return Nothing
    End Function

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
