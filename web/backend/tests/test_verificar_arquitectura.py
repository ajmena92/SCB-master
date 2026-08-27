"""Pruebas de contrato para los verificadores arquitectónicos de Fase 0."""

from __future__ import annotations

import importlib.util
from pathlib import Path

RAIZ_WEB = Path(__file__).resolve().parents[2]
RUTA_VERIFICADOR = RAIZ_WEB / "scripts" / "verificar_arquitectura.py"
ESPECIFICACION = importlib.util.spec_from_file_location("verificar_arquitectura", RUTA_VERIFICADOR)
assert ESPECIFICACION and ESPECIFICACION.loader
verificador = importlib.util.module_from_spec(ESPECIFICACION)
ESPECIFICACION.loader.exec_module(verificador)


def escribir(raiz: Path, relativa: str, contenido: str) -> None:
    ruta = raiz / relativa
    ruta.parent.mkdir(parents=True, exist_ok=True)
    ruta.write_text(contenido, encoding="utf-8")


def test_detecta_referencia_de_ejecucion_hacia_escritorio(tmp_path: Path) -> None:
    escribir(tmp_path, "backend/aplicacion/nucleo/configuracion.py", "RUTA = '../escritorio'\n")

    hallazgos = verificador.buscar_referencias_escritorio(tmp_path)

    assert len(hallazgos) == 1
    assert "escritorio" in hallazgos[0].mensaje


def test_no_revisa_documentacion_ni_fuentes_legacy(tmp_path: Path) -> None:
    escribir(tmp_path, "docs/notas.md", "escritorio es historial\n")
    escribir(tmp_path, "frontend/src/components/Legacy.jsx", "const origen = 'escritorio';\n")

    assert verificador.buscar_referencias_escritorio(tmp_path) == []


def test_detecta_sql_en_servicio_pero_permite_repositorio(tmp_path: Path) -> None:
    escribir(
        tmp_path, "backend/aplicacion/modulos/menu/servicio.py", "consulta = 'SELECT * FROM menu'\n"
    )
    escribir(
        tmp_path,
        "backend/aplicacion/modulos/menu/repositorio.py",
        "consulta = 'SELECT * FROM menu'\n",
    )

    hallazgos = verificador.buscar_sql_fuera_de_repositorios(tmp_path)

    assert len(hallazgos) == 1
    assert hallazgos[0].ruta.endswith("servicio.py")


def test_detecta_http_directo_solo_en_componentes(tmp_path: Path) -> None:
    escribir(
        tmp_path,
        "frontend/src/funcionalidades/menu/componentes/Lista.tsx",
        "fetch('/api/v1/menu')\n",
    )
    escribir(
        tmp_path,
        "frontend/src/funcionalidades/menu/consultas/obtener_menu.ts",
        "fetch('/api/v1/menu')\n",
    )

    hallazgos = verificador.buscar_http_directo_en_componentes(tmp_path)

    assert len(hallazgos) == 1
    assert hallazgos[0].ruta.endswith("Lista.tsx")


def test_exige_excepcion_para_archivo_mayor_a_300_lineas(tmp_path: Path) -> None:
    contenido = "\n".join("linea" for _ in range(301))
    escribir(tmp_path, "frontend/src/funcionalidades/menu/paginas/Menu.tsx", contenido)

    assert len(verificador.buscar_archivos_largos(tmp_path, set())) == 1
    assert (
        verificador.buscar_archivos_largos(
            tmp_path, {"frontend/src/funcionalidades/menu/paginas/Menu.tsx"}
        )
        == []
    )


def test_detecta_vocabulario_ingles_propio_en_ruta_y_en_identificadores(tmp_path: Path) -> None:
    escribir(
        tmp_path,
        "backend/aplicacion/modulos/students/servicio.py",
        "def obtener_students(): pass\ndef obtenerStudents(): pass\n",
    )

    hallazgos = verificador.buscar_ingles_no_permitido(tmp_path, set())

    assert len(hallazgos) == 3
    assert {hallazgo.regla for hallazgo in hallazgos} == {"vocabulario-en-ingles"}
    assert {hallazgo.linea for hallazgo in hallazgos} == {0, 1, 2}


def test_vocabulario_ingles_permitido_y_palabras_no_relacionadas_no_generan_hallazgos(
    tmp_path: Path,
) -> None:
    escribir(
        tmp_path,
        "backend/aplicacion/modulos/students/servicio.py",
        "def obtenerStudents(): pass\ndef crearAplicacion(): pass\n",
    )

    hallazgos = verificador.buscar_ingles_no_permitido(tmp_path, {"students"})

    assert hallazgos == []
