"""Previsualizacion, lectura XLSX y confirmacion idempotente."""

import hashlib
import io
import json
import secrets
from typing import Literal, cast

from cryptography.fernet import Fernet, InvalidToken
from fastapi import HTTPException

from aplicacion.esquemas import FilaImportacion, ImportacionEntrada
from aplicacion.modelos.maestros import AnioLectivo, Matricula, Persona
from aplicacion.modelos.operacion import LoteImportacion
from aplicacion.seguridad import hash_secreto
from aplicacion.limites_importacion import MAXIMO_COLUMNAS, MAXIMO_FILAS, validar_excel


class ServicioImportacion:
    def __init__(self, repo):
        self.repo = repo

    def desde_excel(self, contenido: bytes, anio: int) -> ImportacionEntrada:
        validar_excel(contenido)
        try:
            from openpyxl import load_workbook

            libro = load_workbook(io.BytesIO(contenido), read_only=True, data_only=True)
            hoja = libro.active
            if hoja.max_column and hoja.max_column > MAXIMO_COLUMNAS:
                raise HTTPException(413, "El Excel supera el límite de 32 columnas")
            filas = hoja.iter_rows(values_only=True)
            encabezados = [str(v).strip().lower() if v else "" for v in next(filas, ())]
            requeridas = {"cedula", "nombres", "tipo"}
            if not requeridas <= set(encabezados):
                raise ValueError("faltan columnas cedula, nombres o tipo")
            datos = []
            for numero, valores in enumerate(filas, 1):
                if numero > MAXIMO_FILAS:
                    raise HTTPException(413, "El Excel supera el límite de 5.000 filas")
                fila = dict(zip(encabezados, valores))
                if not any(v is not None for v in valores):
                    continue
                tipo = str(fila.get("tipo") or "").lower()
                datos.append(
                    FilaImportacion(
                        cedula=str(fila.get("cedula") or "") or None,
                        nombres=str(fila.get("nombres") or ""),
                        tipo=cast(Literal["estudiante", "profesor"], tipo),
                        seccion=str(fila.get("seccion") or "") or None,
                    )
                )
            return ImportacionEntrada(anio=anio, filas=datos)
        except HTTPException:
            raise
        except Exception as exc:
            raise HTTPException(422, f"Excel invalido: {exc}") from exc
        finally:
            if "libro" in locals():
                libro.close()

    def _huella(self, datos):
        return hashlib.sha256(
            json.dumps(datos.model_dump(mode="json"), sort_keys=True).encode()
        ).hexdigest()

    def previsualizar(self, datos):
        errores = []
        cedulas = set()
        altas = cambios = 0
        tipos = set()
        existentes = self.repo.personas_por_cedulas({f.cedula for f in datos.filas if f.cedula})
        for indice, fila in enumerate(datos.filas, 1):
            if not fila.cedula:
                errores.append({"fila": indice, "error": "cedula requerida"})
                continue
            if fila.cedula in cedulas:
                errores.append({"fila": indice, "error": "cedula duplicada"})
                continue
            cedulas.add(fila.cedula)
            tipos.add(fila.tipo)
            existente = existentes.get(fila.cedula)
            altas += existente is None
            cambios += existente is not None
            if existente and existente.tipo != fila.tipo:
                errores.append(
                    {"fila": indice, "error": "el tipo no coincide con la persona existente"}
                )
            if fila.tipo == "estudiante" and not fila.seccion:
                errores.append({"fila": indice, "error": "seccion requerida"})
        desactivaciones = self.repo.contar_activas_ausentes(tipos, cedulas)
        return {
            "huella": self._huella(datos),
            "total": len(datos.filas),
            "altas": altas,
            "cambios": cambios,
            "desactivaciones": desactivaciones,
            "errores": errores,
            "aplicable": not errores,
        }

    def confirmar(self, datos, huella):
        resumen = self.previsualizar(datos)
        if resumen["huella"] != huella:
            raise HTTPException(409, "El contenido cambio")
        if not resumen["aplicable"]:
            raise HTTPException(422, detail=resumen["errores"])
        lote = self.repo.lote(huella)
        if lote:
            return {"loteId": lote.id, "repetida": True, "credenciales": [], **resumen}
        anio = self.repo.anio(datos.anio) or self.repo.guardar(
            AnioLectivo(anio=datos.anio, vigente=False)
        )
        credenciales = []
        tipos = {fila.tipo for fila in datos.filas}
        cedulas = {fila.cedula for fila in datos.filas if fila.cedula}
        ausentes = self.repo.activas_ausentes_del_padron(tipos, cedulas)
        existentes = self.repo.personas_por_cedulas(cedulas)
        matriculas = self.repo.matriculas_por_personas(
            [p.id for p in existentes.values() if p.tipo == "estudiante"], anio.id
        )
        matriculas_actualizadas = []
        for fila in datos.filas:
            persona = existentes.get(fila.cedula)
            if not persona:
                persona = Persona(
                    cedula=fila.cedula.strip(),
                    nombres=fila.nombres,
                    tipo=fila.tipo,
                    activo=True,
                )
                pin_temporal = f"{secrets.randbelow(1_000_000):06d}"
                self.repo.guardar_persona_nueva(persona, hash_secreto(pin_temporal))
                credenciales.append(
                    {
                        "cedula": persona.cedula,
                        "nombre": persona.nombres,
                        "pinTemporal": pin_temporal,
                    }
                )
            else:
                persona.nombres, persona.activo = fila.nombres, True
            if fila.tipo != "estudiante":
                continue
            matricula = matriculas.get(persona.id) or Matricula(
                persona_id=persona.id, anio_lectivo_id=anio.id
            )
            matricula.seccion, matricula.turno, matricula.estado = (
                fila.seccion or "",
                "diurno",
                "activo",
            )
            matriculas_actualizadas.append(matricula)
        if matriculas_actualizadas:
            self.repo.guardar(*matriculas_actualizadas)
        self.repo.desactivar_personas(ausentes)
        lote = self.repo.guardar(
            LoteImportacion(huella=huella, estado="confirmado", resumen=json.dumps(resumen))
        )
        return {
            "loteId": lote.id,
            "repetida": False,
            "credenciales": credenciales,
            **resumen,
        }

    def encolar(self, datos: ImportacionEntrada, huella: str, cuenta_solicitante_id: int):
        """Persiste una confirmación idempotente sin ejecutar hashes en HTTP."""
        resumen = self.previsualizar(datos)
        if resumen["huella"] != huella:
            raise HTTPException(409, "El contenido cambio")
        if not resumen["aplicable"]:
            raise HTTPException(422, detail=resumen["errores"])
        trabajo = self.repo.trabajo_por_huella(huella)
        if trabajo is not None and trabajo.cuenta_solicitante_id != cuenta_solicitante_id:
            raise HTTPException(403, "No puede reutilizar un trabajo de otra cuenta")
        if trabajo is None:
            from aplicacion.modelos.operacion import TrabajoImportacion

            trabajo = self.repo.guardar(
                TrabajoImportacion(
                    huella=huella,
                    cuenta_solicitante_id=cuenta_solicitante_id,
                    entrada_json=datos.model_dump_json(),
                    resumen_json=json.dumps(resumen),
                    estado="pendiente",
                )
            )
        return self.resumen_trabajo(trabajo)

    @staticmethod
    def resumen_trabajo(trabajo):
        resumen = json.loads(trabajo.resumen_json)
        return {
            "trabajoId": trabajo.id,
            "estado": trabajo.estado,
            "total": resumen["total"],
            "altas": resumen["altas"],
            "cambios": resumen["cambios"],
        }

    def entregar_resultado(self, trabajo_id: int, cuenta_solicitante_id: int, clave: str):
        trabajo = self.repo.trabajo_para_entrega(trabajo_id)
        if trabajo is None:
            raise HTTPException(404, "Trabajo de importación no encontrado")
        if trabajo.cuenta_solicitante_id != cuenta_solicitante_id:
            raise HTTPException(403, "No puede descargar este resultado")
        if trabajo.resultado_entregado:
            raise HTTPException(410, "Las credenciales ya fueron entregadas")
        if trabajo.estado != "completado" or not trabajo.resultado_cifrado:
            raise HTTPException(409, "El resultado aún no está disponible")
        try:
            resultado = json.loads(Fernet(clave.encode()).decrypt(trabajo.resultado_cifrado.encode()))
        except (InvalidToken, ValueError) as error:
            raise HTTPException(500, "No se pudo recuperar el resultado") from error
        trabajo.resultado_entregado, trabajo.resultado_cifrado = True, None
        return resultado
