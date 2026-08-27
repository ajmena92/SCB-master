from pathlib import Path

from card_renderer import render_card


def test_render_card_outputs_png_and_pdf(tmp_path: Path):
    student = {
        "Cedula": "115000008",
        "Nombre": "Ana",
        "PrimerApellido": "Estudiante",
        "SegundoApellido": "Demo",
        "Seccion": "10-1",
        "TipoBecaDescripcion": "Beca comedor",
    }

    png = render_card(student, None, "115000008", "PNG")
    pdf = render_card(student, None, "115000008", "PDF")

    assert png.startswith(b"\x89PNG\r\n\x1a\n")
    assert pdf.startswith(b"%PDF")
    assert len(png) > 10_000
    assert len(pdf) > 10_000
