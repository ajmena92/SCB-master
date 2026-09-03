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
  return <div className="student-editor-benefit"><span aria-hidden="true">{icono}</span><div><small>{etiqueta}</small><p>{valor}</p></div>{gestionar && <a href={gestionar}>Gestionar</a>}</div>;
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

  if (consultaPersona.isLoading) return <section className="stack"><EstadoCarga /></section>;
  if (!referencia || !persona || persona.tipo !== "estudiante") return <section className="stack"><Aviso tipo="error">No se encontró el estudiante solicitado. Vuelva al padrón y selecciónelo de nuevo.</Aviso><button className="button secondary" type="button" onClick={() => navegar("/admin/panel/personas")}>Volver a estudiantes</button></section>;

  const error = consultaPersona.error || guardar.error || reiniciarPin.error || desactivar.error || rutas.error;
  const ocupada = reiniciarPin.isPending || desactivar.isPending;
  const rutaActual = persona.descripcionRuta ?? "Sin ruta asignada";
  return <section className="student-editor-page">
    <DialogoCredencialTemporal credenciales={credenciales} alCerrar={() => setCredenciales(undefined)} />
    <nav className="edit-breadcrumb" aria-label="Ubicación actual">
      <button className="button link" type="button" onClick={() => navegar("/admin/panel/personas")}><ArrowLeft aria-hidden="true" size={17} /> Estudiantes / PIN</button>
      <CaretRight aria-hidden="true" size={14} />
      <span>Expediente</span>
    </nav>
    <header className="student-editor-heading"><div><p className="eyebrow">Expediente del estudiante</p><h1>{persona.nombres}</h1><div className="student-editor-meta"><span><IdentificationCard aria-hidden="true" size={17} /> {persona.cedula ?? "Cédula no registrada"}</span><span><Bus aria-hidden="true" size={17} /> {persona.seccion ?? "Sin sección"}</span><span className={persona.activo ? "person-tag person-tag--exito" : "person-tag person-tag--alerta"}>{persona.activo ? "Activo" : "Inactivo"}</span></div></div></header>
    {error && <Aviso tipo="error">{errMsg(error)}</Aviso>}{guardar.isSuccess && <Aviso tipo="exito">Los beneficios se actualizaron.</Aviso>}
    <form id="formulario-expediente" className="student-editor-form" onInput={() => setCambiosPendientes(true)} onSubmit={(evento: FormEvent<HTMLFormElement>) => { evento.preventDefault(); guardar.mutate(new FormData(evento.currentTarget)); }}>
      <div className="student-editor-layout">
        <aside className="student-editor-aside"><FotoEstudiante personaId={persona.id} nombre={persona.nombres} /><section className="student-editor-summary" aria-label="Resumen de la matrícula vigente"><header><div><p className="eyebrow">Matrícula vigente</p><h2>Resumen operativo</h2></div><span>{persona.seccion ?? "Sin sección"}</span></header><EstadoBeneficio icono={<BowlFood size={20} />} etiqueta="Comedor" valor={persona.becado ? "Beneficiario" : "No beneficiario"} gestionar="#beca-comedor" /><EstadoBeneficio icono={<Bus size={20} />} etiqueta="Transporte" valor={rutaActual} gestionar="#ruta-transporte" /><EstadoBeneficio icono={<Ticket size={20} />} etiqueta="Saldo de tiquetes" valor={<><strong>{persona.saldoTiquetes ?? 0}</strong><span className="student-editor-ticket-label"> tiquetes disponibles</span></>} /></section></aside>
        <div className="student-editor-content">
          <section className="expediente-section student-editor-section student-editor-readonly"><div className="student-editor-section-heading"><div><p className="eyebrow">Importado del padrón</p><h2>Datos del estudiante</h2></div><span aria-label="Datos bloqueados">🔒 Solo lectura</span></div><p className="section-help">Estos datos los administra el padrón anual. Para corregirlos, actualice el padrón y vuelva a importarlo.</p><div className="form-grid"><Campo etiqueta="Cédula"><input value={persona.cedula ?? ""} disabled /></Campo><Campo etiqueta="Nombre completo"><input value={persona.nombres} disabled /></Campo><Campo etiqueta="Sección"><input value={persona.seccion ?? "Sin sección"} disabled /></Campo></div></section>
          <section id="beneficios" className="expediente-section expediente-beneficios student-editor-section"><div className="student-editor-section-heading"><div><p className="eyebrow">Administrable en SCB</p><h2>Beneficios de la matrícula</h2></div><span>Curso vigente</span></div><p className="section-help">Los cambios se aplican únicamente a la matrícula anual activa.</p>{rutas.isLoading ? <EstadoCarga /> : <div className="form-grid"><Campo etiqueta="Ruta de transporte"><select id="ruta-transporte" name="rutaId" defaultValue={persona.rutaId ?? ""} disabled={!persona.activo}><option value="">No utiliza transporte</option>{rutas.data?.elementos.filter((ruta) => ruta.activo && ruta.codigo !== "0000").map((ruta) => <option key={ruta.idRuta} value={ruta.idRuta}>{ruta.codigo} — {ruta.descripcion}</option>)}</select></Campo><label id="beca-comedor" className="student-editor-toggle"><span><b>Beca de comedor</b><small>Aplica a la beca completa de cinco días.</small></span><span className="student-editor-toggle-control"><input name="becado" type="checkbox" role="switch" defaultChecked={persona.becado} disabled={!persona.activo} aria-label="Asignar beca de comedor" /><span aria-hidden="true" /></span></label></div>}</section>
        </div>
      </div>
    </form>
    <footer className="student-editor-actions" aria-label="Acciones del expediente"><p aria-live="polite">{guardar.isPending ? "Guardando cambios…" : cambiosPendientes ? "Cambios sin guardar" : "Sin cambios pendientes"}</p><div><button className="button warning" type="button" onClick={() => setConfirmacion("pin")} disabled={!persona.activo || ocupada}><Key aria-hidden="true" size={18} /> Reiniciar PIN</button>{persona.activo && <button className="button danger" type="button" onClick={() => setConfirmacion("desactivar")} disabled={ocupada}>Desactivar</button>}<button form="formulario-expediente" className="button primary" disabled={!persona.activo || guardar.isPending || rutas.isLoading || !cambiosPendientes}><FloppyDisk aria-hidden="true" size={18} /> Guardar cambios</button></div></footer>
    <AlertDialog open={Boolean(confirmacion)} onOpenChange={(abierto) => !abierto && setConfirmacion(undefined)}><AlertDialogContent><AlertDialogHeader><AlertDialogTitle>{confirmacion === "pin" ? "¿Reiniciar el PIN?" : "¿Desactivar estudiante?"}</AlertDialogTitle><AlertDialogDescription>{confirmacion === "pin" ? `Se generará un PIN temporal nuevo para ${persona.nombres}. El anterior dejará de funcionar.` : `Se desactivará a ${persona.nombres}, se revocarán sus sesiones y no podrá utilizar el sistema.`}</AlertDialogDescription></AlertDialogHeader><AlertDialogFooter><AlertDialogCancel disabled={ocupada}>Cancelar</AlertDialogCancel><AlertDialogAction className={confirmacion === "desactivar" ? "bg-destructive text-destructive-foreground hover:bg-destructive/90" : undefined} disabled={ocupada} onClick={() => confirmacion === "pin" ? reiniciarPin.mutate() : desactivar.mutate()}>{confirmacion === "desactivar" && <Warning aria-hidden="true" size={18} />}{confirmacion === "pin" ? "Reiniciar PIN" : "Desactivar estudiante"}</AlertDialogAction></AlertDialogFooter></AlertDialogContent></AlertDialog>
  </section>;
}
