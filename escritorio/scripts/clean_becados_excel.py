#!/usr/bin/env python3
from __future__ import annotations

import csv
import json
import re
import sys
import zipfile
from collections import defaultdict
from dataclasses import dataclass
from datetime import datetime, timezone
from pathlib import Path
from typing import Dict, Iterable, List, Tuple
from xml.etree import ElementTree as ET


NS_MAIN = "http://schemas.openxmlformats.org/spreadsheetml/2006/main"
NS_REL_OFFICE = "http://schemas.openxmlformats.org/officeDocument/2006/relationships"
NS_REL_PACKAGE = "http://schemas.openxmlformats.org/package/2006/relationships"
NS_CONTENT_TYPES = "http://schemas.openxmlformats.org/package/2006/content-types"
NS_DOC_PROPS = "http://schemas.openxmlformats.org/officeDocument/2006/extended-properties"
NS_VT = "http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes"
NS_CP = "http://schemas.openxmlformats.org/package/2006/metadata/core-properties"
NS_DC = "http://purl.org/dc/elements/1.1/"
NS_DCTERMS = "http://purl.org/dc/terms/"
NS_XSI = "http://www.w3.org/2001/XMLSchema-instance"

ET.register_namespace("", NS_CONTENT_TYPES)
ET.register_namespace("", NS_MAIN)
ET.register_namespace("r", NS_REL_OFFICE)
ET.register_namespace("", NS_REL_PACKAGE)
ET.register_namespace("cp", NS_CP)
ET.register_namespace("dc", NS_DC)
ET.register_namespace("dcterms", NS_DCTERMS)
ET.register_namespace("xsi", NS_XSI)
ET.register_namespace("", NS_DOC_PROPS)
ET.register_namespace("vt", NS_VT)


HEADER_COLUMNS = [
    "N°",
    "Identificación",
    "Apellido 1",
    "Apellido 2",
    "Nombre 1",
    "Nombre 2",
    "Nivel",
    "MODALIDAD",
    "PRESENTÓ SOLICITUD DIGITE 1",
]


@dataclass
class CleanRow:
    numero: str
    identificacion: str
    cedula_clave: str
    apellido_1: str
    apellido_2: str
    nombre_1: str
    nombre_2: str
    nivel: str
    modalidad: str
    solicitud: str
    fila_excel: int
    cedula_original: str

    def as_excel_row(self) -> List[str]:
        return [
            self.numero,
            self.identificacion,
            self.apellido_1,
            self.apellido_2,
            self.nombre_1,
            self.nombre_2,
            self.nivel,
            self.modalidad,
            self.solicitud,
        ]

    def signature(self) -> Tuple[str, ...]:
        return (
            self.apellido_1,
            self.apellido_2,
            self.nombre_1,
            self.nombre_2,
            self.nivel,
            self.modalidad,
            self.solicitud,
        )

    def name_signature(self) -> Tuple[str, ...]:
        return (self.apellido_1, self.apellido_2, self.nombre_1, self.nombre_2)


def column_letters(index: int) -> str:
    value = index + 1
    letters: List[str] = []
    while value > 0:
        value, remainder = divmod(value - 1, 26)
        letters.append(chr(65 + remainder))
    return "".join(reversed(letters))


def normalize_whitespace(value: str) -> str:
    return re.sub(r"\s+", " ", value.strip())


def normalize_text(value: str) -> str:
    cleaned = normalize_whitespace(value)
    return cleaned.upper()


def normalize_solicitud(value: str) -> str:
    cleaned = normalize_whitespace(value)
    if not cleaned:
        return ""
    return re.sub(r"[^0-9A-Z]", "", cleaned.upper())


def normalize_cedula_key(value: str) -> str:
    cleaned = re.sub(r"[^0-9]", "", normalize_whitespace(value))
    if len(cleaned) == 10 and cleaned.startswith("0"):
        cleaned = cleaned[1:]
    return cleaned


def format_cedula(value: str) -> str:
    cedula_key = normalize_cedula_key(value)
    if len(cedula_key) == 9:
        return f"{cedula_key[0]}-{cedula_key[1:5]}-{cedula_key[5:9]}"
    return re.sub(r"[^0-9A-Z]", "", normalize_whitespace(value).upper())


def read_xlsx_rows(path: Path) -> Tuple[str, List[str], List[List[str]]]:
    with zipfile.ZipFile(path) as archive:
        shared_strings = load_shared_strings(archive)
        workbook_xml = ET.fromstring(archive.read("xl/workbook.xml"))
        rels_xml = ET.fromstring(archive.read("xl/_rels/workbook.xml.rels"))
        rel_map = {rel.attrib["Id"]: rel.attrib["Target"] for rel in rels_xml}

        sheet = workbook_xml.find(f"{{{NS_MAIN}}}sheets/{{{NS_MAIN}}}sheet")
        if sheet is None:
            raise RuntimeError("El archivo no contiene hojas.")

        sheet_name = sheet.attrib.get("name", "Lista")
        rel_id = sheet.attrib[f"{{{NS_REL_OFFICE}}}id"]
        target = rel_map[rel_id].lstrip("/")
        sheet_path = "xl/" + target if not target.startswith("xl/") else target

        sheet_xml = ET.fromstring(archive.read(sheet_path))
        rows: List[List[str]] = []
        for row_node in sheet_xml.findall(f"{{{NS_MAIN}}}sheetData/{{{NS_MAIN}}}row"):
            values_by_col: Dict[int, str] = {}
            max_index = -1
            for cell in row_node.findall(f"{{{NS_MAIN}}}c"):
                reference = cell.attrib.get("r", "")
                match = re.match(r"([A-Z]+)", reference)
                if not match:
                    continue
                column_index = letters_to_index(match.group(1))
                values_by_col[column_index] = get_cell_text(cell, shared_strings)
                max_index = max(max_index, column_index)

            if max_index < 0:
                rows.append([])
                continue

            row_values = ["" for _ in range(max_index + 1)]
            for column_index, value in values_by_col.items():
                row_values[column_index] = value
            rows.append(row_values)

    if not rows:
        raise RuntimeError("La hoja no contiene filas.")

    header = rows[0]
    data_rows = rows[1:]
    return sheet_name, header, data_rows


def load_shared_strings(archive: zipfile.ZipFile) -> List[str]:
    if "xl/sharedStrings.xml" not in archive.namelist():
        return []

    root = ET.fromstring(archive.read("xl/sharedStrings.xml"))
    strings: List[str] = []
    for item in root.findall(f"{{{NS_MAIN}}}si"):
        strings.append("".join(node.text or "" for node in item.iter(f"{{{NS_MAIN}}}t")))
    return strings


def letters_to_index(letters: str) -> int:
    result = 0
    for char in letters:
        result = (result * 26) + (ord(char) - 64)
    return result - 1


def get_cell_text(cell: ET.Element, shared_strings: List[str]) -> str:
    data_type = cell.attrib.get("t", "")
    if data_type == "inlineStr":
        return "".join(node.text or "" for node in cell.iter(f"{{{NS_MAIN}}}t")).strip()

    value_node = cell.find(f"{{{NS_MAIN}}}v")
    raw = "" if value_node is None or value_node.text is None else value_node.text.strip()
    if data_type == "s" and raw.isdigit():
        index = int(raw)
        if 0 <= index < len(shared_strings):
            return shared_strings[index].strip()
    if data_type == "b":
        return "TRUE" if raw == "1" else "FALSE"
    return raw


def row_to_record(row: List[str], row_number: int) -> CleanRow | None:
    values = list(row[: len(HEADER_COLUMNS)]) + [""] * max(0, len(HEADER_COLUMNS) - len(row))
    if not any(normalize_whitespace(value) for value in values):
        return None

    numero, cedula, ape1, ape2, nom1, nom2, nivel, modalidad, solicitud = values[: len(HEADER_COLUMNS)]
    cedula_key = normalize_cedula_key(cedula)
    if not cedula_key:
        return None

    return CleanRow(
        numero=normalize_whitespace(numero),
        identificacion=format_cedula(cedula),
        cedula_clave=cedula_key,
        apellido_1=normalize_text(ape1),
        apellido_2=normalize_text(ape2),
        nombre_1=normalize_text(nom1),
        nombre_2=normalize_text(nom2),
        nivel=normalize_text(nivel),
        modalidad=normalize_text(modalidad),
        solicitud=normalize_solicitud(solicitud),
        fila_excel=row_number,
        cedula_original=normalize_whitespace(cedula),
    )


def deduplicate_rows(rows: List[CleanRow]) -> Tuple[List[CleanRow], List[dict], dict]:
    by_cedula: Dict[str, List[CleanRow]] = defaultdict(list)
    for row in rows:
        by_cedula[row.cedula_clave].append(row)

    clean_rows: List[CleanRow] = []
    conflicts: List[dict] = []
    exact_duplicates = 0
    conflict_groups = 0

    for cedula in sorted(by_cedula.keys(), key=lambda value: (value,)):
        group = by_cedula[cedula]
        if len(group) == 1:
            clean_rows.append(group[0])
            continue

        first = group[0]
        if all(item.signature() == first.signature() for item in group[1:]):
            clean_rows.append(first)
            exact_duplicates += len(group) - 1
            continue

        conflict_groups += 1
        for item in group:
            conflicts.append(
                {
                    "cedula_normalizada": item.identificacion,
                    "cedula_clave": item.cedula_clave,
                    "fila_excel": item.fila_excel,
                    "cedula_original": item.cedula_original,
                    "apellido_1": item.apellido_1,
                    "apellido_2": item.apellido_2,
                    "nombre_1": item.nombre_1,
                    "nombre_2": item.nombre_2,
                    "nivel": item.nivel,
                    "modalidad": item.modalidad,
                    "solicitud": item.solicitud,
                    "motivo": "cedula_duplicada_conflictiva",
                }
            )

    summary = {
        "rows_input": len(rows),
        "rows_output": len(clean_rows),
        "exact_duplicates_omitted": exact_duplicates,
        "conflict_groups": conflict_groups,
        "conflict_rows_omitted": len(conflicts),
    }
    clean_rows.sort(key=lambda item: item.fila_excel)
    conflicts.sort(key=lambda item: (item["cedula_normalizada"], item["fila_excel"]))
    return clean_rows, conflicts, summary


def build_xlsx(path: Path, sheet_name: str, header: List[str], rows: Iterable[List[str]]) -> None:
    row_data = [header] + list(rows)
    now = datetime.now(timezone.utc).replace(microsecond=0)
    created = now.isoformat().replace("+00:00", "Z")

    content_types = build_content_types()
    root_rels = build_root_relationships()
    doc_props_app = build_doc_props_app()
    doc_props_core = build_doc_props_core(created)
    workbook = build_workbook(sheet_name)
    workbook_rels = build_workbook_rels()
    styles = build_styles()
    sheet = build_sheet_xml(row_data)

    with zipfile.ZipFile(path, "w", compression=zipfile.ZIP_DEFLATED) as archive:
        archive.writestr("[Content_Types].xml", xml_bytes(content_types))
        archive.writestr("_rels/.rels", xml_bytes(root_rels))
        archive.writestr("docProps/app.xml", xml_bytes(doc_props_app))
        archive.writestr("docProps/core.xml", xml_bytes(doc_props_core))
        archive.writestr("xl/workbook.xml", xml_bytes(workbook))
        archive.writestr("xl/_rels/workbook.xml.rels", xml_bytes(workbook_rels))
        archive.writestr("xl/styles.xml", xml_bytes(styles))
        archive.writestr("xl/worksheets/sheet1.xml", xml_bytes(sheet))


def build_content_types() -> ET.Element:
    root = ET.Element(f"{{{NS_CONTENT_TYPES}}}Types")
    ET.SubElement(root, f"{{{NS_CONTENT_TYPES}}}Default", Extension="rels", ContentType="application/vnd.openxmlformats-package.relationships+xml")
    ET.SubElement(root, f"{{{NS_CONTENT_TYPES}}}Default", Extension="xml", ContentType="application/xml")
    ET.SubElement(root, f"{{{NS_CONTENT_TYPES}}}Override", PartName="/xl/workbook.xml", ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml")
    ET.SubElement(root, f"{{{NS_CONTENT_TYPES}}}Override", PartName="/xl/worksheets/sheet1.xml", ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml")
    ET.SubElement(root, f"{{{NS_CONTENT_TYPES}}}Override", PartName="/xl/styles.xml", ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml")
    ET.SubElement(root, f"{{{NS_CONTENT_TYPES}}}Override", PartName="/docProps/core.xml", ContentType="application/vnd.openxmlformats-package.core-properties+xml")
    ET.SubElement(root, f"{{{NS_CONTENT_TYPES}}}Override", PartName="/docProps/app.xml", ContentType="application/vnd.openxmlformats-officedocument.extended-properties+xml")
    return root


def build_root_relationships() -> ET.Element:
    root = ET.Element(f"{{{NS_REL_PACKAGE}}}Relationships")
    ET.SubElement(root, f"{{{NS_REL_PACKAGE}}}Relationship", Id="rId1", Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument", Target="xl/workbook.xml")
    ET.SubElement(root, f"{{{NS_REL_PACKAGE}}}Relationship", Id="rId2", Type="http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties", Target="docProps/core.xml")
    ET.SubElement(root, f"{{{NS_REL_PACKAGE}}}Relationship", Id="rId3", Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties", Target="docProps/app.xml")
    return root


def build_doc_props_app() -> ET.Element:
    root = ET.Element(f"{{{NS_DOC_PROPS}}}Properties")
    ET.SubElement(root, f"{{{NS_DOC_PROPS}}}Application").text = "Codex"
    ET.SubElement(root, f"{{{NS_DOC_PROPS}}}DocSecurity").text = "0"
    ET.SubElement(root, f"{{{NS_DOC_PROPS}}}ScaleCrop").text = "false"
    heading_pairs = ET.SubElement(root, f"{{{NS_DOC_PROPS}}}HeadingPairs")
    vector = ET.SubElement(heading_pairs, f"{{{NS_VT}}}vector", size="2", baseType="variant")
    variant1 = ET.SubElement(vector, f"{{{NS_VT}}}variant")
    ET.SubElement(variant1, f"{{{NS_VT}}}lpstr").text = "Worksheets"
    variant2 = ET.SubElement(vector, f"{{{NS_VT}}}variant")
    ET.SubElement(variant2, f"{{{NS_VT}}}i4").text = "1"
    titles = ET.SubElement(root, f"{{{NS_DOC_PROPS}}}TitlesOfParts")
    titles_vector = ET.SubElement(titles, f"{{{NS_VT}}}vector", size="1", baseType="lpstr")
    ET.SubElement(titles_vector, f"{{{NS_VT}}}lpstr").text = "BECADOS limpio"
    ET.SubElement(root, f"{{{NS_DOC_PROPS}}}Company").text = ""
    ET.SubElement(root, f"{{{NS_DOC_PROPS}}}LinksUpToDate").text = "false"
    ET.SubElement(root, f"{{{NS_DOC_PROPS}}}SharedDoc").text = "false"
    ET.SubElement(root, f"{{{NS_DOC_PROPS}}}HyperlinksChanged").text = "false"
    ET.SubElement(root, f"{{{NS_DOC_PROPS}}}AppVersion").text = "1.0"
    return root


def build_doc_props_core(created: str) -> ET.Element:
    root = ET.Element(f"{{{NS_CP}}}coreProperties")
    ET.SubElement(root, f"{{{NS_DC}}}title").text = "BECADOS limpio"
    ET.SubElement(root, f"{{{NS_DC}}}creator").text = "Codex"
    ET.SubElement(root, f"{{{NS_CP}}}lastModifiedBy").text = "Codex"
    ET.SubElement(root, f"{{{NS_DCTERMS}}}created", {f"{{{NS_XSI}}}type": "dcterms:W3CDTF"}).text = created
    ET.SubElement(root, f"{{{NS_DCTERMS}}}modified", {f"{{{NS_XSI}}}type": "dcterms:W3CDTF"}).text = created
    return root


def build_workbook(sheet_name: str) -> ET.Element:
    root = ET.Element(f"{{{NS_MAIN}}}workbook")
    sheets = ET.SubElement(root, f"{{{NS_MAIN}}}sheets")
    ET.SubElement(sheets, f"{{{NS_MAIN}}}sheet", {f"{{{NS_REL_OFFICE}}}id": "rId1", "sheetId": "1", "name": sheet_name})
    return root


def build_workbook_rels() -> ET.Element:
    root = ET.Element(f"{{{NS_REL_PACKAGE}}}Relationships")
    ET.SubElement(root, f"{{{NS_REL_PACKAGE}}}Relationship", Id="rId1", Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet", Target="worksheets/sheet1.xml")
    ET.SubElement(root, f"{{{NS_REL_PACKAGE}}}Relationship", Id="rId2", Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles", Target="styles.xml")
    return root


def build_styles() -> ET.Element:
    root = ET.Element(f"{{{NS_MAIN}}}styleSheet")
    fonts = ET.SubElement(root, f"{{{NS_MAIN}}}fonts", count="1")
    font = ET.SubElement(fonts, f"{{{NS_MAIN}}}font")
    ET.SubElement(font, f"{{{NS_MAIN}}}sz", val="11")
    ET.SubElement(font, f"{{{NS_MAIN}}}name", val="Calibri")
    fills = ET.SubElement(root, f"{{{NS_MAIN}}}fills", count="2")
    ET.SubElement(ET.SubElement(fills, f"{{{NS_MAIN}}}fill"), f"{{{NS_MAIN}}}patternFill", patternType="none")
    ET.SubElement(ET.SubElement(fills, f"{{{NS_MAIN}}}fill"), f"{{{NS_MAIN}}}patternFill", patternType="gray125")
    borders = ET.SubElement(root, f"{{{NS_MAIN}}}borders", count="1")
    border = ET.SubElement(borders, f"{{{NS_MAIN}}}border")
    ET.SubElement(border, f"{{{NS_MAIN}}}left")
    ET.SubElement(border, f"{{{NS_MAIN}}}right")
    ET.SubElement(border, f"{{{NS_MAIN}}}top")
    ET.SubElement(border, f"{{{NS_MAIN}}}bottom")
    ET.SubElement(border, f"{{{NS_MAIN}}}diagonal")
    cell_style_xfs = ET.SubElement(root, f"{{{NS_MAIN}}}cellStyleXfs", count="1")
    ET.SubElement(cell_style_xfs, f"{{{NS_MAIN}}}xf", numFmtId="0", fontId="0", fillId="0", borderId="0")
    cell_xfs = ET.SubElement(root, f"{{{NS_MAIN}}}cellXfs", count="1")
    ET.SubElement(cell_xfs, f"{{{NS_MAIN}}}xf", numFmtId="0", fontId="0", fillId="0", borderId="0", xfId="0")
    cell_styles = ET.SubElement(root, f"{{{NS_MAIN}}}cellStyles", count="1")
    ET.SubElement(cell_styles, f"{{{NS_MAIN}}}cellStyle", name="Normal", xfId="0", builtinId="0")
    return root


def build_sheet_xml(rows: List[List[str]]) -> ET.Element:
    worksheet = ET.Element(f"{{{NS_MAIN}}}worksheet")
    ET.SubElement(worksheet, f"{{{NS_MAIN}}}dimension", ref=f"A1:I{max(1, len(rows))}")
    sheet_views = ET.SubElement(worksheet, f"{{{NS_MAIN}}}sheetViews")
    ET.SubElement(sheet_views, f"{{{NS_MAIN}}}sheetView", workbookViewId="0")
    ET.SubElement(worksheet, f"{{{NS_MAIN}}}sheetFormatPr", defaultRowHeight="15")
    cols = ET.SubElement(worksheet, f"{{{NS_MAIN}}}cols")
    for index, width in enumerate((10, 16, 18, 18, 18, 18, 32, 38, 18), start=1):
        ET.SubElement(cols, f"{{{NS_MAIN}}}col", min=str(index), max=str(index), width=str(width), customWidth="1")
    sheet_data = ET.SubElement(worksheet, f"{{{NS_MAIN}}}sheetData")

    for row_index, row_values in enumerate(rows, start=1):
        row_node = ET.SubElement(sheet_data, f"{{{NS_MAIN}}}row", r=str(row_index))
        for col_index, value in enumerate(row_values):
            cell_ref = f"{column_letters(col_index)}{row_index}"
            cell = ET.SubElement(row_node, f"{{{NS_MAIN}}}c", r=cell_ref, t="inlineStr")
            inline = ET.SubElement(cell, f"{{{NS_MAIN}}}is")
            ET.SubElement(inline, f"{{{NS_MAIN}}}t").text = value

    ET.SubElement(worksheet, f"{{{NS_MAIN}}}pageMargins", left="0.7", right="0.7", top="0.75", bottom="0.75", header="0.3", footer="0.3")
    return worksheet


def xml_bytes(root: ET.Element) -> bytes:
    return ET.tostring(root, encoding="utf-8", xml_declaration=True)


def write_csv(path: Path, rows: List[dict]) -> None:
    fieldnames = list(rows[0].keys()) if rows else ["cedula_normalizada", "fila_excel", "motivo"]
    with path.open("w", newline="", encoding="utf-8-sig") as handle:
        writer = csv.DictWriter(handle, fieldnames=fieldnames)
        writer.writeheader()
        for row in rows:
            writer.writerow(row)


def write_clean_csv(path: Path, rows: List[CleanRow]) -> None:
    with path.open("w", newline="", encoding="utf-8-sig") as handle:
        writer = csv.writer(handle)
        writer.writerow(HEADER_COLUMNS)
        for row in rows:
            writer.writerow(row.as_excel_row())


def main(argv: List[str]) -> int:
    if len(argv) < 2:
        print("uso: clean_becados_excel.py <archivo_xlsx> [directorio_salida]", file=sys.stderr)
        return 1

    source = Path(argv[1]).resolve()
    output_dir = Path(argv[2]).resolve() if len(argv) > 2 else source.parent.resolve()
    if not source.exists():
        print(f"no existe el archivo: {source}", file=sys.stderr)
        return 1

    output_dir.mkdir(parents=True, exist_ok=True)
    timestamp = datetime.now().strftime("%Y%m%d-%H%M%S")
    clean_xlsx = output_dir / f"{source.stem}_limpio_{timestamp}.xlsx"
    clean_csv = output_dir / f"{source.stem}_limpio_{timestamp}.csv"
    conflict_csv = output_dir / f"{source.stem}_conflictos_{timestamp}.csv"
    summary_json = output_dir / f"{source.stem}_resumen_{timestamp}.json"

    sheet_name, header, raw_rows = read_xlsx_rows(source)
    records: List[CleanRow] = []
    blank_or_invalid = 0
    padded_cedulas = 0

    for row_offset, row in enumerate(raw_rows, start=2):
        record = row_to_record(row, row_offset)
        if record is None:
            blank_or_invalid += 1
            continue
        original_digits = re.sub(r"[^0-9]", "", record.cedula_original)
        if len(original_digits) == 10 and original_digits.startswith("0"):
            padded_cedulas += 1
        records.append(record)

    clean_rows, conflicts, dedupe_summary = deduplicate_rows(records)
    build_xlsx(clean_xlsx, sheet_name, header[: len(HEADER_COLUMNS)], [row.as_excel_row() for row in clean_rows])
    write_clean_csv(clean_csv, clean_rows)
    write_csv(conflict_csv, conflicts)

    summary = {
        "source_file": str(source),
        "output_clean_xlsx": str(clean_xlsx),
        "output_clean_csv": str(clean_csv),
        "output_conflicts_csv": str(conflict_csv),
        "sheet_name": sheet_name,
        "rows_read": len(raw_rows),
        "rows_valid_after_basic_cleaning": len(records),
        "rows_blank_or_invalid_omitted": blank_or_invalid,
        "cedulas_leading_zero_removed_from_10_to_9": padded_cedulas,
        **dedupe_summary,
    }
    summary_json.write_text(json.dumps(summary, indent=2, ensure_ascii=False), encoding="utf-8")

    print(json.dumps(summary, indent=2, ensure_ascii=False))
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv))
