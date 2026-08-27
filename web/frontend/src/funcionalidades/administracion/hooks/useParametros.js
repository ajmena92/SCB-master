import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { mensajeError } from "@/compartido/consultas/errores";
import {
  consultarParametros,
  guardarParametros,
  normalizeParametros,
  validateParametros,
} from "@/funcionalidades/administracion/consultas/parametros";

export function useParametros() {
  const [editados, setEditados] = useState(null);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const consulta = useQuery({ queryKey: ["admin", "parametros"], queryFn: consultarParametros });
  const parametros = editados ?? normalizeParametros(consulta.data);
  function actualizarHorario(idHorario, horaLimite) {
    setEditados((actual) => {
      const base = actual ?? normalizeParametros(consulta.data);
      return {
        ...base,
        horarios: base.horarios.map((horario) =>
          horario.idHorario === idHorario ? { ...horario, horaLimite } : horario,
        ),
      };
    });
  }
  function actualizarMinutos(minutosAvisoPrevio) {
    setEditados((actual) => ({
      ...(actual ?? normalizeParametros(consulta.data)),
      minutosAvisoPrevio,
    }));
  }
  async function guardar() {
    const validacion = validateParametros(parametros);
    if (validacion) {
      setSuccess("");
      setError(validacion);
      return;
    }
    setSaving(true);
    setError("");
    setSuccess("");
    try {
      const horarios = parametros.horarios.filter((horario) => horario.activo !== false);
      await guardarParametros({
        minutosAvisoPrevio: Number(parametros.minutosAvisoPrevio),
        horarios: horarios.map(({ idHorario, horaLimite }) => ({ idHorario, horaLimite })),
      });
      setSuccess(
        "Parámetros guardados. El portal aplicará los cambios dinámicamente en la próxima consulta o acción del estudiante.",
      );
    } catch (exception) {
      setError(mensajeError(exception));
    } finally {
      setSaving(false);
    }
  }
  return {
    parametros,
    horariosEditables: parametros.horarios.filter((horario) => horario.activo !== false),
    loading: consulta.isPending,
    loadError: consulta.error ? mensajeError(consulta.error) : "",
    error,
    success,
    saving,
    actualizarHorario,
    actualizarMinutos,
    guardar,
  };
}
