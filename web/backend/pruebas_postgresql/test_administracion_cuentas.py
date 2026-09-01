from sqlalchemy import select
from sqlalchemy.orm import Session

from aplicacion.modelos.maestros import CuentaAdministrativa, Persona
from aplicacion.seguridad import hash_secreto


def test_cuenta_operador_exige_cambio_y_revoca_permiso_inmediatamente(entorno):
    cliente, motor, h = entorno
    profesor = cliente.post(
        "/api/v1/personas",
        headers=h["admin"],
        json={"cedula": "777", "nombres": "Docente Operador", "tipo": "profesor"},
    ).json()
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

    acceso = cliente.post(
        "/api/v1/autenticacion/administracion",
        json={
            "usuario": "NUEVO.OPERADOR",
            "contrasena": datos["credencialesTemporales"]["contrasena"],
        },
    ).json()
    cabecera = {"Authorization": f"Bearer {acceso['token']}"}
    assert acceso["cambioContrasenaObligatorio"] is True
    assert (
        cliente.get("/api/v1/reportes/dashboard?fecha=2026-08-31", headers=cabecera).status_code
        == 403
    )
    cambio = cliente.post(
        "/api/v1/autenticacion/administracion/contrasena",
        headers=cabecera,
        json={
            "contrasenaActual": datos["credencialesTemporales"]["contrasena"],
            "contrasenaNueva": "Otra-clave-segura-2026",
        },
    )
    assert cambio.status_code == 200
    assert cliente.get("/api/v1/sesion", headers=cabecera).status_code == 401

    acceso = cliente.post(
        "/api/v1/autenticacion/administracion",
        json={"usuario": "nuevo.operador", "contrasena": "Otra-clave-segura-2026"},
    ).json()
    cabecera = {"Authorization": f"Bearer {acceso['token']}"}
    assert (
        cliente.get("/api/v1/reportes/dashboard?fecha=2026-08-31", headers=cabecera).status_code
        == 200
    )

    cuenta_id = datos["cuenta"]["id"]
    actualizada = cliente.put(
        f"/api/v1/administracion/cuentas/{cuenta_id}",
        headers=h["admin"],
        json={"permisos": []},
    )
    assert actualizada.status_code == 200, actualizada.text
    assert cliente.get("/api/v1/sesion", headers=cabecera).status_code == 401


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
    profesor_portal = cliente.post(
        "/api/v1/personas",
        headers=h["admin"],
        json={"cedula": "778", "nombres": "Docente Portal", "tipo": "profesor"},
    ).json()
    portal = cliente.post(
        "/api/v1/autenticacion/portal",
        json={"cedula": "778", "pin": profesor_portal["pinTemporal"]},
    ).json()
    assert (
        cliente.post(
            "/api/v1/administracion/vinculacion-inicial",
            headers={"Authorization": f"Bearer {portal['token']}"},
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
            codigo="P-00000003",
            cedula="779",
            nombres="Docente Vinculacion",
            tipo="profesor",
            activo=True,
        )
        sesion.add_all([legado, profesor])
        sesion.commit()
        profesor_id = profesor.id

    acceso = cliente.post(
        "/api/v1/autenticacion/administracion",
        json={"usuario": "legado", "contrasena": "Clave-legada-segura-2026"},
    ).json()
    cabecera = {"Authorization": f"Bearer {acceso['token']}"}
    assert (
        cliente.get("/api/v1/administracion/profesores-disponibles", headers=cabecera).status_code
        == 200
    )
    assert cliente.get("/api/v1/personas", headers=cabecera).status_code == 403
    assert (
        cliente.post(
            "/api/v1/administracion/vinculacion-inicial",
            headers=cabecera,
            json={"personaId": profesor_id},
        ).status_code
        == 200
    )
    assert (
        cliente.post(
            "/api/v1/administracion/vinculacion-inicial",
            headers=cabecera,
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
    estudiante = cliente.post(
        "/api/v1/personas",
        headers=h["admin"],
        json={"cedula": "780", "nombres": "Persona Estudiante", "tipo": "estudiante"},
    ).json()
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

    profesor = cliente.post(
        "/api/v1/personas",
        headers=h["admin"],
        json={"cedula": "781", "nombres": "Profesor Disponible", "tipo": "profesor"},
    ).json()
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
            codigo="P-00000004",
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

    acceso = cliente.post(
        "/api/v1/autenticacion/administracion",
        json={"usuario": "profesor.nuevo", "contrasena": secretos["contrasena"]},
    ).json()
    cabecera = {"Authorization": f"Bearer {acceso['token']}"}
    reset = cliente.post(
        f"/api/v1/administracion/cuentas/{cuenta_id}/restablecer-contrasena",
        headers=h["admin"],
    )
    assert reset.status_code == 200
    assert reset.json()["contrasenaTemporal"] != secretos["contrasena"]
    assert cliente.get("/api/v1/sesion", headers=cabecera).status_code == 401


def test_sesion_se_invalida_si_el_profesor_deja_de_ser_activo(entorno):
    cliente, motor, h = entorno
    with Session(motor) as sesion:
        cuenta = sesion.scalar(
            select(CuentaAdministrativa).where(CuentaAdministrativa.usuario == "operador")
        )
        profesor = sesion.get(Persona, cuenta.persona_id)
        profesor.activo = False
        sesion.commit()
    assert cliente.get("/api/v1/sesion", headers=h["operador"]).status_code == 401
