def test_login_administrativo_se_bloquea_de_forma_persistente(entorno):
    cliente, _, _ = entorno
    credenciales = {"usuario": "admin", "contrasena": "incorrecta"}

    for _ in range(5):
        respuesta = cliente.post("/api/v1/autenticacion/administracion", json=credenciales)
        assert respuesta.status_code == 401

    respuesta = cliente.post("/api/v1/autenticacion/administracion", json=credenciales)
    assert respuesta.status_code == 429


def test_login_exitoso_elimina_los_fallos_previos(entorno):
    cliente, _, _ = entorno
    for _ in range(2):
        respuesta = cliente.post(
            "/api/v1/autenticacion/administracion",
            json={"usuario": "operador", "contrasena": "incorrecta"},
        )
        assert respuesta.status_code == 401

    respuesta = cliente.post(
        "/api/v1/autenticacion/administracion",
        json={"usuario": "operador", "contrasena": "Clave-operador-2026"},
    )
    assert respuesta.status_code == 200
