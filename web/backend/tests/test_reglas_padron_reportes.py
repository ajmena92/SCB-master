"""Regresión de las reglas compartidas entre exportación y tablero."""

from aplicacion.reglas_padron_reportes import filtrar_nominal, ordenar_lista_control


def test_orden_natural_seccion_apellidos_y_sin_seccion():
    filas = [
        {"seccion": "10-1", "nombreCompleto": "Ana Mora Solis"},
        {"seccion": "7-10", "nombreCompleto": "Ana Mora Solis"},
        {"seccion": "—", "nombreCompleto": "Ana Mora Solis"},
        {"seccion": "7-2", "nombreCompleto": "Ana Mora Solis"},
        {"seccion": "7-2", "nombreCompleto": "Zoe Araya Soto"},
    ]
    assert ordenar_lista_control(filas) == [filas[4], filas[3], filas[1], filas[0], filas[2]]
    assert filas[0]["seccion"] == "10-1"


def test_filtros_no_mezclan_beneficio_comedor_con_asignacion_transporte():
    filas = [
        {"idRuta": None, "beneficioClave": "beneficiario", "confirmacionClave": "confirmada"},
        {"idRuta": 1, "beneficioClave": "no_beneficiario", "confirmacionClave": "sin_confirmar"},
    ]
    filtros = {"beneficio": "beneficiario", "asignacion": "con_ruta"}
    assert filtrar_nominal(filas, {**filtros, "servicio": "comedor"}) == [filas[0]]
    assert filtrar_nominal(filas, {**filtros, "servicio": "transporte"}) == [filas[1]]


def test_filtro_busqueda_insensible_a_mayusculas_y_lista_vacia():
    filas = [{"nombreCompleto": "Ana Mora", "seccion": "7-1"}]
    assert filtrar_nominal(filas, {"busqueda": " ANA ", "seccion": "7-1"}) == filas
    assert filtrar_nominal(filas, {"busqueda": "otro"}) == []
    assert ordenar_lista_control([]) == []
