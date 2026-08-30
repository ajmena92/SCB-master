"""Previsualizacion, lectura XLSX y confirmacion idempotente."""

import hashlib
import io
import json
import secrets
from datetime import date

from fastapi import HTTPException

from aplicacion.esquemas import FilaImportacion, ImportacionEntrada
from aplicacion.modelos.maestros import AnioLectivo, AsignacionRuta, Matricula, Persona, Ruta
from aplicacion.modelos.operacion import LoteImportacion
from aplicacion.seguridad import generar_codigo, hash_secreto


class ServicioImportacion:
    def __init__(self, repo):
        self.repo = repo

    def desde_excel(self, contenido: bytes, anio: int) -> ImportacionEntrada:
        try:
            from openpyxl import load_workbook

            hoja = load_workbook(io.BytesIO(contenido), read_only=True, data_only=True).active
            filas = list(hoja.iter_rows(values_only=True))
            encabezados = [str(v).strip().lower() if v else "" for v in filas[0]]
            requeridas = {"cedula", "nombres", "tipo"}
            if not requeridas <= set(encabezados):
                raise ValueError("faltan columnas cedula, nombres o tipo")
            datos = []
            for valores in filas[1:]:
                fila = dict(zip(encabezados, valores))
                if not any(v is not None for v in valores):
                    continue
                datos.append(
                    FilaImportacion(
                        cedula=str(fila.get("cedula") or "") or None,
                        nombres=str(fila.get("nombres") or ""),
                        tipo=str(fila.get("tipo") or "").lower(),
                        seccion=str(fila.get("seccion") or "") or None,
                        turno=str(fila.get("turno") or "") or None,
                        becado=str(fila.get("becado") or "").lower() in {"1", "si", "sí", "true"},
                        ruta=str(fila.get("ruta") or "") or None,
                    )
                )
            return ImportacionEntrada(anio=anio, filas=datos)
        except Exception as exc:
            raise HTTPException(422, f"Excel invalido: {exc}") from exc

    def _huella(self, datos):
        return hashlib.sha256(
            json.dumps(datos.model_dump(mode="json"), sort_keys=True).encode()
        ).hexdigest()

    def previsualizar(self, datos):
        errores = []
        cedulas = set()
        altas = cambios = 0
        for indice, fila in enumerate(datos.filas, 1):
            if not fila.cedula:
                errores.append({"fila": indice, "error": "cedula requerida"})
                continue
            if fila.cedula in cedulas:
                errores.append({"fila": indice, "error": "cedula duplicada"})
                continue
            cedulas.add(fila.cedula)
            existente = self.repo.persona_cedula(fila.cedula)
            altas += existente is None
            cambios += existente is not None
            if fila.tipo == "estudiante" and (not fila.seccion or not fila.turno):
                errores.append({"fila": indice, "error": "seccion y turno requeridos"})
        return {
            "huella": self._huella(datos),
            "total": len(datos.filas),
            "altas": altas,
            "cambios": cambios,
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
        for fila in datos.filas:
            persona = self.repo.persona_cedula(fila.cedula)
            if not persona:
                persona = Persona(
                    codigo=generar_codigo(self.repo.codigo_existe, fila.tipo),
                    cedula=fila.cedula,
                    nombres=fila.nombres,
                    tipo=fila.tipo,
                    activo=True,
                )
                pin_temporal = f"{secrets.randbelow(1_000_000):06d}"
                self.repo.guardar_persona_nueva(persona, hash_secreto(pin_temporal))
                credenciales.append(
                    {
                        "codigo": persona.codigo,
                        "nombre": persona.nombres,
                        "pinTemporal": pin_temporal,
                    }
                )
            else:
                persona.nombres, persona.tipo, persona.activo = fila.nombres, fila.tipo, True
            if fila.tipo != "estudiante":
                continue
            matricula = self.repo.matricula(persona.id, anio.id) or Matricula(
                persona_id=persona.id, anio_lectivo_id=anio.id
            )
            matricula.seccion, matricula.turno, matricula.becado, matricula.estado = (
                fila.seccion or "",
                fila.turno or "",
                fila.becado,
                "activo",
            )
            self.repo.guardar(matricula)
            if fila.ruta:
                ruta = self.repo.ruta_nombre(fila.ruta) or self.repo.guardar(
                    Ruta(nombre=fila.ruta, activo=True)
                )
                inicio = date(datos.anio, 1, 1)
                if not self.repo.asignacion(matricula.id, inicio):
                    self.repo.guardar(
                        AsignacionRuta(
                            matricula_id=matricula.id, ruta_id=ruta.id, fecha_inicio=inicio
                        )
                    )
        lote = self.repo.guardar(
            LoteImportacion(huella=huella, estado="confirmado", resumen=json.dumps(resumen))
        )
        return {
            "loteId": lote.id,
            "repetida": False,
            "credenciales": credenciales,
            **resumen,
        }
