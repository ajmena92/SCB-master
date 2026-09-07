from datetime import date, time, timedelta

from sqlalchemy.orm import Session

from aplicacion.modelos.maestros import (
    AsignacionRuta,
    ConfiguracionInstitucional,
    Matricula,
    Persona,
    Ruta,
)
from aplicacion.modelos.operacion import (
    EventoExportacionListaControl,
    IngresoComedor,
    MarcaTransporte,
    ReservaComedor,
)

from .conftest import autenticar_portal, crear_persona, preparar_estudiante


def test_dashboard_usa_padron_anual_postgresql(entorno):
    cliente, _, auth = entorno
    persona, anio, _ = preparar_estudiante(cliente, auth["admin"], "dashboard-1")

    respuesta = cliente.get(
        "/api/v1/reportes/dashboard",
        headers=auth["admin"],
        params={"fecha": f"{anio['anio']}-08-30", "tipoPersona": "estudiante"},
    )

    assert respuesta.status_code == 200, respuesta.text
    datos = respuesta.json()
    assert datos["asistencia"]["total"] == 1
    assert datos["nominal"]["elementos"][0]["idPersona"] == persona["id"]
    assert datos["cobertura"]["conMatricula"] == 1
    assert len(datos["ultimosCincoDias"]) == 5


def test_dashboard_profesores_no_depende_de_matricula_ni_ruta(entorno):
    cliente, _, auth = entorno

    respuesta = cliente.get(
        "/api/v1/reportes/dashboard",
        headers=auth["admin"],
        params={"fecha": "2026-08-30", "tipoPersona": "profesor"},
    )

    assert respuesta.status_code == 200, respuesta.text
    datos = respuesta.json()
    assert datos["tipoPersona"] == "profesor"
    assert datos["asistencia"]["total"] == 2
    assert datos["seccionesActivas"] == []
    assert all(fila["seccion"] == "—" for fila in datos["nominal"]["elementos"])
    assert all(fila["identificacion"] for fila in datos["nominal"]["elementos"])


def test_dashboard_publica_solo_secciones_activas_agrupadas_y_ordenadas(entorno):
    cliente, motor, auth = entorno
    estudiante_7, anio, _ = preparar_estudiante(cliente, auth["admin"], "seccion-7")
    estudiante_7_9 = crear_persona(cliente, auth["admin"], cedula="seccion-7-9")
    estudiante_8 = crear_persona(cliente, auth["admin"], cedula="seccion-8-5")
    estudiante_10 = crear_persona(cliente, auth["admin"], cedula="seccion-10-2")
    estudiante_inactivo = crear_persona(cliente, auth["admin"], cedula="seccion-inactiva")
    with Session(motor) as sesion:
        sesion.add_all(
            [
                Matricula(
                    persona_id=estudiante_7_9["id"],
                    anio_lectivo_id=anio["id"],
                    seccion="7-9",
                    turno="diurno",
                    becado=False,
                    estado="activo",
                ),
                Matricula(
                    persona_id=estudiante_8["id"],
                    anio_lectivo_id=anio["id"],
                    seccion="8-5",
                    turno="diurno",
                    becado=False,
                    estado="activo",
                ),
                Matricula(
                    persona_id=estudiante_10["id"],
                    anio_lectivo_id=anio["id"],
                    seccion="10-2",
                    turno="diurno",
                    becado=False,
                    estado="activo",
                ),
                Matricula(
                    persona_id=estudiante_inactivo["id"],
                    anio_lectivo_id=anio["id"],
                    seccion="8-4",
                    turno="diurno",
                    becado=False,
                    estado="activo",
                ),
            ]
        )
        sesion.get(Persona, estudiante_inactivo["id"]).activo = False
        sesion.commit()

    respuesta = cliente.get(
        "/api/v1/reportes/dashboard",
        headers=auth["admin"],
        params={"fecha": f"{anio['anio']}-08-30", "tipoPersona": "estudiante"},
    )

    assert respuesta.status_code == 200, respuesta.text
    assert respuesta.json()["seccionesActivas"] == [
        {"grado": 7, "etiqueta": "Séptimo", "secciones": ["7-1", "7-9"]},
        {"grado": 8, "etiqueta": "Octavo", "secciones": ["8-5"]},
        {"grado": 10, "etiqueta": "Décimo", "secciones": ["10-2"]},
    ]


def test_dashboard_informa_capacidad_con_reservas_confirmadas_y_asistencia(entorno):
    cliente, motor, auth = entorno
    fecha = date(2026, 8, 31)
    estudiante_reservado, anio, _ = preparar_estudiante(cliente, auth["admin"], "capacidad-1")
    estudiante_consumido = crear_persona(cliente, auth["admin"], cedula="capacidad-2")
    estudiante_cancelado = crear_persona(cliente, auth["admin"], cedula="capacidad-3")
    estudiante_inactivo = crear_persona(cliente, auth["admin"], cedula="capacidad-inactivo")

    with Session(motor) as sesion:
        sesion.add_all(
            [
                Matricula(
                    persona_id=estudiante_consumido["id"],
                    anio_lectivo_id=anio["id"],
                    seccion="7-1",
                    turno="diurno",
                    becado=False,
                    estado="activo",
                ),
                Matricula(
                    persona_id=estudiante_cancelado["id"],
                    anio_lectivo_id=anio["id"],
                    seccion="7-1",
                    turno="diurno",
                    becado=False,
                    estado="activo",
                ),
                Matricula(
                    persona_id=estudiante_inactivo["id"],
                    anio_lectivo_id=anio["id"],
                    seccion="7-1",
                    turno="diurno",
                    becado=False,
                    estado="activo",
                ),
            ]
        )
        sesion.get(Persona, estudiante_inactivo["id"]).activo = False
        reserva_consumida = ReservaComedor(
            persona_id=estudiante_consumido["id"], fecha=fecha, estado="consumida"
        )
        sesion.add_all(
            [
                ReservaComedor(persona_id=estudiante_reservado["id"], fecha=fecha, estado="reservada"),
                reserva_consumida,
                ReservaComedor(persona_id=estudiante_cancelado["id"], fecha=fecha, estado="cancelada"),
                ReservaComedor(persona_id=estudiante_inactivo["id"], fecha=fecha, estado="reservada"),
            ]
        )
        sesion.flush()
        sesion.add(
            IngresoComedor(
                persona_id=estudiante_consumido["id"],
                fecha=fecha,
                reserva_id=reserva_consumida.id,
                modalidad="reserva",
                consumio_tiquete=False,
                operador_id=1,
            )
        )
        sesion.commit()

    respuesta = cliente.get(
        "/api/v1/reportes/dashboard",
        headers=auth["admin"],
        params={"fecha": fecha.isoformat(), "tipoPersona": "estudiante"},
    )

    assert respuesta.status_code == 200, respuesta.text
    datos = respuesta.json()
    assert datos["capacidad"] == {
        "totalEstudiantes": 3,
        "confirmados": 2,
        "asistieron": 1,
        "pendientes": 1,
    }
    assert "porSeccion" not in datos


def test_lista_control_exporta_un_servicio_y_respeta_los_filtros(entorno):
    cliente, motor, auth = entorno
    fecha = date(2026, 8, 31)
    estudiante, anio, matricula = preparar_estudiante(cliente, auth["admin"], "=formula")
    otro_estudiante = crear_persona(cliente, auth["admin"], cedula="lista-oculta")
    with Session(motor) as sesion:
        sesion.add(
            Matricula(
                persona_id=otro_estudiante["id"],
                anio_lectivo_id=anio["id"],
                seccion="8-1",
                turno="diurno",
                becado=True,
                estado="activo",
            )
        )
        ruta = Ruta(
            codigo="L01",
            nombre="Ruta Lista",
            descripcion="Ruta de pruebas de exportación",
            color_hex="#1E4F8A",
            activo=True,
        )
        sesion.add(ruta)
        sesion.flush()
        sesion.add_all(
            [
                AsignacionRuta(
                    matricula_id=matricula["id"], ruta_id=ruta.id, fecha_inicio=date(2026, 1, 1)
                ),
                IngresoComedor(
                    persona_id=estudiante["id"],
                    fecha=fecha,
                    modalidad="reserva",
                    consumio_tiquete=False,
                    operador_id=1,
                ),
                ReservaComedor(persona_id=estudiante["id"], fecha=fecha, estado="reservada"),
                MarcaTransporte(
                    matricula_id=matricula["id"], ruta_id=ruta.id, fecha=fecha, operador_id=1
                ),
            ]
        )
        sesion.add(
            ConfiguracionInstitucional(
                id=1,
                nombre_colegio="CTP Platanares — Prueba",
                subtitulo_reportes="Control institucional de servicios",
            )
        )
        sesion.commit()

    tablero_comedor = cliente.get(
        "/api/v1/reportes/dashboard",
        headers=auth["admin"],
        params={
            "fecha": fecha.isoformat(),
            "servicio": "comedor",
            "confirmacion": "confirmada",
            "asistencia": "presente",
            "beneficio": "no_beneficiario",
        },
    )
    assert tablero_comedor.status_code == 200, tablero_comedor.text
    assert [fila["idPersona"] for fila in tablero_comedor.json()["nominal"]["elementos"]] == [
        estudiante["id"]
    ]

    tablero_ordenado = cliente.get(
        "/api/v1/reportes/dashboard",
        headers=auth["admin"],
        params={"fecha": fecha.isoformat(), "servicio": "comedor"},
    )
    assert tablero_ordenado.status_code == 200, tablero_ordenado.text
    assert [fila["seccion"] for fila in tablero_ordenado.json()["nominal"]["elementos"]] == [
        "7-1", "8-1"
    ]

    tablero_transporte = cliente.get(
        "/api/v1/reportes/dashboard",
        headers=auth["admin"],
        params={
            "fecha": fecha.isoformat(),
            "servicio": "transporte",
            "asignacion": "con_ruta",
            "asistencia": "presente",
        },
    )
    assert tablero_transporte.status_code == 200, tablero_transporte.text
    assert [fila["idPersona"] for fila in tablero_transporte.json()["nominal"]["elementos"]] == [
        estudiante["id"]
    ]

    parametros = {"fecha": fecha.isoformat(), "servicio": "comedor", "seccion": "7-1"}
    csv_respuesta = cliente.get(
        "/api/v1/reportes/lista-control",
        headers=auth["admin"],
        params={**parametros, "formato": "csv"},
    )
    assert csv_respuesta.status_code == 200, csv_respuesta.text
    assert csv_respuesta.headers["content-type"].startswith("text/csv")
    assert "Aún sin ingreso" not in csv_respuesta.text
    assert "'=formula" in csv_respuesta.text
    assert "lista-oculta" not in csv_respuesta.text

    ordenada = cliente.get(
        "/api/v1/reportes/lista-control",
        headers=auth["admin"],
        params={"fecha": fecha.isoformat(), "servicio": "comedor", "formato": "csv"},
    )
    lineas = ordenada.text.lstrip("\ufeff").splitlines()
    assert lineas[0].split(",") == [
        "N°", "Identificación", "Apellidos", "Nombres", "Sección", "Ruta",
        "Beneficio de comedor", "Estado",
    ]
    assert lineas[1].split(",")[1] == "'=formula"
    assert lineas[2].split(",")[1] == "lista-oculta"

    xlsx_respuesta = cliente.get(
        "/api/v1/reportes/lista-control",
        headers=auth["admin"],
        params={**parametros, "formato": "xlsx"},
    )
    assert xlsx_respuesta.status_code == 200, xlsx_respuesta.text
    assert xlsx_respuesta.content.startswith(b"PK")

    transporte = cliente.get(
        "/api/v1/reportes/lista-control",
        headers=auth["admin"],
        params={**parametros, "servicio": "transporte", "formato": "pdf"},
    )
    assert transporte.status_code == 200, transporte.text
    assert "CTP Platanares — Prueba" in transporte.text
    assert "Control institucional de servicios" in transporte.text
    assert "Lista de control — Transporte" in transporte.text
    assert "Servicio: Transporte" in transporte.text
    assert "Filtros aplicados: Sección: 7-1" in transporte.text
    assert "Asignación de transporte" in transporte.text
    assert "Beneficio de comedor" not in transporte.text
    with Session(motor) as sesion:
        eventos = list(sesion.query(EventoExportacionListaControl).all())
    assert [(evento.servicio, evento.formato, evento.total_registros) for evento in eventos] == [
        ("comedor", "csv", 1),
        ("comedor", "csv", 2),
        ("comedor", "xlsx", 1),
        ("transporte", "pdf", 1),
    ]
    assert all("busqueda" not in evento.filtros for evento in eventos)


def test_portal_muestra_plantilla_semanal_y_carnet(entorno):
    cliente, _, auth = entorno
    persona, _, matricula = preparar_estudiante(cliente, auth["admin"], "portal-1")
    ruta = cliente.post(
        "/api/v1/rutas",
        headers=auth["admin"],
        json={
            "codigo": "1115308",
            "descripcion": "SIERRA",
            "colorHex": "#38BDF8",
        },
    ).json()
    asignacion = cliente.post(
        f"/api/v1/rutas/{ruta['idRuta']}/asignaciones",
        headers=auth["admin"],
        json={
            "matriculaId": matricula["id"],
            "fechaInicio": f"{date.today().year}-01-01",
        },
    )
    assert asignacion.status_code == 201, asignacion.text
    # El catálogo semanal acepta lunes a viernes. La prueba debe ser válida
    # también cuando se ejecuta en fin de semana.
    fecha = date.today() - timedelta(days=date.today().weekday())
    semana_panea = (fecha.day - 1) // 7 + 1
    plantilla = cliente.post(
        "/api/v1/menu/plantillas",
        headers=auth["admin"],
        json={
            "semana": semana_panea,
            "dia": fecha.isoweekday(),
            "titulo": "Almuerzo tradicional",
            "activo": True,
            "componentes": [
                {"nombre": "Arroz", "tipo": "Principal", "orden": 1},
                {"nombre": "Frijoles", "tipo": "Acompañamiento", "orden": 2},
            ],
        },
    ).json()
    assert plantilla["id"] > 0
    portal = autenticar_portal(cliente.app, persona["cedula"])

    estado = portal.get("/api/v1/portal/estado", params={"fecha": fecha.isoformat()})
    carnet = portal.get("/api/v1/portal/carnet", params={"fecha": fecha.isoformat()})

    assert estado.status_code == 200, estado.text
    estado_portal = estado.json()
    estado_comedor = estado_portal["estado"]
    assert estado_portal["menu"]["Titulo"] == "Almuerzo tradicional"
    assert [c["Nombre"] for c in estado_portal["menu"]["Componentes"]] == [
        "Arroz",
        "Frijoles",
    ]
    hora_servidor = time.fromisoformat(estado_comedor["horaServidor"])
    hora_limite = time.fromisoformat(estado_comedor["horaLimite"])
    esperados = max(
        0,
        int(
            (hora_limite.hour * 3600 + hora_limite.minute * 60 + hora_limite.second)
            - (hora_servidor.hour * 3600 + hora_servidor.minute * 60 + hora_servidor.second)
        ),
    )
    if fecha != date.today():
        esperados = 0
    assert abs(estado_comedor["segundosParaCierre"] - esperados) <= 1
    assert estado_comedor["segundosParaApertura"] == 0
    assert carnet.status_code == 200, carnet.text
    assert carnet.json()["codigoQr"].startswith("SCBQR1.")
    assert carnet.json()["seccion"] == "7-1"
    assert carnet.json()["rutaCodigo"] == "1115308"
    assert carnet.json()["rutaDescripcion"] == "SIERRA"
    assert carnet.json()["rutaColor"] == "#38BDF8"
    assert carnet.json()["anioLectivo"] == date.today().year


def test_reserva_portal_no_requiere_repetir_codigo(entorno):
    cliente, _, auth = entorno
    persona, _, _ = preparar_estudiante(cliente, auth["admin"], "reserva-portal-1")
    portal = autenticar_portal(cliente.app, persona["cedula"])

    respuesta = portal.post(
        "/api/v1/comedor/reservas",
        headers=portal.csrf(),
        json={"fecha": date.today().isoformat()},
    )

    assert respuesta.status_code in {201, 409}, respuesta.text
