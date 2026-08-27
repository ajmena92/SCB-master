import { useCallback, useEffect, useState } from "react";
import { mensajeError } from "@/compartido/consultas/errores";
import {
  buscarEstudiantes,
  guardarCorreccion,
} from "@/funcionalidades/administracion/consultas/asistencia";
import { toast } from "sonner";

export function useCorrecciones() {
  const [estudiantes, setEstudiantes] = useState([]);
  const [buscar, setBuscar] = useState("");
  const [loading, setLoading] = useState(true);
  const [idUsuario, setIdUsuario] = useState("");
  const [fecha, setFecha] = useState(() => new Date().toISOString().slice(0, 10));
  const [accion, setAccion] = useState("agregar");
  const [motivo, setMotivo] = useState("");
  const [saving, setSaving] = useState(false);
  const cargar = useCallback(async (texto) => {
    if (texto.trim().length < 2) {
      setEstudiantes([]);
      setLoading(false);
      return;
    }
    setLoading(true);
    try {
      setEstudiantes(await buscarEstudiantes(texto));
    } catch (error) {
      toast.error(mensajeError(error));
    } finally {
      setLoading(false);
    }
  }, []);
  useEffect(() => {
    const timer = setTimeout(() => cargar(buscar), 250);
    return () => clearTimeout(timer);
  }, [buscar, cargar]);
  async function enviar() {
    if (!idUsuario) return toast.error("Seleccioná un estudiante");
    if (!motivo.trim()) return toast.error("El motivo es obligatorio");
    setSaving(true);
    try {
      await guardarCorreccion(idUsuario, {
        estado: accion === "agregar" ? "presente" : "ausente",
        motivo,
      });
      toast.success("Corrección aplicada y auditada");
      setMotivo("");
    } catch (error) {
      toast.error(mensajeError(error));
    } finally {
      setSaving(false);
    }
  }
  return {
    estudiantes,
    buscar,
    loading,
    idUsuario,
    fecha,
    accion,
    motivo,
    saving,
    setBuscar,
    setIdUsuario,
    setFecha,
    setAccion,
    setMotivo,
    enviar,
  };
}
