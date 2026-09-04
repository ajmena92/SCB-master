"""Casos de uso de rutas y su vigencia."""

from fastapi import HTTPException

from aplicacion.modelos.maestros import AsignacionRuta, Ruta


class CasosCatalogosRutas:
    @staticmethod
    def _ruta_salida(ruta, asignados):
        return {"idRuta": ruta.id, "codigo": ruta.codigo, "descripcion": ruta.descripcion, "colorCarnetHex": ruta.color_hex, "activo": ruta.activo, "estudiantesAsignados": asignados}

    def listar_rutas(self):
        return [self._ruta_salida(ruta, asignados) for ruta, asignados in self.repo.listar_rutas()]

    def listar_rutas_activas(self):
        return [self._ruta_salida(ruta, asignados) for ruta, asignados in self.repo.listar_rutas_activas()]

    def crear_ruta(self, datos):
        codigo = datos.codigo.strip()
        descripcion = " ".join(datos.descripcion.split())
        ruta = self.repo.guardar(Ruta(nombre=f"{codigo}-{descripcion}", codigo=codigo, descripcion=descripcion, color_hex=datos.color_hex.upper(), activo=datos.activa))
        return self._ruta_salida(ruta, 0)

    def actualizar_ruta(self, ruta_id, datos):
        ruta = self.repo.ruta(ruta_id)
        if not ruta:
            raise HTTPException(404, "Ruta no encontrada")
        if ruta.codigo == "0":
            raise HTTPException(409, "La ruta 0 esta protegida")
        ruta.codigo = datos.codigo.strip()
        ruta.descripcion = " ".join(datos.descripcion.split())
        ruta.nombre = f"{ruta.codigo}-{ruta.descripcion}"
        ruta.color_hex = datos.color_hex.upper()
        ruta.activo = datos.activa
        self.repo.guardar(ruta)
        return self._ruta_salida(ruta, self.repo.contar_asignados(ruta.id))

    def asignar_ruta(self, ruta_id, datos):
        if not self.repo.ruta(ruta_id) or not self.repo.matricula(datos.matricula_id):
            raise HTTPException(404, "Ruta o matricula no encontrada")
        if self.repo.asignacion_solapada(datos):
            raise HTTPException(409, "La vigencia de ruta se superpone")
        return self.repo.guardar(AsignacionRuta(ruta_id=ruta_id, **datos.model_dump()))
