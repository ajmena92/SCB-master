from aplicacion.modulos.parametros.esquemas import ParametrosEntrada
from aplicacion.modulos.parametros.servicio import ServicioParametros


class RepositorioFalso:
    def obtener(self):
        return {"minutos_aviso_previo": 15}

    def guardar(self, minutos):
        return {"minutos_aviso_previo": minutos}

    def calendario(self, anio, mes):
        return []


def test_parametros_valida_y_guarda_minutos() -> None:
    servicio = ServicioParametros(RepositorioFalso())
    assert servicio.guardar(ParametrosEntrada(minutosAvisoPrevio=30)).minutos_aviso_previo == 30
