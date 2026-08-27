"""Convenciones temporales compartidas con SQL Server."""

from datetime import UTC, datetime


def ahora_utc_sql() -> datetime:
    """Devuelve UTC sin zona para columnas SQL Server de tipo datetime2."""
    return datetime.now(UTC).replace(tzinfo=None)
