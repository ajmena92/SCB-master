"""Perfil, carnet y estado diario del portal PostgreSQL."""

from datetime import date, datetime
from zoneinfo import ZoneInfo


class ServicioPortal:
    def __init__(self, repo):
        self.repo = repo

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
            "barcode": persona.codigo,
            "carne": persona.codigo,
            "tieneFoto": fotografia is not None,
            "anioLectivo": fecha.year,
        }

    def foto_carnet(self, persona):
        return self.repo.foto_persona(persona.id)

    def estado(self, persona, fecha: date):
        menu, componentes, origen = self.repo.menu_fecha(fecha)
        reserva = self.repo.reserva_fecha(persona.id, fecha)
        ahora = datetime.now(ZoneInfo("America/Costa_Rica"))
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
                "periodoAbierto": True,
                "periodoCerrado": False,
                "estado": (
                    "Confirmada"
                    if reserva and reserva.estado in {"reservada", "consumida"}
                    else "Cancelada"
                    if reserva and reserva.estado == "cancelada"
                    else "Pendiente"
                ),
                "descripcionHorario": f"Servicio del {fecha.strftime('%d/%m/%Y')}",
                "horaInicio": "00:00",
                "horaLimite": "23:59",
            },
        }
