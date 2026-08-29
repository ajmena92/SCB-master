from datetime import date, datetime, timezone

import pytest

from aplicacion.modulos.comedor.esquemas import (
    PersonaComedorSalida,
    ProfesorComedorEntrada,
    ReservaEntrada,
    TiquetesEntrada,
)
from aplicacion.modulos.comedor.servicio import ServicioComedor


class RepositorioFalso:
    def __init__(self) -> None:
        self.personas_registradas = [
            {
                "id_persona": 1,
                "tipo_persona": "estudiante",
                "id_estudiante": 10,
                "id_usuario": None,
                "codigo_barras": "E-ABC",
                "nombre_completo": "Estudiante Becado",
                "colegio": None,
                "id_estado_comedor": 1,
                "beneficio_comedor": "Beneficiario",
                "activo": True,
            },
            {
                "id_persona": 2,
                "tipo_persona": "estudiante",
                "id_estudiante": 11,
                "id_usuario": None,
                "codigo_barras": "E-DEF",
                "nombre_completo": "Estudiante No Becado",
                "colegio": None,
                "id_estado_comedor": 2,
                "beneficio_comedor": "No beneficiario",
                "activo": True,
            },
        ]
        self.recargas: list[tuple[int, int, str]] = []
        self.reservas: list[dict] = []

    def personas(self, tipo_persona=None, incluir_inactivas=False):
        return [
            persona
            for persona in self.personas_registradas
            if tipo_persona is None or persona["tipo_persona"] == tipo_persona
        ]

    def persona(self, id_persona):
        return next(
            persona for persona in self.personas_registradas if persona["id_persona"] == id_persona
        )

    def crear_profesor(self, id_usuario, nombre, colegio):
        persona = {
            "id_persona": 3,
            "tipo_persona": "profesor",
            "id_usuario": id_usuario,
            "codigo_barras": f"P-{id_usuario}",
            "nombre_completo": nombre,
            "colegio": colegio,
            "id_estado_comedor": 2,
            "beneficio_comedor": "No beneficiario",
            "activo": True,
        }
        self.personas_registradas.append(persona)
        return persona

    def cuenta(self, id_persona):
        return {
            "id_cuenta": 1,
            "id_persona": id_persona,
            "saldo": 2,
            "reservados": 1,
            "disponibles": 1,
            "actualizado_en": datetime.now(timezone.utc),
        }

    def recargar(self, id_persona, cantidad, concepto, clave, usuario):
        self.recargas.append((id_persona, cantidad, clave))
        return {
            "id_movimiento": 1,
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
        persona = next(p for p in self.personas_registradas if p["id_persona"] == id_persona)
        resultado = {
            "id_reserva": len(self.reservas) + 1,
            "id_persona": id_persona,
            "fecha": fecha,
            "estado": "reservada",
            "requiere_tiquete": persona["id_estado_comedor"] == 2,
            "modalidad": (
                "tiquete" if persona["id_estado_comedor"] == 2 else "beca"
            ),
        }
        self.reservas.append(resultado)
        return resultado

    def ingresar(self, codigo_barras, fecha, usuario):
        persona = next(p for p in self.personas_registradas if p["codigo_barras"] == codigo_barras)
        return {
            "id_ingreso": 1,
            "id_persona": persona["id_persona"],
            "fecha": fecha,
            "modalidad": "tiquete",
            "registrado_por": usuario,
        }


def test_estado_explicito_no_depende_del_nombre_del_beneficio() -> None:
    servicio = ServicioComedor(RepositorioFalso())
    personas = servicio.personas()

    assert [persona.id_estado_comedor for persona in personas] == [1, 2]
    assert all(isinstance(persona, PersonaComedorSalida) for persona in personas)


def test_reserva_becado_no_requiere_tiquete() -> None:
    servicio = ServicioComedor(RepositorioFalso())

    reserva = servicio.reservar(1, date(2026, 8, 28), None)

    assert reserva.modalidad == "beca"
    assert reserva.requiere_tiquete is False


def test_recarga_valida_y_crea_movimiento_idempotente() -> None:
    repositorio = RepositorioFalso()
    servicio = ServicioComedor(repositorio)

    datos = TiquetesEntrada(cantidad=2, claveIdempotencia="recarga-01")
    servicio.recargar(2, datos, 99)

    assert repositorio.recargas == [(2, 2, "recarga-01")]


def test_becado_no_puede_recibir_recargas() -> None:
    with pytest.raises(ValueError, match="becadas"):
        ServicioComedor(RepositorioFalso()).recargar(
            1, TiquetesEntrada(cantidad=1, claveIdempotencia="recarga-becado"), 99
        )


def test_profesor_se_registra_como_no_becado() -> None:
    repositorio = RepositorioFalso()
    servicio = ServicioComedor(repositorio)

    profesor = servicio.crear_profesor(
        ProfesorComedorEntrada(idUsuario=20, nombreCompleto="  Ana   Pérez ", colegio="CTP")
    )

    assert profesor.tipo_persona == "profesor"
    assert profesor.id_estado_comedor == 2
    assert profesor.codigo_barras == "P-20"


def test_tipo_persona_invalido_se_rechaza() -> None:
    with pytest.raises(ValueError, match="tipo de persona"):
        ServicioComedor(RepositorioFalso()).personas("administrador")


def test_reserva_valida_el_contrato_de_fecha() -> None:
    assert ReservaEntrada(fecha="2026-08-28").fecha == date(2026, 8, 28)
