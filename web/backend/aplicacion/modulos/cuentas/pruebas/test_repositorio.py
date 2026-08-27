from contextlib import contextmanager
from datetime import datetime, timezone
from decimal import Decimal
from typing import Iterator, Sequence, cast

from aplicacion.modulos.cuentas.repositorio import RepositorioSqlCuentas
from aplicacion.nucleo.base_datos import ConexionSql, CursorSql, FabricaConexionSql


class ErrorClaveUnica(Exception):
    def __init__(self) -> None:
        super().__init__(2627, "UQ_cuentas_mov_idempotencia")


MOVIMIENTO = {
    "id_movimiento": 21,
    "id_cuenta": 4,
    "tipo": "recarga",
    "monto": Decimal("5.00"),
    "saldo_anterior": Decimal("10.00"),
    "saldo_nuevo": Decimal("15.00"),
    "clave_idempotencia": "recarga-001",
    "concepto": None,
    "creado_en": datetime.now(timezone.utc),
}


class CursorColision:
    description: Sequence[tuple[str, object, object, object, object, object, object]] = tuple(
        (nombre, object, object, object, object, object, object) for nombre in MOVIMIENTO
    )
    rowcount = 1

    def __init__(self) -> None:
        self._fila: tuple[object, ...] | None = None
        self._busquedas = 0

    def execute(self, consulta: str, *parametros: object) -> CursorSql:
        if "UPDLOCK" in consulta:
            self.description = (
                ("id_cuenta", object, object, object, object, object, object),
                ("saldo", object, object, object, object, object, object),
            )
            self._fila = (4, Decimal("10.00"))
        elif consulta.startswith("SELECT id_movimiento"):
            self._busquedas += 1
            self.description = tuple(
                (nombre, object, object, object, object, object, object) for nombre in MOVIMIENTO
            )
            self._fila = None if self._busquedas == 1 else tuple(MOVIMIENTO.values())
        elif consulta.startswith("UPDATE"):
            self._fila = None
        elif consulta.startswith("INSERT"):
            raise ErrorClaveUnica()
        return cast(CursorSql, self)

    def fetchall(self) -> Sequence[Sequence[object]]:
        return ()

    def fetchone(self) -> tuple[object, ...] | None:
        return self._fila


class ConexionColision:
    def __init__(self) -> None:
        self.cursor_obj = CursorColision()
        self.reversiones = 0
        self.confirmaciones = 0

    def cursor(self) -> CursorSql:
        return self.cursor_obj

    def commit(self) -> None:
        self.confirmaciones += 1

    def rollback(self) -> None:
        self.reversiones += 1

    def close(self) -> None:
        pass


class FabricaColision(FabricaConexionSql):
    def __init__(self, conexion: ConexionColision) -> None:
        super().__init__("")
        self.conexion_obj = conexion

    @contextmanager
    def conexion(self) -> Iterator[ConexionSql]:
        try:
            yield self.conexion_obj
            self.conexion_obj.commit()
        except Exception:
            self.conexion_obj.rollback()
            raise


def test_colision_concurrente_recupera_movimiento_y_revierte_saldo() -> None:
    conexion = ConexionColision()
    repositorio = RepositorioSqlCuentas(FabricaColision(conexion))

    resultado = repositorio.movimiento(
        8,
        {
            "tipo": "recarga",
            "monto": Decimal("5.00"),
            "clave_idempotencia": "recarga-001",
            "concepto": None,
        },
        3,
        "WEB",
    )

    assert resultado == MOVIMIENTO
    assert conexion.reversiones == 1
    assert conexion.confirmaciones == 1
