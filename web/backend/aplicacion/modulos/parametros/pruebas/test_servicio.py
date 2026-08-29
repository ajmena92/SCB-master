from aplicacion.modulos.parametros.esquemas import ParametrosEntrada
from aplicacion.modulos.parametros.servicio import ServicioParametros


class RepositorioFalso:
    def obtener(self):
        return {"minutos_aviso_previo": 15, "horarios": []}

    def guardar(self, minutos, horarios, permitir_marca_tardia=False, permitir_sin_marca_transporte=True):
        return {
            "minutos_aviso_previo": minutos,
            "horarios": [
                {
                    "id_horario": 1,
                    "codigo": "diurno",
                    "descripcion": "Diurno",
                    "hora_limite": "12:00",
                    "activo": True,
                }
            ],
        }

    def calendario(self, anio, mes):
        return []


def test_parametros_valida_y_guarda_minutos() -> None:
    servicio = ServicioParametros(RepositorioFalso())
    assert servicio.guardar(ParametrosEntrada(minutosAvisoPrevio=30)).minutos_aviso_previo == 30
