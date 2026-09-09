"""Comprueba acceso a tablas de sesión y el flujo Origin/CSRF sin cuentas reales."""
import http.cookiejar
import json
import sys
import urllib.error
import urllib.request

origin = sys.argv[1]
jar = http.cookiejar.CookieJar()
client = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(jar))

def request(path, *, data=None, headers=None):
    req = urllib.request.Request(origin + path, data=data, headers=headers or {})
    try:
        with client.open(req, timeout=15) as response:
            return response.status, response.read()
    except urllib.error.HTTPError as error:
        return error.code, error.read()

status, _ = request('/api/v1/sesion', headers={'Cookie': 'scb_sesion=verificacion-local-inexistente'})
if status != 204:
    sys.exit(f'Fallo de sesión: HTTP {status}; revise permisos y esquema de PostgreSQL.')
status, _ = request('/api/v1/autenticacion/csrf')
if status != 204:
    sys.exit(f'Fallo al obtener CSRF: HTTP {status}')
token = next((cookie.value for cookie in jar if cookie.name == 'csrf_token'), '')
# JSON vacío: pasa por Origin/CSRF y termina en validación, sin intentos de login.
status, _ = request('/api/v1/autenticacion/administracion', data=json.dumps({}).encode(),
                    headers={'Origin': origin, 'X-CSRF-Token': token,
                             'Content-Type': 'application/json'})
if status != 422:
    sys.exit(f'Fallo de acceso Origin/CSRF: HTTP {status}')
print('Sesión PostgreSQL y protección Origin/CSRF verificadas.')
