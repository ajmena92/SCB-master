from datetime import date, datetime, timezone

import pytest

from aplicacion.modulos.comedor.esquemas import (
    MovimientoTiquetesSalida,
    PersonaComedorSalida,
    ReservaSalida,
    TiquetesEntrada,
)
from aplicacion.modulos.comedor.servicio import ServicioComedor
from aplicacion.modulos.comedor.errores import IngresoDuplicado


class RepositorioComedorFalso:
    def __init__(self) -> None:
        self._persona_data = {
            "id_persona": 1,
            "tipo_persona": "estudiante",
            "id_estudiante": 10,
            "id_usuario": None,
            "codigo_barras": "E-10",
            "nombre_completo": "Estudiante",
            "colegio": None,
            "id_estado_comedor": 1,
            "beneficio_comedor": "Beneficiario",
            "activo": True,
        }

    def personas(self, tipo_persona=None, incluir_inactivas=False):
        return [self._persona_data] if tipo_persona in (None, "estudiante") else []

    def persona(self, id_persona):
        if id_persona != self._persona_data["id_persona"]:
            raise ValueError("La persona no existe")
        return self._persona_data

    def crear_profesor(self, id_usuario, nombre, colegio):
        return {
            "id_persona": 2,
            "tipo_persona": "profesor",
            "id_usuario": id_usuario,
            "codigo_barras": f"P-{id_usuario}",
            "nombre_completo": nombre,
            "colegio": colegio,
            "id_estado_comedor": 2,
            "beneficio_comedor": "No beneficiario",
            "activo": True,
        }

    def cuenta(self, id_persona):
        return {
            "id_cuenta": 1,
            "id_persona": id_persona,
            "saldo": 1,
            "reservados": 0,
            "disponibles": 1,
            "actualizado_en": datetime.now(timezone.utc),
        }

    def recargar(self, id_persona, cantidad, concepto, clave, usuario):
        return {
            "id_movimiento": 4,
            "id_cuenta": 1,
            "tipo": "recarga",
            "cantidad": cantidad,
            "saldo_anterior": 0,
            "saldo_nuevo": cantidad,
            "reservados_anterior": 0,
            "reservados_nuevo": 0,
            "clave_idempotencia": clave,
            "concepto": concepto,
            "creado_por": usuario,
            "creado_en": datetime.now(timezone.utc),
        }

    def reservar(self, id_persona, fecha, usuario):
        return {
            "id_reserva": 1,
            "id_persona": id_persona,
            "fecha": fecha,
            "estado": "reservada",
            "requiere_tiquete": False,
            "modalidad": "beca",
        }

    def cancelar(self, id_persona, fecha, usuario):
        return {
            "id_reserva": 1,
            "id_persona": id_persona,
            "fecha": fecha,
            "estado": "cancelada",
            "requiere_tiquete": False,
            "modalidad": "beca",
        }

    def ingresar(self, codigo_barras, fecha, usuario):
        return {
            "id_ingreso": 1,
            "id_persona": 1,
            "fecha": fecha,
            "modalidad": "beca",
            "registrado_por": usuario,
        }


def test_el_estado_de_comedor_es_explicito_y_separa_profesores() -> None:
    personas = ServicioComedor(RepositorioComedorFalso()).personas()

    assert isinstance(personas[0], PersonaComedorSalida)
    assert personas[0].id_estado_comedor == 1


def test_becado_reserva_sin_tiquete() -> None:
    reserva = ServicioComedor(RepositorioComedorFalso()).reservar(
        1, date(2026, 8, 28), None
    )

    assert isinstance(reserva, ReservaSalida)
    assert reserva.requiere_tiquete is False


def test_recarga_devuelve_movimiento_tipado() -> None:
    repositorio = RepositorioComedorFalso()
    repositorio._persona_data["id_estado_comedor"] = 2
    resultado = ServicioComedor(repositorio).recargar(
        1,
        TiquetesEntrada(cantidad=2, claveIdempotencia="recarga-001"),
        7,
    )

    assert isinstance(resultado, MovimientoTiquetesSalida)
    assert resultado.tipo == "recarga"


def test_tipo_persona_desconocido_se_rechaza() -> None:
    with pytest.raises(ValueError, match="tipo de persona"):
        ServicioComedor(RepositorioComedorFalso()).personas("administrador")


def test_ingreso_duplicado_se_propaga_como_error_de_conflicto() -> None:
    repositorio = RepositorioComedorFalso()
    repositorio.ingresar = lambda *_args: (_ for _ in ()).throw(
        IngresoDuplicado("El ingreso al comedor ya fue registrado para esta fecha")
    )

    with pytest.raises(IngresoDuplicado, match="ya fue registrado"):
        ServicioComedor(repositorio).ingresar("E-10", date(2026, 8, 28), 7)
