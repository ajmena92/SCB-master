"""Persistencia de personas, matrícula, años lectivos y rutas."""

from datetime import date, timedelta

from sqlalchemy import delete, func, or_, select

from aplicacion.modelos.maestros import (
    AnioLectivo, AsignacionRuta, CredencialPortal, CuentaAdministrativa,
    EventoCredencialPortal, FotografiaPersona, Matricula, Persona, Ruta, SesionAcceso,
)
from aplicacion.modelos.operacion import CuentaTiquete
from aplicacion.repositorios import desactivar_anios


class RepositorioCatalogosPersonas:
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
                or_(Persona.cedula.ilike(patron), Persona.nombres.ilike(patron))
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
            "id": persona.id, "referenciaPublica": persona.referencia_publica,
            "cedula": persona.cedula,
            "nombres": persona.nombres, "tipo": persona.tipo, "activo": persona.activo,
            "matriculaId": matricula.id if matricula else None,
            "seccion": matricula.seccion if matricula else None,
            "becado": bool(matricula and matricula.becado),
            "beneficioComedor": "Beneficiario" if matricula and matricula.becado else "No beneficiario",
            "estadoMatricula": matricula.estado if matricula else None,
            "rutaId": ruta.id if ruta is not None and ruta_valida else None,
            "descripcionRuta": ruta.descripcion if ruta is not None and ruta_valida else None,
            "beneficioTransporte": f"Beneficiario – {ruta.descripcion}" if ruta is not None and ruta_valida else "No beneficiario",
            "saldoTiquetes": cuenta.saldo if cuenta else 0,
        }

    def persona(self, persona_id: int):
        return self.sesion.get(Persona, persona_id)

    def persona_referencia_publica(self, referencia_publica: str):
        return self.sesion.scalar(
            select(Persona).where(Persona.referencia_publica == referencia_publica)
        )

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
