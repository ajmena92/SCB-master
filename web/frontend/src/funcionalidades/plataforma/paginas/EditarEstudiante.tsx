import { useState, type FormEvent, type ReactNode } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ArrowLeft, BowlFood, Bus, CaretRight, FloppyDisk, IdentificationCard, Key, Ticket, Warning } from "@phosphor-icons/react";
import { useLocation, useNavigate, useParams } from "react-router-dom";
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle } from "@/components/ui/alert-dialog";
import type { CredencialTemporal, Persona } from "@/compartido/contratos/plataforma";
import { errMsg } from "@/compartido/consultas/errores_api";
import DialogoCredencialTemporal from "../componentes/DialogoCredencialTemporal";
import FotoEstudiante from "../componentes/FotoEstudiante";
import { Aviso, Campo, EstadoCarga } from "../componentes/ElementosComunes";
import { plataformaApi } from "../consultas/plataforma";

type Confirmacion = "pin" | "desactivar" | undefined;

function EstadoBeneficio({ icono, etiqueta, valor, gestionar }: { icono: ReactNode; etiqueta: string; valor: ReactNode; gestionar?: string }) {
  return <div className="flex items-center gap-3 border-b border-border py-3 last:border-b-0"><span className="grid size-9 shrink-0 place-items-center rounded-lg bg-primary/10 text-primary" aria-hidden="true">{icono}</span><div className="min-w-0 flex-1"><small className="block text-xs text-muted-foreground">{etiqueta}</small><p className="truncate text-sm font-semibold text-foreground">{valor}</p></div>{gestionar && <a className="text-xs font-semibold text-primary hover:underline" href={gestionar}>Gestionar</a>}</div>;
}

export default function EditarEstudiante() {
  const { referencia } = useParams();
  const personaNavegacion = (useLocation().state as { persona?: Persona } | null)?.persona;
  const personaInicial = personaNavegacion?.referenciaPublica === referencia ? personaNavegacion : undefined;
  const consultaPersona = useQuery({
    queryKey: ["personas", "referencia", referencia],
    queryFn: () => plataformaApi.personas.obtenerPorReferencia(referencia!),
    enabled: Boolean(referencia && !personaInicial),
  });
  const persona = personaInicial ?? consultaPersona.data;
  const navegar = useNavigate();
  const cliente = useQueryClient();
  const [confirmacion, setConfirmacion] = useState<Confirmacion>();
  const [credenciales, setCredenciales] = useState<CredencialTemporal[]>();
  const [cambiosPendientes, setCambiosPendientes] = useState(false);
  const rutas = useQuery({ queryKey: ["rutas"], queryFn: plataformaApi.rutas.listar, enabled: Boolean(persona) });
  const guardar = useMutation({
    mutationFn: async (formulario: FormData) => {
      if (!persona?.matriculaId) return;
      const ruta = String(formulario.get("rutaId") || "");
      await plataformaApi.matriculas.actualizarBeneficios(persona.matriculaId, {
        becado: formulario.get("becado") === "on", rutaId: ruta ? Number(ruta) : null,
      });
    },
    onSuccess: () => { setCambiosPendientes(false); cliente.invalidateQueries({ queryKey: ["personas"] }); },
  });
  const reiniciarPin = useMutation({ mutationFn: () => plataformaApi.personas.reiniciarPin(persona!.id), onSuccess: (credencial) => { setConfirmacion(undefined); setCredenciales([credencial]); } });
  const desactivar = useMutation({ mutationFn: () => plataformaApi.personas.desactivar(persona!.id), onSuccess: () => { cliente.invalidateQueries({ queryKey: ["personas"] }); navegar("/admin/panel/personas", { replace: true }); } });

  if (consultaPersona.isLoading) return <section className="grid gap-4"><EstadoCarga /></section>;
  if (!referencia || !persona || persona.tipo !== "estudiante") return <section className="grid gap-4"><Aviso tipo="error">No se encontró el estudiante solicitado. Vuelva al padrón y selecciónelo de nuevo.</Aviso><button className="button secondary" type="button" onClick={() => navegar("/admin/panel/personas")}>Volver a estudiantes</button></section>;

  const error = consultaPersona.error || guardar.error || reiniciarPin.error || desactivar.error || rutas.error;
  const ocupada = reiniciarPin.isPending || desactivar.isPending;
  const rutaActual = persona.descripcionRuta ?? "Sin ruta asignada";
  return <section className="mx-auto grid max-w-6xl gap-4 pb-8">
    <DialogoCredencialTemporal credenciales={credenciales} alCerrar={() => setCredenciales(undefined)} />
    <nav className="flex items-center gap-2 text-sm text-muted-foreground" aria-label="Ubicación actual">
      <button className="button link" type="button" onClick={() => navegar("/admin/panel/personas")}><ArrowLeft aria-hidden="true" size={17} /> Estudiantes / PIN</button>
      <CaretRight aria-hidden="true" size={14} />
      <span>Expediente</span>
    </nav>
    <header className="grid gap-2 border-b border-border pb-4"><div><p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">Expediente del estudiante</p><h1 className="font-heading text-2xl font-semibold text-foreground sm:text-3xl">{persona.nombres}</h1><div className="mt-2 flex flex-wrap items-center gap-3 text-sm text-muted-foreground"><span className="inline-flex items-center gap-1"><IdentificationCard aria-hidden="true" size={17} /> {persona.cedula ?? "Cédula no registrada"}</span><span className="inline-flex items-center gap-1"><Bus aria-hidden="true" size={17} /> {persona.seccion ?? "Sin sección"}</span><span className={persona.activo ? "rounded-full bg-success/15 px-2.5 py-1 text-xs font-semibold text-success" : "rounded-full bg-destructive/15 px-2.5 py-1 text-xs font-semibold text-destructive"}>{persona.activo ? "Activo" : "Inactivo"}</span></div></div></header>
    {error && <Aviso tipo="error">{errMsg(error)}</Aviso>}{guardar.isSuccess && <Aviso tipo="exito">Los beneficios se actualizaron.</Aviso>}
    <form id="formulario-expediente" className="grid gap-5" onInput={() => setCambiosPendientes(true)} onSubmit={(evento: FormEvent<HTMLFormElement>) => { evento.preventDefault(); guardar.mutate(new FormData(evento.currentTarget)); }}>
      <div className="grid gap-5 lg:grid-cols-[minmax(17rem,22rem)_minmax(0,1fr)]">
        <aside className="grid content-start gap-4"><FotoEstudiante personaId={persona.id} nombre={persona.nombres} /><section className="rounded-xl border border-border bg-card p-4 shadow-sm" aria-label="Resumen de la matrícula vigente"><header className="mb-2 flex items-start justify-between gap-3"><div><p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">Matrícula vigente</p><h2 className="font-heading text-lg font-semibold">Resumen operativo</h2></div><span className="rounded-full bg-muted px-2 py-1 text-xs text-muted-foreground">{persona.seccion ?? "Sin sección"}</span></header><EstadoBeneficio icono={<BowlFood size={20} />} etiqueta="Comedor" valor={persona.becado ? "Beneficiario" : "No beneficiario"} gestionar="#beca-comedor" /><EstadoBeneficio icono={<Bus size={20} />} etiqueta="Transporte" valor={rutaActual} gestionar="#ruta-transporte" /><EstadoBeneficio icono={<Ticket size={20} />} etiqueta="Saldo de tiquetes" valor={<><strong>{persona.saldoTiquetes ?? 0}</strong><span> tiquetes disponibles</span></>} /></section></aside>
        <div className="grid content-start gap-5">
          <section className="grid gap-4 rounded-xl border border-border bg-card p-4 shadow-sm"><div className="flex flex-wrap items-start justify-between gap-3"><div><p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">Importado del padrón</p><h2 className="font-heading text-xl font-semibold">Datos del estudiante</h2></div><span className="rounded-full bg-muted px-2 py-1 text-xs text-muted-foreground" aria-label="Datos bloqueados">🔒 Solo lectura</span></div><p className="text-sm text-muted-foreground">Estos datos los administra el padrón anual. Para corregirlos, actualice el padrón y vuelva a importarlo.</p><div className="grid gap-4 sm:grid-cols-2"><Campo etiqueta="Cédula"><input value={persona.cedula ?? ""} disabled /></Campo><Campo etiqueta="Nombre completo"><input value={persona.nombres} disabled /></Campo><Campo etiqueta="Sección"><input value={persona.seccion ?? "Sin sección"} disabled /></Campo></div></section>
          <section id="beneficios" className="grid gap-4 rounded-xl border border-border bg-card p-4 shadow-sm"><div className="flex flex-wrap items-start justify-between gap-3"><div><p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">Administrable en SCB</p><h2 className="font-heading text-xl font-semibold">Beneficios de la matrícula</h2></div><span className="rounded-full bg-primary/10 px-2 py-1 text-xs font-semibold text-primary">Curso vigente</span></div><p className="text-sm text-muted-foreground">Los cambios se aplican únicamente a la matrícula anual activa.</p>{rutas.isLoading ? <EstadoCarga /> : <div className="grid gap-4"><Campo etiqueta="Ruta de transporte"><select id="ruta-transporte" name="rutaId" defaultValue={persona.rutaId ?? ""} disabled={!persona.activo}><option value="">No utiliza transporte</option>{rutas.data?.elementos.filter((ruta) => ruta.activo && ruta.codigo !== "0000").map((ruta) => <option key={ruta.idRuta} value={ruta.idRuta}>{ruta.codigo} — {ruta.descripcion}</option>)}</select></Campo><label id="beca-comedor" className="flex cursor-pointer items-center justify-between gap-4 rounded-lg border border-border bg-muted/40 p-3"><span><b className="block text-sm font-semibold">Beca de comedor</b><small className="text-sm text-muted-foreground">Aplica a la beca completa de cinco días.</small></span><span className="relative inline-flex shrink-0"><input className="peer sr-only" name="becado" type="checkbox" role="switch" defaultChecked={persona.becado} disabled={!persona.activo} aria-label="Asignar beca de comedor" /><span aria-hidden="true" className="h-6 w-11 rounded-full bg-border transition peer-checked:bg-primary peer-focus-visible:ring-2 peer-focus-visible:ring-primary/30 after:absolute after:left-1 after:top-1 after:size-4 after:rounded-full after:bg-white after:transition peer-checked:after:translate-x-5" /></span></label></div>}</section>
        </div>
      </div>
    </form>
    <footer className="sticky bottom-3 z-10 flex flex-wrap items-center justify-between gap-3 rounded-xl border border-border bg-card/95 p-3 shadow-lg backdrop-blur" aria-label="Acciones del expediente"><p className="text-sm text-muted-foreground" aria-live="polite">{guardar.isPending ? "Guardando cambios…" : cambiosPendientes ? "Cambios sin guardar" : "Sin cambios pendientes"}</p><div className="flex flex-wrap gap-2"><button className="button warning" type="button" onClick={() => setConfirmacion("pin")} disabled={!persona.activo || ocupada}><Key aria-hidden="true" size={18} /> Reiniciar PIN</button>{persona.activo && <button className="button danger" type="button" onClick={() => setConfirmacion("desactivar")} disabled={ocupada}>Desactivar</button>}<button form="formulario-expediente" className="button primary" disabled={!persona.activo || guardar.isPending || rutas.isLoading || !cambiosPendientes}><FloppyDisk aria-hidden="true" size={18} /> Guardar cambios</button></div></footer>
    <AlertDialog open={Boolean(confirmacion)} onOpenChange={(abierto) => !abierto && setConfirmacion(undefined)}><AlertDialogContent><AlertDialogHeader><AlertDialogTitle>{confirmacion === "pin" ? "¿Reiniciar el PIN?" : "¿Desactivar estudiante?"}</AlertDialogTitle><AlertDialogDescription>{confirmacion === "pin" ? `Se generará un PIN temporal nuevo para ${persona.nombres}. El anterior dejará de funcionar.` : `Se desactivará a ${persona.nombres}, se revocarán sus sesiones y no podrá utilizar el sistema.`}</AlertDialogDescription></AlertDialogHeader><AlertDialogFooter><AlertDialogCancel disabled={ocupada}>Cancelar</AlertDialogCancel><AlertDialogAction className={confirmacion === "desactivar" ? "bg-destructive text-destructive-foreground hover:bg-destructive/90" : undefined} disabled={ocupada} onClick={() => confirmacion === "pin" ? reiniciarPin.mutate() : desactivar.mutate()}>{confirmacion === "desactivar" && <Warning aria-hidden="true" size={18} />}{confirmacion === "pin" ? "Reiniciar PIN" : "Desactivar estudiante"}</AlertDialogAction></AlertDialogFooter></AlertDialogContent></AlertDialog>
  </section>;
}
