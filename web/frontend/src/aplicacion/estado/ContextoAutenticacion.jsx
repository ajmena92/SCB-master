import { createContext, use, useEffect, useState, useCallback } from "react";
import { api } from "@/lib/api";

const ContextoAutenticacionContext = createContext(null);

function listaExplicita(valores) {
  return Array.isArray(valores) ? valores.filter((valor) => typeof valor === "string") : [];
}

function rolPrincipal(roles) {
  if (roles.includes("Administrador")) return "Administrador";
  return roles[0] || "";
}

async function obtenerSesion() {
  const { data } = await api.get("/v1/sesion", { skipAuthFailureHandling: true });
  const roles = data.tipo === "admin" ? listaExplicita(data.usuario?.roles) : [];
  const permisos = data.tipo === "admin" ? listaExplicita(data.usuario?.permisos) : [];
  const usuario =
    data.tipo === "admin"
      ? {
          ...data.usuario,
          Nombre: data.usuario?.Nombre || data.usuario?.NombreCompleto || "",
          Rol: rolPrincipal(roles),
        }
      : data.usuario;
  return {
    session: { tipo: data.tipo, usuario, roles, permisos },
    // En estudiantes el indicador forma parte del perfil; se conserva el
    // nivel superior para respuestas de autenticación antiguas.
    debeCambiarPin: Boolean(data.debeCambiarPin ?? data.usuario?.debeCambiarPin),
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
      setSession(false);
      setDebeCambiarPin(false);
    };
    window.addEventListener("scsc:unauthenticated", onUnauthenticated);
    return () => window.removeEventListener("scsc:unauthenticated", onUnauthenticated);
  }, []);

  const logout = async () => {
    try {
      await api.post("/v1/sesion/cerrar", undefined, { skipAuthFailureHandling: true });
    } catch {
      // Close the local UI even if a stale server session cannot be revoked.
    }
    setSession(false);
    setDebeCambiarPin(false);
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
