import { useState } from "react";
import { mensajeError } from "@/compartido/consultas/errores";
import { registrarConsumo } from "@/funcionalidades/comedor/consultas";
import { toast } from "sonner";

export function useRegistroConsumo() {
  const [idEstudiante, setIdEstudiante] = useState("");
  const [fecha, setFecha] = useState(() => new Date().toISOString().slice(0, 10));
  const [guardando, setGuardando] = useState(false);
  async function registrar() {
    if (!idEstudiante) return toast.error("Indique el estudiante");
    setGuardando(true);
    try {
      await registrarConsumo(idEstudiante, fecha);
      toast.success("Consumo registrado");
      setIdEstudiante("");
    } catch (error) {
      toast.error(mensajeError(error));
    } finally {
      setGuardando(false);
    }
  }
  return { idEstudiante, fecha, guardando, setIdEstudiante, setFecha, registrar };
}
