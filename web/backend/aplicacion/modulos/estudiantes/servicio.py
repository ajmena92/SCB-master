"""Casos de uso puros del dominio de estudiantes."""

from .esquemas import EstudianteEntrada, EstudianteSalida, PaginaEstudiantes
from .repositorio import RepositorioEstudiantes


def _salida(datos: dict) -> EstudianteSalida:
    return EstudianteSalida(**datos)


class ServicioEstudiantes:
    def __init__(self, repositorio: RepositorioEstudiantes) -> None:
        self._repositorio = repositorio

    def listar(self, pagina: int, tamano: int, buscar: str) -> PaginaEstudiantes:
        elementos, total = self._repositorio.listar(pagina, tamano, buscar)
        return PaginaEstudiantes(elementos=[_salida(e) for e in elementos], pagina=pagina, tamano=tamano, total=total)

    def obtener(self, id_estudiante: int) -> EstudianteSalida:
        resultado = self._repositorio.buscar_por_id(id_estudiante)
        if resultado is None:
            raise ValueError("Estudiante no encontrado")
        return _salida(resultado)

    def crear(self, datos: EstudianteEntrada, id_usuario: int, ip: str) -> EstudianteSalida:
        return _salida(self._repositorio.crear(self._normalizar(datos), id_usuario, ip))

    def editar(self, id_estudiante: int, datos: EstudianteEntrada, id_usuario: int, ip: str) -> EstudianteSalida:
        return _salida(self._repositorio.actualizar(id_estudiante, self._normalizar(datos), id_usuario, ip))

    @staticmethod
    def _normalizar(datos: EstudianteEntrada) -> dict:
        valores = datos.model_dump()
        for campo in ("carne", "nombre", "primer_apellido", "segundo_apellido", "cedula", "seccion"):
            if isinstance(valores[campo], str):
                valores[campo] = " ".join(valores[campo].split()) or None
        if not valores["carne"] or not valores["nombre"] or not valores["primer_apellido"]:
            raise ValueError("Los datos básicos del estudiante son obligatorios")
        return valores
