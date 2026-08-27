from datetime import date

from aplicacion.modulos.asistencia.esquemas import CorreccionEntrada, MarcaEntrada
from aplicacion.modulos.asistencia.servicio import ServicioAsistencia


class RepositorioFalso:
    def __init__(self) -> None:
        self.llamadas: list[tuple] = []

    def listar(self, fecha: date) -> list[dict]:
        self.llamadas.append(("listar", fecha))
        return [
            {
                "id_marca": 1,
                "id_estudiante": 7,
                "fecha": fecha,
                "estado": "presente",
                "observacion": None,
                "corregida": False,
            }
        ]

    def registrar(self, datos: dict, id_usuario: int, ip: str) -> dict:
        self.llamadas.append(("registrar", datos, id_usuario, ip))
        return {"id_marca": 2, **datos, "corregida": False}

    def corregir(self, id_marca: int, estado: str, motivo: str, id_usuario: int, ip: str) -> dict:
        self.llamadas.append(("corregir", id_marca, estado, motivo, id_usuario, ip))
        return {
            "id_marca": id_marca,
            "id_estudiante": 7,
            "fecha": date(2026, 8, 26),
            "estado": estado,
            "observacion": None,
            "corregida": True,
        }


def test_listar_devuelve_marcas() -> None:
    repositorio = RepositorioFalso()
    salida = ServicioAsistencia(repositorio).listar(date(2026, 8, 26))
    assert salida[0].id_estudiante == 7
    assert repositorio.llamadas == [("listar", date(2026, 8, 26))]


def test_registrar_normaliza_observacion() -> None:
    repositorio = RepositorioFalso()
    datos = MarcaEntrada(
        idEstudiante=7, fecha=date(2026, 8, 26), estado="tardanza", observacion="  Llegó   tarde "
    )
    salida = ServicioAsistencia(repositorio).registrar(datos, 3, "10.0.0.1")
    assert salida.estado == "tardanza"
    assert repositorio.llamadas[0][1]["observacion"] == "Llegó tarde"


def test_corregir_registra_motivo() -> None:
    repositorio = RepositorioFalso()
    datos = CorreccionEntrada(estado="justificada", motivo="  Ajuste   autorizado ")
    salida = ServicioAsistencia(repositorio).corregir(1, datos, 3, "WEB")
    assert salida.corregida
    assert repositorio.llamadas[0][3] == "Ajuste autorizado"
