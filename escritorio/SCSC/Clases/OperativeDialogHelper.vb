Imports System.Drawing
Imports System.Windows.Forms

Public NotInheritable Class OperativeDialogHelper
    Private Sub New()
    End Sub

    Public Shared Function SolicitarCodigoIncidenciaRapida(ByVal owner As IWin32Window,
                                                           ByVal sugerencia As String) As String
        Using frm As New Form(),
              layout As New TableLayoutPanel(),
              lblTitulo As New Label(),
              lblDetalle As New Label(),
              cboCodigo As New ComboBox(),
              lblAyuda As New Label(),
              panelBotones As New Panel(),
              btnAceptar As New Button(),
              btnCancelar As New Button()

            frm.Text = "Incidencia rápida"
            frm.FormBorderStyle = FormBorderStyle.FixedDialog
            frm.StartPosition = FormStartPosition.CenterParent
            frm.MinimizeBox = False
            frm.MaximizeBox = False
            frm.ShowInTaskbar = False
            frm.BackColor = UIConstants.AppBackground
            frm.Font = UIConstants.FontBody()
            frm.ClientSize = New Size(470, 210)

            layout.Dock = DockStyle.Fill
            layout.Padding = New Padding(18)
            layout.ColumnCount = 1
            layout.RowCount = 5
            layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))
            layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))
            layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))
            layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0!))
            layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))

            lblTitulo.AutoSize = True
            lblTitulo.Text = "Registrar incidencia"
            lblTitulo.Font = New Font("Segoe UI Semibold", 13.0!, FontStyle.Bold)
            lblTitulo.ForeColor = UIConstants.TextPrimary

            lblDetalle.AutoSize = True
            lblDetalle.MaximumSize = New Size(420, 0)
            lblDetalle.Text = "Seleccione o escriba el código operativo. El valor se normaliza al guardar."
            lblDetalle.ForeColor = UIConstants.TextSecondary

            cboCodigo.DropDownStyle = ComboBoxStyle.DropDown
            cboCodigo.Font = New Font("Segoe UI", 11.0!, FontStyle.Regular)
            cboCodigo.Items.AddRange(New Object() {
                "SIN_TIQUETES",
                "ERROR_LECTOR",
                "CARNET_DANADO",
                "MARCA_TARDIA_TRANSPORTE",
                "OTRO"
            })
            cboCodigo.Text = If(String.IsNullOrWhiteSpace(sugerencia), "OTRO", sugerencia.Trim().ToUpperInvariant())
            cboCodigo.Dock = DockStyle.Top

            lblAyuda.AutoSize = True
            lblAyuda.MaximumSize = New Size(420, 0)
            lblAyuda.Text = "Atajos válidos: SIN_TIQUETES, ERROR_LECTOR, CARNET_DANADO, MARCA_TARDIA_TRANSPORTE, OTRO"
            lblAyuda.ForeColor = UIConstants.TextSecondary

            panelBotones.Dock = DockStyle.Fill
            panelBotones.Height = 42

            btnAceptar.Text = "Aceptar"
            btnAceptar.DialogResult = DialogResult.OK
            btnAceptar.FlatStyle = FlatStyle.Flat
            btnAceptar.FlatAppearance.BorderSize = 0
            btnAceptar.BackColor = UIConstants.Accent
            btnAceptar.ForeColor = Color.White
            btnAceptar.Font = UIConstants.FontBodyStrong()
            btnAceptar.SetBounds(250, 4, 96, 32)

            btnCancelar.Text = "Cancelar"
            btnCancelar.DialogResult = DialogResult.Cancel
            btnCancelar.FlatStyle = FlatStyle.Flat
            btnCancelar.FlatAppearance.BorderSize = 1
            btnCancelar.FlatAppearance.BorderColor = UIConstants.Border
            btnCancelar.BackColor = UIConstants.Surface
            btnCancelar.ForeColor = UIConstants.TextPrimary
            btnCancelar.Font = UIConstants.FontBodyStrong()
            btnCancelar.SetBounds(354, 4, 96, 32)

            panelBotones.Controls.Add(btnAceptar)
            panelBotones.Controls.Add(btnCancelar)

            layout.Controls.Add(lblTitulo, 0, 0)
            layout.Controls.Add(lblDetalle, 0, 1)
            layout.Controls.Add(cboCodigo, 0, 2)
            layout.Controls.Add(lblAyuda, 0, 3)
            layout.Controls.Add(panelBotones, 0, 4)

            frm.Controls.Add(layout)
            frm.AcceptButton = btnAceptar
            frm.CancelButton = btnCancelar

            UIThemeManagerV2.Apply(frm, "dialogo")
            cboCodigo.SelectAll()
            cboCodigo.Focus()

            If frm.ShowDialog(owner) <> DialogResult.OK Then
                Return String.Empty
            End If

            Return cboCodigo.Text.Trim()
        End Using
    End Function
End Class
