Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Drawing
Imports System.Linq
Imports System.Windows.Forms

Public NotInheritable Class CrudVisualHelper
    Private Sub New()
    End Sub

    Public Shared Sub ApplyCrudStandard(ByVal form As Form, Optional ByVal role As String = "dialogo")
        If form Is Nothing Then
            Exit Sub
        End If

        UIThemeManagerV2.Apply(form, role)
        UIThemeManagerV2.ApplyCrudModuleChrome(form)

        form.BackColor = UIConstants.AppBackground
        form.BackgroundImage = Nothing
        form.Font = UIConstants.FontBody()

        ApplySurface(form)
        StyleButtons(form)
        StyleDataGrids(form)
        NormalizeActionPanels(form)
        EnsurePrimaryActionsVisible(form)
    End Sub

    Public Shared Function IsInDesignMode(ByVal control As Control) As Boolean
        If control Is Nothing Then
            Return False
        End If

        If LicenseManager.UsageMode = LicenseUsageMode.Designtime Then
            Return True
        End If

        If control.Site IsNot Nothing AndAlso control.Site.DesignMode Then
            Return True
        End If

        Dim parentControl As Control = control.Parent
        While parentControl IsNot Nothing
            If parentControl.Site IsNot Nothing AndAlso parentControl.Site.DesignMode Then
                Return True
            End If
            parentControl = parentControl.Parent
        End While

        Return False
    End Function

    Public Shared Sub ApplyReportStandard(ByVal form As Form)
        If form Is Nothing Then
            Exit Sub
        End If

        UIThemeManagerV2.Apply(form, "reporte")
        form.BackColor = UIConstants.AppBackground
        form.BackgroundImage = Nothing
        form.Font = UIConstants.FontBody()

        ApplySurface(form)
        StyleButtons(form)
        StyleDataGrids(form)
    End Sub

    Public Shared Sub StyleDataGrid(ByVal grid As DataGridView)
        If grid Is Nothing Then
            Exit Sub
        End If

        grid.BackgroundColor = UIConstants.Surface
        grid.BorderStyle = BorderStyle.None
        grid.GridColor = UIConstants.Border
        grid.EnableHeadersVisualStyles = False
        grid.ColumnHeadersDefaultCellStyle.BackColor = UIConstants.SurfaceAlt
        grid.ColumnHeadersDefaultCellStyle.ForeColor = UIConstants.TextPrimary
        grid.ColumnHeadersDefaultCellStyle.Font = UIConstants.FontBodyStrong()
        grid.ColumnHeadersVisible = True
        grid.ColumnHeadersHeight = 34
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        grid.DefaultCellStyle.BackColor = UIConstants.Surface
        grid.DefaultCellStyle.ForeColor = UIConstants.TextPrimary
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(227, 238, 253)
        grid.DefaultCellStyle.SelectionForeColor = UIConstants.TextPrimary
        grid.RowHeadersVisible = False
        grid.RowTemplate.Height = 32
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
    End Sub

    Private Shared Sub ApplySurface(ByVal root As Control)
        For Each ctrl As Control In root.Controls
            ctrl.BackgroundImage = Nothing

            If TypeOf ctrl Is Panel OrElse TypeOf ctrl Is GroupBox Then
                ctrl.BackColor = UIConstants.Surface
            End If

            If TypeOf ctrl Is TextBox Then
                Dim tb As TextBox = DirectCast(ctrl, TextBox)
                tb.BackColor = UIConstants.Surface
                tb.ForeColor = UIConstants.TextPrimary
            ElseIf TypeOf ctrl Is ComboBox Then
                Dim cb As ComboBox = DirectCast(ctrl, ComboBox)
                cb.BackColor = UIConstants.Surface
                cb.ForeColor = UIConstants.TextPrimary
            End If

            If ctrl.HasChildren Then
                ApplySurface(ctrl)
            End If
        Next
    End Sub

    Private Shared Sub StyleButtons(ByVal root As Control)
        For Each ctrl As Control In root.Controls
            If TypeOf ctrl Is Button Then
                Dim btn As Button = DirectCast(ctrl, Button)
                If IsPrimaryActionButtonName(btn.Name) Then
                    ApplyActionButtonStyle(btn, ResolveActionText(btn.Name))
                ElseIf btn.BackgroundImage Is Nothing Then
                    btn.FlatStyle = FlatStyle.Flat
                    btn.FlatAppearance.BorderSize = 1
                    btn.FlatAppearance.BorderColor = UIConstants.Border
                    btn.BackColor = UIConstants.Surface
                    btn.ForeColor = UIConstants.TextPrimary
                    btn.Font = UIConstants.FontBodyStrong()
                End If
            End If

            If ctrl.HasChildren Then
                StyleButtons(ctrl)
            End If
        Next
    End Sub

    Private Shared Sub StyleDataGrids(ByVal root As Control)
        For Each ctrl As Control In root.Controls
            If TypeOf ctrl Is DataGridView Then
                StyleDataGrid(DirectCast(ctrl, DataGridView))
            End If

            If ctrl.HasChildren Then
                StyleDataGrids(ctrl)
            End If
        Next
    End Sub

    Private Shared Sub NormalizeActionPanels(ByVal root As Control)
        For Each ctrl As Control In root.Controls
            If TypeOf ctrl Is Panel Then
                Dim panel As Panel = DirectCast(ctrl, Panel)
                Dim actionButtons As List(Of Button) = panel.Controls _
                    .OfType(Of Button)() _
                    .Where(Function(b) IsPrimaryActionButtonName(b.Name)) _
                    .OrderBy(Function(b) ResolveActionOrder(b.Name)) _
                    .ToList()

                If actionButtons.Count >= 2 Then
                    LayoutActionPanel(panel, actionButtons)
                End If
            End If

            If ctrl.HasChildren Then
                NormalizeActionPanels(ctrl)
            End If
        Next
    End Sub

    Private Shared Sub LayoutActionPanel(ByVal panel As Panel, ByVal buttons As List(Of Button))
        If panel Is Nothing OrElse buttons Is Nothing OrElse buttons.Count = 0 Then
            Exit Sub
        End If

        Dim owner As Form = panel.FindForm()
        Dim count As Integer = buttons.Count
        Dim gap As Integer = 8
        Dim paddingHorizontal As Integer = 8
        Dim buttonHeight As Integer = 38
        Dim panelHeight As Integer = 56
        Dim preferredButtonWidth As Integer = If(count <= 3, 152, 126)
        Dim minButtonWidth As Integer = 92

        Dim maxPanelWidth As Integer = Integer.MaxValue
        If owner IsNot Nothing Then
            maxPanelWidth = Math.Max(280, owner.ClientSize.Width - 24)
        End If

        Dim buttonWidth As Integer = preferredButtonWidth
        Dim requiredWidth As Integer = (paddingHorizontal * 2) + (count * buttonWidth) + ((count - 1) * gap)
        If requiredWidth > maxPanelWidth Then
            buttonWidth = Math.Max(minButtonWidth, CInt(Math.Floor((maxPanelWidth - (paddingHorizontal * 2) - ((count - 1) * gap)) / CDbl(count))))
            requiredWidth = (paddingHorizontal * 2) + (count * buttonWidth) + ((count - 1) * gap)
        End If

        panel.SuspendLayout()
        Try
            panel.BackgroundImage = Nothing
            panel.BorderStyle = BorderStyle.None
            panel.BackColor = UIConstants.Surface
            panel.Height = panelHeight
            panel.Width = requiredWidth

            If owner IsNot Nothing Then
                panel.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
                panel.Left = Math.Max(8, owner.ClientSize.Width - panel.Width - 12)
                panel.Top = Math.Max(8, owner.ClientSize.Height - panel.Height - 12)
            End If

            Dim left As Integer = paddingHorizontal
            For Each btn As Button In buttons
                ApplyActionButtonStyle(btn, ResolveActionText(btn.Name))
                btn.SetBounds(left, 9, buttonWidth, buttonHeight)
                btn.Anchor = AnchorStyles.Top Or AnchorStyles.Left
                btn.Visible = True
                btn.BringToFront()
                left += buttonWidth + gap
            Next
            panel.BringToFront()
        Finally
            panel.ResumeLayout()
        End Try
    End Sub

    Private Shared Function IsPrimaryActionButtonName(ByVal name As String) As Boolean
        Select Case name
            Case "BtnGuardar", "BtnCancelar", "BtnEliminar", "BtnRegresar", "BtnCerrar", "BtnCrear", "BtnRecargar"
                Return True
            Case Else
                Return False
        End Select
    End Function

    Private Shared Function ResolveActionText(ByVal name As String) As String
        Select Case name
            Case "BtnGuardar"
                Return "Guardar"
            Case "BtnCancelar"
                Return "Limpiar"
            Case "BtnEliminar"
                Return "Eliminar"
            Case "BtnRegresar"
                Return "Salir"
            Case "BtnCerrar"
                Return "Cerrar"
            Case "BtnCrear"
                Return "Crear"
            Case "BtnRecargar"
                Return "Recargar"
            Case Else
                Return String.Empty
        End Select
    End Function

    Private Shared Function ResolveActionOrder(ByVal name As String) As Integer
        Select Case name
            Case "BtnGuardar"
                Return 1
            Case "BtnCrear"
                Return 2
            Case "BtnRecargar"
                Return 3
            Case "BtnCancelar"
                Return 4
            Case "BtnEliminar"
                Return 5
            Case "BtnRegresar"
                Return 6
            Case "BtnCerrar"
                Return 7
            Case Else
                Return 99
        End Select
    End Function

    Private Shared Sub ApplyActionButtonStyle(ByVal btn As Button, ByVal fallbackText As String)
        If btn Is Nothing Then
            Exit Sub
        End If

        If Not String.IsNullOrWhiteSpace(fallbackText) Then
            btn.Text = fallbackText
        End If

        btn.BackgroundImage = Nothing
        btn.Image = Nothing
        btn.AutoSize = False
        btn.AutoEllipsis = False
        btn.TextImageRelation = TextImageRelation.Overlay
        btn.ImageAlign = ContentAlignment.MiddleCenter
        btn.TextAlign = ContentAlignment.MiddleCenter
        btn.Padding = New Padding(10, 0, 10, 0)
        btn.FlatStyle = FlatStyle.Flat
        btn.FlatAppearance.BorderSize = 1
        btn.Font = UIConstants.FontBodyStrong()

        If String.Equals(btn.Name, "BtnEliminar", StringComparison.OrdinalIgnoreCase) Then
            btn.BackColor = Color.FromArgb(194, 74, 88)
            btn.ForeColor = Color.White
            btn.FlatAppearance.BorderColor = Color.FromArgb(181, 64, 77)
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(181, 64, 77)
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(161, 55, 68)
        ElseIf String.Equals(btn.Name, "BtnRegresar", StringComparison.OrdinalIgnoreCase) OrElse
               String.Equals(btn.Name, "BtnCerrar", StringComparison.OrdinalIgnoreCase) Then
            btn.BackColor = Color.FromArgb(114, 44, 61)
            btn.ForeColor = Color.White
            btn.FlatAppearance.BorderColor = Color.FromArgb(95, 36, 50)
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(133, 54, 72)
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(95, 36, 50)
        ElseIf String.Equals(btn.Name, "BtnCancelar", StringComparison.OrdinalIgnoreCase) Then
            btn.BackColor = Color.FromArgb(82, 97, 120)
            btn.ForeColor = Color.White
            btn.FlatAppearance.BorderColor = Color.FromArgb(75, 89, 109)
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(71, 84, 104)
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(61, 73, 93)
        Else
            btn.BackColor = Color.FromArgb(36, 112, 191)
            btn.ForeColor = Color.White
            btn.FlatAppearance.BorderColor = Color.FromArgb(31, 99, 171)
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(31, 99, 171)
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(24, 83, 145)
        End If
    End Sub

    Private Shared Sub EnsurePrimaryActionsVisible(ByVal root As Control)
        For Each buttonName As String In New String() {
            "BtnGuardar", "BtnCancelar", "BtnEliminar", "BtnRegresar", "BtnCerrar", "BtnCrear", "BtnRecargar"
        }
            Dim btn As Button = TryCast(FindControlRecursive(root, buttonName), Button)
            If btn IsNot Nothing Then
                btn.Visible = True
                btn.Enabled = True
                btn.BringToFront()
            End If
        Next
    End Sub

    Private Shared Function FindControlRecursive(ByVal root As Control, ByVal controlName As String) As Control
        If root Is Nothing Then
            Return Nothing
        End If

        If String.Equals(root.Name, controlName, StringComparison.OrdinalIgnoreCase) Then
            Return root
        End If

        For Each ctrl As Control In root.Controls
            Dim found As Control = FindControlRecursive(ctrl, controlName)
            If found IsNot Nothing Then
                Return found
            End If
        Next

        Return Nothing
    End Function
End Class
