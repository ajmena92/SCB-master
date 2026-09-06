import { useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { Button } from "@/components/ui/button";
import { errMsg } from "@/compartido/consultas/errores_api";
import { EstadoPanel } from "@/compartido/componentes/Estados";
import { EncabezadoPagina } from "@/funcionalidades/plataforma/componentes/ElementosComunes";
import { DialogoDetalleMenu, DialogoSustitucionMenu } from "../componentes/DialogosCalendarioMenu";
import { GrillaCalendarioMenu } from "../componentes/GrillaCalendarioMenu";
import {
  componenteSustitucion,
  fechaCostaRica,
  MESES,
  rangoMes,
  semanasCalendario,
  type DiaCalendario,
  type SustitucionMenu,
} from "../calendario";
import {
  actualizarServicioMenu,
  consultarCalendarioMenu,
  guardarSustitucionMenu,
} from "../consultas/calendario_menu";

export default function CalendarioMenu() {
  const ahora = new Date();
  const [anio, setAnio] = useState(ahora.getFullYear());
  const [mes, setMes] = useState(ahora.getMonth() + 1);
  const [detalle, setDetalle] = useState<DiaCalendario | null>(null);
  const [sustitucion, setSustitucion] = useState<SustitucionMenu | null>(null);
  const cliente = useQueryClient();
  const rango = rangoMes(anio, mes);
  const invalidar = () =>
    cliente.invalidateQueries({ queryKey: ["menu", "calendario", anio, mes] });
  const calendario = useQuery({
    queryKey: ["menu", "calendario", anio, mes],
    queryFn: () => consultarCalendarioMenu(rango),
  });
  const actualizar = useMutation({
    mutationFn: actualizarServicioMenu,
    onSuccess: () => {
      setDetalle(null);
      invalidar();
    },
  });
  const guardarSustitucion = useMutation({
    mutationFn: guardarSustitucionMenu,
    onSuccess: () => {
      setSustitucion(null);
      invalidar();
    },
  });
  const mover = (delta: number) => {
    const fecha = new Date(anio, mes - 1 + delta, 1);
    setAnio(fecha.getFullYear());
    setMes(fecha.getMonth() + 1);
  };
  const crearSustitucion = (dia: DiaCalendario) => {
    setSustitucion({
      fecha: dia.fecha,
      titulo: dia.titulo ?? "",
      observaciones: "",
      componentes: dia.componentes.map(componenteSustitucion),
    });
    setDetalle(null);
  };
  const semanas = useMemo(() => semanasCalendario(calendario.data ?? []), [calendario.data]);
  return (
    <section className="space-y-6" aria-labelledby="calendario-menu-titulo">
      <EncabezadoPagina
        id="calendario-menu-titulo"
        titulo="Calendario del menú"
        descripcion="Consulte el servicio programado, revise cada día lectivo y registre sustituciones cuando corresponda."
        accion={
          <div className="flex items-center gap-2">
            <Button
              variant="outline"
              size="icon"
              aria-label="Mes anterior"
              onClick={() => mover(-1)}
            >
              <ChevronLeft className="h-4 w-4" />
            </Button>
            <strong className="w-40 text-center" aria-live="polite">
              {MESES[mes - 1]} {anio}
            </strong>
            <Button
              variant="outline"
              size="icon"
              aria-label="Mes siguiente"
              onClick={() => mover(1)}
            >
              <ChevronRight className="h-4 w-4" />
            </Button>
          </div>
        }
      />
      {calendario.error && <EstadoPanel variante="error">{errMsg(calendario.error)}</EstadoPanel>}
      <div aria-busy={calendario.isPending}>
        <GrillaCalendarioMenu semanas={semanas} hoy={fechaCostaRica()} onSeleccionar={setDetalle} />
      </div>
      <DialogoDetalleMenu
        detalle={detalle}
        guardando={actualizar.isPending}
        onCerrar={() => setDetalle(null)}
        onAlternarServicio={(dia) =>
          actualizar.mutate({ fecha: dia.fecha, habilitado: !dia.habilitado })
        }
        onCrearSustitucion={crearSustitucion}
      />
      <DialogoSustitucionMenu
        sustitucion={sustitucion}
        guardando={guardarSustitucion.isPending}
        onCerrar={() => setSustitucion(null)}
        onCambiar={setSustitucion}
        onGuardar={(datos) => guardarSustitucion.mutate(datos)}
      />
    </section>
  );
}
