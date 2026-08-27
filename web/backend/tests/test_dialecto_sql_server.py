from aplicacion.nucleo.dialecto_sql_server import DialectoSqlServerCompatible


def test_dialecto_adapta_sql_server_17_a_capacidades_16() -> None:
    dialecto = DialectoSqlServerCompatible()
    dialecto.server_version_info = (17, 0, 1125, 2)
    # Evita conexión: solo verificamos la normalización antes de la lógica base.
    dialecto._setup_version_attributes()
    assert dialecto.server_version_info == (16, 0, 1125, 2)
