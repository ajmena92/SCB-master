#!/usr/bin/env python3
"""Importa fotografías históricas por cédula, con modo simulación por defecto."""
from __future__ import annotations

import argparse
import csv
import hashlib
from io import BytesIO
import os
import re
from pathlib import Path

from PIL import Image, ImageOps

MAX_BYTES = 5 * 1024 * 1024
MIMES = {".jpg": "image/jpeg", ".jpeg": "image/jpeg", ".png": "image/png", ".webp": "image/webp"}
MAX_DIMENSION = 800
JPEG_QUALITY = 84


def digits(value: str) -> str:
    return "".join(re.findall(r"\d", value))


def normalize_photo(path: Path) -> tuple[bytes, int, int]:
    """Return a carnet-sized JPEG with EXIF orientation applied."""
    with Image.open(path) as image:
        if image.width < 120 or image.height < 120 or image.width > 5000 or image.height > 5000:
            raise ValueError("dimensiones fuera de 120..5000 píxeles")
        image = ImageOps.exif_transpose(image).convert("RGB")
        image.thumbnail((MAX_DIMENSION, MAX_DIMENSION), Image.Resampling.LANCZOS)
        width, height = image.size
        buffer = BytesIO()
        image.save(buffer, format="JPEG", quality=JPEG_QUALITY, optimize=True, progressive=True)
        return buffer.getvalue(), width, height


def main() -> int:
    import pyodbc

    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--folder", type=Path, required=True)
    parser.add_argument("--apply", action="store_true", help="guardar cambios; sin esta opción solo informa")
    parser.add_argument("--admin-id", type=int, default=None)
    parser.add_argument("--report", type=Path, default=None, help="ruta opcional para reporte CSV")
    args = parser.parse_args()
    connection_string = os.getenv("SQL_CONNECTION_STRING", "").strip()
    if not connection_string:
        raise SystemExit("Defina SQL_CONNECTION_STRING antes de ejecutar la importación")
    if args.apply and not args.admin_id:
        raise SystemExit("--admin-id es obligatorio con --apply")

    with pyodbc.connect(connection_string, autocommit=False) as cn:
        cur = cn.cursor()
        cur.execute("SELECT IdUsuario,Cedula FROM dbo.Usuario WHERE Activo=1")
        students = {digits(str(row.Cedula)): row.IdUsuario for row in cur.fetchall() if digits(str(row.Cedula))}
        counters = {"matched": 0, "missing": 0, "invalid": 0, "skipped": 0}
        report_rows = []
        for path in sorted(args.folder.iterdir(), key=lambda item: item.name.lower()):
            if not path.is_file() or path.suffix.lower() not in MIMES:
                counters["skipped"] += 1
                continue
            carne = digits(path.stem)
            student_id = students.get(carne)
            if not student_id:
                counters["missing"] += 1
                print(f"NO ENCONTRADO\t{path.name}")
                report_rows.append({"archivo": path.name, "cedula": carne, "estado": "NO_ENCONTRADO", "detalle": "No existe estudiante activo"})
                continue
            try:
                content = path.read_bytes()
                if len(content) > MAX_BYTES:
                    raise ValueError("supera 5 MB")
                with Image.open(path) as image:
                    image.verify()
                content, width, height = normalize_photo(path)
            except Exception as exc:
                counters["invalid"] += 1
                print(f"INVALIDA\t{path.name}\t{exc}")
                report_rows.append({"archivo": path.name, "cedula": carne, "estado": "INVALIDA", "detalle": str(exc)})
                continue
            counters["matched"] += 1
            print(f"COINCIDE\t{path.name}\tCedula={carne}\tIdUsuario={student_id}")
            report_rows.append({"archivo": path.name, "cedula": carne, "estado": "COINCIDE", "detalle": f"IdUsuario={student_id}; bytes={len(content)}; dimensiones={width}x{height}"})
            if args.apply:
                cur.execute("""MERGE ComedorPortal.FotoEstudiante AS target
                    USING (SELECT ? AS IdUsuario) AS source ON target.IdUsuario=source.IdUsuario
                    WHEN MATCHED THEN UPDATE SET Contenido=?,TipoMime=?,TamanoBytes=?,Ancho=?,Alto=?,HashSha256=?,
                        FechaCarga=SYSUTCDATETIME(),IdUsuarioCarga=?,Activa=1
                    WHEN NOT MATCHED THEN INSERT (IdUsuario,Contenido,TipoMime,TamanoBytes,Ancho,Alto,HashSha256,IdUsuarioCarga,Activa)
                        VALUES (?,?,?,?,?,?,?,?,1);""",
                    student_id, content, MIMES[path.suffix.lower()], len(content), width, height, hashlib.sha256(content).digest(), args.admin_id,
                    student_id, content, MIMES[path.suffix.lower()], len(content), width, height, hashlib.sha256(content).digest(), args.admin_id)
                cur.execute("""INSERT INTO ComedorPortal.AuditoriaConfirmacion
                    (IdUsuarioEstudiante,IdUsuarioAdmin,Evento,Detalle,DireccionIp)
                    VALUES (?,?,?,?,?)""", student_id, args.admin_id, "FotoCargada", f"Importación inicial: {path.name}", "IMPORT")
        if args.apply:
            cn.commit()
        else:
            cn.rollback()
        if args.report:
            args.report.parent.mkdir(parents=True, exist_ok=True)
            with args.report.open("w", newline="", encoding="utf-8") as report_file:
                writer = csv.DictWriter(report_file, fieldnames=("archivo", "cedula", "estado", "detalle"))
                writer.writeheader()
                writer.writerows(report_rows)
        print(f"Resumen: {counters} | modo={'APLICAR' if args.apply else 'SIMULACIÓN'}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
