"""Operaciones administrativas canónicas de estudiantes."""
import secrets
from collections.abc import Callable, Iterator
from datetime import date

from fastapi import APIRouter, Cookie, Depends, Header, HTTPException, Response, status
from pydantic import BaseModel, Field

from aplicacion.nucleo.identidad.seguridad import hash_contrasena, verificar_contrasena
from aplicacion.nucleo.identidad.servicio import AutenticacionFallida, ServicioIdentidad


class CambioAsignacion(BaseModel):
    id_beneficio: int | None = Field(default=None, alias="idBeneficio", ge=1)
    id_ruta: int | None = Field(default=None, alias="idRuta", ge=1)


class GeneracionPinesSeccion(BaseModel):
    seccion: str | None = Field(default=None, max_length=30)
    turno: str | None = Field(default=None, max_length=30)

class PinGenerado(BaseModel):
    id_estudiante: int = Field(alias="idEstudiante")
    pin: str

class AccesoEstudiante(BaseModel):
    carne: str = Field(min_length=1, max_length=30)
    pin: str = Field(pattern=r"^\d{6}$")

class CambioPinEstudiante(BaseModel):
    pin_actual: str = Field(alias="pinActual", pattern=r"^\d{6}$")
    pin_nuevo: str = Field(alias="pinNuevo", pattern=r"^\d{6}$")


def crear_enrutador_operaciones(obtener_repositorio: Callable[[], Iterator], exigir_permiso: Callable[..., Callable], exigir_csrf: Callable, obtener_identidad: Callable[[], ServicioIdentidad] | None = None, obtener_identidad_estudiante: Callable[[], ServicioIdentidad] | None = None, obtener_menu: Callable[[], Iterator] | None = None, obtener_asistencia: Callable[[], Iterator] | None = None, cookies_seguras: bool = True, duracion_sesion_estudiante: int = 31536000, **_kwargs) -> APIRouter:
    r = APIRouter(prefix="/estudiantes", tags=["estudiantes-administracion"])
    identidad_portal = obtener_identidad_estudiante or obtener_identidad

    def estudiante_actual(repo, identidad, id_sesion, secreto):
        if identidad is None or not id_sesion or not secreto:
            raise HTTPException(401, "La sesión no es válida")
        try:
            sesion = identidad.validar_sesion(id_sesion, secreto)
        except AutenticacionFallida as exc:
            raise HTTPException(401, str(exc)) from exc
        credencial = repo.buscar_credencial_por_id(sesion.id_usuario)
        if not credencial or not credencial.get("activo"):
            raise HTTPException(401, "El estudiante no está disponible")
        return {
            "idEstudiante": credencial["id_estudiante"],
            "carne": credencial["carne"],
            "nombre": credencial["nombre"],
            "primerApellido": credencial.get("primer_apellido"),
            "segundoApellido": credencial.get("segundo_apellido"),
            "cedula": credencial.get("cedula"),
            "seccion": credencial.get("seccion"),
            "turno": credencial.get("turno"),
            "idRuta": credencial.get("id_ruta"),
            "rutaCodigo": credencial.get("ruta_codigo"),
            "rutaDescripcion": credencial.get("ruta_descripcion"),
            "rutaColor": credencial.get("ruta_color"),
            "idBeneficio": credencial.get("id_beneficio"),
            "tipoBeca": credencial.get("tipo_beca"),
            "debeCambiarPin": bool(credencial.get("debe_cambiar_pin")),
            "tieneFoto": bool(credencial.get("tiene_foto")),
            "barcode": credencial["carne"],
        }

    @r.get("/menu")
    def menu(repo=Depends(obtener_repositorio), identidad: ServicioIdentidad | None = Depends(identidad_portal), id_sesion: str | None = Cookie(default=None), secreto: str | None = Cookie(default=None), menu_repo=Depends(obtener_menu) if obtener_menu else None):
        estudiante_actual(repo, identidad, id_sesion, secreto)
        if menu_repo is None:
            raise HTTPException(503, "El menú no está configurado")
        return {"menu": menu_repo.listar()}

    @r.get("/carnet")
    def carnet(repo=Depends(obtener_repositorio), identidad: ServicioIdentidad | None = Depends(identidad_portal), id_sesion: str | None = Cookie(default=None), secreto: str | None = Cookie(default=None)):
        return estudiante_actual(repo, identidad, id_sesion, secreto)

    @r.get("/carnet/foto")
    def foto_carnet(repo=Depends(obtener_repositorio), identidad: ServicioIdentidad | None = Depends(identidad_portal), id_sesion: str | None = Cookie(default=None), secreto: str | None = Cookie(default=None)):
        estudiante = estudiante_actual(repo, identidad, id_sesion, secreto)
        foto = repo.obtener_foto(int(estudiante["idEstudiante"]))
        if foto is None:
            return Response(status_code=404)
        return Response(content=foto[0], media_type=foto[1])

    @r.get("/carnet.pdf")
    def carnet_pdf(repo=Depends(obtener_repositorio), identidad: ServicioIdentidad | None = Depends(identidad_portal), id_sesion: str | None = Cookie(default=None), secreto: str | None = Cookie(default=None)):
        estudiante = estudiante_actual(repo, identidad, id_sesion, secreto)
        cuerpo = f"BT /F1 18 Tf 72 720 Td (Carnet estudiante {estudiante['carne']}) Tj ET"
        pdf = ("%PDF-1.4\n1 0 obj<</Type/Catalog/Pages 2 0 R>>endobj\n"
               "2 0 obj<</Type/Pages/Count 1/Kids[3 0 R]>>endobj\n"
               "3 0 obj<</Type/Page/Parent 2 0 R/MediaBox[0 0 612 792]/Resources<</Font<</F1 4 0 R>>>>/Contents 5 0 R>>endobj\n"
               "4 0 obj<</Type/Font/Subtype/Type1/BaseFont/Helvetica>>endobj\n"
               f"5 0 obj<</Length {len(cuerpo)}>>stream\n{cuerpo}\nendstream endobj\nxref\n0 6\n0000000000 65535 f \ntrailer<</Size 6/Root 1 0 R>>\nstartxref\n0\n%%EOF\n").encode()
        return Response(pdf, media_type="application/pdf", headers={"Content-Disposition": f"inline; filename=carnet-{estudiante['idEstudiante']}.pdf"})

    @r.get("/asistencia/hoy")
    def asistencia_hoy(repo=Depends(obtener_repositorio), identidad: ServicioIdentidad | None = Depends(identidad_portal), id_sesion: str | None = Cookie(default=None), secreto: str | None = Cookie(default=None), asistencia_repo=Depends(obtener_asistencia) if obtener_asistencia else None):
        estudiante = estudiante_actual(repo, identidad, id_sesion, secreto)
        if asistencia_repo is None:
            raise HTTPException(503, "La asistencia no está configurada")
        marcas = [m for m in asistencia_repo.listar(date.today()) if int(m["id_estudiante"]) == int(estudiante["idEstudiante"])]
        return {"marcas": marcas}

    @r.post("/asistencia/{accion}")
    def registrar_asistencia(accion: str, repo=Depends(obtener_repositorio), identidad: ServicioIdentidad | None = Depends(identidad_portal), id_sesion: str | None = Cookie(default=None), secreto: str | None = Cookie(default=None), asistencia_repo=Depends(obtener_asistencia) if obtener_asistencia else None):
        estudiante = estudiante_actual(repo, identidad, id_sesion, secreto)
        if asistencia_repo is None:
            raise HTTPException(503, "La asistencia no está configurada")
        estados = {"confirm": "presente", "decline": "ausente"}
        estado = estados.get(accion)
        if estado is None:
            raise HTTPException(400, "Acción de asistencia no válida")
        return asistencia_repo.registrar({"id_estudiante": int(estudiante["idEstudiante"]), "fecha": date.today(), "estado": estado, "observacion": None}, int(estudiante["idEstudiante"]), "WEB")

    @r.post("/autenticacion")
    def autenticar(datos: AccesoEstudiante, respuesta: Response, repo=Depends(obtener_repositorio), identidad: ServicioIdentidad | None = Depends(obtener_identidad_estudiante) if obtener_identidad_estudiante else None):
        if identidad is None:
            raise HTTPException(503, "La identidad no está configurada")
        credencial = repo.buscar_credencial(datos.carne)
        if not credencial or not credencial.get("activo") or not credencial.get("hash_contrasena") or not verificar_contrasena(datos.pin, str(credencial["hash_contrasena"])):
            raise HTTPException(status.HTTP_401_UNAUTHORIZED, "Las credenciales no son válidas")
        resultado = identidad.crear_sesion(int(credencial["id_estudiante"]), str(credencial["carne"]))
        token_csrf = secrets.token_urlsafe(32)
        identidad.establecer_csrf(resultado.id_sesion, token_csrf)
        respuesta.set_cookie("id_sesion", resultado.id_sesion, max_age=duracion_sesion_estudiante, httponly=True, secure=cookies_seguras, samesite="strict")
        respuesta.set_cookie("secreto_sesion", resultado.secreto_sesion, max_age=duracion_sesion_estudiante, httponly=True, secure=cookies_seguras, samesite="strict")
        respuesta.set_cookie("csrf_token", token_csrf, max_age=duracion_sesion_estudiante, httponly=False, secure=cookies_seguras, samesite="strict")
        return {"idEstudiante": resultado.id_usuario, "debeCambiarPin": bool(credencial.get("debe_cambiar_pin"))}

    @r.post("/pin")
    def cambiar_pin(datos: CambioPinEstudiante, repo=Depends(obtener_repositorio), identidad: ServicioIdentidad | None = Depends(obtener_identidad_estudiante) if obtener_identidad_estudiante else None, id_sesion: str | None = Cookie(default=None), secreto: str | None = Cookie(default=None), token_csrf: str | None = Header(default=None, alias="X-CSRF-Token"), csrf_cookie: str | None = Cookie(default=None, alias="csrf_token")):
        if identidad is None or not id_sesion or not secreto:
            raise HTTPException(status.HTTP_401_UNAUTHORIZED, "La sesión no es válida")
        try:
            sesion = identidad.validar_sesion(id_sesion, secreto)
        except AutenticacionFallida as exc:
            raise HTTPException(status.HTTP_401_UNAUTHORIZED, str(exc)) from exc
        if not token_csrf or token_csrf != csrf_cookie or not identidad.validar_csrf(sesion, token_csrf):
            raise HTTPException(status.HTTP_403_FORBIDDEN, "El token CSRF no es válido")
        credencial = repo.buscar_credencial_por_id(sesion.id_usuario)
        if not credencial or not verificar_contrasena(datos.pin_actual, str(credencial.get("hash_contrasena", ""))):
            raise HTTPException(status.HTTP_400_BAD_REQUEST, "El PIN actual no es válido")
        repo.actualizar_pin(sesion.id_usuario, hash_contrasena(datos.pin_nuevo))
        return {"estado": "actualizado"}

    @r.get("/secciones")
    def secciones(turno: str | None = None, _=Depends(exigir_permiso("estudiantes.leer")), repo=Depends(obtener_repositorio)):
        return repo.secciones(turno)

    @r.get("/{id_estudiante}/perfil")
    def perfil(id_estudiante: int, _=Depends(exigir_permiso("estudiantes.leer")), repo=Depends(obtener_repositorio)):
        return repo.perfil_detallado(id_estudiante)

    @r.put("/{id_estudiante}/beneficio", status_code=204)
    def beneficio(id_estudiante: int, datos: CambioAsignacion, _=Depends(exigir_permiso("beneficios.editar")), __=Depends(exigir_csrf), repo=Depends(obtener_repositorio)):
        repo.asignar_beneficio(id_estudiante, datos.id_beneficio)

    @r.put("/{id_estudiante}/ruta", status_code=204)
    def ruta(id_estudiante: int, datos: CambioAsignacion, _=Depends(exigir_permiso("rutas.administrar")), __=Depends(exigir_csrf), repo=Depends(obtener_repositorio)):
        repo.asignar_ruta(id_estudiante, datos.id_ruta)

    @r.post("/{id_estudiante}/reset-pin", response_model=PinGenerado, response_model_by_alias=True)
    def reset_pin(id_estudiante: int, _=Depends(exigir_permiso("estudiantes.editar")), __=Depends(exigir_csrf), repo=Depends(obtener_repositorio)):
        pin = f"{secrets.randbelow(1_000_000):06d}"
        repo.reiniciar_pin(id_estudiante, hash_contrasena(pin))
        return PinGenerado(idEstudiante=id_estudiante, pin=pin)

    @r.post("/pines/seccion")
    def generar_pines_seccion(datos: GeneracionPinesSeccion, _=Depends(exigir_permiso("estudiantes.editar")), __=Depends(exigir_csrf), repo=Depends(obtener_repositorio)):
        datos.seccion = datos.seccion.strip() if datos.seccion and datos.seccion.strip() else None
        estudiantes = [e for e in repo.listar_para_generacion_pines() if ((datos.seccion is None and not e.get("seccion")) or e.get("seccion") == datos.seccion) and (not datos.turno or e.get("turno") == datos.turno)]
        generados = [PinGenerado(idEstudiante=int(e["id_estudiante"]), pin=f"{secrets.randbelow(1_000_000):06d}") for e in estudiantes]
        repo.actualizar_pines_seccion(datos.seccion, {p.id_estudiante: hash_contrasena(p.pin) for p in generados})
        filas = []
        for estudiante, pin in zip(estudiantes, generados):
            nombre = " ".join(str(estudiante.get(c, "") or "") for c in ("nombre", "primer_apellido", "segundo_apellido")).strip()
            filas.append({"idEstudiante": pin.id_estudiante, "nombreCompleto": nombre, "cedula": estudiante.get("cedula", ""), "horario": estudiante.get("turno", ""), "seccion": estudiante.get("seccion", datos.seccion), "pin": pin.pin})
        if not filas:
            raise HTTPException(404, "No hay estudiantes para la sección indicada")
        return {
            "total": len(filas),
            "seccion": datos.seccion or "Sin sección",
            "turno": datos.turno,
            "estudiantes": filas,
        }

    return r
