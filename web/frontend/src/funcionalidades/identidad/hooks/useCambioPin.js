import { useCallback, useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";

import { useAutenticacion } from "@/aplicacion/estado/ContextoAutenticacion";
import { api } from "@/compartido/consultas/cliente_http";
import { errMsg } from "@/compartido/consultas/errores_api";

export function useCambioPin() {
  const navigate = useNavigate();
  const { setDebeCambiarPin, debeCambiarPin } = useAutenticacion();
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
        await api.post("/v1/estudiantes/pin", {
          pinActual: formulario.actual,
          pinNuevo: formulario.nuevo,
        });
        setDebeCambiarPin(false);
        toast.success("PIN actualizado correctamente");
        navigate("/comedor", { replace: true });
      } catch (error) {
        setEstado({ cargando: false, error: errMsg(error) });
        return;
      }
      setEstado((actual) => ({ ...actual, cargando: false }));
    },
    [formulario, navigate, setDebeCambiarPin],
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
