from aplicacion.modulos.beneficios.esquemas import AsignacionEntrada, BeneficioEntrada
from aplicacion.modulos.beneficios.servicio import ServicioBeneficios


class RepositorioFalso:
    def listar(self, incluir_inactivos: bool = False) -> list[dict]:
        return [
            {
                "id_beneficio": 1,
                "nombre": "Comedor",
                "descripcion": None,
                "dias_permitidos": 5,
                "activo": True,
            }
        ]

    def crear(self, datos: dict, id_usuario: int, ip: str) -> dict:
        return {"id_beneficio": 2, **datos}

    def actualizar(self, id_beneficio: int, datos: dict, id_usuario: int, ip: str) -> dict:
        return {"id_beneficio": id_beneficio, **datos}

    def asignacion(self, id_estudiante: int) -> dict:
        return {"id_estudiante": id_estudiante, "id_beneficio": 1}

    def asignar(
        self, id_estudiante: int, id_beneficio: int | None, id_usuario: int, ip: str
    ) -> dict:
        return {"id_estudiante": id_estudiante, "id_beneficio": id_beneficio}


def test_normaliza_catalogo_y_asignacion() -> None:
    servicio = ServicioBeneficios(RepositorioFalso())
    beneficio = servicio.crear(
        BeneficioEntrada(nombre="  Comedor  ", descripcion="  Apoyo  "), 3, "WEB"
    )
    assert beneficio.nombre == "Comedor"
    asignacion = servicio.asignar(8, AsignacionEntrada(idBeneficio=None), 3, "WEB")
    assert asignacion.id_beneficio is None
