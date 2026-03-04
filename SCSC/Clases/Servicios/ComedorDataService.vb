Imports System.Data.SqlClient

Public Class ComedorDataService
    Public Structure MarcaComedorResultado
        Public TextoTiquetes As String
        Public ErrorTiquetes As Boolean
        Public RegistroGuardado As Boolean
    End Structure

    Private ReadOnly _cls As FuncionesDB

    Public Sub New(Optional ByVal cls As FuncionesDB = Nothing)
        If cls Is Nothing Then
            _cls = New FuncionesDB()
        Else
            _cls = cls
        End If
    End Sub

    Public Function CargarUsuariosConMarcaTransporte(ByVal cn As SqlConnection, ByVal fechaServer As Date) As DataSet
        Dim dsUsuarios As DataSet = _cls.ConsultarTSQL("Usuario",
                                                       "SELECT IdUsuario,TipoBeca,HuellaDactilar,Nombre,PrimerApellido,SegundoApellido,CodTipo,Cedula,IdHorario FROM Usuario WHERE Activo = 1",
                                                       Cn:=cn)
        Dim dsMarcasEntrada As DataSet = _cls.ConsultarTSQL("Marcas",
                                                            "SELECT IdUsuario,Fecha FROM RegistroTransporte WHERE " & ArmaFechaQueryHora("Fecha", fechaServer.Date, fechaServer),
                                                            Cn:=cn)

        dsUsuarios.Tables(0).Columns.Add("MarcaTransporte", GetType(Integer))
        dsUsuarios.Tables(0).Columns.Add("HoraMarca", GetType(DateTime))

        For Each iRow As DataRow In dsUsuarios.Tables(0).Rows
            Dim marca() As DataRow = dsMarcasEntrada.Tables(0).Select(String.Format("IdUsuario = '{0}'", iRow!IdUsuario))
            If marca.Length > 0 Then
                iRow!MarcaTransporte = 1
                iRow!HoraMarca = marca(0).ItemArray(1)
            Else
                iRow!MarcaTransporte = 0
            End If
        Next

        Return dsUsuarios
    End Function

    Public Function CargarBecas(ByVal cn As SqlConnection) As DataSet
        Return _cls.ConsultarTSQL("Becas", "Select IdBeca,DiasBeca From TipoBeca", Cn:=cn)
    End Function

    Public Function CargarHorarios(ByVal cn As SqlConnection) As DataSet
        Return _cls.ConsultarTSQL("Horarios", "Select IdHorario,HoraLimite From Horario", Cn:=cn)
    End Function

    Public Function RegistrarMarca(ByVal usuario As DataRow,
                                   ByVal dsBeca As DataSet,
                                   ByVal diaSemana As String,
                                   ByVal cn As SqlConnection) As MarcaComedorResultado
        Dim pTransac As SqlTransaction = Nothing
        Dim resultado As MarcaComedorResultado

        Try
            Dim guardarTransaccion As Boolean = True
            Dim cantTiquetes As Integer = 0
            Dim valores() As FuncionesDB.Campos
            Dim becado As Integer = 0

            For Each beca As DataRow In dsBeca.Tables(0).Rows
                If beca!IdBeca = usuario!TipoBeca Then
                    becado = InStr(beca!DiasBeca, diaSemana)
                    Exit For
                End If
            Next

            _cls.IniciaSQL(cn, pTransac)
            valores = _cls.InicializarArray
            _cls.ArmaValor(valores, "IdUsuario", usuario!IdUsuario)

            If becado > 0 Then
                resultado.TextoTiquetes = " Usuario Becado"
                _cls.ArmaValor(valores, "Beca", 1)
            Else
                Dim data As DataSet = _cls.ConsultarTSQL("Usuario", "SELECT CantidadTiquetes FROM Usuario WHERE IdUsuario = @IdUsuario", valores, cn, pTransac)
                If data.Tables(0).Rows(0)!CantidadTiquetes < 1 Then
                    guardarTransaccion = False
                    resultado.ErrorTiquetes = True
                Else
                    cantTiquetes = data.Tables(0).Rows(0)!CantidadTiquetes - 1
                    _cls.AplicaSQL("UPDATE Usuario set CantidadTiquetes = CantidadTiquetes - 1 WHERE IdUsuario = @IdUsuario", cn, pTransac, valores)
                End If
                resultado.TextoTiquetes = cantTiquetes & " Tiquetes"
                _cls.ArmaValor(valores, "Beca", 0)
            End If

            _cls.ArmaValor(valores, "TipoPago", 2)
            _cls.ArmaValor(valores, "Cantidad", 1)
            _cls.ArmaValor(valores, "TipoUsuario", usuario!CodTipo)
            If guardarTransaccion Then
                _cls.Insert("RegistroComedor", valores, cn, pTransac)
                resultado.RegistroGuardado = True
            End If

            _cls.FinalSQL(pTransac)
            Return resultado
        Catch
            If Not pTransac Is Nothing Then
                Try
                    _cls.RollSQL(pTransac)
                Catch
                    ' omitir error secundario de rollback para conservar comportamiento actual
                End Try
            End If
            Throw
        End Try
    End Function
End Class
