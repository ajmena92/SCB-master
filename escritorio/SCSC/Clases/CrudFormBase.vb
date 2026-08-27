Imports System.Windows.Forms

Public MustInherit Class CrudFormBase
    Inherits Form

    Protected Overridable Sub InicializarCrudUI()
        CrudVisualHelper.ApplyCrudStandard(Me, "dialogo")
    End Sub

    Protected Overridable Sub AplicarEstadosAccion()
        ' Hook para que cada formulario habilite/deshabilite acciones segun su estado.
    End Sub

    Protected Sub RegistrarAtajosCrud()
        Me.KeyPreview = True
    End Sub

    Protected Sub MostrarMensajeEstado(ByVal texto As String, ByVal esError As Boolean)
        If String.IsNullOrWhiteSpace(texto) Then
            Exit Sub
        End If

        Dim caption As String = If(esError, "Error", "Informacion")
        Dim style As MsgBoxStyle = If(esError, MsgBoxStyle.Critical, MsgBoxStyle.Information)
        MsgBox(texto, style, caption)
    End Sub
End Class
