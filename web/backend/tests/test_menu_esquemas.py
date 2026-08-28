from aplicacion.modulos.menu.esquemas import PlantillaMenuSalida


def test_salida_acepta_nombres_de_componentes_migrados() -> None:
    nombre = "Menú especial: " + "descripción extensa " * 15

    salida = PlantillaMenuSalida(
        id_plantilla=1,
        semana=1,
        dia=1,
        titulo="Plantilla de prueba",
        componentes=[{"nombre": nombre, "tipo": "Principal", "orden": 1}],
    )

    assert salida.componentes[0].nombre == nombre
