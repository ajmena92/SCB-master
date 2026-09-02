import { useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ArrowLeft, Key, Warning } from "@phosphor-icons/react";
import { useLocation, useNavigate, useParams } from "react-router-dom";
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle } from "@/components/ui/alert-dialog";
import type { CredencialTemporal, Persona } from "@/compartido/contratos/plataforma";
import { errMsg } from "@/compartido/consultas/errores_api";
import DialogoCredencialTemporal from "../componentes/DialogoCredencialTemporal";
import FotoEstudiante from "../componentes/FotoEstudiante";
import { Aviso, Campo, EstadoCarga } from "../componentes/ElementosComunes";
import { plataformaApi } from "../consultas/plataforma";

type Confirmacion = "pin" | "desactivar" | undefined;

export default function EditarEstudiante() {
  const { id } = useParams();
  const personaNavegacion = (useLocation().state as { persona?: Persona } | null)?.persona;
  const personaId = Number(id);
  const consultaPersona = useQuery({
    queryKey: ["personas", personaId],
    queryFn: () => plataformaApi.personas.obtener(personaId),
    enabled: Number.isInteger(personaId) && personaId > 0 && (!personaNavegacion || personaNavegacion.id !== personaId),
  });
  const persona = personaNavegacion?.id === personaId ? personaNavegacion : consultaPersona.data;
  const navegar = useNavigate();
  const cliente = useQueryClient();
  const [confirmacion, setConfirmacion] = useState<Confirmacion>();
  const [credenciales, setCredenciales] = useState<CredencialTemporal[]>();
  const rutas = useQuery({ queryKey: ["rutas"], queryFn: plataformaApi.rutas.listar, enabled: Boolean(persona) });
  const guardar = useMutation({ mutationFn: async (formulario: FormData) => {
    if (!persona?.matriculaId) return;
    const ruta = String(formulario.get("rutaId") || "");
    await plataformaApi.matriculas.actualizarBeneficios(persona.matriculaId, { becado: formulario.get("becado") === "on", rutaId: ruta ? Number(ruta) : null });
  }, onSuccess: () => cliente.invalidateQueries({ queryKey: ["personas"] }) });
  const reiniciarPin = useMutation({ mutationFn: () => plataformaApi.personas.reiniciarPin(persona!.id), onSuccess: (credencial) => { setConfirmacion(undefined); setCredenciales([credencial]); } });
  const desactivar = useMutation({ mutationFn: () => plataformaApi.personas.desactivar(persona!.id), onSuccess: () => { cliente.invalidateQueries({ queryKey: ["personas"] }); navegar("/admin/panel/personas", { replace: true }); } });

  if (consultaPersona.isLoading) return <section className="stack"><EstadoCarga /></section>;
  if (!id || !persona || String(persona.id) !== id || persona.tipo !== "estudiante") return <section className="stack"><Aviso tipo="error">No se encontró el estudiante solicitado. Vuelva al padrón y selecciónelo de nuevo.</Aviso><button className="button secondary" type="button" onClick={() => navegar("/admin/panel/personas")}>Volver a estudiantes</button></section>;
  const error = consultaPersona.error || guardar.error || reiniciarPin.error || desactivar.error || rutas.error;
  const ocupada = reiniciarPin.isPending || desactivar.isPending;
  return <section className="student-editor-page">
    <DialogoCredencialTemporal credenciales={credenciales} alCerrar={() => setCredenciales(undefined)} />
    <button className="button link student-editor-back" type="button" onClick={() => navegar("/admin/panel/personas")}><ArrowLeft aria-hidden="true" size={18} /> Volver a Estudiantes / PIN</button>
    <header className="student-editor-heading"><div><p className="eyebrow">Expediente del estudiante</p><h2>{persona.nombres}</h2><p>Los datos personales y académicos proceden del padrón anual. Aquí solo se administran beca de comedor, transporte, foto y PIN.</p></div><span className={persona.activo ? "person-tag person-tag--exito" : "person-tag person-tag--alerta"}>{persona.activo ? "Activo" : "Inactivo"}</span></header>
    {error && <Aviso tipo="error">{errMsg(error)}</Aviso>}{guardar.isSuccess && <Aviso tipo="exito">Beneficios actualizados.</Aviso>}
    <form className="student-editor-form" onSubmit={(evento: FormEvent<HTMLFormElement>) => { evento.preventDefault(); guardar.mutate(new FormData(evento.currentTarget)); }}>
      <div className="student-editor-content">
        <section className="expediente-section"><p className="eyebrow">Solo lectura</p><h2>Datos del padrón</h2><p className="section-help">Para corregir estos datos, actualice el padrón anual y vuelva a importarlo.</p><div className="form-grid"><Campo etiqueta="Cédula"><input value={persona.cedula ?? ""} readOnly /></Campo><Campo etiqueta="Nombre completo"><input value={persona.nombres} readOnly /></Campo><Campo etiqueta="Sección"><input value={persona.seccion ?? "Sin sección"} readOnly /></Campo></div></section>
        <FotoEstudiante personaId={persona.id} nombre={persona.nombres} />
        <section className="expediente-section expediente-beneficios"><p className="eyebrow">Administrable en SCB</p><h2>Beneficios</h2><p className="section-help">Los cambios se aplican a la matrícula anual activa.</p>{rutas.isLoading ? <EstadoCarga /> : <div className="form-grid"><Campo etiqueta="Ruta de transporte"><select name="rutaId" defaultValue={persona.rutaId ?? ""} disabled={!persona.activo}><option value="">No utiliza transporte</option>{rutas.data?.elementos.filter((ruta) => ruta.activo && ruta.codigo !== "0000").map((ruta) => <option key={ruta.idRuta} value={ruta.idRuta}>{ruta.codigo} — {ruta.descripcion}</option>)}</select></Campo><label className="check"><input name="becado" type="checkbox" defaultChecked={persona.becado} disabled={!persona.activo} /> Beneficiario de comedor</label></div>}</section>
        <Aviso>Saldo disponible: ₡{persona.saldoTiquetes ?? 0}. El beneficio de comedor aplica a la beca completa de cinco días.</Aviso>
      </div>
      <footer className="student-editor-actions"><button className="button secondary" type="button" onClick={() => setConfirmacion("pin")} disabled={!persona.activo || ocupada}><Key aria-hidden="true" size={18} /> Reiniciar PIN</button>{persona.activo && <button className="button danger" type="button" onClick={() => setConfirmacion("desactivar")} disabled={ocupada}>Desactivar</button>}<button className="button primary" disabled={!persona.activo || guardar.isPending || rutas.isLoading}>Guardar cambios</button></footer>
    </form>
    <AlertDialog open={Boolean(confirmacion)} onOpenChange={(abierto) => !abierto && setConfirmacion(undefined)}><AlertDialogContent><AlertDialogHeader><AlertDialogTitle>{confirmacion === "pin" ? "¿Reiniciar el PIN?" : "¿Desactivar estudiante?"}</AlertDialogTitle><AlertDialogDescription>{confirmacion === "pin" ? `Se generará un PIN temporal nuevo para ${persona.nombres}. El anterior dejará de funcionar.` : `Se desactivará a ${persona.nombres}, se revocarán sus sesiones y no podrá utilizar el sistema.`}</AlertDialogDescription></AlertDialogHeader><AlertDialogFooter><AlertDialogCancel disabled={ocupada}>Cancelar</AlertDialogCancel><AlertDialogAction className={confirmacion === "desactivar" ? "bg-destructive text-destructive-foreground hover:bg-destructive/90" : undefined} disabled={ocupada} onClick={() => confirmacion === "pin" ? reiniciarPin.mutate() : desactivar.mutate()}>{confirmacion === "desactivar" && <Warning aria-hidden="true" size={18} />}{confirmacion === "pin" ? "Reiniciar PIN" : "Desactivar estudiante"}</AlertDialogAction></AlertDialogFooter></AlertDialogContent></AlertDialog>
  </section>;
}
