#!/usr/bin/env python3
from __future__ import annotations

import argparse
import base64
import csv
import ctypes
import json
import os
import shutil
import subprocess
import sys
import tempfile
import xml.etree.ElementTree as ET
from collections import defaultdict
from datetime import datetime
from pathlib import Path
from typing import Any

SCRIPT_DIR = Path(__file__).resolve().parent
if str(SCRIPT_DIR) not in sys.path:
    sys.path.insert(0, str(SCRIPT_DIR))

from clean_becados_excel import (
    HEADER_COLUMNS,
    build_xlsx,
    deduplicate_rows,
    read_xlsx_rows,
    row_to_record,
    write_clean_csv,
    write_csv,
)


class ImportErrorRuntime(RuntimeError):
    pass


def parse_args(argv: list[str]) -> argparse.Namespace:
    repo_default = Path(__file__).resolve().parent.parent
    parser = argparse.ArgumentParser(
        description="Limpia BECADOS.xlsx y actualiza TipoBeca directamente desde Python."
    )
    parser.add_argument("--repo", default=str(repo_default))
    parser.add_argument("--excel-path", default="")
    parser.add_argument("--output-dir", default="")
    parser.add_argument("--skip-database", action="store_true")
    parser.add_argument("--reset-registros", action="store_true")
    parser.add_argument("--reset-recargas", action="store_true")
    parser.add_argument("--reset-becas-estudiantes", action="store_true")
    parser.add_argument("--allow-missing-users", action="store_true")
    parser.add_argument("--connection-string", default="")
    parser.add_argument("--deployment-config-path", default="")
    parser.add_argument("--app-config-path", default="")
    parser.add_argument("--connection-name", default="Conexion")
    parser.add_argument("--beca-completa-id", type=int, default=2)
    parser.add_argument("--sin-beca-id", type=int, default=1)
    parser.add_argument("--beca-completa-descripcion", default="COMPLETA")
    parser.add_argument("--sin-beca-descripcion", default="NO BENEFICIARIO")
    parser.add_argument("--control-carnet-prefix", default="")
    return parser.parse_args(argv)


def resolve_excel_path(repo: Path, excel_path: str) -> Path:
    if excel_path.strip():
        return Path(excel_path).expanduser().resolve()
    return (repo / "Lista inicial" / "BECADOS.xlsx").resolve()


def resolve_output_dir(repo: Path, output_dir: str) -> Path:
    if output_dir.strip():
        return Path(output_dir).expanduser().resolve()
    return (repo / "artifacts" / "import-becados").resolve()


def build_student_name(conflict: dict[str, str]) -> str:
    parts = [
        conflict.get("apellido_1", "").strip(),
        conflict.get("apellido_2", "").strip(),
        conflict.get("nombre_1", "").strip(),
        conflict.get("nombre_2", "").strip(),
    ]
    return " ".join(part for part in parts if part)


def write_error_log(path: Path, conflicts: list[dict[str, str]]) -> None:
    lines = [
        "LOG DE ERRORES DE IMPORTACION DE BECADOS",
        "",
    ]

    if not conflicts:
        lines.append("Sin errores de cedulas duplicadas conflictivas.")
        path.write_text("\n".join(lines) + "\n", encoding="utf-8")
        return

    grouped: dict[str, list[dict[str, str]]] = defaultdict(list)
    for conflict in conflicts:
        grouped[conflict.get("cedula_normalizada", "").strip()].append(conflict)

    lines.append(
        "Se detectaron cedulas duplicadas conflictivas. Estos registros fueron omitidos del import y la actualizacion continuo con los demas."
    )
    lines.append("")

    for cedula in sorted(grouped):
        lines.append(f"CEDULA: {cedula}")
        for conflict in sorted(grouped[cedula], key=lambda item: int(item.get("fila_excel", "0") or "0")):
            lines.append(
                "  Fila {fila}: {nombre} | Nivel: {nivel} | Modalidad: {modalidad}".format(
                    fila=conflict.get("fila_excel", ""),
                    nombre=build_student_name(conflict),
                    nivel=conflict.get("nivel", ""),
                    modalidad=conflict.get("modalidad", ""),
                )
            )
        lines.append("")

    path.write_text("\n".join(lines), encoding="utf-8")


def append_error_log_section(path: Path, title: str, entries: list[str]) -> None:
    existing = ""
    if path.exists():
        existing = path.read_text(encoding="utf-8").rstrip() + "\n\n"
    lines = [title, ""]
    lines.extend(entries)
    path.write_text(existing + "\n".join(lines).rstrip() + "\n", encoding="utf-8")


def clean_excel(
    source: Path,
    output_dir: Path,
    timestamp: str,
) -> tuple[list[Any], Path, Path, Path, Path, dict[str, Any]]:
    output_dir.mkdir(parents=True, exist_ok=True)
    clean_xlsx = output_dir / f"{source.stem}_limpio_{timestamp}.xlsx"
    clean_csv = output_dir / f"{source.stem}_limpio_{timestamp}.csv"
    conflict_csv = output_dir / f"{source.stem}_conflictos_{timestamp}.csv"
    error_log = output_dir / f"{source.stem}_errores_{timestamp}.log"
    summary_json = output_dir / f"{source.stem}_resumen_{timestamp}.json"

    sheet_name, header, raw_rows = read_xlsx_rows(source)
    records = []
    blank_or_invalid = 0
    padded_cedulas = 0

    for row_offset, row in enumerate(raw_rows, start=2):
        record = row_to_record(row, row_offset)
        if record is None:
            blank_or_invalid += 1
            continue

        original_digits = "".join(ch for ch in record.cedula_original if ch.isdigit())
        if len(original_digits) == 10 and original_digits.startswith("0"):
            padded_cedulas += 1

        records.append(record)

    clean_rows, conflicts, dedupe_summary = deduplicate_rows(records)
    build_xlsx(clean_xlsx, sheet_name, header[: len(HEADER_COLUMNS)], [row.as_excel_row() for row in clean_rows])
    write_clean_csv(clean_csv, clean_rows)
    write_csv(conflict_csv, conflicts)
    write_error_log(error_log, conflicts)

    summary = {
        "source_file": str(source),
        "output_clean_xlsx": str(clean_xlsx),
        "output_clean_csv": str(clean_csv),
        "output_conflicts_csv": str(conflict_csv),
        "output_error_log": str(error_log),
        "sheet_name": sheet_name,
        "rows_read": len(raw_rows),
        "rows_valid_after_basic_cleaning": len(records),
        "rows_blank_or_invalid_omitted": blank_or_invalid,
        "cedulas_leading_zero_removed_from_10_to_9": padded_cedulas,
        **dedupe_summary,
    }
    summary_json.write_text(json.dumps(summary, indent=2, ensure_ascii=False), encoding="utf-8")
    return clean_rows, clean_csv, conflict_csv, error_log, summary_json, summary


def resolve_placeholder_value(value: str) -> str:
    if not value:
        return ""
    prefix = "__SET_IN_ENV__:"
    if value.upper().startswith(prefix):
        env_name = value[len(prefix):].strip()
        return os.environ.get(env_name, "")
    return value


def parse_connection_string(connection_string: str) -> dict[str, Any]:
    parts: dict[str, str] = {}
    for raw_piece in connection_string.split(";"):
        piece = raw_piece.strip()
        if not piece or "=" not in piece:
            continue
        key, value = piece.split("=", 1)
        parts[key.strip().lower()] = resolve_placeholder_value(value.strip())

    server = parts.get("server") or parts.get("data source") or parts.get("datasource") or ""
    database = parts.get("database") or parts.get("initial catalog") or parts.get("initialcatalog") or ""
    integrated_value = parts.get("integrated security") or parts.get("trusted_connection") or ""
    integrated_security = integrated_value.strip().lower() in {"true", "yes", "sspi"}
    username = parts.get("user id") or parts.get("uid") or parts.get("user") or ""
    password = parts.get("password") or parts.get("pwd") or ""

    return {
        "server": server,
        "database": database,
        "integrated_security": integrated_security,
        "username": username,
        "password": password,
    }


def default_deployment_config_candidates() -> list[Path]:
    candidates: list[Path] = []
    if os.name == "nt":
        program_data = Path(os.environ.get("ProgramData", r"C:\ProgramData"))
        candidates.append(program_data / "SCSC" / "deployment.config.json")
    candidates.append(Path("/mnt/c/ProgramData/SCSC/deployment.config.json"))
    return candidates


def find_windows_powershell() -> Path | None:
    candidates = [
        Path("/mnt/c/WINDOWS/System32/WindowsPowerShell/v1.0/powershell.exe"),
        Path("/mnt/c/Windows/System32/WindowsPowerShell/v1.0/powershell.exe"),
    ]
    for candidate in candidates:
        if candidate.exists():
            return candidate
    return None


def default_app_config_path(repo: Path) -> Path:
    return repo / "SCSC" / "app.config"


def decrypt_dpapi_base64_windows(encrypted_password: str, entropy: bytes) -> str:
    if not encrypted_password:
        return ""
    if os.name != "nt":
        windows_powershell = find_windows_powershell()
        if windows_powershell is None:
            raise ImportErrorRuntime(
                "No se puede descifrar deployment.config.json fuera de Windows sin powershell.exe de Windows."
            )

        ps_script = (
            "$ErrorActionPreference='Stop';"
            "Add-Type -AssemblyName System.Security;"
            f"$enc='{encrypted_password}';"
            f"$entropy=[System.Text.Encoding]::UTF8.GetBytes('{entropy.decode('utf-8')}');"
            "$bytes=[Convert]::FromBase64String($enc);"
            "$clear=[System.Security.Cryptography.ProtectedData]::Unprotect("
            "$bytes,$entropy,[System.Security.Cryptography.DataProtectionScope]::LocalMachine);"
            "[Console]::Out.Write([System.Text.Encoding]::UTF8.GetString($clear));"
        )
        completed = subprocess.run(
            [str(windows_powershell), "-NoProfile", "-NonInteractive", "-ExecutionPolicy", "Bypass", "-Command", ps_script],
            capture_output=True,
            text=True,
            encoding="utf-8",
            errors="replace",
        )
        if completed.returncode != 0:
            raise ImportErrorRuntime(
                "No se pudo descifrar deployment.config.json con powershell.exe: "
                + ((completed.stderr or completed.stdout).strip() or "error desconocido")
            )
        return (completed.stdout or "").strip()

    class DataBlob(ctypes.Structure):
        _fields_ = [
            ("cbData", ctypes.c_uint32),
            ("pbData", ctypes.POINTER(ctypes.c_ubyte)),
        ]

    def make_blob(raw: bytes) -> tuple[DataBlob, Any]:
        buffer = ctypes.create_string_buffer(raw, len(raw))
        blob = DataBlob(
            cbData=len(raw),
            pbData=ctypes.cast(buffer, ctypes.POINTER(ctypes.c_ubyte)),
        )
        return blob, buffer

    encrypted_bytes = base64.b64decode(encrypted_password)
    input_blob, input_buffer = make_blob(encrypted_bytes)
    entropy_blob, entropy_buffer = make_blob(entropy)
    output_blob = DataBlob()

    crypt_unprotect = ctypes.windll.crypt32.CryptUnprotectData
    local_free = ctypes.windll.kernel32.LocalFree
    success = crypt_unprotect(
        ctypes.byref(input_blob),
        None,
        ctypes.byref(entropy_blob),
        None,
        None,
        0,
        ctypes.byref(output_blob),
    )
    if not success:
        raise ctypes.WinError()

    try:
        clear_bytes = ctypes.string_at(output_blob.pbData, output_blob.cbData)
        return clear_bytes.decode("utf-8")
    finally:
        local_free(output_blob.pbData)
        _ = input_buffer, entropy_buffer


def resolve_connection_from_deployment_config(path: Path) -> dict[str, Any]:
    data = json.loads(path.read_text(encoding="utf-8"))
    server = str(data.get("Server") or ".").strip() or "."
    database = str(data.get("Database") or "SCSC").strip() or "SCSC"
    auth_mode = str(data.get("AuthenticationMode") or "").strip()
    username = str(data.get("UserName") or "").strip()
    integrated_security = auth_mode.lower() == "windows"
    password = ""
    if not integrated_security:
        password = decrypt_dpapi_base64_windows(
            str(data.get("EncryptedPassword") or ""),
            b"SCSC_DEPLOYMENT_CONFIG_V1",
        )

    return {
        "server": server,
        "database": database,
        "integrated_security": integrated_security,
        "username": username,
        "password": password,
        "source": f"deployment.config.json ({path})",
    }


def resolve_connection_from_app_config(path: Path, connection_name: str) -> dict[str, Any] | None:
    root = ET.fromstring(path.read_text(encoding="utf-8"))

    def find_value(name: str) -> str:
        connection_strings = root.find("connectionStrings")
        if connection_strings is not None:
            for add in connection_strings.findall("add"):
                if (add.get("name") or "").strip() == name:
                    return resolve_placeholder_value(add.get("connectionString", "").strip())

        app_settings = root.find("appSettings")
        if app_settings is not None:
            for add in app_settings.findall("add"):
                if (add.get("key") or "").strip() == name:
                    return resolve_placeholder_value(add.get("value", "").strip())
        return ""

    names_to_try = [connection_name]
    if connection_name != "ConexionLocal":
        names_to_try.append("ConexionLocal")

    for name in names_to_try:
        value = find_value(name)
        if not value:
            continue
        parsed = parse_connection_string(value)
        if parsed["server"] and parsed["database"]:
            parsed["source"] = f"app.config '{name}' ({path})"
            return parsed

    return None


def resolve_connection_info(args: argparse.Namespace, repo: Path) -> dict[str, Any]:
    if args.connection_string.strip():
        connection = parse_connection_string(args.connection_string.strip())
        if not connection["server"] or not connection["database"]:
            raise ImportErrorRuntime("La --connection-string no contiene server/database utilizables.")
        connection["source"] = "cadena explicita"
        return connection

    deployment_error = ""
    deployment_candidates = [Path(args.deployment_config_path).expanduser().resolve()] if args.deployment_config_path.strip() else default_deployment_config_candidates()
    deployment_present = False
    for path in deployment_candidates:
        if not path.exists():
            continue
        deployment_present = True
        try:
            return resolve_connection_from_deployment_config(path)
        except Exception as exc:
            deployment_error = str(exc)

    explicit_app_config = bool(args.app_config_path.strip())
    app_config_path = Path(args.app_config_path).expanduser().resolve() if args.app_config_path.strip() else default_app_config_path(repo)
    if (explicit_app_config or not deployment_present) and app_config_path.exists():
        app_connection = resolve_connection_from_app_config(app_config_path, args.connection_name)
        if app_connection is not None:
            return app_connection

    if deployment_present:
        if deployment_error:
            raise ImportErrorRuntime(deployment_error)
        raise ImportErrorRuntime("Existe deployment.config.json pero no se pudo resolver una conexion util desde la licencia.")

    if deployment_error:
        raise ImportErrorRuntime(deployment_error)
    raise ImportErrorRuntime("No se pudo resolver ninguna conexion SQL util.")


def find_sqlcmd() -> str | None:
    for candidate in ("/home/dev/.local/bin/sqlcmd", "sqlcmd.exe", "sqlcmd"):
        resolved = shutil.which(candidate) if not candidate.startswith("/") else candidate
        if resolved and Path(resolved).exists():
            return resolved
    return None


def sql_escape(value: str) -> str:
    return "N'" + value.replace("'", "''") + "'"


def chunked(items: list[Any], size: int) -> list[list[Any]]:
    return [items[index:index + size] for index in range(0, len(items), size)]


def build_stage_sql(clean_rows: list[Any]) -> str:
    lines = [
        "SET NOCOUNT ON;",
        "IF OBJECT_ID('tempdb..#BecadosImport') IS NOT NULL DROP TABLE #BecadosImport;",
        """
CREATE TABLE #BecadosImport (
  Numero NVARCHAR(50) COLLATE DATABASE_DEFAULT NULL,
  Cedula NVARCHAR(50) COLLATE DATABASE_DEFAULT NOT NULL,
  PrimerApellido NVARCHAR(150) COLLATE DATABASE_DEFAULT NULL,
  SegundoApellido NVARCHAR(150) COLLATE DATABASE_DEFAULT NULL,
  PrimerNombre NVARCHAR(150) COLLATE DATABASE_DEFAULT NULL,
  SegundoNombre NVARCHAR(150) COLLATE DATABASE_DEFAULT NULL,
  NombreCompleto NVARCHAR(300) COLLATE DATABASE_DEFAULT NULL,
  Nivel NVARCHAR(150) COLLATE DATABASE_DEFAULT NULL,
  Modalidad NVARCHAR(150) COLLATE DATABASE_DEFAULT NULL,
  CodigoSolicitud NVARCHAR(50) COLLATE DATABASE_DEFAULT NULL,
  FilaExcel INT NOT NULL
);
""".strip(),
    ]

    columns = (
        "Numero, Cedula, PrimerApellido, SegundoApellido, PrimerNombre, "
        "SegundoNombre, NombreCompleto, Nivel, Modalidad, CodigoSolicitud, FilaExcel"
    )

    for batch in chunked(clean_rows, 100):
        values_sql = []
        for row in batch:
            nombre_completo = " ".join(part for part in [row.nombre_1, row.nombre_2] if part).strip()
            values_sql.append(
                "(" + ", ".join(
                    [
                        sql_escape(row.numero or ""),
                        sql_escape(row.cedula_clave or ""),
                        sql_escape(row.apellido_1 or ""),
                        sql_escape(row.apellido_2 or ""),
                        sql_escape(row.nombre_1 or ""),
                        sql_escape(row.nombre_2 or ""),
                        sql_escape(nombre_completo),
                        sql_escape(row.nivel or ""),
                        sql_escape(row.modalidad or ""),
                        sql_escape(row.solicitud or ""),
                        str(int(row.fila_excel)),
                    ]
                ) + ")"
            )
        lines.append(f"INSERT INTO #BecadosImport ({columns}) VALUES\n" + ",\n".join(values_sql) + ";")

    return "\n".join(lines)


def get_normalized_usuario_cedula_sql_expression(control_carnet_prefix: str) -> str:
    prefix_literal = sql_escape(control_carnet_prefix or "")
    base_expression = (
        "UPPER(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE("
        f"LTRIM(RTRIM(ISNULL(U.Cedula,''))), {prefix_literal}, ''), 'CTPP', ''), ' ', ''), '-', ''), '.', ''), '/', ''))"
    )
    return (
        "CASE WHEN {base} NOT LIKE '%[^0-9]%' AND LEN({base}) = 10 AND LEFT({base}, 1) = '0' "
        "THEN SUBSTRING({base}, 2, 9) ELSE {base} END"
    ).format(base=base_expression)


def write_dict_rows_csv(path: Path, rows: list[dict[str, Any]], fieldnames: list[str]) -> None:
    with path.open("w", newline="", encoding="utf-8-sig") as handle:
        writer = csv.DictWriter(handle, fieldnames=fieldnames)
        writer.writeheader()
        for row in rows:
            writer.writerow(row)


def filter_rows_by_cedulas(clean_rows: list[Any], excluded_cedulas: set[str]) -> list[Any]:
    if not excluded_cedulas:
        return list(clean_rows)
    return [row for row in clean_rows if row.cedula_clave not in excluded_cedulas]


def run_sqlcmd(connection_info: dict[str, Any], sql_text: str, output_dir: Path, label: str) -> str:
    sqlcmd = find_sqlcmd()
    if not sqlcmd:
        raise ImportErrorRuntime("No se encontro sqlcmd. Instala SQL Server command-line tools o proporciona un backend Python compatible.")

    output_dir.mkdir(parents=True, exist_ok=True)
    sql_file = None
    try:
        with tempfile.NamedTemporaryFile(
            mode="w",
            suffix=f"-{label}.sql",
            prefix="import-becados-",
            dir=output_dir,
            encoding="utf-8",
            delete=False,
        ) as handle:
            handle.write(sql_text)
            sql_file = Path(handle.name)

        command = [
            sqlcmd,
            "-b",
            "-W",
            "-s",
            "\t",
            "-h",
            "-1",
            "-S",
            connection_info["server"],
            "-d",
            connection_info["database"],
            "-i",
            str(sql_file),
        ]

        if connection_info["integrated_security"]:
            command.append("-E")
        else:
            command.extend(["-U", connection_info["username"], "-P", connection_info["password"]])

        completed = subprocess.run(
            command,
            capture_output=True,
            text=True,
            encoding="utf-8",
            errors="replace",
        )
        if completed.returncode != 0:
            raise ImportErrorRuntime(
                "sqlcmd fallo para {label}: {stderr}{stdout}Archivo SQL: {sql_file}".format(
                    label=label,
                    stderr=(completed.stderr or "").strip() + ("\n" if completed.stderr else ""),
                    stdout=(completed.stdout or "").strip() + ("\n" if completed.stdout else ""),
                    sql_file=sql_file,
                )
            )

        try:
            sql_file.unlink()
        except OSError:
            pass
        return completed.stdout or ""
    finally:
        _ = sql_file


def parse_sqlcmd_rows(output: str, columns: list[str]) -> list[dict[str, str]]:
    rows: list[dict[str, str]] = []
    for raw_line in output.splitlines():
        line = raw_line.strip()
        if not line:
            continue
        if line.startswith("Changed database context") or line.startswith("Sqlcmd:"):
            continue
        values = [item.strip() for item in raw_line.rstrip("\r").split("\t")]
        if len(values) < len(columns):
            continue
        rows.append(dict(zip(columns, values[: len(columns)])))
    return rows


def get_control_carnet_prefix(connection_info: dict[str, Any], output_dir: Path) -> str:
    output = run_sqlcmd(
        connection_info,
        "SET NOCOUNT ON; SELECT TOP 1 ISNULL(ControlCarnet,'') FROM Parametro ORDER BY Id;",
        output_dir,
        "control-carnet",
    )
    rows = parse_sqlcmd_rows(output, ["ControlCarnet"])
    if not rows:
        return ""
    return rows[0]["ControlCarnet"].strip()


def get_db_duplicate_rows(
    connection_info: dict[str, Any],
    clean_rows: list[Any],
    control_carnet_prefix: str,
    output_dir: Path,
) -> list[dict[str, str]]:
    match_expression = get_normalized_usuario_cedula_sql_expression(control_carnet_prefix)
    sql = build_stage_sql(clean_rows) + f"""
SELECT
  B.Cedula,
  B.NombreCompleto,
  B.FilaExcel,
  COUNT(1) AS Coincidencias
FROM Usuario U
INNER JOIN #BecadosImport B
  ON {match_expression} = B.Cedula
WHERE U.CodTipo = 1
GROUP BY B.Cedula, B.NombreCompleto, B.FilaExcel
HAVING COUNT(1) > 1
ORDER BY B.Cedula, B.FilaExcel;
"""
    output = run_sqlcmd(connection_info, sql, output_dir, "duplicados-db")
    return parse_sqlcmd_rows(output, ["Cedula", "NombreCompleto", "FilaExcel", "Coincidencias"])


def get_not_found_rows(
    connection_info: dict[str, Any],
    clean_rows: list[Any],
    control_carnet_prefix: str,
    output_dir: Path,
) -> list[dict[str, str]]:
    match_expression = get_normalized_usuario_cedula_sql_expression(control_carnet_prefix)
    sql = build_stage_sql(clean_rows) + f"""
SELECT
  B.Cedula,
  B.NombreCompleto,
  B.FilaExcel
FROM #BecadosImport B
LEFT JOIN Usuario U
  ON {match_expression} = B.Cedula
  AND U.CodTipo = 1
WHERE U.IdUsuario IS NULL
ORDER BY B.Cedula, B.FilaExcel;
"""
    output = run_sqlcmd(connection_info, sql, output_dir, "no-encontrados")
    return parse_sqlcmd_rows(output, ["Cedula", "NombreCompleto", "FilaExcel"])


def execute_import(
    connection_info: dict[str, Any],
    clean_rows: list[Any],
    control_carnet_prefix: str,
    args: argparse.Namespace,
    output_dir: Path,
) -> dict[str, str]:
    match_expression = get_normalized_usuario_cedula_sql_expression(control_carnet_prefix)
    sql = build_stage_sql(clean_rows) + f"""
DECLARE @RegistrosComedorEliminados INT = 0;
DECLARE @RegistrosTransporteEliminados INT = 0;
DECLARE @RegistrosDocentesEliminados INT = 0;
DECLARE @UsuariosRecargasReiniciadas INT = 0;
DECLARE @UsuariosBecaReiniciada INT = 0;
DECLARE @UsuariosActualizados INT = 0;

IF {1 if args.reset_registros else 0} = 1
BEGIN
  DELETE FROM RegistroComedor;
  SET @RegistrosComedorEliminados = @@ROWCOUNT;

  DELETE FROM RegistroTransporte;
  SET @RegistrosTransporteEliminados = @@ROWCOUNT;

  DELETE FROM RegistroDocentes;
  SET @RegistrosDocentesEliminados = @@ROWCOUNT;
END

IF {1 if args.reset_recargas else 0} = 1
BEGIN
  UPDATE U
  SET U.CantidadTiquetes = 0
  FROM Usuario U
  WHERE U.CodTipo = 1;

  SET @UsuariosRecargasReiniciadas = @@ROWCOUNT;
END

IF {1 if args.reset_becas_estudiantes else 0} = 1
BEGIN
  UPDATE U
  SET U.TipoBeca = {int(args.sin_beca_id)}
  FROM Usuario U
  WHERE U.CodTipo = 1
    AND ISNULL(U.TipoBeca, -1) <> {int(args.sin_beca_id)};

  SET @UsuariosBecaReiniciada = @@ROWCOUNT;
END

UPDATE U
SET
  U.TipoBeca = {int(args.beca_completa_id)},
  U.Activo = 1
FROM Usuario U
INNER JOIN #BecadosImport B
  ON {match_expression} = B.Cedula
WHERE U.CodTipo = 1
  AND (
    ISNULL(U.TipoBeca, -1) <> {int(args.beca_completa_id)}
    OR ISNULL(CAST(U.Activo AS INT), 0) <> 1
  );

SET @UsuariosActualizados = @@ROWCOUNT;

SELECT
  @RegistrosComedorEliminados AS RegistrosComedorEliminados,
  @RegistrosTransporteEliminados AS RegistrosTransporteEliminados,
  @RegistrosDocentesEliminados AS RegistrosDocentesEliminados,
  @UsuariosRecargasReiniciadas AS UsuariosRecargasReiniciadas,
  @UsuariosBecaReiniciada AS UsuariosBecaReiniciada,
  @UsuariosActualizados AS UsuariosActualizados,
  {int(args.beca_completa_id)} AS IdBecaCompleta,
  {int(args.sin_beca_id)} AS IdBecaSinBeca,
  (SELECT COUNT(1) FROM #BecadosImport) AS TotalExcel,
  (
    SELECT COUNT(1)
    FROM Usuario U
    INNER JOIN #BecadosImport B
      ON {match_expression} = B.Cedula
    WHERE U.CodTipo = 1
  ) AS UsuariosEncontrados;
"""
    output = run_sqlcmd(connection_info, sql, output_dir, "import")
    rows = parse_sqlcmd_rows(
        output,
        [
            "RegistrosComedorEliminados",
            "RegistrosTransporteEliminados",
            "RegistrosDocentesEliminados",
            "UsuariosRecargasReiniciadas",
            "UsuariosBecaReiniciada",
            "UsuariosActualizados",
            "IdBecaCompleta",
            "IdBecaSinBeca",
            "TotalExcel",
            "UsuariosEncontrados",
        ],
    )
    if not rows:
        raise ImportErrorRuntime("No se recibio resumen de la importacion SQL.")
    return rows[0]


def merge_summary_json(path: Path, extra_data: dict[str, Any]) -> None:
    data = json.loads(path.read_text(encoding="utf-8"))
    data.update(extra_data)
    path.write_text(json.dumps(data, indent=2, ensure_ascii=False), encoding="utf-8")


def main(argv: list[str]) -> int:
    args = parse_args(argv)
    repo = Path(args.repo).expanduser().resolve()
    excel_path = resolve_excel_path(repo, args.excel_path)
    output_dir = resolve_output_dir(repo, args.output_dir)
    timestamp = datetime.now().strftime("%Y%m%d-%H%M%S")

    if not excel_path.exists():
        print(f"No existe el archivo Excel: {excel_path}", file=sys.stderr)
        return 1

    clean_rows, clean_csv, conflict_csv, error_log, summary_json, summary = clean_excel(excel_path, output_dir, timestamp)

    print("Limpieza completada:")
    print(f"  CSV limpio: {clean_csv}")
    print(f"  Conflictos: {conflict_csv}")
    print(f"  Log errores: {error_log}")
    print(f"  Resumen: {summary_json}")
    print(f"  Filas limpias: {summary['rows_output']}")
    print(f"  Grupos con conflicto: {summary['conflict_groups']}")
    if summary["conflict_groups"] > 0:
        print("Se omitiran esas cedulas y la actualizacion seguira con los demas registros.")

    if args.skip_database:
        return 0

    connection_info = resolve_connection_info(args, repo)
    print(f"Usando conexion SQL desde: {connection_info['source']}")

    control_carnet_prefix = args.control_carnet_prefix.strip() or get_control_carnet_prefix(connection_info, output_dir)
    if control_carnet_prefix:
        print(f"ControlCarnet detectado: {control_carnet_prefix}")

    db_duplicate_csv = output_dir / f"becados-cedulas-duplicadas-db-{timestamp}.csv"
    not_found_csv = output_dir / f"becados-no-encontrados-{timestamp}.csv"

    effective_rows = list(clean_rows)

    db_duplicates = get_db_duplicate_rows(connection_info, effective_rows, control_carnet_prefix, output_dir)
    if db_duplicates:
        write_dict_rows_csv(db_duplicate_csv, db_duplicates, ["Cedula", "NombreCompleto", "FilaExcel", "Coincidencias"])
        append_error_log_section(
            error_log,
            "CEDULAS DUPLICADAS EN USUARIO",
            [
                "Se omitieron del update porque en Usuario la misma cedula coincide con mas de un estudiante.",
                "",
                *[
                    "  Cedula: {Cedula} | Nombre: {NombreCompleto} | FilaExcel: {FilaExcel} | Coincidencias: {Coincidencias}".format(**row)
                    for row in db_duplicates
                ],
            ],
        )
        effective_rows = filter_rows_by_cedulas(effective_rows, {row["Cedula"] for row in db_duplicates})
        print(f"Usuarios omitidos por cedula duplicada en la base: {len(db_duplicates)}. Detalle: {db_duplicate_csv}")

    not_found_rows = get_not_found_rows(connection_info, effective_rows, control_carnet_prefix, output_dir)
    if not_found_rows:
        write_dict_rows_csv(not_found_csv, not_found_rows, ["Cedula", "NombreCompleto", "FilaExcel"])
        append_error_log_section(
            error_log,
            "BECADOS NO ENCONTRADOS EN USUARIO",
            [
                "Se omitieron del update porque no se localizaron en la tabla Usuario.",
                "",
                *[
                    "  Cedula: {Cedula} | Nombre: {NombreCompleto} | FilaExcel: {FilaExcel}".format(**row)
                    for row in not_found_rows
                ],
            ],
        )
        effective_rows = filter_rows_by_cedulas(effective_rows, {row["Cedula"] for row in not_found_rows})
        print(f"Usuarios no encontrados en Usuario: {len(not_found_rows)}. Detalle: {not_found_csv}")
    else:
        print("Todos los becados del Excel fueron localizados en Usuario.")

    if not effective_rows:
        raise ImportErrorRuntime("No quedaron registros univocos para actualizar despues de omitir conflictos, duplicados en Usuario y no encontrados.")

    summary_db = execute_import(connection_info, effective_rows, control_carnet_prefix, args, output_dir)
    merge_summary_json(
        summary_json,
        {
            "db_duplicate_csv": str(db_duplicate_csv) if db_duplicates else "",
            "not_found_csv": str(not_found_csv) if not_found_rows else "",
            "rows_effective_for_update": len(effective_rows),
            "sql_summary": summary_db,
            "control_carnet_prefix": control_carnet_prefix,
        },
    )

    print("")
    print("Importacion completada.")
    print(f"Total Excel: {summary_db['TotalExcel']}")
    print(f"Usuarios encontrados: {summary_db['UsuariosEncontrados']}")
    print(f"Usuarios actualizados a TipoBeca {args.beca_completa_id}: {summary_db['UsuariosActualizados']}")
    print(f"Registros comedor eliminados: {summary_db['RegistrosComedorEliminados']}")
    print(f"Registros transporte eliminados: {summary_db['RegistrosTransporteEliminados']}")
    print(f"Registros docentes eliminados: {summary_db['RegistrosDocentesEliminados']}")
    print(f"Usuarios con recargas reiniciadas: {summary_db['UsuariosRecargasReiniciadas']}")
    print(f"Usuarios con beca reiniciada a {args.sin_beca_id}: {summary_db['UsuariosBecaReiniciada']}")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main(sys.argv[1:]))
    except ImportErrorRuntime as exc:
        print(str(exc), file=sys.stderr)
        raise SystemExit(1)
