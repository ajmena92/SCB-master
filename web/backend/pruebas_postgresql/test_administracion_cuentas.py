from sqlalchemy import select
from sqlalchemy.orm import Session

from aplicacion.modelos.maestros import CuentaAdministrativa, Persona
from aplicacion.seguridad import hash_secreto

from .conftest import autenticar_administracion, autenticar_portal, crear_persona


def test_cuenta_operador_exige_cambio_y_revoca_permiso_inmediatamente(entorno):
    cliente, motor, h = entorno
    profesor = crear_persona(
        cliente, h["admin"], tipo="profesor", cedula="777", nombres="Docente Operador"
    )
    creada = cliente.post(
        "/api/v1/administracion/cuentas",
        headers=h["admin"],
        json={
            "personaId": profesor["id"],
            "usuario": "Nuevo.Operador",
            "rol": "operador",
            "permisos": ["dashboard.leer"],
        },
    )
    assert creada.status_code == 201, creada.text
    datos = creada.json()
    assert datos["cuenta"]["usuario"] == "nuevo.operador"
    assert "contrasena" in datos["credencialesTemporales"]
    assert "pin" not in datos["credencialesTemporales"]

    cuenta = autenticar_administracion(cliente.app, "NUEVO.OPERADOR", datos["credencialesTemporales"]["contrasena"])
    assert cuenta.get("/api/v1/sesion").json()["cambioContrasenaObligatorio"] is True
    assert (
        cuenta.get("/api/v1/reportes/dashboard?fecha=2026-08-31").status_code
        == 403
    )
    cambio = cuenta.post(
        "/api/v1/autenticacion/administracion/contrasena",
        headers=cuenta.csrf(),
        json={
            "contrasenaActual": datos["credencialesTemporales"]["contrasena"],
            "contrasenaNueva": "Otra-clave-segura-2026",
        },
    )
    assert cambio.status_code == 200
    assert cuenta.get("/api/v1/sesion").status_code == 204


    cuenta = autenticar_administracion(cliente.app, "nuevo.operador", "Otra-clave-segura-2026")
    assert (
        cuenta.get("/api/v1/reportes/dashboard?fecha=2026-08-31").status_code
        == 200
    )

    cuenta_id = datos["cuenta"]["id"]
    actualizada = cliente.put(
        f"/api/v1/administracion/cuentas/{cuenta_id}",
        headers=h["admin"],
        json={"permisos": []},
    )
    assert actualizada.status_code == 200, actualizada.text
    assert cuenta.get("/api/v1/sesion").status_code == 204


def test_operador_cambia_su_contrasena_sin_cambio_obligatorio(entorno):
    _, _, h = entorno
    operador = h["operador_cliente"]
    incorrecta = operador.post(
        "/api/v1/autenticacion/administracion/contrasena",
        headers=operador.csrf(),
        json={
            "contrasenaActual": "clave-incorrecta-2026",
            "contrasenaNueva": "Clave-operador-nueva-2026",
        },
    )
    assert incorrecta.status_code == 401
    assert operador.get("/api/v1/sesion").status_code == 200

    cambio = operador.post(
        "/api/v1/autenticacion/administracion/contrasena",
        headers=operador.csrf(),
        json={
            "contrasenaActual": "Clave-operador-2026",
            "contrasenaNueva": "Clave-operador-nueva-2026",
        },
    )
    assert cambio.status_code == 200, cambio.text
    assert operador.get("/api/v1/sesion").status_code == 204
    nueva_sesion = autenticar_administracion(
        operador.app, "operador", "Clave-operador-nueva-2026"
    )
    assert nueva_sesion.get("/api/v1/sesion").status_code == 200


def test_permite_cambiar_el_profesor_vinculado_y_revoca_sus_sesiones(entorno):
    cliente, _, h = entorno
    profesor_origen = crear_persona(
        cliente, h["admin"], tipo="profesor", cedula="771", nombres="Docente Origen"
    )
    profesor_destino = crear_persona(
        cliente, h["admin"], tipo="profesor", cedula="772", nombres="Docente Destino"
    )
    creada = cliente.post(
        "/api/v1/administracion/cuentas",
        headers=h["admin"],
        json={
            "personaId": profesor_origen["id"],
            "usuario": "cuenta.reasignable",
            "rol": "operador",
            "permisos": ["dashboard.leer"],
        },
    )
    assert creada.status_code == 201, creada.text
    cuenta_id = creada.json()["cuenta"]["id"]
    cuenta = autenticar_administracion(
        cliente.app, "cuenta.reasignable", creada.json()["credencialesTemporales"]["contrasena"]
    )

    actualizada = cliente.put(
        f"/api/v1/administracion/cuentas/{cuenta_id}",
        headers=h["admin"],
        json={"personaId": profesor_destino["id"]},
    )
    assert actualizada.status_code == 200, actualizada.text
    assert actualizada.json()["persona"]["id"] == profesor_destino["id"]
    assert cuenta.get("/api/v1/sesion").status_code == 204


def test_administrador_puede_cambiar_el_usuario_de_operador(entorno):
    cliente, _, h = entorno
    respuesta = cliente.put(
        "/api/v1/administracion/cuentas/2",
        headers=h["admin"],
        json={"usuario": "Operador.Nuevo"},
    )

    assert respuesta.status_code == 200, respuesta.text
    assert respuesta.json()["usuario"] == "operador.nuevo"
    assert autenticar_administracion(cliente.app, "OPERADOR.NUEVO", "Clave-operador-2026")


def test_protege_cuenta_propia_y_ultimo_administrador(entorno):
    cliente, _, h = entorno
    sesion = cliente.get("/api/v1/sesion", headers=h["admin"]).json()
    respuesta = cliente.put(
        f"/api/v1/administracion/cuentas/{sesion['cuentaId']}",
        headers=h["admin"],
        json={"activo": False},
    )
    assert respuesta.status_code == 409
    respuesta = cliente.put(
        f"/api/v1/administracion/cuentas/{sesion['cuentaId']}",
        headers=h["admin"],
        json={"rol": "operador", "permisos": ["dashboard.leer"]},
    )
    assert respuesta.status_code == 409


def test_vinculacion_inicial_es_unica_y_rechaza_portal(entorno):
    cliente, motor, h = entorno
    profesor_portal = crear_persona(
        cliente, h["admin"], tipo="profesor", cedula="778", nombres="Docente Portal"
    )
    portal = autenticar_portal(cliente.app, "778", profesor_portal["pinTemporal"])
    assert (
        portal.post(
            "/api/v1/administracion/vinculacion-inicial",
            headers=portal.csrf(),
            json={"personaId": profesor_portal["id"]},
        ).status_code
        == 403
    )

    with Session(motor) as sesion:
        legado = CuentaAdministrativa(
            usuario="legado",
            contrasena_hash=hash_secreto("Clave-legada-segura-2026"),
            rol="administrador",
            activo=True,
            persona_id=None,
            vinculacion_pendiente=True,
        )
        profesor = Persona(
            cedula="779",
            nombres="Docente Vinculacion",
            tipo="profesor",
            activo=True,
        )
        sesion.add_all([legado, profesor])
        sesion.commit()
        profesor_id = profesor.id

    cuenta = autenticar_administracion(cliente.app, "legado", "Clave-legada-segura-2026")
    assert (
        cuenta.get("/api/v1/administracion/profesores-disponibles").status_code
        == 200
    )
    assert cuenta.get("/api/v1/personas").status_code == 403
    assert (
        cuenta.post(
            "/api/v1/administracion/vinculacion-inicial",
            headers=cuenta.csrf(),
            json={"personaId": profesor_id},
        ).status_code
        == 200
    )
    assert (
        cuenta.post(
            "/api/v1/administracion/vinculacion-inicial",
            headers=cuenta.csrf(),
            json={"personaId": profesor_id},
        ).status_code
        == 409
    )

    with Session(motor) as sesion:
        assert (
            sesion.scalar(
                select(CuentaAdministrativa).where(CuentaAdministrativa.usuario == "legado")
            ).persona_id
            == profesor_id
        )


def test_valida_profesor_permisos_y_usuario_sin_distinguir_mayusculas(entorno):
    cliente, motor, h = entorno
    estudiante = crear_persona(
        cliente, h["admin"], cedula="780", nombres="Persona Estudiante"
    )
    invalida = cliente.post(
        "/api/v1/administracion/cuentas",
        headers=h["admin"],
        json={
            "personaId": estudiante["id"],
            "usuario": "estudiante.admin",
            "rol": "operador",
            "permisos": [],
        },
    )
    assert invalida.status_code == 422

    profesor = crear_persona(
        cliente, h["admin"], tipo="profesor", cedula="781", nombres="Profesor Disponible"
    )
    desconocido = cliente.post(
        "/api/v1/administracion/cuentas",
        headers=h["admin"],
        json={
            "personaId": profesor["id"],
            "usuario": "permiso.raro",
            "rol": "operador",
            "permisos": ["permiso.inexistente"],
        },
    )
    assert desconocido.status_code == 422

    creada = cliente.post(
        "/api/v1/administracion/cuentas",
        headers=h["admin"],
        json={
            "personaId": profesor["id"],
            "usuario": "Caso.Unico",
            "rol": "operador",
            "permisos": [],
        },
    )
    assert creada.status_code == 201
    duplicada = cliente.post(
        "/api/v1/administracion/cuentas",
        headers=h["admin"],
        json={
            "profesorNuevo": {"cedula": "782", "nombres": "Otro Profesor"},
            "usuario": "CASO.UNICO",
            "rol": "operador",
            "permisos": [],
        },
    )
    assert duplicada.status_code == 409
    repetida = cliente.post(
        "/api/v1/administracion/cuentas",
        headers=h["admin"],
        json={
            "personaId": profesor["id"],
            "usuario": "otra.cuenta",
            "rol": "operador",
            "permisos": [],
        },
    )
    assert repetida.status_code == 409

    with Session(motor) as sesion:
        disponible = Persona(
            cedula="783",
            nombres="Profesor Inactivo",
            tipo="profesor",
            activo=False,
        )
        sesion.add(disponible)
        sesion.commit()
        inactivo_id = disponible.id
    inactiva = cliente.post(
        "/api/v1/administracion/cuentas",
        headers=h["admin"],
        json={
            "personaId": inactivo_id,
            "usuario": "inactivo",
            "rol": "operador",
            "permisos": [],
        },
    )
    assert inactiva.status_code == 422


def test_profesor_nuevo_entrega_secretos_y_reset_revoca_sesiones(entorno):
    cliente, _, h = entorno
    creada = cliente.post(
        "/api/v1/administracion/cuentas",
        headers=h["admin"],
        json={
            "profesorNuevo": {"cedula": "784", "nombres": "Profesor Nuevo"},
            "usuario": "profesor.nuevo",
            "rol": "operador",
            "permisos": ["comedor.operar"],
        },
    )
    assert creada.status_code == 201, creada.text
    salida = creada.json()
    secretos = salida["credencialesTemporales"]
    assert secretos["pin"].isdigit() and len(secretos["pin"]) == 6
    assert secretos["pin"] != secretos["contrasena"]
    cuenta_id = salida["cuenta"]["id"]

    cuenta = autenticar_administracion(cliente.app, "profesor.nuevo", secretos["contrasena"])
    reset = cliente.post(
        f"/api/v1/administracion/cuentas/{cuenta_id}/restablecer-contrasena",
        headers=h["admin"],
    )
    assert reset.status_code == 200
    assert reset.json()["contrasenaTemporal"] != secretos["contrasena"]
    assert cuenta.get("/api/v1/sesion").status_code == 204


def test_administrador_no_puede_restablecer_su_propia_cuenta(entorno):
    cliente, motor, h = entorno
    with Session(motor) as sesion:
        cuenta = sesion.scalar(
            select(CuentaAdministrativa).where(CuentaAdministrativa.usuario == "admin")
        )
    respuesta = cliente.post(
        f"/api/v1/administracion/cuentas/{cuenta.id}/restablecer-contrasena",
        headers=h["admin"],
    )
    assert respuesta.status_code == 409
    assert "Cambiar mi contraseña" in respuesta.json()["detail"]


def test_sesion_se_invalida_si_el_profesor_deja_de_ser_activo(entorno):
    cliente, motor, h = entorno
    with Session(motor) as sesion:
        cuenta = sesion.scalar(
            select(CuentaAdministrativa).where(CuentaAdministrativa.usuario == "operador")
        )
        profesor = sesion.get(Persona, cuenta.persona_id)
        profesor.activo = False
        sesion.commit()
    assert cliente.get("/api/v1/sesion", headers=h["operador"]).status_code == 204
