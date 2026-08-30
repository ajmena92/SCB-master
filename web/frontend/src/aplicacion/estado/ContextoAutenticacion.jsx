import { createContext, use, useEffect, useState, useCallback } from "react";
import { api } from "@/compartido/consultas/cliente_http";
import { borrarTokenSesion } from "@/compartido/consultas/token_sesion";

const ContextoAutenticacionContext = createContext(null);

function listaExplicita(valores) {
  return Array.isArray(valores) ? valores.filter((valor) => typeof valor === "string") : [];
}

function rolPrincipal(roles) {
  if (roles.includes("Administrador")) return "Administrador";
  return roles[0] || "";
}

async function obtenerSesion() {
  const { data, status } = await api.get("/v1/sesion", {
    omitirManejoFalloAutenticacion: true,
  });
  if (status === 204 || !data) return { session: false, debeCambiarPin: false };
  const esAdministrativa = data.tipo === "administracion" || data.tipo === "admin";
  const roles = esAdministrativa
    ? listaExplicita(data.usuario?.roles ?? (data.rol ? [data.rol] : []))
    : [];
  const permisos = esAdministrativa ? listaExplicita(data.usuario?.permisos) : [];
  const usuario = esAdministrativa
    ? {
        ...(typeof data.usuario === "object" ? data.usuario : {}),
        Nombre: data.usuario?.Nombre || data.usuario?.NombreCompleto || data.usuario || "",
        Rol: rolPrincipal(roles),
      }
    : { ...(data.usuario ?? {}), codigo: data.codigo, nombres: data.nombres ?? data.codigo };
  return {
    session: { tipo: esAdministrativa ? "admin" : data.rol, usuario, roles, permisos },
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
    } catch {
      setSession(false);
      setDebeCambiarPin(false);
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
      value={{ session, setSession, debeCambiarPin, setDebeCambiarPin, loadMe, logout }}
    >
      {children}
    </ContextoAutenticacionContext>
  );
}

export const useAutenticacion = () => use(ContextoAutenticacionContext);
