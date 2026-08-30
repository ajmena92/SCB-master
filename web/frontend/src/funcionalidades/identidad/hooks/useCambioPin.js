import { useCallback, useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";

import { useAutenticacion } from "@/aplicacion/estado/ContextoAutenticacion";
import { api } from "@/compartido/consultas/cliente_http";
import { errMsg } from "@/compartido/consultas/errores_api";
import { borrarTokenSesion } from "@/compartido/consultas/token_sesion";

export function useCambioPin() {
  const navigate = useNavigate();
  const { setDebeCambiarPin, setSession, debeCambiarPin } = useAutenticacion();
  const [formulario, setFormulario] = useState({ actual: "", nuevo: "", confirmar: "" });
  const [estado, setEstado] = useState({ cargando: false, error: "" });
  const cambiar = useCallback(
    (campo) => (valor) => setFormulario((actual) => ({ ...actual, [campo]: valor })),
    [],
  );

  const enviar = useCallback(
    async (evento) => {
      evento.preventDefault();
      if (formulario.nuevo.length !== 6) {
        setEstado({ cargando: false, error: "El nuevo PIN debe tener 6 dígitos" });
        return;
      }
      if (formulario.nuevo !== formulario.confirmar) {
        setEstado({ cargando: false, error: "Los PIN nuevos no coinciden" });
        return;
      }
      setEstado({ cargando: true, error: "" });
      try {
        await api.post("/v1/autenticacion/portal/pin", {
          pinActual: formulario.actual,
          pinNuevo: formulario.nuevo,
        });
        // El backend revoca todas las sesiones al cambiar el PIN. No se debe
        // conservar un token ya inválido ni abrir el portal como si siguiera
        // autenticado.
        borrarTokenSesion();
        setSession(false);
        setDebeCambiarPin(false);
        toast.success("PIN actualizado. Ingresá nuevamente con tu nuevo PIN.");
        navigate("/", { replace: true });
      } catch (error) {
        setEstado({ cargando: false, error: errMsg(error) });
        return;
      }
      setEstado((actual) => ({ ...actual, cargando: false }));
    },
    [formulario, navigate, setDebeCambiarPin, setSession],
  );

  return {
    ...formulario,
    cambiarActual: cambiar("actual"),
    cambiarNuevo: cambiar("nuevo"),
    cambiarConfirmar: cambiar("confirmar"),
    enviar,
    debeCambiarPin,
    ...estado,
  };
}
