"""Preparación de datos sintéticos para el medidor aislado de Fase 4."""

import io
from datetime import date
from zipfile import ZIP_STORED, ZipFile

from openpyxl import Workbook
from sqlalchemy import select
from sqlalchemy.orm import Session

from aplicacion.modelos.maestros import AnioLectivo, Matricula, Persona, Ruta
from aplicacion.modelos.operacion import (
    CuentaTiquete,
    IngresoComedor,
    MarcaTransporte,
    ReservaComedor,
    Tarifa,
    VentaTiquete,
)


def preparar_operacion(motor):
    with Session(motor) as sesion:
        anio = sesion.scalar(select(AnioLectivo).where(AnioLectivo.anio == 2026))
        anio.vigente = True
        ruta = Ruta(nombre="Ruta sintética", codigo="SINT", descripcion="Solo medición")
        tarifa = Tarifa(tipo_persona="estudiante", monto=700, fecha_inicio=date(2026, 1, 1))
        sesion.add_all([ruta, tarifa])
        sesion.flush()
        personas = sesion.scalars(select(Persona).where(Persona.tipo == "estudiante")).all()
        matriculas = dict(sesion.execute(select(Matricula.persona_id, Matricula.id)).all())
        for persona in personas:
            sesion.add_all(
                [
                    CuentaTiquete(persona_id=persona.id, saldo=2, reservados=0),
                    ReservaComedor(persona_id=persona.id, fecha=date(2026, 9, 5)),
                    IngresoComedor(
                        persona_id=persona.id,
                        fecha=date(2026, 9, 4),
                        modalidad="reserva",
                        consumio_tiquete=False,
                        operador_id=1,
                    ),
                    MarcaTransporte(
                        matricula_id=matriculas[persona.id],
                        ruta_id=ruta.id,
                        fecha=date(2026, 9, 4),
                        operador_id=1,
                    ),
                    VentaTiquete(
                        persona_id=persona.id,
                        tarifa_id=tarifa.id,
                        cantidad=2,
                        tarifa_aplicada=700,
                        total=1400,
                        medio_pago="efectivo",
                        operador_id=1,
                    ),
                ]
            )
        sesion.commit()


def excel_casi_limite(datos):
    """XLSX válido sin compresión, cerca de 12 MiB, con columna extra ignorada.

    Estresa tamaño de archivo y parsing; no representa el máximo de filas ni
    una bomba ZIP. Se mide por separado del padrón normal de cuatro columnas.
    """
    libro = Workbook(write_only=True)
    hoja = libro.create_sheet()
    hoja.append(["cedula", "nombres", "tipo", "seccion", "observacion"])
    longitud = min(32000, (12 * 1024 * 1024 - 1024 * 1024) // len(datos.filas))
    for fila in datos.filas:
        hoja.append([fila.cedula, fila.nombres, fila.tipo, fila.seccion, "x" * longitud])
    salida = io.BytesIO()
    libro.save(salida)
    destino = io.BytesIO()
    with ZipFile(salida) as origen, ZipFile(destino, "w", compression=ZIP_STORED) as archivo:
        for nombre in origen.namelist():
            archivo.writestr(nombre, origen.read(nombre))
    contenido = destino.getvalue()
    # Completar exactamente 12 MiB con una entrada ZIP auxiliar válida. No
    # aumenta las filas del padrón y permite probar el límite multipart real.
    nombre_relleno = "medicion.txt"
    sobrecarga = 30 + 46 + 2 * len(nombre_relleno)
    faltante = 12 * 1024 * 1024 - len(contenido) - sobrecarga
    if faltante < 0:
        raise ValueError("El volumen elegido excede el escenario de 12 MiB")
    with ZipFile(destino, "a", compression=ZIP_STORED) as archivo:
        archivo.writestr(nombre_relleno, b"x" * faltante)
    assert len(destino.getvalue()) == 12 * 1024 * 1024
    return destino.getvalue()
