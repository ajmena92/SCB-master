"""Casos de uso de rutas, sin dependencias de FastAPI ni SQL."""

from aplicacion.modulos.transporte.esquemas import RutaEntrada, RutaSalida
from aplicacion.modulos.transporte.paleta import opciones, validar
from aplicacion.modulos.transporte.repositorio import RepositorioRutas


def convertir_ruta(ruta: dict) -> RutaSalida:
    return RutaSalida(
        id_ruta=ruta["id_ruta"],
        codigo=ruta["codigo"],
        descripcion=ruta["descripcion"],
        activo=bool(ruta["activo"]),
        color_carnet_hex=ruta["color_hex"],
        estudiantes_asignados=ruta.get("estudiantes_asignados", 0),
    )


class ServicioRutas:
    def __init__(self, repositorio: RepositorioRutas) -> None:
        self._repositorio = repositorio

    def listar(self) -> list[RutaSalida]:
        return [convertir_ruta(ruta) for ruta in self._repositorio.listar(incluir_inactivas=True)]

    def listar_paleta(self) -> list[dict]:
        return opciones()

    def crear(self, datos: RutaEntrada, id_usuario: int, ip: str) -> RutaSalida:
        codigo = datos.codigo.strip()
        descripcion = " ".join(datos.descripcion.split())
        color = validar(datos.color_hex)
        if codigo == "0":
            raise ValueError("La ruta 0 está protegida")
        return convertir_ruta(
            self._repositorio.crear(codigo, descripcion, datos.activo, color, id_usuario, ip)
        )

    def editar(self, id_ruta: int, datos: RutaEntrada, id_usuario: int, ip: str) -> RutaSalida:
        codigo = datos.codigo.strip()
        descripcion = " ".join(datos.descripcion.split())
        color = validar(datos.color_hex)
        if codigo == "0":
            raise ValueError("La ruta 0 está protegida")
        return convertir_ruta(
            self._repositorio.actualizar(
                id_ruta, codigo, descripcion, datos.activo, color, id_usuario, ip
            )
        )
