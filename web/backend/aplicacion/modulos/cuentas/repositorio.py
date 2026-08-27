"""Persistencia exclusiva del esquema cuentas."""

from typing import Protocol

from aplicacion.nucleo.base_datos import CursorSql, FabricaConexionSql


class RepositorioCuentas(Protocol):
    def saldo(self, id_estudiante: int) -> dict: ...
    def movimiento(self, id_estudiante: int, datos: dict, id_usuario: int, ip: str) -> dict: ...


class RepositorioSqlCuentas:
    def __init__(self, fabrica: FabricaConexionSql) -> None:
        self._fabrica = fabrica

    @staticmethod
    def _fila(cursor: CursorSql) -> dict | None:
        fila = cursor.fetchone()
        if fila is None:
            return None
        return dict(zip((col[0] for col in cursor.description), fila))

    def saldo(self, id_estudiante: int) -> dict:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                "SELECT id_cuenta, id_estudiante, saldo, actualizado_en "
                "FROM cuentas.cuenta_saldo WHERE id_estudiante=?",
                id_estudiante,
            )
            resultado = self._fila(cursor)
            if resultado is None:
                raise ValueError("La cuenta del estudiante no existe")
            return resultado

    @staticmethod
    def _es_conflicto_idempotencia(error: Exception) -> bool:
        """Reconoce los códigos de clave única de SQL Server.

        El INSERT se ejecuta únicamente sobre ``cuentas.movimiento`` y su
        única restricción de unicidad de negocio es la clave de idempotencia.
        Se limita la captura a 2601/2627 para no convertir otros fallos de
        persistencia en respuestas idempotentes.
        """
        return any(codigo in str(error) for codigo in ("2601", "2627"))

    @staticmethod
    def _buscar_movimiento(
        cursor: CursorSql, id_estudiante: int, clave_idempotencia: str
    ) -> dict | None:
        cursor.execute(
            "SELECT id_movimiento, id_cuenta, tipo, monto, saldo_anterior, saldo_nuevo, "
            "clave_idempotencia, concepto, creado_en FROM cuentas.movimiento "
            "WHERE id_estudiante=? AND clave_idempotencia=?",
            id_estudiante,
            clave_idempotencia,
        )
        return RepositorioSqlCuentas._fila(cursor)

    def movimiento(self, id_estudiante: int, datos: dict, id_usuario: int, ip: str) -> dict:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            existente = self._buscar_movimiento(cursor, id_estudiante, datos["clave_idempotencia"])
            if existente is not None:
                return existente
            cursor.execute(
                "SELECT id_cuenta, saldo FROM cuentas.cuenta_saldo WITH (UPDLOCK, ROWLOCK) "
                "WHERE id_estudiante=?",
                id_estudiante,
            )
            cuenta = self._fila(cursor)
            if cuenta is None:
                raise ValueError("La cuenta del estudiante no existe")
            saldo_anterior = cuenta["saldo"]
            delta = datos["monto"] if datos["tipo"] == "recarga" else -datos["monto"]
            if datos["tipo"] == "ajuste":
                delta = datos["monto"]
            saldo_nuevo = saldo_anterior + delta
            if saldo_nuevo < 0:
                raise ValueError("El saldo no puede quedar negativo")
            cursor.execute(
                "UPDATE cuentas.cuenta_saldo SET saldo=?, actualizado_en=SYSUTCDATETIME() "
                "WHERE id_cuenta=?",
                saldo_nuevo,
                cuenta["id_cuenta"],
            )
            try:
                cursor.execute(
                    "INSERT INTO cuentas.movimiento (id_cuenta, id_estudiante, tipo, monto, "
                    "saldo_anterior, saldo_nuevo, clave_idempotencia, concepto, creado_por, direccion_ip) "
                    "OUTPUT INSERTED.id_movimiento, INSERTED.id_cuenta, INSERTED.tipo, INSERTED.monto, "
                    "INSERTED.saldo_anterior, INSERTED.saldo_nuevo, INSERTED.clave_idempotencia, "
                    "INSERTED.concepto, INSERTED.creado_en VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                    cuenta["id_cuenta"],
                    id_estudiante,
                    datos["tipo"],
                    datos["monto"],
                    saldo_anterior,
                    saldo_nuevo,
                    datos["clave_idempotencia"],
                    datos.get("concepto"),
                    id_usuario,
                    ip or "WEB",
                )
            except Exception as error:
                if not self._es_conflicto_idempotencia(error):
                    raise
                # El UPDATE de saldo pertenece a esta transacción y no puede
                # quedar aplicado después de perder la carrera de unicidad.
                conexion.rollback()
                existente = self._buscar_movimiento(
                    cursor, id_estudiante, datos["clave_idempotencia"]
                )
                if existente is None:
                    raise RuntimeError(
                        "La clave de idempotencia colisionó, pero no se pudo recuperar"
                    ) from error
                return existente
            resultado = self._fila(cursor)
            if resultado is None:
                raise RuntimeError("No se pudo registrar el movimiento")
            return resultado
