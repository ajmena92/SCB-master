import { useEffect, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { mensajeError } from "@/compartido/consultas/errores";
import { consultarCalendario } from "@/funcionalidades/administracion/consultas/parametros";
import { toast } from "sonner";

export function useCalendario() {
  const [hoy] = useState(() => new Date());
  const [anio, setAnio] = useState(hoy.getFullYear());
  const [mes, setMes] = useState(hoy.getMonth() + 1);
  const consulta = useQuery({
    queryKey: ["admin", "menu", "calendario", anio, mes],
    queryFn: () => consultarCalendario(anio, mes),
  });
  useEffect(() => {
    if (consulta.error) toast.error(mensajeError(consulta.error));
  }, [consulta.error]);
  function mover(delta) {
    let nuevoMes = mes + delta;
    let nuevoAnio = anio;
    if (nuevoMes < 1) {
      nuevoMes = 12;
      nuevoAnio -= 1;
    }
    if (nuevoMes > 12) {
      nuevoMes = 1;
      nuevoAnio += 1;
    }
    setMes(nuevoMes);
    setAnio(nuevoAnio);
  }
  const dias = consulta.data ?? [];
  return {
    hoyISO: hoy.toISOString().slice(0, 10),
    anio,
    mes,
    dias,
    loading: consulta.isPending,
    mover,
    semanas: [1, 2, 3, 4, 5].filter((semana) => dias.some((dia) => dia.semanaMes === semana)),
  };
}
