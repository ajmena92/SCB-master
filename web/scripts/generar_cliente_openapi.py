"""Genera los contratos TypeScript de la API canónica a partir de OpenAPI.

La aplicación se importa con dependencias SQL ficticias: generar el contrato no
abre conexiones ni necesita credenciales. La salida se ordena para que CI pueda
detectar cambios del contrato sin depender del entorno de ejecución.
"""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, cast


RAIZ = Path(__file__).resolve().parents[2]
BACKEND = RAIZ / "web" / "backend"
SALIDA = RAIZ / "web" / "frontend" / "src" / "compartido" / "contratos" / "api.ts"


def _tipo_esquema(esquema: dict[str, Any], nombres: dict[str, str]) -> str:
    if "$ref" in esquema:
        return nombres[esquema["$ref"].rsplit("/", 1)[-1]]
    if "anyOf" in esquema:
        opciones = [_tipo_esquema(opcion, nombres) for opcion in esquema["anyOf"]]
        opciones = [opcion for opcion in opciones if opcion != "unknown"]
        return " | ".join(dict.fromkeys(opciones)) or "unknown"
    if "oneOf" in esquema:
        opciones = [_tipo_esquema(opcion, nombres) for opcion in esquema["oneOf"]]
        return " | ".join(dict.fromkeys(opciones)) or "unknown"
    if "allOf" in esquema:
        opciones = [_tipo_esquema(opcion, nombres) for opcion in esquema["allOf"]]
        return " & ".join(opciones) or "unknown"
    if "enum" in esquema:
        return " | ".join(json.dumps(valor, ensure_ascii=False) for valor in esquema["enum"])
    if esquema.get("type") == "array":
        return f"Array<{_tipo_esquema(esquema.get('items', {}), nombres)}>"
    if esquema.get("type") == "object" or "properties" in esquema:
        return "Record<string, unknown>"
    return {
        "null": "null",
        "string": "string",
        "integer": "number",
        "number": "number",
        "boolean": "boolean",
    }.get(esquema.get("type"), "unknown")


def _nombre_tipo(nombre: str) -> str:
    partes = [parte for parte in nombre.replace("-", "_").split("_") if parte]
    return "".join(parte[:1].upper() + parte[1:] for parte in partes) or "Esquema"


def _generar(especificacion: dict[str, Any]) -> str:
    esquemas = especificacion.get("components", {}).get("schemas", {})
    nombres = {nombre: _nombre_tipo(nombre) for nombre in esquemas}
    lineas = [
        "/* eslint-disable */",
        "/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */",
        "",
        "export type MetodoHttp = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE';",
        "",
    ]
    for nombre in sorted(esquemas):
        esquema = esquemas[nombre]
        tipo = nombres[nombre]
        propiedades = esquema.get("properties", {})
        requeridas = set(esquema.get("required", []))
        if not propiedades:
            lineas.append(f"export type {tipo} = {_tipo_esquema(esquema, nombres)};")
        else:
            lineas.append(f"export interface {tipo} {{")
            for propiedad in sorted(propiedades):
                opcional = "" if propiedad in requeridas else "?"
                clave = propiedad if propiedad.isidentifier() else json.dumps(propiedad, ensure_ascii=False)
                lineas.append(f"  {clave}{opcional}: {_tipo_esquema(propiedades[propiedad], nombres)};")
            lineas.append("}")
        lineas.append("")

    lineas.extend([
        "export interface OperacionApi {",
        "  metodo: MetodoHttp;",
        "  ruta: string;",
        "  operacionId: string;",
        "}",
        "",
        "export const OPERACIONES_API: readonly OperacionApi[] = [",
    ])
    for ruta in sorted(especificacion.get("paths", {})):
        for metodo in sorted(especificacion["paths"][ruta]):
            operacion = especificacion["paths"][ruta][metodo]
            if metodo not in {"get", "post", "put", "patch", "delete"}:
                continue
            operacion_id = operacion.get("operationId", f"{metodo}_{ruta}")
            lineas.append(
                f"  {{ metodo: {metodo.upper()!r}, ruta: {ruta!r}, operacionId: {operacion_id!r} }},"
            )
    lineas.extend(["] as const;", ""])
    return "\n".join(lineas)


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--verificar", action="store_true", help="falla si la salida no está actualizada")
    args = parser.parse_args()
    sys.path.insert(0, str(BACKEND))
    from aplicacion.entrada import DependenciasAplicacion, crear_aplicacion
    from aplicacion.nucleo.base_datos import FabricaConexionSql

    aplicacion = crear_aplicacion(
        DependenciasAplicacion(cast(FabricaConexionSql, object()), cookies_seguras=False)
    )
    salida = _generar(aplicacion.openapi())
    actual = SALIDA.read_text(encoding="utf-8") if SALIDA.exists() else None
    if args.verificar:
        if actual != salida:
            print(f"Contrato OpenAPI desactualizado: {SALIDA}", file=sys.stderr)
            return 1
        return 0
    SALIDA.parent.mkdir(parents=True, exist_ok=True)
    SALIDA.write_text(salida, encoding="utf-8")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
