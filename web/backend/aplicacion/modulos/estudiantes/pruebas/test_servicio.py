from typing import cast

from aplicacion.modulos.estudiantes.esquemas import EstudianteEntrada
from aplicacion.modulos.estudiantes.repositorio import RepositorioEstudiantes
from aplicacion.modulos.estudiantes.servicio import ServicioEstudiantes


class RepositorioFalso:
    def __init__(self) -> None:
        self.datos = {
            "id_estudiante": 4,
            "carne": "A-4",
            "nombre": "Ana",
            "primer_apellido": "Rojas",
            "segundo_apellido": None,
            "cedula": None,
            "seccion": "10-1",
            "activo": True,
        }

    def listar(self, pagina: int, tamano: int, buscar: str) -> tuple[list[dict], int]:
        return [self.datos], 1

    def buscar_por_id(self, id_estudiante: int) -> dict | None:
        return self.datos if id_estudiante == 4 else None

    def crear(self, datos: dict, id_usuario: int, ip: str) -> dict:
        return {**self.datos, **datos, "id_estudiante": 5}

    def actualizar(self, id_estudiante: int, datos: dict, id_usuario: int, ip: str) -> dict:
        if id_estudiante != 4:
            raise ValueError("Estudiante no encontrado")
        return {**self.datos, **datos}


def entrada():
    return EstudianteEntrada(
        carne=" A-4 ",
        nombre=" Ana ",
        primerApellido=" Rojas ",
        segundoApellido=" López ",
        seccion=" 10-1 ",
    )


def repositorio() -> RepositorioEstudiantes:
    return cast(RepositorioEstudiantes, RepositorioFalso())


def test_lista_y_contrato_camel_case():
    resultado = ServicioEstudiantes(repositorio()).listar(1, 25, "")
    assert resultado.total == 1
    assert resultado.model_dump(by_alias=True)["elementos"][0]["idEstudiante"] == 4


def test_crear_normaliza_espacios():
    resultado = ServicioEstudiantes(repositorio()).crear(entrada(), 8, "127.0.0.1")
    assert resultado.nombre == "Ana"
    assert resultado.segundo_apellido == "López"


def test_obtener_inexistente():
    try:
        ServicioEstudiantes(repositorio()).obtener(99)
    except ValueError as exc:
        assert str(exc) == "Estudiante no encontrado"
    else:
        raise AssertionError("Se esperaba estudiante inexistente")
