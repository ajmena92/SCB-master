"""Crea o rota el administrador inicial leyendo la contraseña desde un archivo seguro."""

from __future__ import annotations

import argparse
import os
from pathlib import Path

from sqlalchemy import select
from sqlalchemy.orm import Session

import aplicacion.modelos  # noqa: F401
from aplicacion.modelos.maestros import CuentaAdministrativa
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
    if len(contrasena) < 12:
        raise SystemExit("La contraseña debe tener al menos 12 caracteres")
    with Session(crear_motor(url)) as sesion, sesion.begin():
        cuenta = sesion.scalar(
            select(CuentaAdministrativa).where(CuentaAdministrativa.usuario == args.usuario)
        )
        if cuenta is None:
            cuenta = CuentaAdministrativa(
                usuario=args.usuario,
                contrasena_hash=hash_secreto(contrasena),
                rol="administrador",
                activo=True,
            )
            sesion.add(cuenta)
        else:
            cuenta.contrasena_hash = hash_secreto(contrasena)
            cuenta.rol = "administrador"
            cuenta.activo = True
    print(f"Administrador '{args.usuario}' listo")


if __name__ == "__main__":
    main()
