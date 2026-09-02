"""Persistencia tipada de catalogos, matriculas, rutas y menu."""

from datetime import date, timedelta

from sqlalchemy import delete, func, or_, select
from sqlalchemy.orm import Session

from aplicacion.modelos.maestros import (
    AnioLectivo,
    AsignacionRuta,
    CredencialPortal,
    CuentaAdministrativa,
    EventoCredencialPortal,
    FotografiaPersona,
    Matricula,
    Persona,
    Ruta,
    SesionAcceso,
)
from aplicacion.modelos.operacion import CuentaTiquete
from aplicacion.modelos.menu import (
    CalendarioMenu,
    ConfiguracionCicloMenu,
    ComponenteMenu,
    ComponentePublicado,
    ComponenteSustitucionMenu,
    PlantillaMenu,
    PublicacionMenu,
    SustitucionMenu,
)
from aplicacion.repositorios import desactivar_anios


class RepositorioCatalogos:
    def __init__(self, sesion: Session):
        self.sesion = sesion

    def listar_personas(
        self, buscar="", estado="activos", tipo=None, pagina=1, tamano=50,
        ordenar_por="nombres", direccion="asc",
    ):
        consulta = select(Persona)
        if estado == "activos":
            consulta = consulta.where(Persona.activo.is_(True))
        elif estado == "inactivos":
            consulta = consulta.where(Persona.activo.is_(False))
        if tipo:
            consulta = consulta.where(Persona.tipo == tipo)
        termino = " ".join(buscar.split())
        if termino:
            patron = f"%{termino}%"
            consulta = consulta.where(
                or_(Persona.codigo.ilike(patron), Persona.cedula.ilike(patron), Persona.nombres.ilike(patron))
            )
        total = int(self.sesion.scalar(select(func.count()).select_from(consulta.subquery())) or 0)
        columnas_orden = {
            "nombres": Persona.nombres,
            "cedula": Persona.cedula,
            "tipo": Persona.tipo,
            "estado": Persona.activo,
        }
        columna = columnas_orden[ordenar_por]
        orden = columna.desc() if direccion == "desc" else columna.asc()
        personas = self.sesion.scalars(
            consulta.order_by(orden, Persona.id.asc()).offset((pagina - 1) * tamano).limit(tamano)
        ).all()
        anio = self.sesion.scalar(select(AnioLectivo).where(AnioLectivo.vigente.is_(True)))
        salida = [self._persona_resumen(persona, anio) for persona in personas]
        return {"elementos": salida, "total": total, "pagina": pagina, "tamano": tamano}

    def resumen_personas(self):
        anio = self.anio_vigente()
        if not anio:
            return {"estudiantes_activos": 0, "estudiantes_inactivos": 0}
        consulta = select(func.count(Matricula.id)).join(Persona).where(
            Matricula.anio_lectivo_id == anio.id,
            Persona.tipo == "estudiante",
        )
        return {
            "estudiantes_activos": int(
                self.sesion.scalar(consulta.where(Persona.activo.is_(True))) or 0
            ),
            "estudiantes_inactivos": int(
                self.sesion.scalar(consulta.where(Persona.activo.is_(False))) or 0
            ),
        }

    def obtener_persona_resumen(self, persona_id: int):
        persona = self.persona(persona_id)
        if persona is None:
            return None
        return self._persona_resumen(persona, self.anio_vigente())

    def _persona_resumen(self, persona, anio):
        matricula = (
            self.sesion.scalar(
                select(Matricula).where(
                    Matricula.persona_id == persona.id, Matricula.anio_lectivo_id == anio.id
                )
            )
            if anio and persona.tipo == "estudiante"
            else None
        )
        asignacion = (
            self.sesion.scalar(
                select(AsignacionRuta)
                .where(
                    AsignacionRuta.matricula_id == matricula.id,
                    AsignacionRuta.fecha_inicio <= date.today(),
                    or_(AsignacionRuta.fecha_fin.is_(None), AsignacionRuta.fecha_fin >= date.today()),
                )
                .order_by(AsignacionRuta.fecha_inicio.desc(), AsignacionRuta.id.desc())
            )
            if matricula
            else None
        )
        ruta = self.sesion.get(Ruta, asignacion.ruta_id) if asignacion else None
        cuenta = self.sesion.get(CuentaTiquete, persona.id)
        ruta_valida = ruta and ruta.activo and ruta.codigo != "0000"
        return {
            "id": persona.id, "codigo": persona.codigo, "cedula": persona.cedula,
            "nombres": persona.nombres, "tipo": persona.tipo, "activo": persona.activo,
            "matriculaId": matricula.id if matricula else None,
            "seccion": matricula.seccion if matricula else None,
            "becado": bool(matricula and matricula.becado),
            "beneficioComedor": "Beneficiario" if matricula and matricula.becado else "No beneficiario",
            "estadoMatricula": matricula.estado if matricula else None,
            "rutaId": ruta.id if ruta_valida else None,
            "descripcionRuta": ruta.descripcion if ruta_valida else None,
            "beneficioTransporte": f"Beneficiario – {ruta.descripcion}" if ruta_valida else "No beneficiario",
            "saldoTiquetes": cuenta.saldo if cuenta else 0,
        }

    def persona(self, persona_id: int):
        return self.sesion.get(Persona, persona_id)

    def foto_persona(self, persona_id: int):
        return self.sesion.scalar(
            select(FotografiaPersona).where(FotografiaPersona.persona_id == persona_id)
        )

    def guardar_foto_persona(self, persona_id: int, contenido: bytes, tipo_contenido: str):
        foto = self.foto_persona(persona_id)
        if foto is None:
            self.sesion.add(
                FotografiaPersona(
                    persona_id=persona_id, contenido=contenido, tipo_contenido=tipo_contenido
                )
            )
        else:
            foto.contenido = contenido
            foto.tipo_contenido = tipo_contenido
        self.sesion.flush()

    def eliminar_foto_persona(self, persona_id: int):
        foto = self.foto_persona(persona_id)
        if foto is not None:
            self.sesion.delete(foto)
            self.sesion.flush()

    def codigo_existe(self, codigo: str) -> bool:
        return self.sesion.scalar(select(Persona.id).where(Persona.codigo == codigo)) is not None

    def guardar_persona(self, persona, credencial, cuenta) -> None:
        self.sesion.add(persona)
        self.sesion.flush()
        credencial.persona_id = cuenta.persona_id = persona.id
        self.sesion.add_all([
            credencial,
            cuenta,
            EventoCredencialPortal(persona_id=persona.id, tipo="creacion"),
        ])

    def actualizar_persona(self, persona, datos):
        persona.cedula = datos.cedula.strip() if datos.cedula else None
        persona.nombres = " ".join(datos.nombres.split())
        self.sesion.flush()
        return persona

    def desactivar_persona(self, persona) -> None:
        persona.activo = False
        self.sesion.execute(delete(SesionAcceso).where(SesionAcceso.persona_id == persona.id))
        self.sesion.flush()

    def tiene_cuenta_administrativa(self, persona_id: int) -> bool:
        return self.sesion.scalar(
            select(CuentaAdministrativa.id).where(CuentaAdministrativa.persona_id == persona_id)
        ) is not None

    def cambiar_ruta_matricula(self, matricula, ruta_id: int | None):
        actual = self.sesion.scalar(
            select(AsignacionRuta)
            .where(AsignacionRuta.matricula_id == matricula.id, AsignacionRuta.fecha_fin.is_(None))
            .order_by(AsignacionRuta.id.desc())
        )
        if actual and actual.ruta_id == ruta_id:
            return actual
        if actual:
            actual.fecha_fin = date.today() - timedelta(days=1)
        if ruta_id is None:
            self.sesion.flush()
            return None
        asignacion = AsignacionRuta(matricula_id=matricula.id, ruta_id=ruta_id, fecha_inicio=date.today())
        self.sesion.add(asignacion)
        self.sesion.flush()
        return asignacion

    def ruta_activa_matricula(self, matricula_id: int) -> int | None:
        return self.sesion.scalar(
            select(AsignacionRuta.ruta_id)
            .where(AsignacionRuta.matricula_id == matricula_id, AsignacionRuta.fecha_fin.is_(None))
            .order_by(AsignacionRuta.id.desc())
        )

    def reiniciar_pin(self, persona, hash_pin: str, cuenta_id: int, tipo: str) -> None:
        credencial = self.sesion.get(CredencialPortal, persona.id)
        if credencial is None:
            self.sesion.add(CredencialPortal(persona_id=persona.id, pin_hash=hash_pin, cambio_obligatorio=True))
        else:
            credencial.pin_hash = hash_pin
            credencial.cambio_obligatorio = True
        self.sesion.execute(delete(SesionAcceso).where(SesionAcceso.persona_id == persona.id))
        self.sesion.add(EventoCredencialPortal(persona_id=persona.id, cuenta_administrativa_id=cuenta_id, tipo=tipo))
        self.sesion.flush()

    def estudiantes_seccion(self, anio_id: int, seccion: str):
        return self.sesion.scalars(
            select(Persona)
            .join(Matricula, Matricula.persona_id == Persona.id)
            .where(
                Matricula.anio_lectivo_id == anio_id, Matricula.seccion == seccion,
                Matricula.estado == "activo", Persona.tipo == "estudiante", Persona.activo.is_(True),
            )
            .order_by(Persona.nombres)
        ).all()

    def listar_anios(self):
        return self.sesion.scalars(select(AnioLectivo).order_by(AnioLectivo.anio.desc())).all()

    def anio(self, anio_id: int):
        return self.sesion.get(AnioLectivo, anio_id)

    def secciones_anio(self, anio_id: int):
        return self.sesion.scalars(
            select(Matricula.seccion)
            .join(Persona, Persona.id == Matricula.persona_id)
            .where(
                Matricula.anio_lectivo_id == anio_id,
                Matricula.estado == "activo",
                Persona.tipo == "estudiante",
                Persona.activo.is_(True),
            )
            .distinct()
            .order_by(Matricula.seccion)
        ).all()

    def guardar_anio(self, registro: AnioLectivo):
        if registro.vigente:
            desactivar_anios(self.sesion)
        self.sesion.add(registro)
        self.sesion.flush()
        return registro

    def activar_anio(self, anio_id: int):
        registro = self.sesion.get(AnioLectivo, anio_id)
        if registro:
            desactivar_anios(self.sesion)
            registro.vigente = True
        return registro

    def configuracion_ciclo_menu(self):
        return self.sesion.get(ConfiguracionCicloMenu, 1)

    def guardar_configuracion_ciclo_menu(self, inicio_ciclo_menu):
        registro = self.configuracion_ciclo_menu()
        if registro is None:
            registro = ConfiguracionCicloMenu(id=1, inicio_ciclo_menu=inicio_ciclo_menu)
            self.sesion.add(registro)
        else:
            registro.inicio_ciclo_menu = inicio_ciclo_menu
        self.sesion.flush()
        return registro

    def listar_matriculas(self, anio_id: int | None):
        consulta = select(Matricula)
        if anio_id:
            consulta = consulta.where(Matricula.anio_lectivo_id == anio_id)
        return self.sesion.scalars(consulta.order_by(Matricula.id)).all()

    def matricula(self, matricula_id: int):
        return self.sesion.get(Matricula, matricula_id)

    def guardar(self, registro):
        self.sesion.add(registro)
        self.sesion.flush()
        return registro

    def listar_rutas(self):
        return self.sesion.execute(
            select(Ruta, func.count(AsignacionRuta.id))
            .outerjoin(AsignacionRuta, AsignacionRuta.ruta_id == Ruta.id)
            .group_by(Ruta.id)
            .order_by(Ruta.codigo)
        ).all()

    def listar_rutas_activas(self):
        return self.sesion.execute(
            select(Ruta, func.count(AsignacionRuta.id))
            .outerjoin(AsignacionRuta, AsignacionRuta.ruta_id == Ruta.id)
            .where(Ruta.activo.is_(True), Ruta.codigo != "0000")
            .group_by(Ruta.id)
            .order_by(Ruta.codigo)
        ).all()

    def anio_vigente(self):
        return self.sesion.scalar(select(AnioLectivo).where(AnioLectivo.vigente.is_(True)))

    def contar_asignados(self, ruta_id: int) -> int:
        return int(
            self.sesion.scalar(
                select(func.count(AsignacionRuta.id)).where(AsignacionRuta.ruta_id == ruta_id)
            )
            or 0
        )

    def ruta(self, ruta_id: int):
        return self.sesion.get(Ruta, ruta_id)

    def asignacion_solapada(self, entrada) -> bool:
        from datetime import date

        from sqlalchemy import or_

        return (
            self.sesion.scalar(
                select(AsignacionRuta.id).where(
                    AsignacionRuta.matricula_id == entrada.matricula_id,
                    AsignacionRuta.fecha_inicio <= (entrada.fecha_fin or date.max),
                    or_(
                        AsignacionRuta.fecha_fin.is_(None),
                        AsignacionRuta.fecha_fin >= entrada.fecha_inicio,
                    ),
                )
            )
            is not None
        )

    def listar_plantillas(self):
        salida = []
        for plantilla in self.sesion.scalars(
            select(PlantillaMenu).order_by(PlantillaMenu.semana, PlantillaMenu.dia)
        ):
            componentes = self.sesion.scalars(
                select(ComponenteMenu)
                .where(ComponenteMenu.plantilla_id == plantilla.id)
                .order_by(ComponenteMenu.orden)
            ).all()
            salida.append((plantilla, componentes))
        return salida

    def guardar_plantilla(self, plantilla, componentes):
        self.sesion.add(plantilla)
        self.sesion.flush()
        for componente in componentes:
            componente.plantilla_id = plantilla.id
        self.sesion.add_all(componentes)
        return plantilla

    def plantilla_posicion(self, semana, dia):
        return self.sesion.scalar(
            select(PlantillaMenu).where(PlantillaMenu.semana == semana, PlantillaMenu.dia == dia)
        )

    def reemplazar_plantilla(self, datos, componentes):
        plantilla = self.plantilla_posicion(datos.semana, datos.dia)
        if plantilla is None:
            plantilla = PlantillaMenu(
                semana=datos.semana,
                dia=datos.dia,
                titulo=datos.titulo,
                observaciones=datos.observaciones,
                activo=datos.activo,
            )
            return self.guardar_plantilla(plantilla, componentes)
        plantilla.titulo = datos.titulo
        plantilla.observaciones = datos.observaciones
        plantilla.activo = datos.activo
        self.sesion.execute(
            delete(ComponenteMenu).where(ComponenteMenu.plantilla_id == plantilla.id)
        )
        self.sesion.flush()
        for componente in componentes:
            componente.plantilla_id = plantilla.id
        self.sesion.add_all(componentes)
        return plantilla

    def listar_publicaciones(self):
        salida = []
        for publicacion in self.sesion.scalars(
            select(PublicacionMenu).order_by(PublicacionMenu.fecha.desc())
        ):
            componentes = self.sesion.scalars(
                select(ComponentePublicado)
                .where(ComponentePublicado.publicacion_id == publicacion.id)
                .order_by(ComponentePublicado.orden)
            ).all()
            salida.append((publicacion, componentes))
        return salida

    def plantilla_componentes(self, semana: int, dia: int):
        plantilla = self.plantilla_posicion(semana, dia)
        componentes = (
            self.sesion.scalars(
                select(ComponenteMenu)
                .where(ComponenteMenu.plantilla_id == plantilla.id)
                .order_by(ComponenteMenu.orden)
            ).all()
            if plantilla
            else []
        )
        return plantilla, componentes

    def sustitucion_componentes(self, fecha):
        sustitucion = self.sesion.scalar(
            select(SustitucionMenu).where(SustitucionMenu.fecha == fecha)
        )
        componentes = (
            self.sesion.scalars(
                select(ComponenteSustitucionMenu)
                .where(ComponenteSustitucionMenu.sustitucion_id == sustitucion.id)
                .order_by(ComponenteSustitucionMenu.orden)
            ).all()
            if sustitucion
            else []
        )
        return sustitucion, componentes

    def reemplazar_sustitucion(self, datos, componentes):
        sustitucion, _ = self.sustitucion_componentes(datos.fecha)
        if sustitucion is None:
            sustitucion = SustitucionMenu(
                fecha=datos.fecha, titulo=datos.titulo, observaciones=datos.observaciones
            )
            self.sesion.add(sustitucion)
            self.sesion.flush()
        else:
            sustitucion.titulo = datos.titulo
            sustitucion.observaciones = datos.observaciones
            self.sesion.execute(
                delete(ComponenteSustitucionMenu).where(
                    ComponenteSustitucionMenu.sustitucion_id == sustitucion.id
                )
            )
            self.sesion.flush()
        for componente in componentes:
            componente.sustitucion_id = sustitucion.id
        self.sesion.add_all(componentes)
        return sustitucion

    def guardar_publicacion(self, publicacion, componentes):
        self.sesion.add(publicacion)
        self.sesion.flush()
        for componente in componentes:
            componente.publicacion_id = publicacion.id
        self.sesion.add_all(componentes)
        return publicacion

    def listar_calendario(self, desde, hasta):
        return self.sesion.scalars(
            select(CalendarioMenu)
            .where(CalendarioMenu.fecha >= desde, CalendarioMenu.fecha <= hasta)
            .order_by(CalendarioMenu.fecha)
        ).all()

    def calendario_fecha(self, fecha):
        return self.sesion.get(CalendarioMenu, fecha)

    def reemplazar_publicacion(self, fecha, titulo, observaciones, origen, componentes):
        publicacion = self.sesion.scalar(
            select(PublicacionMenu).where(PublicacionMenu.fecha == fecha)
        )
        if publicacion:
            return None
        publicacion = PublicacionMenu(
            fecha=fecha, titulo=titulo, observaciones=observaciones, origen=origen
        )
        return self.guardar_publicacion(publicacion, componentes)

    def publicacion_fecha(self, fecha):
        return self.sesion.scalar(select(PublicacionMenu).where(PublicacionMenu.fecha == fecha))

    def publicaciones_rango(self, desde, hasta):
        return self.sesion.scalars(
            select(PublicacionMenu)
            .where(PublicacionMenu.fecha >= desde, PublicacionMenu.fecha <= hasta)
        ).all()

    def sustituciones_rango(self, desde, hasta):
        return self.sesion.scalars(
            select(SustitucionMenu)
            .where(SustitucionMenu.fecha >= desde, SustitucionMenu.fecha <= hasta)
        ).all()

    def listar_sustituciones(self):
        salida = []
        for sustitucion in self.sesion.scalars(
            select(SustitucionMenu).order_by(SustitucionMenu.fecha)
        ):
            componentes = self.sesion.scalars(
                select(ComponenteSustitucionMenu)
                .where(ComponenteSustitucionMenu.sustitucion_id == sustitucion.id)
                .order_by(ComponenteSustitucionMenu.orden)
            ).all()
            salida.append((sustitucion, componentes))
        return salida
