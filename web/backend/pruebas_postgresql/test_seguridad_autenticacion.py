def test_login_administrativo_se_bloquea_de_forma_persistente(entorno):
    cliente, _, _ = entorno
    credenciales = {"usuario": "admin", "contrasena": "incorrecta"}

    for _ in range(5):
        respuesta = cliente.post(
            "/api/v1/autenticacion/administracion", json=credenciales, headers=cliente.csrf()
        )
        assert respuesta.status_code == 401

    respuesta = cliente.post(
        "/api/v1/autenticacion/administracion", json=credenciales, headers=cliente.csrf()
    )
    assert respuesta.status_code == 429


def test_login_exitoso_elimina_los_fallos_previos(entorno):
    cliente, _, _ = entorno
    for _ in range(2):
        respuesta = cliente.post(
            "/api/v1/autenticacion/administracion",
            json={"usuario": "operador", "contrasena": "incorrecta"},
            headers=cliente.csrf(),
        )
        assert respuesta.status_code == 401

    respuesta = cliente.post(
        "/api/v1/autenticacion/administracion",
        json={"usuario": "operador", "contrasena": "Clave-operador-2026"},
        headers=cliente.csrf(),
    )
    assert respuesta.status_code == 200


def test_login_emite_cookie_segura_y_no_expone_portador(entorno):
    cliente, _, _ = entorno
    respuesta = cliente.post(
        "/api/v1/autenticacion/administracion",
        json={"usuario": "admin", "contrasena": "Clave-segura-2026"},
        headers=cliente.csrf(),
    )
    assert respuesta.status_code == 200
    assert "token" not in respuesta.json()
    cookie = respuesta.headers["set-cookie"]
    assert "scb_sesion=" in cookie
    assert "HttpOnly" in cookie
    assert "SameSite=lax" in cookie
    assert "Path=/api/v1" in cookie
    assert "Max-Age=" in cookie
    assert "expires=" in cookie.lower()
    assert "Domain=" not in cookie


def test_csrf_es_legible_desde_la_spa_y_sesion_se_restringe_a_la_api(entorno):
    cliente, _, _ = entorno
    respuesta = cliente.get("/api/v1/autenticacion/csrf")

    cookies = respuesta.headers.get_list("set-cookie")
    assert any("csrf_token=" in cookie and "Path=/" in cookie for cookie in cookies)
    assert all("Path=/api/v1" not in cookie for cookie in cookies if "csrf_token=" in cookie)


def test_logout_y_renovacion_exigen_csrf_y_limpian_o_rotan_cookies(entorno):
    _, _, h = entorno
    cliente = h["admin_cliente"]
    antes = cliente.cookies["scb_sesion"]
    assert cliente.post("/api/v1/autenticacion/renovar", headers=cliente.csrf()).status_code == 204
    assert cliente.cookies["scb_sesion"] != antes

    respuesta = cliente.post("/api/v1/autenticacion/logout", headers=cliente.csrf())
    assert respuesta.status_code == 204
    assert any(
        "csrf_token=" in cookie and "Path=/" in cookie
        for cookie in respuesta.headers.get_list("set-cookie")
    )


def test_mutacion_rechaza_origin_o_csrf_y_sesion_bearer(entorno):
    cliente, _, _ = entorno
    assert (
        cliente.post(
            "/api/v1/autenticacion/logout", headers={"Origin": "http://malicioso"}
        ).status_code
        == 403
    )
    assert (
        cliente.post(
            "/api/v1/autenticacion/logout", headers={"Origin": "http://localhost:5173"}
        ).status_code
        == 403
    )
    assert (
        cliente.get("/api/v1/sesion", headers={"Authorization": "Bearer no-valido"}).status_code
        == 401
    )


def test_preflight_cors_permite_solo_csrf_y_patch_del_origen_configurado(entorno):
    cliente, _, _ = entorno
    respuesta = cliente.request(
        "OPTIONS",
        "/api/v1/autenticacion/logout",
        headers={
            "Origin": "http://localhost:5173",
            "Access-Control-Request-Method": "PATCH",
            "Access-Control-Request-Headers": "X-CSRF-Token",
        },
    )

    assert respuesta.status_code == 200
    assert "PATCH" in respuesta.headers["access-control-allow-methods"]
    assert "x-csrf-token" in respuesta.headers["access-control-allow-headers"].lower()
    rechazado = cliente.request(
        "OPTIONS",
        "/api/v1/autenticacion/logout",
        headers={
            "Origin": "https://malicioso.example",
            "Access-Control-Request-Method": "PATCH",
        },
    )
    assert rechazado.status_code == 400
