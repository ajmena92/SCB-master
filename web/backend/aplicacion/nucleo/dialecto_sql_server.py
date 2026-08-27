"""Compatibilidad explícita con SQL Server 2025 (versión mayor 17)."""

from sqlalchemy.dialects.mssql.pyodbc import MSDialect_pyodbc


class DialectoSqlServerCompatible(MSDialect_pyodbc):
    """Trata SQL Server 17 como SQL Server 16 para capacidades conocidas.

    SQLAlchemy 2.0.43 reconoce hasta la versión 16; la versión 17 conserva
    las capacidades usadas por estas migraciones. El ajuste es deliberado y
    acotado, no silencia advertencias globalmente.
    """

    def _setup_version_attributes(self) -> None:
        super()._setup_version_attributes()
        if self.server_version_info and self.server_version_info[0] == 17:
            self.server_version_info = (16, *self.server_version_info[1:])
