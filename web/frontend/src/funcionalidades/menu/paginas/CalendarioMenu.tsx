import { useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  Ban,
  CalendarDays,
  ChevronLeft,
  ChevronRight,
  CookingPot,
  Ellipsis,
  ListChecks,
  Plus,
  Replace,
  Trash2,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { api } from "@/compartido/consultas/cliente_http";
import { errMsg } from "@/compartido/consultas/errores_api";

type DiaCalendario = {
  fecha: string;
  habilitado: boolean;
  esLectivo: boolean;
  semana: number | null;
  dia: number;
  diaMes: number;
  motivo?: string | null;
  origen: "cerrado" | "no_lectivo" | "sin_menu" | "plantilla" | "sustitucion";
  titulo?: string | null;
  componentes: string[];
  tieneSustitucion: boolean;
};

type ComponenteSustitucion = { clave: string; nombre: string };

const MESES = [
  "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
  "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre",
];
const DIAS = ["Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo"];

function fechaIso(anio: number, mes: number, dia: number) {
  return `${anio}-${String(mes).padStart(2, "0")}-${String(dia).padStart(2, "0")}`;
}

function rangoMes(anio: number, mes: number) {
  const desde = fechaIso(anio, mes, 1);
  const hasta = fechaIso(anio, mes, new Date(anio, mes, 0).getDate());
  return { desde, hasta };
}

function fechaCostaRica(): string {
  const partes = new Intl.DateTimeFormat("en-CA", {
    timeZone: "America/Costa_Rica", year: "numeric", month: "2-digit", day: "2-digit",
  }).formatToParts(new Date());
  const valor = (tipo: string) => partes.find((parte) => parte.type === tipo)?.value ?? "";
  return `${valor("year")}-${valor("month")}-${valor("day")}`;
}

function semanasCalendario(dias: DiaCalendario[]): Array<Array<DiaCalendario | null>> {
  if (!dias.length) return [];
  const celdas: Array<DiaCalendario | null> = Array.from({ length: dias[0].dia - 1 }, () => null);
  celdas.push(...dias);
  while (celdas.length % 7) celdas.push(null);
  return Array.from({ length: celdas.length / 7 }, (_, indice) => celdas.slice(indice * 7, indice * 7 + 7));
}

function fechaVisible(fecha: string): string {
  return new Intl.DateTimeFormat("es-CR", { timeZone: "America/Costa_Rica", weekday: "long", day: "numeric", month: "long", year: "numeric" }).format(new Date(`${fecha}T12:00:00`));
}

function componenteSustitucion(nombre = ""): ComponenteSustitucion {
  return { clave: `${Date.now()}-${Math.random()}`, nombre };
}

export default function CalendarioMenu() {
  const ahora = new Date();
  const [anio, setAnio] = useState(ahora.getFullYear());
  const [mes, setMes] = useState(ahora.getMonth() + 1);
  const cliente = useQueryClient();
  const [detalle, setDetalle] = useState<DiaCalendario | null>(null);
  const [sustitucion, setSustitucion] = useState<{ fecha: string; titulo: string; observaciones: string; componentes: ComponenteSustitucion[] } | null>(null);
  const rango = rangoMes(anio, mes);
  const calendario = useQuery({
    queryKey: ["menu", "calendario", anio, mes],
    queryFn: async () => (await api.get<DiaCalendario[]>("/v1/menu/calendario", { params: rango })).data,
  });
  const actualizar = useMutation({
    mutationFn: (dia: Pick<DiaCalendario, "fecha" | "habilitado">) => api.put("/v1/menu/calendario", dia),
    onSuccess: () => {
      setDetalle(null);
      cliente.invalidateQueries({ queryKey: ["menu", "calendario", anio, mes] });
    },
  });
  const guardarSustitucion = useMutation({
    mutationFn: (datos: NonNullable<typeof sustitucion>) => api.put(`/v1/menu/sustituciones/${datos.fecha}`, {
      fecha: datos.fecha,
      titulo: datos.titulo,
      observaciones: datos.observaciones || null,
      componentes: datos.componentes.filter((item) => item.nombre.trim()).map((item, indice) => ({
        nombre: item.nombre.trim(), tipo: "Principal", orden: indice + 1,
      })),
    }),
    onSuccess: () => {
      setSustitucion(null);
      cliente.invalidateQueries({ queryKey: ["menu", "calendario", anio, mes] });
    },
  });
  const semanas = useMemo(() => semanasCalendario(calendario.data ?? []), [calendario.data]);
  const hoy = fechaCostaRica();
  const mover = (delta: number) => {
    const fecha = new Date(anio, mes - 1 + delta, 1);
    setAnio(fecha.getFullYear());
    setMes(fecha.getMonth() + 1);
  };
  return (
    <section className="space-y-6" aria-labelledby="calendario-menu-titulo">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h2 id="calendario-menu-titulo" className="flex items-center gap-2 font-display text-2xl font-bold">
            <CalendarDays className="h-6 w-6 text-primary" /> Calendario del menú
          </h2>
          <p className="mt-1 max-w-3xl text-sm leading-relaxed text-muted-foreground">Consultá el servicio programado para cada fecha antes de la jornada. Seleccioná un día lectivo para ver el detalle, registrar una sustitución o cerrar el servicio cuando corresponda. Sábado y domingo se muestran solo como referencia.</p>
        </div>
        <div className="flex items-center gap-2">
          <Button variant="outline" size="icon" aria-label="Mes anterior" onClick={() => mover(-1)}><ChevronLeft className="h-4 w-4" /></Button>
          <strong className="w-40 text-center" aria-live="polite">{MESES[mes - 1]} {anio}</strong>
          <Button variant="outline" size="icon" aria-label="Mes siguiente" onClick={() => mover(1)}><ChevronRight className="h-4 w-4" /></Button>
        </div>
      </div>
      {calendario.error ? <p role="alert" className="rounded-xl border border-destructive/30 p-4 text-destructive">{errMsg(calendario.error)}</p> : null}
      <div aria-busy={calendario.isPending}>
        <div className="rounded-2xl border bg-card p-2 shadow-sm shadow-primary/5 sm:p-4">
          <div className="space-y-3">
          <div className="hidden grid-cols-[repeat(5,minmax(0,1fr))_minmax(4.5rem,.58fr)_minmax(4.5rem,.58fr)] gap-2 text-center sm:grid">
            {DIAS.map((nombre, indice) => <p key={nombre} className={`text-center text-xs font-bold uppercase tracking-wide ${indice > 4 ? "text-muted-foreground/60" : "text-muted-foreground"}`}>{nombre}</p>)}
          </div>
          {semanas.map((semana, indice) => (
            <section key={indice} aria-label={`Semana calendario ${indice + 1}`} className="grid grid-cols-1 gap-2 border-b border-border/60 pb-3 last:border-0 last:pb-0 sm:grid-cols-[repeat(5,minmax(0,1fr))_minmax(4.5rem,.58fr)_minmax(4.5rem,.58fr)] sm:gap-2 sm:border-0 sm:pb-0">
              <p className="px-1 text-xs font-semibold uppercase tracking-wide text-muted-foreground sm:hidden">Semana del calendario {indice + 1}</p>
              {semana.map((dia, posicion) => {
                if (!dia) return <div key={posicion} aria-hidden="true" className="hidden min-h-28 sm:block" />;
                const esHoy = dia.fecha === hoy;
                const estado = dia.origen === "sustitucion" ? "Sustitución" : dia.origen === "cerrado" ? "Cerrado" : dia.origen === "sin_menu" ? "Sin menú" : "Menú";
                return <article key={dia.fecha} role={dia.esLectivo ? "button" : undefined} tabIndex={dia.esLectivo ? 0 : undefined} onClick={() => dia.esLectivo && setDetalle(dia)} onKeyDown={(evento) => { if (dia.esLectivo && (evento.key === "Enter" || evento.key === " ")) { evento.preventDefault(); setDetalle(dia); } }} className={`relative rounded-xl border px-3 py-3 sm:min-h-28 sm:p-3 ${dia.esLectivo ? "cursor-pointer transition-all duration-200 hover:-translate-y-0.5 hover:border-primary/50 hover:shadow-md hover:shadow-primary/10 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary" : "border-dashed border-border/50 bg-muted/[0.07] sm:px-2"} ${esHoy ? "border-primary bg-primary/[0.06] ring-1 ring-primary" : ""} ${dia.origen === "cerrado" ? "border-destructive/30 bg-destructive/5" : ""} ${dia.origen === "sustitucion" ? "border-amber-500/60 bg-amber-500/10 hover:border-amber-500" : ""}`}>
                  <div className="flex items-center justify-between gap-2"><strong className={`flex h-7 min-w-7 items-center text-sm font-bold tabular-nums ${esHoy ? "justify-center rounded-full bg-primary text-primary-foreground" : "text-foreground"}`}>{dia.diaMes}</strong>{dia.esLectivo && <span className="flex items-center gap-1"><Badge className="h-6 border-primary/15 bg-primary/10 px-2 text-[10px] font-bold text-primary hover:bg-primary/10" variant={dia.origen === "cerrado" ? "destructive" : "secondary"}>{dia.semana ? `S${dia.semana}` : estado}</Badge><Ellipsis aria-label="Ver detalle del día" className="h-4 w-4 text-muted-foreground" /></span>}</div>
                  {dia.esLectivo && <>
                    <p className="mt-1.5 line-clamp-2 text-sm font-semibold leading-snug text-foreground">{dia.titulo ?? "Sin menú configurado"}</p>
                    <p className={`mt-2 flex items-center gap-1 text-xs ${dia.origen === "sustitucion" ? "font-semibold text-amber-700 dark:text-amber-300" : "text-muted-foreground"}`}>{dia.origen === "sustitucion" ? <Replace className="h-3 w-3" /> : dia.origen === "cerrado" ? <Ban className="h-3 w-3" /> : <CookingPot className="h-3 w-3" />}{dia.origen === "sustitucion" ? "Sustitución aplicada" : dia.semana ? `Semana del mes · ${dia.semana}` : estado}</p>
                  </>}
                </article>;
              })}
            </section>
          ))}
          </div>
          <div className="mt-3 flex flex-wrap gap-x-5 gap-y-2 border-t pt-3 text-xs text-muted-foreground"><span className="inline-flex items-center gap-1"><CookingPot className="h-3.5 w-3.5" /> Menú</span><span className="inline-flex items-center gap-1"><Replace className="h-3.5 w-3.5" /> Sustitución</span><span className="inline-flex items-center gap-1"><Ban className="h-3.5 w-3.5" /> Cierre institucional</span><span className="inline-flex items-center gap-1"><Ellipsis className="h-3.5 w-3.5" /> Ver detalle</span></div>
        </div>
      </div>
      <Dialog open={detalle !== null} onOpenChange={(abierto) => !abierto && setDetalle(null)}>
        <DialogContent className="max-h-[92dvh] overflow-y-auto sm:max-w-lg">
          <DialogHeader>
            <DialogTitle className="font-display text-xl">Menú del día</DialogTitle>
          </DialogHeader>
          {detalle && <div className="space-y-4">
            <div className={`rounded-2xl border p-5 sm:p-6 ${detalle.origen === "sustitucion" ? "border-amber-500/40 bg-amber-500/10" : detalle.origen === "cerrado" ? "border-destructive/30 bg-destructive/5" : "bg-muted/30"}`}>
              <div className="flex items-start justify-between gap-3"><div><p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">{fechaVisible(detalle.fecha)}</p><p className="mt-2 font-display text-2xl font-bold leading-tight text-foreground">{detalle.titulo ?? (detalle.habilitado ? "Sin menú configurado" : "No hay servicio de comedor")}</p><div className="mt-3 flex flex-wrap gap-2">{detalle.semana && <Badge variant="secondary">Semana del mes · {detalle.semana}</Badge>}{detalle.origen === "sustitucion" && <Badge className="border-amber-500/40 bg-amber-500/15 text-amber-800 hover:bg-amber-500/15 dark:text-amber-200">Sustitución aplicada</Badge>}</div></div><span className={`grid h-12 w-12 shrink-0 place-items-center rounded-2xl ${detalle.origen === "sustitucion" ? "bg-amber-500/20 text-amber-700 dark:text-amber-300" : detalle.origen === "cerrado" ? "bg-destructive/10 text-destructive" : "bg-primary/10 text-primary"}`}>{detalle.origen === "sustitucion" ? <Replace className="h-6 w-6" /> : detalle.origen === "cerrado" ? <Ban className="h-6 w-6" /> : <CookingPot className="h-6 w-6" />}</span></div>
              {detalle.origen === "sustitucion" && <p className="mt-5 rounded-xl border border-amber-500/25 bg-background/70 p-3 text-sm leading-relaxed text-foreground">Este menú reemplaza temporalmente la plantilla PANEA para esta fecha.</p>}
              {detalle.motivo && <p className="mt-5 rounded-xl border bg-background/70 p-3 text-sm text-muted-foreground">{detalle.motivo}</p>}
            </div>
            {detalle.componentes.length > 0 && <div className="rounded-2xl border bg-card p-4 sm:p-5"><p className="flex items-center gap-2 text-xs font-bold uppercase tracking-wide text-muted-foreground"><ListChecks className="h-4 w-4" /> Preparación y acompañamientos</p><ul className="mt-4 grid gap-2 sm:grid-cols-2">{detalle.componentes.map((componente, indice) => <li key={`${componente}-${indice}`} className="flex items-center gap-3 rounded-xl border bg-muted/30 px-3 py-3 text-sm font-medium text-foreground"><span className="grid h-6 w-6 shrink-0 place-items-center rounded-full bg-primary/10 text-xs font-bold text-primary">{indice + 1}</span>{componente}</li>)}</ul></div>}
          </div>}
          <DialogFooter className="gap-2 sm:gap-0">
            {detalle?.habilitado && <Button variant="outline" className="gap-2 border-amber-500/40 text-amber-800 hover:bg-amber-500/10 dark:text-amber-200" onClick={() => { setSustitucion({ fecha: detalle.fecha, titulo: detalle.titulo ?? "", observaciones: "", componentes: detalle.componentes.map(componenteSustitucion) }); setDetalle(null); }}><Replace className="h-4 w-4" />{detalle.origen === "sustitucion" ? "Editar sustitución" : "Crear sustitución"}</Button>}
            {detalle && <Button variant={detalle.habilitado ? "destructive" : "default"} disabled={actualizar.isPending} onClick={() => actualizar.mutate({ fecha: detalle.fecha, habilitado: !detalle.habilitado })}>{detalle.habilitado ? "Cerrar servicio" : "Habilitar servicio"}</Button>}
          </DialogFooter>
        </DialogContent>
      </Dialog>
      <Dialog open={sustitucion !== null} onOpenChange={(abierto) => !abierto && setSustitucion(null)}>
        <DialogContent>
          <DialogHeader><DialogTitle>Sustitución de menú</DialogTitle></DialogHeader>
          {sustitucion && <div className="space-y-3">
            <div><Label>Fecha</Label><Input value={sustitucion.fecha} disabled /></div>
            <div><Label>Título</Label><Input value={sustitucion.titulo} onChange={(e) => setSustitucion({ ...sustitucion, titulo: e.target.value })} /></div>
            <div><Label>Observaciones</Label><Textarea value={sustitucion.observaciones} onChange={(e) => setSustitucion({ ...sustitucion, observaciones: e.target.value })} /></div>
            <div className="rounded-xl border bg-muted/20 p-3"><div className="mb-3 flex items-center justify-between gap-3"><div><Label>Componentes</Label><p className="mt-1 text-xs text-muted-foreground">Agregá cada preparación o acompañamiento por separado.</p></div><Button type="button" variant="outline" size="sm" className="gap-1" onClick={() => setSustitucion({ ...sustitucion, componentes: [...sustitucion.componentes, componenteSustitucion()] })}><Plus className="h-4 w-4" /> Agregar</Button></div><div className="space-y-2">{sustitucion.componentes.map((componente, indice) => <div key={componente.clave} className="flex items-center gap-2"><span className="grid h-8 w-8 shrink-0 place-items-center rounded-full bg-primary/10 text-xs font-bold text-primary">{indice + 1}</span><Input placeholder="Ej. Arroz blanco" value={componente.nombre} onChange={(evento) => setSustitucion({ ...sustitucion, componentes: sustitucion.componentes.map((actual, posicion) => posicion === indice ? { ...actual, nombre: evento.target.value } : actual) })} /><Button type="button" variant="ghost" size="icon" aria-label={`Eliminar componente ${indice + 1}`} disabled={sustitucion.componentes.length === 1} onClick={() => setSustitucion({ ...sustitucion, componentes: sustitucion.componentes.filter((_, posicion) => posicion !== indice) })}><Trash2 className="h-4 w-4 text-destructive" /></Button></div>)}</div></div>
          </div>}
          <DialogFooter><Button variant="outline" onClick={() => setSustitucion(null)}>Cancelar</Button><Button disabled={!sustitucion?.titulo || !sustitucion.componentes.some((componente) => componente.nombre.trim()) || guardarSustitucion.isPending} onClick={() => sustitucion && guardarSustitucion.mutate(sustitucion)}>Guardar sustitución</Button></DialogFooter>
        </DialogContent>
      </Dialog>
    </section>
  );
}
