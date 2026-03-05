Imports System.Globalization

Public Class FrmParametrosSistema
    Private ReadOnly Cls As New FuncionesDB()
    Private ReadOnly ParametroSvc As New ParametroSistemaService()

    Private Sub FrmParametrosSistema_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            UIThemeManagerV2.Apply(Me, "dialogo")
            UIThemeManagerV2.ApplyCrudModuleChrome(Me)
            ConfigurarEstilo()
            CargarDatos()
        Catch ex As Exception
            MsgBox("Error al cargar parámetros: " & ex.Message, MsgBoxStyle.Critical)
            Me.Dispose()
        End Try
    End Sub

    Private Sub ConfigurarEstilo()
        Me.BackColor = UIConstants.AppBackground
        PanelAcciones.BackColor = UIConstants.Surface
        LabelTitulo.Font = New Font("Segoe UI Semibold", 14.0!, FontStyle.Bold)
        LabelTitulo.ForeColor = UIConstants.TextPrimary
        LblEstado.ForeColor = UIConstants.TextSecondary

        For Each btn As Button In New Button() {BtnRecargar, BtnCrear, BtnGuardar, BtnEliminar, BtnCerrar}
            btn.FlatStyle = FlatStyle.Flat
            btn.FlatAppearance.BorderSize = 1
            btn.FlatAppearance.BorderColor = UIConstants.Border
            btn.BackColor = UIConstants.Surface
            btn.ForeColor = UIConstants.TextPrimary
            btn.Font = UIConstants.FontBodyStrong()
        Next

        BtnGuardar.BackColor = UIConstants.Accent
        BtnGuardar.ForeColor = Color.White

        DgvParametros.BackgroundColor = UIConstants.Surface
        DgvParametros.GridColor = UIConstants.Border
        DgvParametros.EnableHeadersVisualStyles = False
        DgvParametros.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(229, 231, 235)
        DgvParametros.ColumnHeadersDefaultCellStyle.ForeColor = UIConstants.TextPrimary
    End Sub

    Private Sub BtnRecargar_Click(sender As Object, e As EventArgs) Handles BtnRecargar.Click
        CargarDatos()
    End Sub

    Private Sub BtnCrear_Click(sender As Object, e As EventArgs) Handles BtnCrear.Click
        Dim cn As New SqlClient.SqlConnection()
        Try
            Cls.AbrirConexion(cn, False)
            ParametroSvc.AsegurarEsquema(cn)
            ParametroSvc.CrearFila1(cn)
            ParametroSvc.MigrarDesdeAppConfigSiCorresponde(cn)
            LblEstado.Text = "Fila 0 creada/inicializada."
            CargarDatos(cn)
        Catch ex As Exception
            MsgBox("No se pudo crear la fila 0: " & ex.Message, MsgBoxStyle.Critical)
        Finally
            If cn.State = ConnectionState.Open Then
                Cls.CerrarConexion(cn)
            End If
        End Try
    End Sub

    Private Sub BtnGuardar_Click(sender As Object, e As EventArgs) Handles BtnGuardar.Click
        Dim cn As New SqlClient.SqlConnection()
        Try
            Cls.AbrirConexion(cn, False)
            ParametroSvc.AsegurarEsquema(cn)
            ParametroSvc.CrearFila1(cn)

            Dim cfg As ParametroSistemaService.ParametroSistemaConfig = LeerDesdeGrid()
            ParametroSvc.GuardarFila1(cn, cfg)

            Dim filasIgnoradas As Integer = ParametroSvc.ContarFilasIgnoradas(cn)
            LblEstado.Text = ConstruirMensajeEstado("Parámetros guardados correctamente en Parametro(Id=0).", filasIgnoradas)
            MsgBox("Parámetros guardados.", MsgBoxStyle.Information)
        Catch ex As Exception
            MsgBox("Error al guardar parámetros: " & ex.Message, MsgBoxStyle.Critical)
        Finally
            If cn.State = ConnectionState.Open Then
                Cls.CerrarConexion(cn)
            End If
        End Try
    End Sub

    Private Sub BtnEliminar_Click(sender As Object, e As EventArgs) Handles BtnEliminar.Click
        If MsgBox("Se restablecerán los parámetros operativos de la fila 0. ¿Desea continuar?", MsgBoxStyle.YesNo Or MsgBoxStyle.Question) <> MsgBoxResult.Yes Then
            Exit Sub
        End If

        Dim cn As New SqlClient.SqlConnection()
        Try
            Cls.AbrirConexion(cn, False)
            ParametroSvc.AsegurarEsquema(cn)
            ParametroSvc.CrearFila1(cn)
            ParametroSvc.RestablecerFila1(cn)
            LblEstado.Text = "Parámetros restablecidos en Parametro(Id=0)."
            CargarDatos(cn)
        Catch ex As Exception
            MsgBox("Error al restablecer parámetros: " & ex.Message, MsgBoxStyle.Critical)
        Finally
            If cn.State = ConnectionState.Open Then
                Cls.CerrarConexion(cn)
            End If
        End Try
    End Sub

    Private Sub BtnCerrar_Click(sender As Object, e As EventArgs) Handles BtnCerrar.Click
        Me.Close()
    End Sub

    Private Sub CargarDatos(Optional ByVal cnExistente As SqlClient.SqlConnection = Nothing)
        Dim cerrarConexion As Boolean = False
        Dim cn As SqlClient.SqlConnection = cnExistente
        If cn Is Nothing Then
            cn = New SqlClient.SqlConnection()
            Cls.AbrirConexion(cn, False)
            cerrarConexion = True
        End If

        Try
            ParametroSvc.AsegurarEsquema(cn)
            ParametroSvc.CrearFila1(cn)
            ParametroSvc.MigrarDesdeAppConfigSiCorresponde(cn)

            Dim cfg As ParametroSistemaService.ParametroSistemaConfig = ParametroSvc.ObtenerFila1(cn)
            If cfg Is Nothing Then
                Throw New InvalidOperationException("No se encontró Parametro(Id=0).")
            End If

            DgvParametros.Rows.Clear()
            AddParam("Institucion", cfg.Institucion)
            AddParam("CodPresupuestario", cfg.CodPresupuestario)
            AddParam("Ubicacion", cfg.Ubicacion)
            AddParam("Leyenda", cfg.Leyenda)
            AddParam("ControlCarnet", cfg.ControlCarnet)
            AddParam("PrecioDocente", cfg.PrecioDocente.ToString(CultureInfo.InvariantCulture))
            AddParam("PrecioEstudiante", cfg.PrecioEstudiante.ToString(CultureInfo.InvariantCulture))
            AddParam("PermitirSinMarcaTransporte", cfg.PermitirSinMarcaTransporte.ToString())

            Dim filasIgnoradas As Integer = ParametroSvc.ContarFilasIgnoradas(cn)
            LblEstado.Text = ConstruirMensajeEstado("Parámetros cargados desde Parametro(Id=0).", filasIgnoradas)
        Finally
            If cerrarConexion AndAlso cn.State = ConnectionState.Open Then
                Cls.CerrarConexion(cn)
            End If
        End Try
    End Sub

    Private Function LeerDesdeGrid() As ParametroSistemaService.ParametroSistemaConfig
        Dim cfg As New ParametroSistemaService.ParametroSistemaConfig()
        cfg.Id = 0
        cfg.Institucion = ObtenerValor("Institucion")
        cfg.CodPresupuestario = ObtenerValor("CodPresupuestario")
        cfg.Ubicacion = ObtenerValor("Ubicacion")
        cfg.Leyenda = ObtenerValor("Leyenda")
        cfg.ControlCarnet = ObtenerValor("ControlCarnet")
        cfg.PrecioDocente = ParseDecimal(ObtenerValor("PrecioDocente"))
        cfg.PrecioEstudiante = ParseDecimal(ObtenerValor("PrecioEstudiante"))
        cfg.PermitirSinMarcaTransporte = ParseBool(ObtenerValor("PermitirSinMarcaTransporte"))
        Return cfg
    End Function

    Private Function ObtenerValor(ByVal clave As String) As String
        For Each row As DataGridViewRow In DgvParametros.Rows
            If String.Equals(Convert.ToString(row.Cells(0).Value), clave, StringComparison.OrdinalIgnoreCase) Then
                Return Convert.ToString(row.Cells(1).Value)
            End If
        Next
        Return String.Empty
    End Function

    Private Sub AddParam(ByVal clave As String, ByVal valor As String)
        DgvParametros.Rows.Add(clave, valor)
    End Sub

    Private Function ParseDecimal(ByVal raw As String, Optional ByVal defaultValue As Decimal = 0D) As Decimal
        Dim v As Decimal
        If Decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, v) Then
            Return v
        End If
        If Decimal.TryParse(raw, NumberStyles.Any, CultureInfo.CurrentCulture, v) Then
            Return v
        End If
        Return defaultValue
    End Function

    Private Function ParseBool(ByVal raw As String) As Boolean
        If String.IsNullOrWhiteSpace(raw) Then
            Return False
        End If

        raw = raw.Trim()
        If raw = "1" Then Return True
        If raw = "0" Then Return False

        Dim b As Boolean
        If Boolean.TryParse(raw, b) Then
            Return b
        End If
        Return False
    End Function

    Private Function ConstruirMensajeEstado(ByVal mensajeBase As String, ByVal filasIgnoradas As Integer) As String
        If filasIgnoradas <= 0 Then
            Return mensajeBase
        End If

        Return mensajeBase & " Filas ignoradas (Id<>0): " & filasIgnoradas.ToString(CultureInfo.InvariantCulture) & "."
    End Function
End Class
