"""Crea o rota el administrador inicial leyendo la contraseña desde un archivo seguro."""

from __future__ import annotations

import argparse
import os
from pathlib import Path

from sqlalchemy import delete, func, select
from sqlalchemy.orm import Session

import aplicacion.modelos  # noqa: F401
from aplicacion.modelos.maestros import CuentaAdministrativa, SesionAcceso
from aplicacion.nucleo.postgresql import crear_motor
from aplicacion.seguridad import hash_secreto


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--usuario", required=True)
    parser.add_argument("--contrasena-file", required=True)
    args = parser.parse_args()
    url = os.environ.get("DATABASE_URL", "")
    if not url.startswith(("postgresql://", "postgresql+psycopg://")):
        raise SystemExit("DATABASE_URL PostgreSQL es requerida")
    contrasena = Path(args.contrasena_file).read_text(encoding="utf-8").strip()
    if len(contrasena) < 10:
        raise SystemExit("La contraseña debe tener al menos 10 caracteres")
    usuario = args.usuario.strip().lower()
    with Session(crear_motor(url)) as sesion, sesion.begin():
        cuenta = sesion.scalar(
            select(CuentaAdministrativa).where(func.lower(CuentaAdministrativa.usuario) == usuario)
        )
        if cuenta is None:
            cuenta = CuentaAdministrativa(
                usuario=usuario,
                contrasena_hash=hash_secreto(contrasena),
                rol="administrador",
                activo=True,
                persona_id=None,
                vinculacion_pendiente=True,
            )
            sesion.add(cuenta)
            sesion.flush()
        else:
            cuenta.usuario = usuario
            cuenta.contrasena_hash = hash_secreto(contrasena)
            cuenta.rol = "administrador"
            cuenta.activo = True
        sesion.execute(delete(SesionAcceso).where(SesionAcceso.cuenta_id == cuenta.id))
    print(f"Administrador '{usuario}' listo")


if __name__ == "__main__":
    main()
