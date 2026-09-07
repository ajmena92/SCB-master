"""Worker durable y de concurrencia única para confirmar importaciones."""

from __future__ import annotations

import json
from datetime import datetime, timezone

from cryptography.fernet import Fernet
from sqlalchemy.orm import Session, sessionmaker

from aplicacion.casos_importacion import ServicioImportacion
from aplicacion.esquemas import ImportacionEntrada
from aplicacion.repositorios_importacion import RepositorioImportacion


def procesar_un_trabajo(fabrica: sessionmaker[Session], clave_resultados: str) -> int | None:
    """Procesa un trabajo reclamado por fila bloqueada; retorna su id o ``None``.

    La transacción de aplicación incluye el lote y el resultado cifrado, por lo
    que una caída no puede confirmar altas sin conservar su entrega única.
    """
    with fabrica() as sesion:
        repo = RepositorioImportacion(sesion)
        trabajo = repo.tomar_trabajo_pendiente()
        if trabajo is None:
            return None
        trabajo_id = trabajo.id
        sesion.commit()

    try:
        with fabrica() as sesion:
            repo = RepositorioImportacion(sesion)
            # Conserva el bloqueo hasta el commit: la recuperación SKIP LOCKED
            # no puede reclamar un trabajo vivo aunque supere los 30 minutos.
            trabajo = repo.trabajo_para_entrega(trabajo_id)
            assert trabajo is not None and trabajo.estado == "ejecutando"
            entrada = ImportacionEntrada.model_validate_json(trabajo.entrada_json)
            resultado = ServicioImportacion(repo).confirmar(entrada, trabajo.huella)
            # El resultado contiene PIN temporales: solo se persiste cifrado.
            trabajo.resultado_cifrado = Fernet(clave_resultados.encode()).encrypt(
                json.dumps(resultado).encode()
            ).decode()
            trabajo.estado = "completado"
            trabajo.finalizado_en = datetime.now(timezone.utc)
            sesion.commit()
            return trabajo_id
    except Exception as error:
        with fabrica() as sesion:
            trabajo = RepositorioImportacion(sesion).trabajo(trabajo_id)
            if trabajo is not None:
                trabajo.estado = "fallido"
                trabajo.error = "La importación no pudo completarse"
                trabajo.finalizado_en = datetime.now(timezone.utc)
                sesion.commit()
        raise error
