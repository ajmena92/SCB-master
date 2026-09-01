"""Catalogo canonico de permisos administrativos PostgreSQL."""

from __future__ import annotations

PERMISOS_ADMINISTRATIVOS = (
    ("dashboard.leer", "Consultar inicio", "Indicadores generales del sistema", "Inicio"),
    ("comedor.operar", "Operar comedor", "Capturas y autorizaciones de comedor", "Comedor"),
    (
        "transporte.operar",
        "Operar transporte",
        "Consultar rutas y capturar marcas de transporte",
        "Rutas y transporte",
    ),
    (
        "rutas.administrar",
        "Administrar rutas",
        "Crear, editar y asignar rutas",
        "Rutas y transporte",
    ),
    (
        "personas.administrar",
        "Administrar personas",
        "Gestionar personas y matriculas",
        "Personas",
    ),
    ("menu.administrar", "Administrar menu", "Gestionar el menu del dia", "Menu"),
    ("tiquetes.operar", "Operar tiquetes", "Consultar y vender tiquetes", "Tiquetes"),
    (
        "tarifas.administrar",
        "Administrar tarifas",
        "Crear tarifas de tiquetes",
        "Tiquetes",
    ),
    ("reportes.leer", "Consultar reportes", "Consultar y exportar reportes", "Reportes"),
    (
        "importaciones.administrar",
        "Administrar anos e importacion",
        "Gestionar anos lectivos e importaciones",
        "Anos e importacion",
    ),
)

CLAVES_PERMISOS = frozenset(fila[0] for fila in PERMISOS_ADMINISTRATIVOS)
