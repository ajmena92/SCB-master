"""Retira las tablas históricas del menú después de validar la migración."""

import os
import sys

import pyodbc

TABLAS_LEGACY = (
    "ComedorPortal.MenuComponente",
    "ComedorPortal.MenuPlantilla",
)


def _existe(cursor, tabla: str) -> bool:
    esquema, nombre = tabla.split(".", 1)
    cursor.execute(
        "SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = ? AND TABLE_NAME = ?",
        esquema,
        nombre,
    )
    return cursor.fetchone() is not None


def retirar() -> int:
    if os.getenv("BORRAR_TABLAS_MENU_HISTORICAS") != "CONFIRMADO":
        raise RuntimeError(
            "El retiro exige BORRAR_TABLAS_MENU_HISTORICAS=CONFIRMADO. "
            "Primero respalde y valide la migración en staging."
        )

    cadena = os.getenv("SQL_CONNECTION_STRING", "").strip()
    if not cadena:
        raise RuntimeError("SQL_CONNECTION_STRING es requerida para el mantenimiento")

    conexion = pyodbc.connect(cadena, autocommit=False)
    try:
        cursor = conexion.cursor()
        legado_padre = _existe(cursor, TABLAS_LEGACY[1])
        legado_hijo = _existe(cursor, TABLAS_LEGACY[0])
        if not legado_padre and not legado_hijo:
            print("Las tablas históricas del menú ya fueron retiradas.")
            conexion.rollback()
            return 0
        if legado_padre != legado_hijo:
            raise RuntimeError("El legado del menú está incompleto; no se ejecuta ningún DROP")
        if not _existe(cursor, "menu.plantilla") or not _existe(cursor, "menu.componente"):
            raise RuntimeError("Faltan tablas canónicas; no se ejecuta ningún DROP")

        cursor.execute("SELECT COUNT(*) FROM ComedorPortal.MenuPlantilla")
        legacy_plantillas = cursor.fetchone()[0]
        cursor.execute("SELECT COUNT(*) FROM menu.plantilla")
        canonicas_plantillas = cursor.fetchone()[0]
        cursor.execute("SELECT COUNT(*) FROM ComedorPortal.MenuComponente")
        legacy_componentes = cursor.fetchone()[0]
        cursor.execute("SELECT COUNT(*) FROM menu.componente")
        canonicos_componentes = cursor.fetchone()[0]
        if (legacy_plantillas, legacy_componentes) != (canonicas_plantillas, canonicos_componentes):
            raise RuntimeError(
                "Los conteos histórico/canónico no coinciden "
                f"(plantillas {legacy_plantillas}/{canonicas_plantillas}, "
                f"componentes {legacy_componentes}/{canonicos_componentes}); no se ejecuta ningún DROP"
            )

        cursor.execute("DROP TABLE ComedorPortal.MenuComponente")
        cursor.execute("DROP TABLE ComedorPortal.MenuPlantilla")
        conexion.commit()
        print(
            "Retiradas ComedorPortal.MenuComponente y "
            "ComedorPortal.MenuPlantilla después de validar la migración."
        )
        return 0
    except Exception:
        conexion.rollback()
        raise
    finally:
        conexion.close()


if __name__ == "__main__":
    try:
        raise SystemExit(retirar())
    except Exception as error:
        print(f"Mantenimiento cancelado: {error}", file=sys.stderr)
        raise SystemExit(1)
