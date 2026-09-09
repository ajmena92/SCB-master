#!/usr/bin/env python3
"""
Validador de configuración de entorno para SCB Portal Web.
Valida las variables de .env contra el esquema JSON definido en schema.json.
"""

import json
import sys
from pathlib import Path
from typing import Dict, List, Tuple

try:
    import jsonschema
except ImportError:
    print("ERROR: jsonschema no está instalado. Instálalo con: pip install jsonschema", file=sys.stderr)
    sys.exit(1)


def cargar_schema(schema_path: str) -> Dict:
    """Carga el esquema JSON de validación."""
    try:
        with open(schema_path, 'r', encoding='utf-8') as f:
            return json.load(f)
    except FileNotFoundError:
        print(f"ERROR: Esquema no encontrado: {schema_path}", file=sys.stderr)
        sys.exit(1)
    except json.JSONDecodeError as e:
        print(f"ERROR: Esquema JSON inválido: {e}", file=sys.stderr)
        sys.exit(1)


def cargar_env(env_path: str) -> Dict[str, object]:
    """Carga variables de un archivo .env."""
    env_vars = {}
    
    if not Path(env_path).exists():
        print(f"ADVERTENCIA: Archivo .env no encontrado: {env_path}", file=sys.stderr)
        return env_vars
    
    try:
        with open(env_path, 'r', encoding='utf-8') as f:
            for linea in f:
                linea = linea.strip()
                # Ignorar líneas vacías y comentarios
                if not linea or linea.startswith('#'):
                    continue
                
                if '=' in linea:
                    clave, valor = linea.split('=', 1)
                    valor_limpio = valor.strip()
                    # Convertir tipos si es necesario
                    if valor_limpio.lower() in ('true', 'false'):
                        env_vars[clave.strip()] = valor_limpio.lower() == 'true'
                    elif valor_limpio.lstrip('-').isdigit():
                        env_vars[clave.strip()] = int(valor_limpio)
                    else:
                        env_vars[clave.strip()] = valor_limpio
    except IOError as e:
        print(f"ERROR al leer .env: {e}", file=sys.stderr)
        sys.exit(1)
    
    return env_vars


def validar_config(config: Dict, schema: Dict) -> Tuple[bool, List[str]]:
    """Valida configuración contra el esquema."""
    errores = []
    
    try:
        jsonschema.validate(instance=config, schema=schema)
    except jsonschema.ValidationError as e:
        errores.append(f"Validación de esquema fallida: {e.message}")
        if e.path:
            errores.append(f"  Campo: {'.'.join(str(p) for p in e.path)}")
    except jsonschema.SchemaError as e:
        errores.append(f"Esquema inválido: {e.message}")
    
    # Validaciones adicionales personalizadas
    errores.extend(validar_cors(config))
    errores.extend(validar_app_env(config))
    
    return len(errores) == 0, errores


def validar_secretos(config: Dict, validar_existencia: bool = False) -> List[str]:
    """Valida estructura de secretos. Solo valida existencia si validar_existencia=True.
    
    En template (.env.example), NO validamos existencia (no deben estar presentes).
    En preflight (entorno real), SI validamos existencia.
    """
    errores = []
    campos_secretos = [
        'POSTGRES_ADMIN_PASSWORD_FILE',
        'POSTGRES_APP_PASSWORD_FILE',
        'POSTGRES_MIGRATOR_PASSWORD_FILE',
        'CSRF_SECRET_FILE',
        'CARNET_QR_CLAVE_FILE',
        'IMPORTACION_RESULTADOS_KEY_FILE',
    ]
    
    for campo in campos_secretos:
        if campo in config:
            ruta = config[campo]
            # Validar formato (debe terminar en _FILE)
            if not campo.endswith('_FILE'):
                errores.append(f"ERROR: Campo secreto debe terminar en _FILE: {campo}")
            
            # Solo validar existencia si se pide (preflight de entorno real)
            if validar_existencia and ruta and not ruta.startswith('/run/secrets/'):
                if not Path(ruta).exists():
                    errores.append(f"ERROR: Archivo secreto no encontrado: {ruta} ({campo})")
    
    return errores


def validar_cors(config: Dict) -> List[str]:
    """Valida CORS según el entorno."""
    errores = []
    cors_origin = config.get('CORS_ORIGIN', '')
    app_env = config.get('APP_ENV', 'production')
    cookie_secure = config.get('COOKIE_SECURE', True)
    if not isinstance(cookie_secure, bool):
        errores.append('ERROR: COOKIE_SECURE debe ser booleano.')
        return errores
    
    # En producción, CORS debe ser HTTPS
    if app_env == 'production' and cors_origin.startswith('http://'):
        errores.append(f"ERROR: CORS_ORIGIN debe ser HTTPS en producción: {cors_origin}")
    
    # COOKIE_SECURE debe ser true en producción con HTTPS
    if app_env == 'production' and cors_origin.startswith('https://') and not cookie_secure:
        errores.append("ERROR: COOKIE_SECURE debe ser true si CORS_ORIGIN es HTTPS")
    
    return errores


def validar_app_env(config: Dict) -> List[str]:
    """Valida consistencia del entorno."""
    errores = []
    app_env = config.get('APP_ENV', 'production')
    
    entornos_validos = {'development', 'production', 'test'}
    if app_env not in entornos_validos:
        errores.append(f"ERROR: APP_ENV inválido: {app_env}. Valores válidos: {entornos_validos}")
    
    return errores


def main():
    """Punto de entrada principal.
    
    Uso:
      python3 validar_variables.py --template local|production
      python3 validar_variables.py --preflight /ruta/a/.env
    
    --template: Validar un template local o de producción (sin exigir secretos)
    --preflight: Validar .env real antes de desplegar (exige secretos)
    """
    script_dir = Path(__file__).resolve().parent
    web_root = script_dir.parent
    ops_dir = web_root / 'ops'
    schema_path = ops_dir / 'schema.json'
    
    # Detectar modo: template vs preflight
    modo_template = '--template' in sys.argv
    modo_preflight = '--preflight' in sys.argv
    if modo_template == modo_preflight:
        print('ERROR: indique exactamente --template o --preflight.', file=sys.stderr)
        return 2
    
    # Elegir archivo a validar
    if modo_template:
        indice = sys.argv.index('--template')
        if len(sys.argv) <= indice + 1 or sys.argv[indice + 1] not in {'local', 'production'}:
            print('ERROR: --template requiere local o production.', file=sys.stderr)
            return 2
        nombre = sys.argv[indice + 1]
        env_files = [ops_dir / f'.env.{nombre}.example']
        validar_existencia_secretos = False
    else:
        indice = sys.argv.index('--preflight')
        if len(sys.argv) <= indice + 1:
            print('ERROR: --preflight requiere la ruta del archivo .env real.', file=sys.stderr)
            return 2
        env_files = [Path(sys.argv[indice + 1]).resolve()]
        validar_existencia_secretos = True
    
    print(f"🔍 Validando configuración SCB Portal Web")
    print(f"   Schema: {schema_path}")
    print(f"   Modo: {'TEMPLATE (sin secretos)' if not validar_existencia_secretos else 'PREFLIGHT (con secretos)'}")
    for env_path in env_files:
        print(f"   Config: {env_path}")
    print()
    
    # Cargar
    schema = cargar_schema(str(schema_path))
    valido = True
    errores = []
    for env_path in env_files:
        config = cargar_env(str(env_path))
        if not config:
            errores.append(f'ERROR: no se pudo cargar configuración desde {env_path}.')
            continue
        archivo_valido, archivo_errores = validar_config(config, schema)
        archivo_errores.extend(
            validar_secretos(config, validar_existencia=validar_existencia_secretos)
        )
        valido = valido and archivo_valido and not archivo_errores
        errores.extend(archivo_errores)
    
    # Reportar
    if errores:
        for error in errores:
            print(f"  ⚠️  {error}")
        print()
    
    if not errores:
        print("✅ Configuración válida")
        return 0
    else:
        print("❌ Configuración inválida. Corrige los errores arriba.")
        return 1


if __name__ == '__main__':
    sys.exit(main())
