import { useCallback, useState } from "react";
import { useNavigate } from "react-router-dom";

import { useAutenticacion } from "@/aplicacion/estado/ContextoAutenticacion";
import { api } from "@/compartido/consultas/cliente_http";
import { errMsg } from "@/compartido/consultas/errores_api";
import { guardarTokenSesion } from "@/compartido/consultas/token_sesion";

export function clasificarErrorAutenticacion(error) {
  const estado = error?.response?.status;
  if (!estado) return "conexion";
  if (estado >= 500) return "servidor";
  return "credenciales";
}

function useFormularioInicial(campos) {
  const [formulario, setFormulario] = useState(campos);
  const cambiar = useCallback(
    (campo) => (valor) => setFormulario((actual) => ({ ...actual, [campo]: valor })),
    [],
  );
  return { formulario, cambiar };
}

export function useInicioSesionAdministrativo() {
  const navegar = useNavigate();
  const { loadMe } = useAutenticacion();
  const { formulario, cambiar } = useFormularioInicial({ nombreUsuario: "", contrasena: "" });
  const [estado, setEstado] = useState({ cargando: false, error: "", tipoError: "credenciales" });

  const enviar = useCallback(
    async (evento) => {
      evento.preventDefault();
      setEstado({ cargando: true, error: "", tipoError: "credenciales" });
      try {
        const { data } = await api.post(
          "/v1/autenticacion/administracion",
          { usuario: formulario.nombreUsuario, contrasena: formulario.contrasena },
          { omitirManejoFalloAutenticacion: true, omitirCsrf: true },
        );
        guardarTokenSesion(data.token);
        await loadMe();
        navegar("/admin/panel", { replace: true });
      } catch (error) {
        setEstado({
          cargando: false,
          error: errMsg(error, { showUnauthorizedDetail: true }),
          tipoError: clasificarErrorAutenticacion(error),
        });
        return;
      }
      setEstado((actual) => ({ ...actual, cargando: false }));
    },
    [formulario, loadMe, navegar],
  );

  return {
    nombreUsuario: formulario.nombreUsuario,
    contrasena: formulario.contrasena,
    cambiarNombreUsuario: cambiar("nombreUsuario"),
    cambiarContrasena: cambiar("contrasena"),
    enviar,
    ...estado,
  };
}

export function useInicioSesionEstudiantil() {
  const navegar = useNavigate();
  const { loadMe, setDebeCambiarPin } = useAutenticacion();
  const { formulario, cambiar } = useFormularioInicial({ carne: "", pin: "" });
  const [estado, setEstado] = useState({ cargando: false, error: "", tipoError: "credenciales" });

  const enviar = useCallback(
    async (evento) => {
      evento.preventDefault();
      if (formulario.pin.length !== 6) {
        setEstado({
          cargando: false,
          error: "El PIN debe tener 6 dígitos.",
          tipoError: "validacion",
        });
        return;
      }
      setEstado({ cargando: true, error: "", tipoError: "credenciales" });
      try {
        const { data } = await api.post(
          "/v1/autenticacion/portal",
          { codigo: formulario.carne, pin: formulario.pin },
          { omitirManejoFalloAutenticacion: true, omitirCsrf: true },
        );
        guardarTokenSesion(data.token);
        await loadMe();
        const cambioObligatorio = Boolean(data.cambioObligatorio);
        setDebeCambiarPin(cambioObligatorio);
        navegar(cambioObligatorio ? "/cambiar-pin" : "/portal", { replace: true });
      } catch (error) {
        setEstado({
          cargando: false,
          error: errMsg(error, { showUnauthorizedDetail: true }),
          tipoError: clasificarErrorAutenticacion(error),
        });
        cambiar("pin")("");
        return;
      }
      setEstado((actual) => ({ ...actual, cargando: false }));
    },
    [cambiar, formulario, loadMe, navegar, setDebeCambiarPin],
  );

  return {
    carne: formulario.carne,
    pin: formulario.pin,
    cambiarCarne: cambiar("carne"),
    cambiarPin: cambiar("pin"),
    enviar,
    ...estado,
  };
}
