Public NotInheritable Class FrmAyuda

    Private Sub FrmAyuda_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        UIThemeManagerV2.Apply(Me, "dialogo")
        ApplyModernFormStyle()
        ' Set the title of the form.
        Dim ApplicationTitle As String
        If My.Application.Info.Title <> "" Then
            ApplicationTitle = My.Application.Info.Title
        Else
            ApplicationTitle = System.IO.Path.GetFileNameWithoutExtension(My.Application.Info.AssemblyName)
        End If
        Me.Text = String.Format("About {0}", ApplicationTitle)
        ' Initialize all of the text displayed on the About Box.
        ' TODO: Customize the application's assembly information in the "Application" pane of the project 
        '    properties dialog (under the "Project" menu).
        Me.LabelProductName.Text = My.Application.Info.ProductName
        Me.LabelVersion.Text = String.Format("Version {0}", My.Application.Info.Version.ToString)
        Me.LabelCopyright.Text = My.Application.Info.Copyright
        Me.LabelCompanyName.Text = My.Application.Info.CompanyName
        Me.TextBoxDescription.Text = My.Application.Info.Description
    End Sub

    Private Sub ApplyModernFormStyle()
        Me.BackColor = UIConstants.AppBackground
        Me.BackgroundImage = Nothing
        Me.Font = UIConstants.FontBody()
        Me.MinimumSize = New Size(860, 420)
        Me.Size = New Size(980, 520)
        LabelProductName.Font = UIConstants.FontSubtitle()
        LabelProductName.ForeColor = UIConstants.TextPrimary
        LabelVersion.Font = UIConstants.FontBodyStrong()
        LabelVersion.ForeColor = UIConstants.TextSecondary
        LabelCompanyName.Font = UIConstants.FontBodyStrong()
        LabelCompanyName.ForeColor = UIConstants.TextSecondary
        LabelCopyright.Font = UIConstants.FontBody()
        LabelCopyright.ForeColor = UIConstants.TextSecondary
        TextBoxDescription.BackColor = UIConstants.Surface
        TextBoxDescription.BorderStyle = BorderStyle.FixedSingle
        TextBoxDescription.ForeColor = UIConstants.TextPrimary
        OKButton.FlatStyle = FlatStyle.Flat
        OKButton.FlatAppearance.BorderSize = 0
        OKButton.BackColor = UIConstants.Accent
        OKButton.ForeColor = Color.White
        OKButton.Font = UIConstants.FontBodyStrong()
        OKButton.Text = "Cerrar"
    End Sub

    Private Sub OKButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OKButton.Click
        Me.Close()
    End Sub

    Private Sub LabelProductName_Click(sender As Object, e As EventArgs) Handles LabelProductName.Click

    End Sub

    Private Sub TextBoxDescription_TextChanged(sender As Object, e As EventArgs) Handles TextBoxDescription.TextChanged

    End Sub
End Class
