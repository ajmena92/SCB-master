import { createContext, use, useEffect, useState, useCallback } from "react";
import { api } from "@/compartido/consultas/cliente_http";
import { borrarTokenSesion, obtenerTokenSesion } from "@/compartido/consultas/token_sesion";

const ContextoAutenticacionContext = createContext(null);

async function obtenerSesion() {
  if (!obtenerTokenSesion()) return { session: false, debeCambiarPin: false };
  const { data, status } = await api.get("/v1/sesion", {
    omitirManejoFalloAutenticacion: true,
  });
  if (status === 204 || !data) return { session: false, debeCambiarPin: false };
  if (data.tipo === "administracion") {
    if (
      !Number.isInteger(data.cuentaId) ||
      typeof data.usuario !== "string" ||
      !["administrador", "operador"].includes(data.rol) ||
      !Array.isArray(data.permisos)
    )
      throw new Error("La API devolvió una sesión administrativa inválida.");
    return {
      session: {
        tipo: "administracion",
        cuentaId: data.cuentaId,
        personaId: data.personaId,
        usuario: data.usuario,
        nombres: data.nombres,
        rol: data.rol,
        permisos: data.permisos.filter((permiso) => typeof permiso === "string"),
        cambioContrasenaObligatorio: Boolean(data.cambioContrasenaObligatorio),
        vinculacionPendiente: Boolean(data.vinculacionPendiente),
      },
      debeCambiarPin: false,
    };
  }
  const usuario = {
    ...(data.usuario ?? {}),
    codigo: data.codigo,
    nombres: data.nombres ?? data.codigo,
  };
  return {
    session: { tipo: data.rol, usuario },
    // En estudiantes el indicador forma parte del perfil; se conserva el
    // nivel superior para respuestas de autenticación antiguas.
    debeCambiarPin: Boolean(
      data.cambioObligatorio ?? data.debeCambiarPin ?? data.usuario?.debeCambiarPin,
    ),
  };
}

export function ProveedorAutenticacion({ children }) {
  const [session, setSession] = useState(null); // null=checking, false=none, object=logged
  const [debeCambiarPin, setDebeCambiarPin] = useState(false);

  const loadMe = useCallback(async () => {
    try {
      const autenticacion = await obtenerSesion();
      setSession(autenticacion.session);
      setDebeCambiarPin(autenticacion.debeCambiarPin);
      return autenticacion;
    } catch {
      setSession(false);
      setDebeCambiarPin(false);
      return { session: false, debeCambiarPin: false };
    }
  }, []);

  useEffect(() => {
    let activo = true;
    obtenerSesion()
      .then((autenticacion) => {
        if (!activo) return;
        setSession(autenticacion.session);
        setDebeCambiarPin(autenticacion.debeCambiarPin);
      })
      .catch(() => {
        if (!activo) return;
        setSession(false);
        setDebeCambiarPin(false);
      });
    return () => {
      activo = false;
    };
  }, []);

  useEffect(() => {
    const onUnauthenticated = () => {
      borrarTokenSesion();
      setSession(false);
      setDebeCambiarPin(false);
    };
    window.addEventListener("scsc:unauthenticated", onUnauthenticated);
    return () => window.removeEventListener("scsc:unauthenticated", onUnauthenticated);
  }, []);

  const logout = async () => {
    try {
      await api.post("/v1/autenticacion/logout");
    } catch {
      // Aunque el servidor no responda, el navegador no debe conservar una
      // credencial que la persona decidió cerrar.
    } finally {
      borrarTokenSesion();
      setSession(false);
      setDebeCambiarPin(false);
    }
  };

  return (
    <ContextoAutenticacionContext
      value={{
        session,
        setSession,
        debeCambiarPin,
        setDebeCambiarPin,
        loadMe,
        logout,
        limpiarSesion: () => {
          borrarTokenSesion();
          setSession(false);
          setDebeCambiarPin(false);
        },
      }}
    >
      {children}
    </ContextoAutenticacionContext>
  );
}

export const useAutenticacion = () => use(ContextoAutenticacionContext);
