#!/usr/bin/env python3
"""
Validador de configuración de entorno para SCB Portal Web.
Valida las variables de .env contra el esquema JSON definido en schema.json.
"""

import json
import os
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


def cargar_env(env_path: str) -> Dict[str, str]:
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
                    env_vars[clave.strip()] = valor.strip()
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
    errores.extend(validar_secretos(config))
    errores.extend(validar_cors(config))
    errores.extend(validar_app_env(config))
    
    return len(errores) == 0, errores


def validar_secretos(config: Dict) -> List[str]:
    """Valida que los archivos de secretos existan."""
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
            if ruta and not ruta.startswith('/run/secrets/'):  # Saltar validación en prod
                if not Path(ruta).exists():
                    errores.append(f"ADVERTENCIA: Archivo secreto no encontrado: {ruta} ({campo})")
    
    return errores


def validar_cors(config: Dict) -> List[str]:
    """Valida CORS según el entorno."""
    errores = []
    cors_origin = config.get('CORS_ORIGIN', '')
    app_env = config.get('APP_ENV', 'production')
    cookie_secure = config.get('COOKIE_SECURE', 'true').lower() in ('true', '1', 'yes')
    
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
    """Punto de entrada principal."""
    # Obtener rutas
    script_dir = Path(__file__).parent
    repo_root = script_dir.parent.parent
    ops_dir = repo_root / 'web' / 'ops'
    env_path = repo_root / '.env'
    schema_path = ops_dir / 'schema.json'
    
    print(f"🔍 Validando configuración SCB Portal Web")
    print(f"   Schema: {schema_path}")
    print(f"   Config: {env_path}")
    print()
    
    # Cargar
    schema = cargar_schema(str(schema_path))
    config = cargar_env(str(env_path))
    
    # Agregar variables del entorno del sistema
    config.update(os.environ)
    
    # Validar
    valido, errores = validar_config(config, schema)
    
    # Reportar
    if errores:
        for error in errores:
            print(f"  ⚠️  {error}")
        print()
    
    if valido:
        print("✅ Configuración válida")
        return 0
    else:
        print("❌ Configuración inválida. Corrige los errores arriba.")
        return 1


if __name__ == '__main__':
    sys.exit(main())
