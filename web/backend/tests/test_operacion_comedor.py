from datetime import date

from aplicacion.repositorios_operacion import RepositorioOperacion


class SesionCaptura:
    def __init__(self) -> None:
        self.consultas: list[str] = []

    def scalar(self, consulta):
        self.consultas.append(str(consulta))
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
    assert "reservada" in consulta_meta
    assert "consumida" in consulta_meta
