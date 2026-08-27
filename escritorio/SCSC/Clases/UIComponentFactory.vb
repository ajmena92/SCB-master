Imports System.Drawing
Imports System.Windows.Forms

Public NotInheritable Class UIComponentFactory
    Private Sub New()
    End Sub

    Public Shared Function CreatePrimaryButton(ByVal text As String) As Button
        Dim btn As New Button()
        btn.Text = text
        btn.Height = 38
        btn.FlatStyle = FlatStyle.Flat
        btn.FlatAppearance.BorderSize = 0
        btn.BackColor = UIConstants.Accent
        btn.ForeColor = Color.White
        btn.Font = UIConstants.FontBodyStrong()
        btn.Cursor = Cursors.Hand
        AddHandler btn.MouseEnter, Sub(sender, e) DirectCast(sender, Button).BackColor = UIConstants.AccentHover
        AddHandler btn.MouseLeave, Sub(sender, e) DirectCast(sender, Button).BackColor = UIConstants.Accent
        Return btn
    End Function

    Public Shared Function CreateSecondaryButton(ByVal text As String) As Button
        Dim btn As New Button()
        btn.Text = text
        btn.Height = 36
        btn.FlatStyle = FlatStyle.Flat
        btn.FlatAppearance.BorderColor = UIConstants.Border
        btn.FlatAppearance.BorderSize = 1
        btn.BackColor = UIConstants.Surface
        btn.ForeColor = UIConstants.TextPrimary
        btn.Font = UIConstants.FontBody()
        btn.Cursor = Cursors.Hand
        AddHandler btn.MouseEnter, Sub(sender, e) DirectCast(sender, Button).BackColor = UIConstants.SurfaceAlt
        AddHandler btn.MouseLeave, Sub(sender, e) DirectCast(sender, Button).BackColor = UIConstants.Surface
        Return btn
    End Function

    Public Shared Function CreateCard(ByVal title As String) As Panel
        Dim card As New Panel()
        card.BackColor = UIConstants.Surface
        card.BorderStyle = BorderStyle.FixedSingle
        card.Padding = New Padding(UIConstants.SpaceMd)

        If Not String.IsNullOrWhiteSpace(title) Then
            Dim lbl As New Label()
            lbl.Text = title
            lbl.Dock = DockStyle.Top
            lbl.Height = 28
            lbl.Font = UIConstants.FontSubtitle()
            lbl.ForeColor = UIConstants.TextPrimary
            card.Controls.Add(lbl)
        End If

        Return card
    End Function

    Public Shared Function CreateSectionHeader(ByVal text As String) As Label
        Dim lbl As New Label()
        lbl.Text = text
        lbl.Font = UIConstants.FontTitle()
        lbl.ForeColor = UIConstants.TextPrimary
        lbl.AutoSize = True
        lbl.BackColor = Color.Transparent
        Return lbl
    End Function
End Class
