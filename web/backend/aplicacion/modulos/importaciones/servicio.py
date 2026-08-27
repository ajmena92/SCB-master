from __future__ import annotations

import csv
import io

from .esquemas import ErrorFila, LoteSalida, Previsualizacion
from .repositorio import RepositorioImportaciones


class ServicioImportaciones:
    def __init__(self, repositorio: RepositorioImportaciones) -> None:
        self._repositorio = repositorio

    def previsualizar(self, contenido: bytes, limite: int | None = 100) -> Previsualizacion:
        try:
            texto = contenido.decode("utf-8-sig")
        except UnicodeDecodeError:
            return Previsualizacion(
                cabeceras=[],
                filas=[],
                errores=[ErrorFila(fila=0, mensaje="El archivo debe estar codificado en UTF-8")],
                total_filas=0,
                valida=False,
            )
        lector = csv.DictReader(io.StringIO(texto, newline=""))
        cab = list(lector.fieldnames or [])
        errores: list[ErrorFila] = []
        filas: list[dict[str, str]] = []
        if not cab or any(not h or not h.strip() for h in cab):
            errores.append(ErrorFila(fila=1, mensaje="El CSV debe contener cabeceras no vacías"))
        if len(set(cab)) != len(cab):
            errores.append(ErrorFila(fila=1, mensaje="Las cabeceras no pueden repetirse"))
        for numero, fila in enumerate(lector, 2):
            if None in fila:
                errores.append(
                    ErrorFila(fila=numero, mensaje="La fila tiene más columnas que las cabeceras")
                )
                continue
            if not any((v or "").strip() for v in fila.values()):
                errores.append(ErrorFila(fila=numero, mensaje="La fila está vacía"))
                continue
            filas.append({k: (v or "").strip() for k, v in fila.items()})
        return Previsualizacion(
            cabeceras=cab,
            filas=filas if limite is None else filas[:limite],
            errores=errores,
            total_filas=len(filas),
            valida=not errores and bool(filas),
        )

    def ejecutar(self, nombre: str, contenido: bytes, usuario: int) -> LoteSalida:
        # La vista HTTP se limita a 100 filas; la ejecución conserva todas las
        # filas válidas del archivo para que el lote sea reversible completo.
        vista = self.previsualizar(contenido, limite=None)
        if not vista.valida:
            raise ValueError("El archivo no supera la validación")
        return LoteSalida(
            **self._repositorio.crear_lote(
                nombre, vista.filas, [e.model_dump() for e in vista.errores], usuario
            )
        )

    def obtener(self, id_lote: int) -> LoteSalida:
        return LoteSalida(**self._repositorio.lote(id_lote))

    def revertir(self, id_lote: int, usuario: int) -> LoteSalida:
        return LoteSalida(**self._repositorio.revertir(id_lote, usuario))
