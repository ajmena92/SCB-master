#!/usr/bin/env bash
# Script para generar SBOM (Software Bill of Materials) con Syft
# Genera reportes en SPDX y CycloneDX

set -eu

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
OUTPUT_DIR="${REPO_ROOT}/sbom"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)

echo "📋 Generando SBOM (Software Bill of Materials)..."
mkdir -p "${OUTPUT_DIR}"

# Verificar que Syft está instalado
if ! command -v syft &> /dev/null; then
    echo "⚠️  Syft no está instalado. Instalando..."
    curl -sSfL https://raw.githubusercontent.com/anchore/syft/main/install.sh | sh -s -- -b /usr/local/bin
fi

# 1. SBOM del código fuente (Python backend)
echo "📦 Generando SBOM para backend Python..."
syft "${REPO_ROOT}/web/backend" \
    -o spdx-json \
    > "${OUTPUT_DIR}/backend-sbom-${TIMESTAMP}.spdx.json"

syft "${REPO_ROOT}/web/backend" \
    -o cyclonedx-json \
    > "${OUTPUT_DIR}/backend-sbom-${TIMESTAMP}.cyclonedx.json"

# 2. SBOM del frontend (Node.js)
echo "📦 Generando SBOM para frontend Node.js..."
syft "${REPO_ROOT}/web/frontend" \
    -o spdx-json \
    > "${OUTPUT_DIR}/frontend-sbom-${TIMESTAMP}.spdx.json"

syft "${REPO_ROOT}/web/frontend" \
    -o cyclonedx-json \
    > "${OUTPUT_DIR}/frontend-sbom-${TIMESTAMP}.cyclonedx.json"

# 3. SBOM del proyecto completo
echo "📦 Generando SBOM para proyecto completo..."
syft "${REPO_ROOT}/web" \
    -o spdx-json \
    > "${OUTPUT_DIR}/project-sbom-${TIMESTAMP}.spdx.json"

syft "${REPO_ROOT}/web" \
    -o cyclonedx-json \
    > "${OUTPUT_DIR}/project-sbom-${TIMESTAMP}.cyclonedx.json"

# 4. Crear symlinks a las versiones más recientes
ln -sf "backend-sbom-${TIMESTAMP}.spdx.json" "${OUTPUT_DIR}/backend-sbom-latest.spdx.json"
ln -sf "backend-sbom-${TIMESTAMP}.cyclonedx.json" "${OUTPUT_DIR}/backend-sbom-latest.cyclonedx.json"
ln -sf "frontend-sbom-${TIMESTAMP}.spdx.json" "${OUTPUT_DIR}/frontend-sbom-latest.spdx.json"
ln -sf "frontend-sbom-${TIMESTAMP}.cyclonedx.json" "${OUTPUT_DIR}/frontend-sbom-latest.cyclonedx.json"
ln -sf "project-sbom-${TIMESTAMP}.spdx.json" "${OUTPUT_DIR}/project-sbom-latest.spdx.json"
ln -sf "project-sbom-${TIMESTAMP}.cyclonedx.json" "${OUTPUT_DIR}/project-sbom-latest.cyclonedx.json"

# 5. Generar reporte de componentes
echo "📊 Generando reporte de componentes..."
cat > "${OUTPUT_DIR}/sbom-report-${TIMESTAMP}.txt" << EOF
===========================================================
SBOM REPORT - $(date)
===========================================================

PROYECTO: SCB Portal Web
REPOSITORIO: $REPO_ROOT
TIMESTAMP: $TIMESTAMP

ARCHIVOS GENERADOS:
===========================================================

Backend Python:
  - backend-sbom-${TIMESTAMP}.spdx.json (SPDX format)
  - backend-sbom-${TIMESTAMP}.cyclonedx.json (CycloneDX format)

Frontend Node.js:
  - frontend-sbom-${TIMESTAMP}.spdx.json (SPDX format)
  - frontend-sbom-${TIMESTAMP}.cyclonedx.json (CycloneDX format)

Proyecto Completo:
  - project-sbom-${TIMESTAMP}.spdx.json (SPDX format)
  - project-sbom-${TIMESTAMP}.cyclonedx.json (CycloneDX format)

SYMLINKS (Últimas versiones):
  - backend-sbom-latest.spdx.json
  - backend-sbom-latest.cyclonedx.json
  - frontend-sbom-latest.spdx.json
  - frontend-sbom-latest.cyclonedx.json
  - project-sbom-latest.spdx.json
  - project-sbom-latest.cyclonedx.json

UBICACIÓN: $OUTPUT_DIR

===========================================================
FORMATOS:
  - SPDX: Software Package Data Exchange (ISO/IEC 5962)
  - CycloneDX: SBOM estándar de industria

USO:
  - Integrar en repositorio de componentes
  - Propagar a herramientas de gestión de riesgos
  - Auditoría de software
  - Compliance (SBOM es requisito en muchas legislaciones)

===========================================================
EOF

cat "${OUTPUT_DIR}/sbom-report-${TIMESTAMP}.txt"

echo ""
echo "✅ SBOMs generados exitosamente en: ${OUTPUT_DIR}"
echo ""
echo "📌 Próximos pasos:"
echo "   1. Revisar SBOMs generados"
echo "   2. Integrar en CI/CD (GitHub Actions)"
echo "   3. Subir a repositorio de componentes (Nexus, Artifactory, etc.)"
echo "   4. Monitorear vulnerabilidades con herramientas como Dependabot"
