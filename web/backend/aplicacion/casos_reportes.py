"""Casos de uso para reportes operativos y tablero PostgreSQL."""

from collections import defaultdict
from datetime import timedelta


class ServicioReportes:
    def __init__(self, repo):
        self.repo = repo

    def comedor(self, desde, hasta):
        return self.repo.comedor(desde, hasta)

    def transporte(self, desde, hasta):
        return self.repo.transporte(desde, hasta)

    def ventas(self, desde, hasta):
        return self.repo.ventas(desde, hasta)

    def dashboard(self, fecha, filtros):
        tipo = filtros.get("tipoPersona", "estudiante")
        filas = list(self.repo.personas_dashboard(fecha, tipo))
        dias = []
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

        nominal = []
        rutas: dict[tuple[int | None, str], set[int]] = defaultdict(set)
        secciones: dict[str, set[int]] = defaultdict(set)
        beneficiarios = 0
        for persona, matricula, ruta in filas:
            becado = bool(matricula and matricula.becado)
            beneficiarios += int(becado)
            nombre_ruta = ruta.nombre if ruta else "Sin ruta"
            rutas[(ruta.id if ruta else None, nombre_ruta)].add(persona.id)
            secciones[matricula.seccion if matricula else "Sin sección"].add(persona.id)
            presente = persona.id in presentes_hoy
            nominal.append(
                {
                    "idPersona": persona.id,
                    "nombreCompleto": persona.nombres,
                    "seccion": matricula.seccion if matricula else "—",
                    "ruta": nombre_ruta,
                    "idRuta": ruta.id if ruta else None,
                    "beneficioComedor": "Beneficiario" if becado else "No beneficiario",
                    "estado": "Presente" if presente else "Sin registro",
                    "estadoClave": "presente" if presente else "sin_registro",
                    "historico": False,
                }
            )

        busqueda = str(filtros.get("busqueda", "")).casefold().strip()
        ruta_filtro = str(filtros.get("ruta", "")).strip()
        seccion = str(filtros.get("seccion", "")).casefold().strip()
        estado = str(filtros.get("estado", "")).strip()
        beneficio = str(filtros.get("beneficioTransporte", "")).strip()
        if busqueda:
            nominal = [r for r in nominal if busqueda in r["nombreCompleto"].casefold()]
        if ruta_filtro:
            nominal = [r for r in nominal if str(r["idRuta"]) == ruta_filtro]
        if seccion:
            nominal = [r for r in nominal if seccion in r["seccion"].casefold()]
        if estado:
            nominal = [r for r in nominal if r["estadoClave"] == estado]
        if beneficio == "beneficiario":
            nominal = [r for r in nominal if r["idRuta"] is not None]
        elif beneficio == "no_beneficiario":
            nominal = [r for r in nominal if r["idRuta"] is None]

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
            {"tipo": senal.replace(" ", "_"), "titulo": titulos_alertas.get(senal, senal), "cantidad": cantidad}
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
            "asistencia": {
                "porcentaje": round(presentes * 100 / total, 1) if total else 0,
                "presentes": presentes,
                "total": total,
                "sinRegistro": total - presentes,
                "ausentes": 0,
            },
            "beneficiariosComedor": beneficiarios,
            "noBeneficiarios": total - beneficiarios,
            "consumoComedor": presentes,
            "porRuta": por_ruta,
            "porSeccion": [
                {"nombre": nombre, "total": len(personas)}
                for nombre, personas in sorted(secciones.items(), key=lambda elemento: elemento[0])
            ],
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
