"""Casos de uso puros del dominio de beneficios."""

from .esquemas import AsignacionEntrada, AsignacionSalida, BeneficioEntrada, BeneficioSalida
from .repositorio import RepositorioBeneficios


def _salida(datos: dict) -> BeneficioSalida:
    return BeneficioSalida(**datos)


class ServicioBeneficios:
    def __init__(self, repositorio: RepositorioBeneficios) -> None:
        self._repositorio = repositorio

    def listar(self) -> list[BeneficioSalida]:
        return [_salida(item) for item in self._repositorio.listar()]

    def crear(self, datos: BeneficioEntrada, id_usuario: int, ip: str) -> BeneficioSalida:
        return _salida(self._repositorio.crear(self._normalizar(datos), id_usuario, ip))

    def editar(
        self, id_beneficio: int, datos: BeneficioEntrada, id_usuario: int, ip: str
    ) -> BeneficioSalida:
        return _salida(
            self._repositorio.actualizar(id_beneficio, self._normalizar(datos), id_usuario, ip)
        )

    def obtener_asignacion(self, id_estudiante: int) -> AsignacionSalida:
        if id_estudiante < 1:
            raise ValueError("El estudiante no es válido")
        return AsignacionSalida(**self._repositorio.asignacion(id_estudiante))

    def asignar(
        self, id_estudiante: int, datos: AsignacionEntrada, id_usuario: int, ip: str
    ) -> AsignacionSalida:
        if id_estudiante < 1:
            raise ValueError("El estudiante no es válido")
        return AsignacionSalida(
            **self._repositorio.asignar(id_estudiante, datos.id_beneficio, id_usuario, ip)
        )

    @staticmethod
    def _normalizar(datos: BeneficioEntrada) -> dict:
        valores = datos.model_dump()
        valores["nombre"] = " ".join(datos.nombre.split())
        valores["descripcion"] = " ".join(datos.descripcion.split()) if datos.descripcion else None
        if not valores["nombre"]:
            raise ValueError("El nombre del beneficio es obligatorio")
        return valores
