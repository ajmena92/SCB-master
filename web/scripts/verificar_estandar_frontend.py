"""Guardas rápidas para impedir que los módulos evadan el estándar visual."""

from pathlib import Path
import re
import sys


RAIZ = Path(__file__).resolve().parents[1] / "frontend" / "src"
ARCHIVOS = tuple(RAIZ.rglob("*.js")) + tuple(RAIZ.rglob("*.jsx")) + tuple(
    RAIZ.rglob("*.ts")
) + tuple(RAIZ.rglob("*.tsx"))


def main() -> int:
    fallos: list[str] = []
    for archivo in ARCHIVOS:
        texto = archivo.read_text(encoding="utf-8")
        relativo = archivo.relative_to(RAIZ).as_posix()
        if "from \"sonner\"" in texto or "from 'sonner'" in texto:
            permitido = relativo in {
                "compartido/notificaciones/notificaciones.ts",
                "components/ui/sonner.jsx",
            }
            if not permitido:
                fallos.append(f"{relativo}: importa sonner fuera de la capa compartida")
        if re.search(r"\bfont-black\b", texto):
            fallos.append(f"{relativo}: font-black no está permitido")
    if fallos:
        print("\n".join(fallos), file=sys.stderr)
        return 1
    print(f"Estándar frontend verificado: {len(ARCHIVOS)} archivos")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
