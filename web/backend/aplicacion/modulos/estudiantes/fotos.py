from collections.abc import Callable, Iterator
from typing import Protocol

from fastapi import APIRouter, Depends, File, HTTPException, Response, UploadFile

from aplicacion.nucleo.archivos import ArchivoExcedeLimite, leer_archivo_limitado


class RepositorioFotos(Protocol):
    def obtener_foto(self, id_estudiante: int) -> tuple[bytes, str] | None: ...
    def guardar_foto(self, id_estudiante: int, contenido: bytes, tipo: str) -> None: ...
    def eliminar_foto(self, id_estudiante: int) -> None: ...


MAXIMO_FOTO_BYTES = 5_000_000


def crear_enrutador_fotos(
    obtener_repositorio: Callable[[], Iterator[RepositorioFotos]],
    exigir_permiso: Callable[[str], Callable[..., object]],
    exigir_csrf: Callable[..., object],
) -> APIRouter:
    r = APIRouter(prefix="/estudiantes", tags=["fotografias"])

    @r.get("/{id_estudiante}/foto")
    def consultar(
        id_estudiante: int,
        _=Depends(exigir_permiso("estudiantes.leer")),
        repo=Depends(obtener_repositorio),
    ):
        foto = repo.obtener_foto(id_estudiante)
        if foto is None:
            return Response(status_code=404)
        return Response(content=foto[0], media_type=foto[1])

    @r.delete("/{id_estudiante}/foto", status_code=204)
    def eliminar(
        id_estudiante: int,
        _=Depends(exigir_permiso("estudiantes.editar")),
        __=Depends(exigir_csrf),
        repo=Depends(obtener_repositorio),
    ):
        repo.eliminar_foto(id_estudiante)

    @r.post("/{id_estudiante}/foto", status_code=204)
    async def cargar(
        id_estudiante: int,
        archivo: UploadFile = File(...),
        _=Depends(exigir_permiso("estudiantes.editar")),
        __=Depends(exigir_csrf),
        repo=Depends(obtener_repositorio),
    ):
        if archivo.content_type not in {"image/jpeg", "image/png"}:
            return Response(status_code=415)
        try:
            contenido = await leer_archivo_limitado(archivo, MAXIMO_FOTO_BYTES)
        except ArchivoExcedeLimite as exc:
            raise HTTPException(413, "La fotografía supera el tamaño máximo permitido") from exc
        if not contenido:
            return Response(status_code=413)
        repo.guardar_foto(id_estudiante, contenido, archivo.content_type)

    @r.get("/{id_estudiante}/carnet.pdf")
    def carnet(
        id_estudiante: int,
        _=Depends(exigir_permiso("estudiantes.leer")),
        repo=Depends(obtener_repositorio),
    ):
        # PDF mínimo, autónomo y estable; el diseño puede evolucionar sin Crystal Reports.
        cuerpo = f"BT /F1 18 Tf 72 720 Td (Carnet estudiante {id_estudiante}) Tj ET"
        pdf = (
            "%PDF-1.4\n1 0 obj<</Type/Catalog/Pages 2 0 R>>endobj\n"
            "2 0 obj<</Type/Pages/Count 1/Kids[3 0 R]>>endobj\n"
            "3 0 obj<</Type/Page/Parent 2 0 R/MediaBox[0 0 612 792]/Resources<</Font<</F1 4 0 R>>>>/Contents 5 0 R>>endobj\n"
            "4 0 obj<</Type/Font/Subtype/Type1/BaseFont/Helvetica>>endobj\n"
            f"5 0 obj<</Length {len(cuerpo)}>>stream\n{cuerpo}\nendstream endobj\n"
            "xref\n0 6\n0000000000 65535 f \ntrailer<</Size 6/Root 1 0 R>>\nstartxref\n0\n%%EOF\n"
        ).encode()
        return Response(
            pdf,
            media_type="application/pdf",
            headers={"Content-Disposition": f"inline; filename=carnet-{id_estudiante}.pdf"},
        )

    return r
