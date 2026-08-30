from aplicacion.modulos.estudiantes.beneficios import normalizar_beneficio_transporte


def test_ruta_0000_es_no_beneficiario_y_no_se_expone() -> None:
    resultado = normalizar_beneficio_transporte(
        {
            "id_ruta": 4,
            "ruta_codigo": "0000",
            "ruta_descripcion": "Sin beneficio",
            "ruta_activa": True,
        }
    )

    assert resultado["tiene_beneficio_transporte"] is False
    assert resultado["beneficio_transporte"] == "No beneficiario"
    assert resultado["id_ruta"] is None
    assert resultado["ruta_codigo"] is None
    assert resultado["ruta_descripcion"] is None


def test_ruta_activa_muestra_descripcion_oficial() -> None:
    resultado = normalizar_beneficio_transporte(
        {
            "id_ruta": 7,
            "ruta_codigo": "0125",
            "ruta_descripcion": "Ruta San José",
            "ruta_activa": True,
        }
    )

    assert resultado["tiene_beneficio_transporte"] is True
    assert resultado["beneficio_transporte"] == "Beneficiario – Ruta San José"
    assert resultado["ruta_codigo"] == "0125"


def test_ruta_inactiva_es_no_beneficiario_operativo() -> None:
    resultado = normalizar_beneficio_transporte(
        {
            "id_ruta": 8,
            "ruta_codigo": "1050",
            "ruta_descripcion": "Ruta Centro",
            "ruta_activa": False,
        }
    )

    assert resultado["tiene_beneficio_transporte"] is False
    assert resultado["beneficio_transporte"] == "No beneficiario"
