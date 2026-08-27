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
DIRECTORIO_SALIDA = RAIZ / "web" / "frontend" / "src" / "compartido" / "contratos"
SALIDA_INDICE = DIRECTORIO_SALIDA / "api.ts"
DIRECTORIO_OPERACIONES = DIRECTORIO_SALIDA / "operaciones"
DOMINIOS = (
    "identidad",
    "estudiantes",
    "transporte",
    "asistencia",
    "beneficios",
    "cuentas",
    "reportes",
    "importaciones",
    "auditoria",
    "menu",
    "comedor",
    "soporte",
    "administracion",
    "parametros",
    "salud",
    "comunes",
)

ESQUEMAS_POR_DOMINIO = {
    "identidad": {"AccesoEstudiante", "AutenticacionSalida", "CambioPinEstudiante", "CredencialesEntrada", "SesionActualSalida"},
    "estudiantes": {"AsignacionEntrada", "AsignacionSalida", "Body_cargar_api_v1_estudiantes__id_estudiante__foto_post", "CambioAsignacion", "EstudianteEntrada", "EstudianteSalida", "GeneracionPinesSeccion", "PaginaEstudiantes", "PinGenerado"},
    "transporte": {"RutaEntrada", "RutaSalida"},
    "asistencia": {"CorreccionEntrada", "MarcaEntrada", "MarcaSalida"},
    "beneficios": {"BeneficioEntrada", "BeneficioSalida"},
    "cuentas": {"MovimientoEntrada", "MovimientoSalida", "SaldoSalida"},
    "reportes": {"ReporteEstudiante", "ReporteEstudiantes", "ReporteRuta", "ReporteTransporte"},
    "importaciones": {"Body_ejecutar_api_v1_importaciones_lotes_post", "Body_previsualizar_api_v1_importaciones_previsualizaciones_post", "ErrorFila", "LoteSalida", "Previsualizacion"},
    "auditoria": {"EventoSalida"},
    "menu": {"ComponenteMenu", "PlantillaMenuEntrada", "PlantillaMenuSalida"},
    "comedor": {"RegistroComedorEntrada", "RegistroComedorSalida"},
    "soporte": {"SolicitudEntrada", "SolicitudSalida"},
    "administracion": {"PermisoSalida", "RolEntrada", "RolSalida", "UsuarioEntrada", "UsuarioSalida"},
    "parametros": {"DiaCalendario", "ParametrosEntrada", "ParametrosSalida"},
    "salud": {"EstadoSalud"},
    "comunes": {"HTTPValidationError", "ValidationError"},
}


def _dominio_ruta(ruta: str) -> str:
    """Clasifica una ruta OpenAPI según su primer segmento funcional."""
    segmentos = [segmento for segmento in ruta.split("/") if segmento]
    if not segmentos:
        return "comunes"
    if segmentos[0] == "api" and len(segmentos) == 2:
        return "salud"
    if len(segmentos) > 1 and segmentos[1] == "v1":
        segmentos = segmentos[2:]
    if not segmentos or segmentos[0] in {"autenticacion", "sesion"}:
        return "identidad"
    if segmentos[0] == "calendario":
        return "parametros"
    if segmentos[0] == "estudiantes":
        return "estudiantes"
    return segmentos[0] if segmentos[0] in DOMINIOS else "comunes"


def _referencias(esquema: dict[str, Any]) -> set[str]:
    referencias: set[str] = set()

    def visitar(valor: Any) -> None:
        if isinstance(valor, dict):
            if "$ref" in valor:
                referencias.add(valor["$ref"].rsplit("/", 1)[-1])
            for hijo in valor.values():
                visitar(hijo)
        elif isinstance(valor, list):
            for hijo in valor:
                visitar(hijo)

    visitar(esquema)
    return referencias


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


def _generar_esquemas(
    especificacion: dict[str, Any], dominio: str, propietarios: dict[str, str]
) -> str:
    esquemas = especificacion.get("components", {}).get("schemas", {})
    nombres = {nombre: _nombre_tipo(nombre) for nombre in esquemas}
    propios = sorted(ESQUEMAS_POR_DOMINIO[dominio] & set(esquemas))
    referencias_externas = {
        referencia
        for nombre in propios
        for referencia in _referencias(esquemas[nombre])
        if propietarios.get(referencia) != dominio
    }
    lineas = [
        "/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */",
        "",
    ]
    for referencia in sorted(referencias_externas):
        origen = propietarios[referencia]
        lineas.append(f'import type {{ {nombres[referencia]} }} from "./{origen}";')
    if referencias_externas:
        lineas.append("")
    for nombre in propios:
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

    return "\n".join(lineas)


def _operaciones_por_dominio(especificacion: dict[str, Any]) -> dict[str, list[str]]:
    operaciones = {dominio: [] for dominio in DOMINIOS}
    for ruta in sorted(especificacion.get("paths", {})):
        for metodo in sorted(especificacion["paths"][ruta]):
            operacion = especificacion["paths"][ruta][metodo]
            if metodo not in {"get", "post", "put", "patch", "delete"}:
                continue
            dominio = _dominio_ruta(ruta)
            operacion_id = operacion.get("operationId", f"{metodo}_{ruta}")
            metodo_texto = json.dumps(metodo.upper())
            ruta_texto = json.dumps(ruta)
            operacion_texto = json.dumps(operacion_id)
            linea = (
                f"  {{ metodo: {metodo_texto}, ruta: {ruta_texto}, "
                f"operacionId: {operacion_texto}, dominio: {json.dumps(dominio)} }},"
            )
            if len(linea) <= 100:
                operaciones[dominio].append(linea)
            else:
                operaciones[dominio].extend(
                    [
                        "  {",
                        f"    metodo: {metodo_texto},",
                        f"    ruta: {ruta_texto},",
                        f"    operacionId: {operacion_texto},",
                        f"    dominio: {json.dumps(dominio)},",
                        "  },",
                    ]
                )
    return operaciones


def _generar_operaciones_dominio(dominio: str, operaciones: list[str]) -> str:
    constante = f"OPERACIONES_{dominio.upper()}"
    if not operaciones:
        return "\n".join(
            [
                "/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */",
                "",
                'import type { OperacionApi } from "../operaciones";',
                "",
                f"export const {constante}: readonly OperacionApi[] = [] as const;",
                "",
            ]
        )
    lineas = [
        "/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */",
        "",
        'import type { OperacionApi } from "../operaciones";',
        "",
        f"export const {constante}: readonly OperacionApi[] = [",
        *operaciones,
        "] as const;",
        "",
    ]
    return "\n".join(lineas)


def _generar_operaciones(especificacion: dict[str, Any]) -> str:
    operaciones = _operaciones_por_dominio(especificacion)
    imports = [
        f'import {{ OPERACIONES_{dominio.upper()} }} from "./operaciones/{dominio}";'
        for dominio in DOMINIOS
    ]
    lineas = [
        "/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */",
        "",
        *imports,
        "",
        'export type MetodoHttp = "GET" | "POST" | "PUT" | "PATCH" | "DELETE";',
        "",
        "export interface OperacionApi {",
        "  metodo: MetodoHttp;",
        "  ruta: string;",
        "  operacionId: string;",
        "  dominio: string;",
        "}",
        "",
        "export const OPERACIONES_API: readonly OperacionApi[] = [",
    ]
    for dominio in DOMINIOS:
        lineas.append(f"  ...OPERACIONES_{dominio.upper()},")
    lineas.extend(["] as const;", ""])
    return "\n".join(lineas)


def _generar_indice() -> str:
    lineas = [
        "/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */",
        "",
    ]
    lineas.extend(f'export * from "./{dominio}";' for dominio in DOMINIOS)
    lineas.append('export * from "./operaciones";')
    lineas.append("")
    return "\n".join(lineas)


def _generar(especificacion: dict[str, Any]) -> dict[Path, str]:
    esquemas = especificacion.get("components", {}).get("schemas", {})
    propietarios = {
        esquema: dominio
        for dominio, nombres in ESQUEMAS_POR_DOMINIO.items()
        for esquema in nombres
        if esquema in esquemas
    }
    faltantes = set(esquemas) - set(propietarios)
    if faltantes:
        raise RuntimeError(f"Esquemas sin dominio asignado: {sorted(faltantes)}")
    salidas = {
        DIRECTORIO_SALIDA / f"{dominio}.ts": _generar_esquemas(especificacion, dominio, propietarios)
        for dominio in DOMINIOS
    }
    salidas[DIRECTORIO_SALIDA / "operaciones.ts"] = _generar_operaciones(especificacion)
    salidas.update(
        {
            DIRECTORIO_OPERACIONES / f"{dominio}.ts": _generar_operaciones_dominio(dominio, operaciones)
            for dominio, operaciones in _operaciones_por_dominio(especificacion).items()
        }
    )
    salidas[SALIDA_INDICE] = _generar_indice()
    return salidas


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
    salidas = _generar(aplicacion.openapi())
    for salida in salidas:
        salida.parent.mkdir(parents=True, exist_ok=True)
    if args.verificar:
        for salida, contenido in salidas.items():
            actual = salida.read_text(encoding="utf-8") if salida.exists() else None
            if actual != contenido:
                print(f"Contrato OpenAPI desactualizado: {salida}", file=sys.stderr)
                return 1
        return 0
    DIRECTORIO_SALIDA.mkdir(parents=True, exist_ok=True)
    for salida, contenido in salidas.items():
        salida.write_text(contenido, encoding="utf-8")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
