from aplicacion.modulos.transporte.esquemas import RutaEntrada
from aplicacion.modulos.transporte.servicio import ServicioRutas


class RepositorioFalso:
    def __init__(self) -> None:
        self.llamadas: list[tuple] = []

    def listar(self, incluir_inactivas: bool = False) -> list[dict]:
        self.llamadas.append(("listar", incluir_inactivas))
        return [{
            "id_ruta": 1, "codigo": "A1", "descripcion": "Ruta central",
            "activo": True, "color_hex": "#38BDF8", "estudiantes_asignados": 3,
        }]

    def crear(self, *argumentos: object) -> dict:
        self.llamadas.append(("crear", *argumentos))
        return {
            "id_ruta": 2, "codigo": argumentos[0], "descripcion": argumentos[1],
            "activo": argumentos[2], "color_hex": argumentos[3],
        }

    def actualizar(self, *argumentos: object) -> dict:
        self.llamadas.append(("actualizar", *argumentos))
        return {
            "id_ruta": argumentos[0], "codigo": argumentos[1], "descripcion": argumentos[2],
            "activo": argumentos[3], "color_hex": argumentos[4],
        }


def entrada() -> RutaEntrada:
    return RutaEntrada(codigo=" A1 ", descripcion="  Ruta   central ", colorHex="#38bdf8")


def test_listar_convierte_contrato_canonico() -> None:
    resultado = ServicioRutas(RepositorioFalso()).listar()
    assert resultado[0].id_ruta == 1
    assert resultado[0].estudiantes_asignados == 3


def test_crear_normaliza_texto_y_color() -> None:
    repositorio = RepositorioFalso()
    resultado = ServicioRutas(repositorio).crear(entrada(), 7, "10.0.0.1")
    assert resultado.color_carnet_hex == "#38BDF8"
    assert repositorio.llamadas[0] == ("crear", "A1", "Ruta central", True, "#38BDF8", 7, "10.0.0.1")


def test_no_permite_ruta_protegida() -> None:
    datos = RutaEntrada(codigo="0", descripcion="Ruta protegida", colorHex="#FFFFFF")
    try:
        ServicioRutas(RepositorioFalso()).crear(datos, 7, "WEB")
    except ValueError as error:
        assert "protegida" in str(error)
    else:
        raise AssertionError("Se esperaba rechazar la ruta 0")
