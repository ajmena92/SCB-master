"""Composición de los repositorios SQL del dominio de estudiantes."""

from .repositorio import RepositorioSqlEstudiantes
from .repositorio_asignaciones import RepositorioSqlAsignaciones
from .repositorio_credenciales import RepositorioSqlCredenciales
from .repositorio_fotos import RepositorioSqlFotos
from .repositorio_pines import RepositorioSqlPines


class RepositorioSqlEstudiantesCompleto(
    RepositorioSqlEstudiantes,
    RepositorioSqlFotos,
    RepositorioSqlCredenciales,
    RepositorioSqlAsignaciones,
    RepositorioSqlPines,
):
    """Implementación concreta del contrato agregado de estudiantes."""

    pass
