from datetime import datetime, timezone
from decimal import Decimal
from typing import Literal

import pytest

from aplicacion.modulos.cuentas.esquemas import MovimientoEntrada
from aplicacion.modulos.cuentas.servicio import ServicioCuentas


class RepositorioFalso:
    def __init__(self) -> None:
        self.saldo_actual = Decimal("10.00")
        self.movimientos: dict[str, dict] = {}

    def saldo(self, id_estudiante: int) -> dict:
        return {
            "id_cuenta": 4,
            "id_estudiante": id_estudiante,
            "saldo": self.saldo_actual,
            "actualizado_en": datetime.now(timezone.utc),
        }

    def movimiento(self, id_estudiante: int, datos: dict, id_usuario: int, ip: str) -> dict:
        if datos["clave_idempotencia"] in self.movimientos:
            return self.movimientos[datos["clave_idempotencia"]]
        anterior = self.saldo_actual
        self.saldo_actual += datos["monto"] if datos["tipo"] != "consumo" else -datos["monto"]
        if self.saldo_actual < 0:
            raise ValueError("El saldo no puede quedar negativo")
        resultado = {
            "id_movimiento": len(self.movimientos) + 1,
            "id_cuenta": 4,
            "tipo": datos["tipo"],
            "monto": datos["monto"],
            "saldo_anterior": anterior,
            "saldo_nuevo": self.saldo_actual,
            "clave_idempotencia": datos["clave_idempotencia"],
            "concepto": datos.get("concepto"),
            "creado_en": datetime.now(timezone.utc),
        }
        self.movimientos[datos["clave_idempotencia"]] = resultado
        return resultado


def movimiento(
    tipo: Literal["recarga", "consumo", "ajuste"] = "recarga",
    monto: str = "5.00",
    clave: str = "recarga-001",
) -> MovimientoEntrada:
    return MovimientoEntrada(tipo=tipo, monto=Decimal(monto), claveIdempotencia=clave)


def test_recarga_actualiza_saldo() -> None:
    repo = RepositorioFalso()
    resultado = ServicioCuentas(repo).registrar_movimiento(8, movimiento(), 3, "WEB")
    assert resultado.saldo_nuevo == Decimal("15.00")


def test_clave_idempotencia_no_duplica_movimiento() -> None:
    repo = RepositorioFalso()
    servicio = ServicioCuentas(repo)
    primero = servicio.registrar_movimiento(8, movimiento(), 3, "WEB")
    segundo = servicio.registrar_movimiento(8, movimiento(), 3, "WEB")
    assert primero.id_movimiento == segundo.id_movimiento
    assert repo.saldo_actual == Decimal("15.00")


def test_consumo_no_puede_dejar_saldo_negativo() -> None:
    repo = RepositorioFalso()
    with pytest.raises(ValueError, match="negativo"):
        ServicioCuentas(repo).registrar_movimiento(
            8, movimiento("consumo", "11.00", "consumo-001"), 3, "WEB"
        )


def test_saldo_y_movimiento_rechazan_estudiante_invalido() -> None:
    servicio = ServicioCuentas(RepositorioFalso())
    assert servicio.saldo(8).saldo == Decimal("10.00")
    with pytest.raises(ValueError, match="no es válido"):
        servicio.saldo(0)
    with pytest.raises(ValueError, match="no es válido"):
        servicio.registrar_movimiento(0, movimiento(), 3, "WEB")
