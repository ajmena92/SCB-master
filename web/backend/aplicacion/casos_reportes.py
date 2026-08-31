"""Casos de uso para reportes operativos y tablero PostgreSQL."""

from collections import Counter
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
        dias = [fecha - timedelta(days=desfase) for desfase in range(4, -1, -1)]
        ingresos = list(self.repo.ingresos_en_fechas(dias))
        presentes_hoy = {persona_id for persona_id, dia in ingresos if dia == fecha}

        nominal = []
        rutas: Counter[tuple[int | None, str]] = Counter()
        beneficiarios = 0
        for persona, matricula, ruta in filas:
            becado = bool(matricula and matricula.becado)
            beneficiarios += int(becado)
            nombre_ruta = ruta.nombre if ruta else "Sin ruta"
            rutas[(ruta.id if ruta else None, nombre_ruta)] += 1
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
        conteo_dia = Counter(dia for _, dia in ingresos)
        serie = [
            {
                "dia": dia.strftime("%d/%m"),
                "presentes": conteo_dia[dia],
                "ausentes": max(0, total - conteo_dia[dia]),
                "porcentaje": round(conteo_dia[dia] * 100 / total, 1) if total else 0,
            }
            for dia in dias
        ]
        por_ruta = [
            {"idRuta": id_ruta, "nombre": nombre, "total": cantidad, "presentes": 0, "consumo": 0}
            for (id_ruta, nombre), cantidad in sorted(
                rutas.items(), key=lambda elemento: elemento[1], reverse=True
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
            "porEstadoComedor": estados_comedor,
            "alertas": [],
            "semana": serie,
            "ultimosCincoDias": serie,
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
