from datetime import date

from aplicacion.repositorios_operacion import RepositorioOperacion


class SesionCaptura:
    def __init__(self) -> None:
        self.consultas: list[str] = []
        self.parametros: list[dict[str, object]] = []

    def scalar(self, consulta):
        self.consultas.append(str(consulta))
        self.parametros.append(consulta.compile().params)
        return 0

    def execute(self, _consulta):
        return ResultadoVacio()


class ResultadoVacio:
    def all(self):
        return []


def test_meta_de_operacion_cuenta_reservas_estudiantiles_no_canceladas() -> None:
    sesion = SesionCaptura()

    RepositorioOperacion(sesion).estado_captura(date(2026, 9, 2))

    consulta_meta = sesion.consultas[1]
    assert "reserva_comedor" in consulta_meta
    assert "persona.tipo" in consulta_meta
    valores = {
        valor
        for parametro in sesion.parametros[1].values()
        for valor in (parametro if isinstance(parametro, list) else [parametro])
    }
    assert {"reservada", "consumida"} <= valores
