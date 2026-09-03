"""Perfil, carnet y estado diario del portal PostgreSQL."""

from datetime import date, datetime, time
from zoneinfo import ZoneInfo

from fastapi import HTTPException

from aplicacion.codigo_qr_carnet import CodigoQrCarnet, ErrorCodigoQrCarnet


class ServicioPortal:
    def __init__(self, repo, clave_qr_carnet: str = ""):
        self.repo = repo
        self.clave_qr_carnet = clave_qr_carnet

    def _codigo_qr(self, persona_id: int, fecha: date) -> str:
        if not self.clave_qr_carnet:
            raise HTTPException(503, "El carnet QR no está configurado")
        try:
            return CodigoQrCarnet(self.clave_qr_carnet).emitir(
                id_persona=persona_id,
                anio_lectivo=fecha.year,
                hoy=fecha,
            )
        except ErrorCodigoQrCarnet as error:
            raise HTTPException(503, "El carnet QR no está configurado") from error

    def carnet(self, persona, fecha: date):
        matricula = self.repo.matricula_fecha(persona.id, fecha)
        ruta = self.repo.ruta_fecha(matricula.id, fecha) if matricula else None
        fotografia = self.repo.foto_persona(persona.id)
        partes = persona.nombres.split()
        return {
            "tipoPersona": persona.tipo,
            "idEstudiante": persona.id,
            "nombre": " ".join(partes[:-2]) if len(partes) > 2 else persona.nombres,
            "primerApellido": partes[-2] if len(partes) > 2 else "",
            "segundoApellido": partes[-1] if len(partes) > 2 else "",
            "cedula": persona.cedula,
            "seccion": matricula.seccion if matricula else "Personal docente",
            "rutaCodigo": ruta.codigo if ruta else None,
            "rutaColor": ruta.color_hex if ruta else "#CBD5E1",
            "rutaDescripcion": ruta.descripcion if ruta else "Sin ruta asignada",
            "idEstadoComedor": 1 if matricula and matricula.becado else 2,
            "beneficioComedor": "Beneficiario"
            if matricula and matricula.becado
            else "No beneficiario",
            "colegio": "Colegio Técnico Profesional de Platanares",
            "codigoQr": self._codigo_qr(persona.id, fecha),
            "tieneFoto": fotografia is not None,
            "anioLectivo": fecha.year,
        }

    def foto_carnet(self, persona):
        return self.repo.foto_persona(persona.id)

    def estado(self, persona, fecha: date):
        menu, componentes, origen = self.repo.menu_fecha(fecha)
        reserva = self.repo.reserva_fecha(persona.id, fecha)
        ahora = datetime.now(ZoneInfo("America/Costa_Rica"))
        horario = self.repo.horario_reserva_general()
        hora_limite = horario.hora_limite if horario else "09:40"
        es_hoy = fecha == ahora.date()
        hora_cierre = time.fromisoformat(hora_limite)
        dentro_del_plazo = not es_hoy or ahora.time() <= hora_cierre
        cierre = datetime.combine(fecha, hora_cierre, tzinfo=ZoneInfo("America/Costa_Rica"))
        segundos_para_cierre = max(0, int((cierre - ahora).total_seconds())) if es_hoy else 0
        return {
            "menu": (
                {
                    "Titulo": menu.titulo,
                    "Componentes": [
                        {"Orden": c.orden, "Nombre": c.nombre, "TipoComponente": c.tipo}
                        for c in componentes
                    ],
                    "origen": origen,
                }
                if menu
                else None
            ),
            "estado": {
                "horaServidor": ahora.strftime("%H:%M:%S"),
                "segundosParaCierre": segundos_para_cierre,
                "segundosParaApertura": 0,
                "periodoAbierto": dentro_del_plazo,
                "periodoCerrado": not dentro_del_plazo,
                "estado": (
                    "Confirmada"
                    if reserva and reserva.estado in {"reservada", "consumida"}
                    else "Cancelada"
                    if reserva and reserva.estado == "cancelada"
                    else "Pendiente"
                ),
                "descripcionHorario": f"Confirmación hasta las {hora_limite}",
                "horaInicio": "00:00",
                "horaLimite": hora_limite,
                "sinTiquete": bool(
                    reserva and reserva.estado == "reservada" and reserva.sin_tiquete
                ),
            },
        }
