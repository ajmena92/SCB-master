"""Pruebas de integración del contrato operativo de comedor.

Estas pruebas cruzan contratos Pydantic, migración y documentación operativa.
No sustituyen las pruebas unitarias de cada dominio ni las pruebas HTTP contra
SQL Server; esas pruebas se ejecutan en el entorno de staging.
"""

from __future__ import annotations

from datetime import date
from pathlib import Path

from aplicacion.modulos.comedor.esquemas import (
    EstadoComedor,
    IngresoEntrada,
    ReservaEntrada,
    TiquetesEntrada,
)
from aplicacion.modulos.comedor.api import crear_enrutador
from aplicacion.modulos.reportes.dashboard import DashboardSalida, MetricaAsistencia
from aplicacion.modulos.comedor.profesor_portal import crear_enrutador_profesores
from aplicacion.modulos.comedor.errores import IngresoDuplicado


def test_contratos_de_comedor_validan_estado_tiquete_reserva_e_ingreso() -> None:
    assert EstadoComedor.__args__ == (1, 2)
    tiquetes = TiquetesEntrada(cantidad=2, claveIdempotencia="compra-2026")
    reserva = ReservaEntrada(fecha="2026-08-28")
    ingreso = IngresoEntrada(codigoBarras="EST-10", fecha="2026-08-28")

    assert tiquetes.cantidad == 2
    assert tiquetes.clave_idempotencia == "compra-2026"
    assert str(reserva.fecha) == "2026-08-28"
    assert ingreso.codigo_barras == "EST-10"


def test_composicion_expone_reservas_ingresos_y_filtro_de_personas() -> None:
    def exigir_permiso(_permiso: str):
        return lambda: {"idUsuario": 99}

    enrutador = crear_enrutador(
        lambda: object(), exigir_permiso, lambda: None, lambda: object()
    )
    rutas = {ruta.path for ruta in enrutador.routes}

    assert {
        "/comedor/personas",
        "/comedor/personas/{id_persona}/tiquetes",
        "/comedor/personas/{id_persona}/movimientos",
        "/comedor/reservas/estudiante",
        "/comedor/operacion/ingresos",
    } <= rutas


def test_contrato_rechaza_tiquete_sin_idempotencia_y_cantidad_invalida() -> None:
    import pytest

    with pytest.raises(ValueError):
        TiquetesEntrada(cantidad=0, claveIdempotencia="corta")


def test_ingreso_recibe_solo_codigo_y_fecha() -> None:
    ingreso = IngresoEntrada(codigoBarras="E-10", fecha="2026-08-28")
    assert ingreso.codigo_barras == "E-10"
    assert "codigoHorario" not in ingreso.model_dump(by_alias=True)


def test_operacion_expone_configuracion_estado_historial_e_ingreso() -> None:
    from aplicacion.modulos.comedor.api import crear_enrutador

    enrutador = crear_enrutador(lambda: object(), lambda _: lambda: {}, lambda: None, lambda: object())
    rutas = {ruta.path for ruta in enrutador.routes}
    assert {
        "/comedor/operacion/configuracion",
        "/comedor/operacion/estado",
        "/comedor/operacion/historial",
        "/comedor/operacion/ingresos",
    } <= rutas


def test_migracion_registra_politicas_auditoria_y_reconciliacion() -> None:
    raiz = Path(__file__).resolve().parents[3]
    migracion_29 = (raiz / "backend/alembic/versions/0029_uso_transporte_y_auditoria_comedor.py").read_text()
    migracion_30 = (raiz / "backend/alembic/versions/0030_politicas_y_auditoria_operacion.py").read_text()
    migracion_31 = (raiz / "backend/alembic/versions/0031_reconciliacion_corte_comedor.py").read_text()
    assert "transporte.uso_diario" in migracion_29
    assert "dbo.RegistroTransporte" in migracion_29
    assert "marca_transporte_existente" in migracion_29
    assert "permitir_marca_tardia" in migracion_30
    assert "comedor.auditoria_ingreso" in migracion_30
    assert "comedor.reconciliacion_migracion" in migracion_31
    horarios = (raiz / "backend/alembic/versions/0028_horarios_operacion_comedor.py").read_text()
    trazabilidad = (raiz / "backend/alembic/versions/0032_trazabilidad_horarios.py").read_text()
    assert "dbo.Horario" in horarios
    assert "HoraLimite" in horarios
    assert "50060" in horarios
    assert "hora_limite_origen" in trazabilidad
    assert "COL_LENGTH(N'dbo.Horario', N'IdHorario')" in horarios
    assert "50062" in horarios
    assert "50063" in horarios
    assert "50064" in horarios
    assert "IdHorario" in horarios
    migracion_final = (raiz / "backend/alembic/versions/0033_horarios_origen_comedor.py").read_text()
    assert "id_horario_origen" in migracion_final
    assert "Descripcion" in migracion_final
    assert "%NOCTURN%" in migracion_final
    assert "ROW_NUMBER() OVER (ORDER BY IdHorario)" not in migracion_final


def test_operacion_prioriza_duplicado_y_reutiliza_hora_servidor_en_auditoria() -> None:
    raiz = Path(__file__).resolve().parents[3]
    repositorio = (raiz / "backend/aplicacion/modulos/comedor/repositorio_operacion.py").read_text()
    assert repositorio.index("raise IngresoDuplicado") < repositorio.index("tardio =")
    assert "hora_marca" in repositorio and "momento_servidor" in repositorio
    assert "hora_servidor,registrado_por,terminal_id" in repositorio
    assert repositorio.count("momento_servidor") >= 3


def test_dashboard_usa_la_fecha_canonica_de_asignacion_de_ruta() -> None:
    raiz = Path(__file__).resolve().parents[3]
    repositorio = (raiz / "backend/aplicacion/modulos/reportes/repositorio.py").read_text()
    assert "ORDER BY fecha_creacion DESC, id_asignacion DESC" in repositorio
    assert "ORDER BY fecha DESC, id_asignacion DESC" not in repositorio


def test_reconciliacion_operativa_cubre_diferencias_del_corte() -> None:
    raiz = Path(__file__).resolve().parents[3]
    script = (raiz / "backend/scripts/reconciliar_migracion_comedor.py").read_text()
    for tipo in ("ruta_multiple", "saldo_negativo", "carnet_duplicado", "persona_sin_vinculo", "horario_sin_origen", "ingreso_duplicado"):
        assert f'"{tipo}"' in script
    assert "--apply" in script
    assert "comedor.reconciliacion_migracion" in script
    for tipo in ("saldo_local_web", "conteo_ingresos_local_web", "estado_comedor_local_web", "profesores_habilitados_local_web", "ingresos_por_fecha_local_web"):
        assert f'"{tipo}"' in script
    assert "OBJECT_ID" in script
    assert "INNER JOIN comedor.estado_comedor" in script
    assert "p.estado_comedor" not in script
    assert ") OR (" not in script


def test_corte_detecta_carnets_duplicados_antes_de_crear_indices() -> None:
    raiz = Path(__file__).resolve().parents[3]
    migracion = (raiz / "backend/alembic/versions/0024_corte_comedor_tiquetes.py").read_text()
    assert migracion.index("50034") < migracion.index("UQ_comedor_persona_barcode")


def test_migracion_0034_traslada_datos_legados_y_conserva_duplicados() -> None:
    raiz = Path(__file__).resolve().parents[3]
    migracion = (raiz / "backend/alembic/versions/0034_migracion_datos_legados.py").read_text()
    sql = (raiz / "sql/migrations/034_migracion_datos_legados.sql").read_text()
    for texto in (
        "dbo.Usuario",
        "dbo.RegistroComedor",
        "dbo.RegistroTransporte",
        "ComedorPortal.FotoEstudiante",
        "comedor.migracion_ingreso_0034",
        "estudiantes.fotografia",
        "comedor.movimiento_tiquetes",
    ):
        assert texto in migracion
        assert texto in sql
    assert "TipoBeca=2" in migracion
    assert "tipo_persona='profesor'" in migracion


def test_migracion_y_runbook_exigen_modalidad_y_ejecucion_controlada() -> None:
    raiz = Path(__file__).resolve().parents[3]
    migracion = (raiz / "backend/alembic/versions/0023_registro_comedor_modalidad.py").read_text()
    corte = (raiz / "backend/alembic/versions/0024_corte_comedor_tiquetes.py").read_text()
    runbook = (raiz / "docs/RUNBOOK_DEPLOY_PRODUCCION.md").read_text()

    assert "modalidad IN ('beca', 'tiquete', 'otro')" in migracion
    assert "IF OBJECT_ID(N'comedor.registro', N'U') IS NULL" in migracion
    assert "CONFIRMAR_MIGRACION_DBA=SI" in runbook
    assert "respaldo" in runbook.lower()
    assert "comedor" in runbook.lower()
    assert "comedor.persona" in corte
    assert "comedor.cuenta_tiquetes" in corte
    assert "comedor.reserva" in corte
    assert "comedor.ingreso" in corte
    assert "DROP TABLE comedor.registro" not in corte
    assert "registros sin estudiante" in corte
    assert "registros sin persona" in corte
    assert "r.modalidad" in corte
    assert "huella_idempotencia" in corte
    assert "DROP TABLE comedor.registro" not in corte
    assert "0026_idempotencia_corte_comedor" in (
        raiz / "backend/alembic/versions/0026_idempotencia_corte_comedor.py"
    ).read_text()
    assert "LIKE" not in corte
    assert "huella_idempotencia" in (
        raiz / "backend/aplicacion/modulos/comedor/repositorio_catalogo.py"
    ).read_text()


def test_crear_estudiante_sincroniza_catalogo_y_cuenta_de_comedor() -> None:
    raiz = Path(__file__).resolve().parents[3]
    repositorio = (raiz / "backend/aplicacion/modulos/estudiantes/repositorio.py").read_text()

    assert "INSERT INTO comedor.persona" in repositorio
    assert "INSERT INTO comedor.cuenta_tiquetes" in repositorio
    assert "id_estado_comedor" in repositorio


def test_migracion_asocia_turno_por_id_horario_de_origen() -> None:
    raiz = Path(__file__).resolve().parents[3]
    migracion = (raiz / "backend/alembic/versions/0034_migracion_datos_legados.py").read_text()
    normalizacion = (raiz / "backend/alembic/versions/0035_normaliza_estado_horario_comedor.py").read_text()

    assert "ho.id_horario_origen=u.IdHorario" in migracion
    assert "CASE WHEN u.IdHorario=1" not in migracion
    assert "o.id_horario_origen=u.IdHorario" in normalizacion
    assert "50066" in normalizacion


def test_migracion_valida_horarios_operativos_canonicos() -> None:
    raiz = Path(__file__).resolve().parents[3]
    migracion = (
        raiz / "backend/alembic/versions/0037_valida_horarios_operativos.py"
    ).read_text()

    assert "CK_estudiantes_turno_comedor_canonico" in migracion
    assert "50069" in migracion
    assert "50070" in migracion


def test_migracion_cataloga_profesores_solo_desde_roles_de_identidad() -> None:
    raiz = Path(__file__).resolve().parents[3]
    alembic = (raiz / "backend/alembic/versions/0027_catalogo_profesores_identidad.py").read_text()
    sql = (raiz / "sql/migrations/027_catalogo_profesores_identidad.sql").read_text()

    for fuente in (alembic, sql):
        assert "identidad.usuario_rol" in fuente
        assert "identidad.rol" in fuente
        assert "N'profesor'" in fuente
        assert "N'docente'" in fuente
        assert "comedor.persona" in fuente
        assert "comedor.cuenta_tiquetes" in fuente
        assert "INSERT INTO identidad.usuario" not in fuente
        assert "dbo.Usuario" not in fuente
        assert "dbo.usuario" not in fuente.lower()

    assert "0026_idempotencia_corte_comedor" in alembic
    assert "nombre_usuario" in alembic
    assert "colegio" in alembic


def test_contrato_dashboard_expone_tipo_persona_y_cobertura() -> None:
    campos = DashboardSalida.model_fields

    assert "tipo_persona" in campos
    assert "cobertura_registro" in MetricaAsistencia.model_fields


def test_documentacion_declara_separacion_de_profesores_y_estadisticas() -> None:
    raiz = Path(__file__).resolve().parents[3]
    requisitos = (raiz / "docs/REQUISITOS_COMEDOR.md").read_text()

    assert "Las estadísticas estudiantiles excluyen siempre a" in requisitos
    assert "becado_comedor" in requisitos
    assert "no_becado_comedor" in requisitos
    assert "reserva" in requisitos
    assert "ingreso" in requisitos


def test_portal_profesor_publica_el_contrato_que_consumen_frontend() -> None:
    def exigir_csrf():
        return None

    enrutador = crear_enrutador_profesores(
        obtener_repositorio=lambda: object(),
        obtener_identidad=lambda: object(),
        obtener_menu=lambda: object(),
        exigir_csrf=exigir_csrf,
        obtener_fecha_local=lambda: date(2026, 8, 28),
    )
    rutas = {(ruta.path, tuple(sorted(ruta.methods or ()))) for ruta in enrutador.routes}

    assert ("/profesores/menu", ("GET",)) in rutas
    assert ("/profesores/carnet", ("GET",)) in rutas
    assert ("/profesores/asistencia/hoy", ("GET",)) in rutas
    assert ("/profesores/asistencia/{accion}", ("POST",)) in rutas

    ruta_asistencia = next(
        ruta
        for ruta in enrutador.routes
        if ruta.path == "/profesores/asistencia/{accion}"
    )
    dependencias = {dependencia.call.__name__ for dependencia in ruta_asistencia.dependant.dependencies}
    assert "profesor_actual" in dependencias
    assert "exigir_csrf" in dependencias


def test_salida_del_portal_profesor_expone_solo_datos_de_comedor() -> None:
    from aplicacion.modulos.comedor.esquemas import ProfesorPortalSalida

    salida = ProfesorPortalSalida(
        tipo_persona="profesor",
        id_persona=4,
        id_usuario=8,
        nombre="Ana Pérez",
        colegio="CTP Platanares",
        id_estado_comedor=2,
        beneficio_comedor="No beneficiario",
        activo=True,
        barcode="P-8",
    )

    assert salida.model_dump(by_alias=True)["tipoPersona"] == "profesor"
    assert salida.model_dump(by_alias=True)["barcode"] == "P-8"
