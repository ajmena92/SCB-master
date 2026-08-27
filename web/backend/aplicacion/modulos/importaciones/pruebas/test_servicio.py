from aplicacion.modulos.importaciones.servicio import ServicioImportaciones


def test_previsualizacion_csv_valida_y_errores() -> None:
    vista = ServicioImportaciones(None).previsualizar(b"codigo,nombre\nA1,Ana\n\n")  # type: ignore[arg-type]
    assert vista.valida
    assert vista.filas == [{"codigo": "A1", "nombre": "Ana"}]


def test_previsualizacion_rechaza_columnas_extras() -> None:
    vista = ServicioImportaciones(None).previsualizar(b"codigo\nA1,extra\n")  # type: ignore[arg-type]
    assert not vista.valida
    assert vista.errores[0].fila == 2
