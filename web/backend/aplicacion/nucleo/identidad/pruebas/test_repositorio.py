from contextlib import contextmanager
from datetime import datetime, timezone
from typing import Iterator, cast

from aplicacion.nucleo.base_datos import ConexionSql, CursorSql, FabricaConexionSql
from aplicacion.nucleo.identidad.esquemas import SesionPersistida
from aplicacion.nucleo.identidad.repositorio import RepositorioSqlSesiones, RepositorioSqlUsuarios


class CursorDoble:
    description = cast(tuple[tuple[str, object, object, object, object, object, object], ...], ())
    rowcount = 1

    def __init__(self, filas: list[tuple[object, ...]] | None = None) -> None:
        self.filas = filas or []
        self.consultas: list[tuple[str, tuple[object, ...]]] = []

    def execute(self, consulta: str, *parametros: object) -> CursorSql:
        self.consultas.append((consulta, parametros))
        return cast(CursorSql, self)

    def fetchall(self) -> list[tuple[object, ...]]:
        return self.filas

    def fetchone(self) -> tuple[object, ...] | None:
        return self.filas[0] if self.filas else None


class ConexionDoble:
    def __init__(self, cursor: CursorDoble) -> None:
        self.cursor_doble = cursor
        self.confirmada = False

    def cursor(self) -> CursorSql:
        return cast(CursorSql, self.cursor_doble)

    def commit(self) -> None:
        self.confirmada = True

    def rollback(self) -> None:
        raise AssertionError("La prueba no debe revertir")

    def close(self) -> None:
        pass


class FabricaDoble:
    def __init__(self, conexiones: list[ConexionDoble]) -> None:
        self.conexiones = iter(conexiones)

    @contextmanager
    def conexion(self) -> Iterator[ConexionSql]:
        yield cast(ConexionSql, next(self.conexiones))


def test_usuarios_sql_agrega_permisos_de_filas_repetidas() -> None:
    cursor = CursorDoble(
        [
            (7, "operador", "$argon2id$v=19$hash", True, "rutas.administrar"),
            (7, "operador", "$argon2id$v=19$hash", True, "menu.leer"),
        ]
    )
    usuario = RepositorioSqlUsuarios(cast(FabricaConexionSql, FabricaDoble([ConexionDoble(cursor)]))).buscar_por_nombre("operador")
    assert usuario is not None
    assert usuario.id_usuario == 7
    assert usuario.permisos == frozenset({"rutas.administrar", "menu.leer"})
    assert cursor.consultas[0][1] == ("operador",)


def test_usuarios_sql_devuelve_none_si_no_existe() -> None:
    cursor = CursorDoble()
    fabrica = cast(FabricaConexionSql, FabricaDoble([ConexionDoble(cursor)]))
    assert RepositorioSqlUsuarios(fabrica).buscar_por_nombre("nadie") is None


def test_sesiones_sql_persiste_busca_y_revoca_sin_secreto_en_claro() -> None:
    ahora = datetime(2026, 1, 1, tzinfo=timezone.utc)
    cursor = CursorDoble(
        [("sesion-1", 7, "a" * 64, datetime(2026, 1, 1, 1), None, False)]
    )
    conexion = ConexionDoble(cursor)
    fabrica = cast(FabricaConexionSql, FabricaDoble([conexion, conexion, conexion]))
    repositorio = RepositorioSqlSesiones(fabrica)
    sesion = SesionPersistida(idSesion="sesion-1", idUsuario=7, secretoHash="a" * 64, expiraEn=ahora)
    repositorio.guardar(sesion)
    encontrada = repositorio.buscar_vigente("sesion-1", ahora)
    repositorio.revocar("sesion-1", ahora)
    assert encontrada is not None
    assert encontrada.expira_en.tzinfo == timezone.utc
    assert cursor.consultas[0][1][2] == "a" * 64
    assert cursor.consultas[-1][1] == (ahora, "sesion-1")
