#!/usr/bin/env bash
# Script para generar cliente OpenAPI desde la API FastAPI
# Se ejecuta en CI/CD y ante cambios en la API

set -eu

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
BACKEND_DIR="${REPO_ROOT}/web/backend"
FRONTEND_DIR="${REPO_ROOT}/web/frontend"
OPENAPI_OUTPUT="${FRONTEND_DIR}/src/api/client"

echo "📦 Generando cliente OpenAPI..."

# Verificar que la API está ejecutándose
if ! python -c "import fastapi" 2>/dev/null; then
    echo "ERROR: FastAPI no está instalado. Instálalo con: pip install -r requirements-desarrollo.txt"
    exit 1
fi

cd "${BACKEND_DIR}"

# Generar esquema OpenAPI
python3 << 'PYTHON_SCRIPT'
import json
import sys
from pathlib import Path

# Importar aplicación FastAPI
sys.path.insert(0, str(Path.cwd()))
from aplicacion.main import app

# Generar schema
schema = app.openapi()

# Guardar
output_path = Path(__file__).parent.parent / 'frontend' / 'src' / 'api' / 'openapi-schema.json'
output_path.parent.mkdir(parents=True, exist_ok=True)

with open(output_path, 'w', encoding='utf-8') as f:
    json.dump(schema, f, indent=2, ensure_ascii=False)

print(f"✅ Schema generado: {output_path}")
PYTHON_SCRIPT

# Generar cliente si está disponible openapi-generator
if command -v openapi-generator-cli &> /dev/null; then
    echo "📝 Generando cliente TypeScript..."
    
    openapi-generator-cli generate \
        -i "${FRONTEND_DIR}/src/api/openapi-schema.json" \
        -g typescript-fetch \
        -o "${OPENAPI_OUTPUT}" \
        --skip-validate-spec \
        -c "${REPO_ROOT}/web/ops/openapi-generator-config.json" 2>/dev/null || true
    
    echo "✅ Cliente OpenAPI generado en ${OPENAPI_OUTPUT}"
else
    echo "⚠️  openapi-generator-cli no disponible. Schema generado pero cliente no."
fi

echo "✅ Generación OpenAPI completa"
