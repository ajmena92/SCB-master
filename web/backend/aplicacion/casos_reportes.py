"""Casos de uso para reportes operativos y tablero PostgreSQL."""

from collections import defaultdict
from datetime import date, timedelta

from aplicacion.reglas_padron_reportes import (
    FilaNominal,
    filtrar_nominal,
    ordenar_lista_control,
    partes_nombre,
)
from aplicacion.secciones import agrupar_secciones_activas


class ServicioReportes:
    def __init__(self, repo):
        self.repo = repo

    def comedor(self, desde, hasta):
        return self.repo.comedor(desde, hasta)

    def transporte(self, desde, hasta):
        return self.repo.transporte(desde, hasta)

    def ventas(self, desde, hasta):
        return self.repo.ventas(desde, hasta)

    def configuracion_institucional(self):
        configuracion = self.repo.configuracion_institucional()
        if configuracion:
            return {
                "nombre_colegio": configuracion.nombre_colegio,
                "subtitulo_reportes": configuracion.subtitulo_reportes,
            }
        return {
            "nombre_colegio": "Colegio Técnico Profesional de Platanares",
            "subtitulo_reportes": "Comedor estudiantil",
        }

    def lista_control(self, fecha, servicio, filtros):
        """Entrega el padrón completo filtrado para una sola operación.

        La tabla del dashboard es paginada; esta salida no. Ambas comparten los
        mismos filtros para que una descarga no contradiga la pantalla.
        """
        if servicio not in {"comedor", "transporte"}:
            raise ValueError("El servicio debe ser comedor o transporte")
        filas = list(self.repo.personas_dashboard(fecha, "estudiante"))
        presentes = {persona_id for persona_id, _ in self.repo.ingresos_en_fechas([fecha])}
        confirmados = self.repo.reservas_confirmadas_en_fecha(fecha)
        matriculas_con_marca = self.repo.matriculas_con_marca_transporte_en_fecha(fecha)
        nominal: list[FilaNominal] = []
        for persona, matricula, ruta in filas:
            tiene_ruta = ruta is not None
            if servicio == "comedor":
                estado_clave = "presente" if persona.id in presentes else "sin_registro"
                estado = "Ingresó al comedor" if estado_clave == "presente" else "Aún sin ingreso"
                beneficio = "Beneficiario" if matricula and matricula.becado else "No beneficiario"
                columna_servicio = "Beneficio de comedor"
            else:
                estado_clave = (
                    "presente"
                    if matricula and matricula.id in matriculas_con_marca
                    else "sin_registro"
                )
                estado = "Usó transporte" if estado_clave == "presente" else "Aún sin marca"
                beneficio = "Con ruta asignada" if tiene_ruta else "Sin ruta asignada"
                columna_servicio = "Asignación de transporte"
            nominal.append(
                {
                    "idPersona": persona.id,
                    "identificacion": persona.cedula,
                    "nombreCompleto": persona.nombres,
                    "seccion": matricula.seccion if matricula else "—",
                    "ruta": ruta.nombre if ruta else "Sin ruta",
                    "idRuta": ruta.id if ruta else None,
                    "beneficio": beneficio,
                    "beneficioClave": "beneficiario"
                    if matricula and matricula.becado
                    else "no_beneficiario",
                    "columnaServicio": columna_servicio,
                    "estado": estado,
                    "estadoClave": estado_clave,
                    "confirmacionClave": (
                        "confirmada" if persona.id in confirmados else "sin_confirmar"
                    ),
                }
            )
        filtradas = filtrar_nominal(nominal, {**filtros, "servicio": servicio})
        for fila in filtradas:
            fila["apellidos"], fila["nombres"] = partes_nombre(fila["nombreCompleto"])
        return ordenar_lista_control(filtradas)

    def registrar_exportacion_lista_control(
        self, cuenta_id, servicio, formato, fecha, filtros, total
    ):
        self.repo.registrar_exportacion_lista_control(
            cuenta_id, servicio, formato, fecha, filtros, total
        )

    def dashboard(self, fecha, filtros):
        tipo = filtros.get("tipoPersona", "estudiante")
        filas = list(self.repo.personas_dashboard(fecha, tipo))
        dias: list[date] = []
        cursor = fecha
        while len(dias) < 20:
            if cursor.weekday() < 5:
                dias.append(cursor)
            cursor -= timedelta(days=1)
        dias.reverse()
        ingresos = list(self.repo.ingresos_en_fechas(dias))
        personas_padron = {persona.id for persona, _, _ in filas}
        ingresos_por_dia: dict = defaultdict(set)
        for persona_id, dia in ingresos:
            if persona_id in personas_padron:
                ingresos_por_dia[dia].add(persona_id)
        presentes_hoy = ingresos_por_dia[fecha]
        confirmados_hoy = self.repo.reservas_confirmadas_en_fecha(fecha).intersection(
            personas_padron
        )
        matriculas_con_marca = self.repo.matriculas_con_marca_transporte_en_fecha(fecha)
        servicio_nominal = filtros.get("servicio", "comedor") if tipo == "estudiante" else ""

        nominal: list[FilaNominal] = []
        rutas: dict[tuple[int | None, str], set[int]] = defaultdict(set)
        beneficiarios = 0
        for persona, matricula, ruta in filas:
            becado = bool(matricula and matricula.becado)
            beneficiarios += int(becado)
            nombre_ruta = ruta.nombre if ruta else "Sin ruta"
            rutas[(ruta.id if ruta else None, nombre_ruta)].add(persona.id)
            presente_comedor = persona.id in presentes_hoy
            presente = (
                matricula is not None and matricula.id in matriculas_con_marca
                if servicio_nominal == "transporte"
                else presente_comedor
            )
            nominal.append(
                {
                    "idPersona": persona.id,
                    "identificacion": persona.cedula,
                    "nombreCompleto": persona.nombres,
                    "seccion": matricula.seccion if matricula else "—",
                    "ruta": nombre_ruta,
                    "idRuta": ruta.id if ruta else None,
                    "beneficioComedor": "Beneficiario" if becado else "No beneficiario",
                    "beneficioServicio": (
                        "Con ruta asignada"
                        if ruta
                        else "Sin ruta asignada"
                        if servicio_nominal == "transporte"
                        else "Beneficiario"
                        if becado
                        else "No beneficiario"
                    ),
                    "beneficioClave": "beneficiario" if becado else "no_beneficiario",
                    "estado": (
                        "Usó transporte"
                        if presente
                        else "Aún sin marca"
                        if servicio_nominal == "transporte"
                        else "Ingresó al comedor"
                        if presente
                        else "Aún sin ingreso"
                    ),
                    "estadoClave": "presente" if presente else "sin_registro",
                    "confirmacionClave": (
                        "confirmada" if persona.id in confirmados_hoy else "sin_confirmar"
                    ),
                    "historico": False,
                }
            )

        nominal = ordenar_lista_control(filtrar_nominal(nominal, filtros))

        total = len(filas)
        presentes = len(presentes_hoy.intersection({p.id for p, _, _ in filas}))
        por_pagina = max(1, min(100, int(filtros.get("porPagina", 25))))
        pagina = max(1, int(filtros.get("pagina", 1)))
        inicio = (pagina - 1) * por_pagina
        serie = [
            {
                "dia": dia.strftime("%d/%m"),
                "presentes": len(ingresos_por_dia[dia]),
                "ausentes": max(0, total - len(ingresos_por_dia[dia])),
                "porcentaje": round(len(ingresos_por_dia[dia]) * 100 / total, 1) if total else 0,
            }
            for dia in dias
        ]
        por_ruta = [
            {
                "idRuta": id_ruta,
                "nombre": nombre,
                "total": len(personas),
                "presentes": len(personas.intersection(presentes_hoy)),
                "consumo": len(personas.intersection(presentes_hoy)),
            }
            for (id_ruta, nombre), personas in sorted(
                rutas.items(), key=lambda elemento: len(elemento[1]), reverse=True
            )
        ]
        estados_comedor = [
            {
                "nombre": "Beneficiarios",
                "total": beneficiarios,
                "presentes": sum(
                    1 for p, m, _ in filas if m and m.becado and p.id in presentes_hoy
                ),
                "consumo": sum(1 for p, m, _ in filas if m and m.becado and p.id in presentes_hoy),
            },
            {
                "nombre": "No beneficiarios",
                "total": total - beneficiarios,
                "presentes": sum(
                    1 for p, m, _ in filas if not (m and m.becado) and p.id in presentes_hoy
                ),
                "consumo": sum(
                    1 for p, m, _ in filas if not (m and m.becado) and p.id in presentes_hoy
                ),
            },
        ]
        titulos_alertas = {
            "beneficiario con baja asistencia": "Becados con baja asistencia",
            "beneficiario sin consumo reciente": "Becados sin consumo reciente",
            "candidato para revisión de beneficio": "Posibles candidatos a revisión de beneficio",
        }
        alertas = [
            {
                "tipo": senal.replace(" ", "_"),
                "titulo": titulos_alertas.get(senal, senal),
                "cantidad": cantidad,
            }
            for senal, cantidad in self.repo.alertas_analiticas(fecha)
        ]
        casos_analiticos = [
            {
                "idPersona": persona.id,
                "nombreCompleto": persona.nombres,
                "seccion": matricula.seccion,
                "senal": indicador.senal,
                "porcentajeAsistencia": float(indicador.porcentaje_asistencia),
                "consumosComedor": indicador.consumos_comedor,
            }
            for indicador, persona, matricula in self.repo.casos_analiticos(fecha)
        ]
        return {
            "tipoPersona": tipo,
            "seccionesActivas": (
                agrupar_secciones_activas(
                    [matricula.seccion for _, matricula, _ in filas if matricula]
                )
                if tipo == "estudiante"
                else []
            ),
            "asistencia": {
                "porcentaje": round(presentes * 100 / total, 1) if total else 0,
                "presentes": presentes,
                "total": total,
                "sinRegistro": total - presentes,
                "ausentes": 0,
            },
            "capacidad": {
                "totalEstudiantes": total,
                "confirmados": len(confirmados_hoy),
                "asistieron": presentes,
                "pendientes": max(0, len(confirmados_hoy) - presentes),
            },
            "beneficiariosComedor": beneficiarios,
            "noBeneficiarios": total - beneficiarios,
            "consumoComedor": presentes,
            "porRuta": por_ruta,
            "porEstadoComedor": estados_comedor,
            "alertas": alertas,
            "casosAnaliticos": casos_analiticos,
            "semana": serie[-5:],
            "ultimosCincoDias": serie[-5:],
            "tendenciaVeinteDias": serie,
            "nominal": {
                "elementos": nominal[inicio : inicio + por_pagina],
                "total": len(nominal),
                "pagina": pagina,
                "porPagina": por_pagina,
            },
            "cobertura": {
                "personas": total,
                "conMatricula": sum(1 for _, m, _ in filas if m),
                "conRuta": sum(1 for _, _, r in filas if r),
                "conBeneficio": beneficiarios,
            },
        }
