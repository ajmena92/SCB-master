import { useEffect, useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { FilePdf, Key, MagnifyingGlass, X } from "@phosphor-icons/react";
import { useNavigate } from "react-router-dom";
import { AlertDialog, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle } from "@/components/ui/alert-dialog";
import { Dialog, DialogBody, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import type { CredencialTemporal, Persona } from "@/compartido/contratos/plataforma";
import { errMsg } from "@/compartido/consultas/errores_api";
import DialogoCredencialTemporal from "../componentes/DialogoCredencialTemporal";
import ListadoPersonas, { type OrdenPersonas } from "../componentes/ListadoPersonas";
import { Aviso, Campo, EncabezadoPagina, EstadoCarga } from "../componentes/ElementosComunes";
import { plataformaApi } from "../consultas/plataforma";

type EstadoFiltro = "activos" | "inactivos" | "todos";
type ConfirmacionPin =
  | { tipo: "individual"; persona: Persona }
  | { tipo: "seccion"; anioLectivoId: number; seccion: string }
  | undefined;
const TAMANO_PAGINA = 25;

export default function PersonasMatriculas() {
  const cliente = useQueryClient();
  const navegar = useNavigate();
  const [buscar, setBuscar] = useState("");
  const [buscarAplicado, setBuscarAplicado] = useState("");
  const [estado, setEstado] = useState<EstadoFiltro>("activos");
  const [tipo, setTipo] = useState<"" | "estudiante" | "profesor">("");
  const [pagina, setPagina] = useState(1);
  const [ordenarPor, setOrdenarPor] = useState<OrdenPersonas>("nombres");
  const [direccion, setDireccion] = useState<"asc" | "desc">("asc");
  const [anioLectivoIdPines, setAnioLectivoIdPines] = useState<number>();
  const [seccionPines, setSeccionPines] = useState("");
  const [credenciales, setCredenciales] = useState<CredencialTemporal[]>();
  const [reportePines, setReportePines] = useState<{ anio: number; seccion: string }>();
  const [operacionesPinAbiertas, setOperacionesPinAbiertas] = useState(false);
  const [mensaje, setMensaje] = useState<string>();
  const [confirmacionPin, setConfirmacionPin] = useState<ConfirmacionPin>();

  const personas = useQuery({
    queryKey: ["personas", buscarAplicado, estado, tipo, pagina, ordenarPor, direccion],
    queryFn: () => plataformaApi.personas.listar({ buscar: buscarAplicado, estado, tipo: tipo || undefined, pagina, tamano: TAMANO_PAGINA, ordenar_por: ordenarPor, direccion }),
  });
  const resumen = useQuery({ queryKey: ["personas", "resumen"], queryFn: plataformaApi.personas.resumen });
  const anios = useQuery({ queryKey: ["anios"], queryFn: plataformaApi.anios.listar });
  const secciones = useQuery({
    queryKey: ["anios", anioLectivoIdPines, "secciones"],
    queryFn: () => plataformaApi.anios.secciones(anioLectivoIdPines!),
    enabled: Boolean(anioLectivoIdPines),
  });
  const resumenPines = useQuery({
    queryKey: ["anios", anioLectivoIdPines, "seccion", seccionPines, "resumen-pines"],
    queryFn: () => plataformaApi.anios.resumenPinesSeccion(anioLectivoIdPines!, seccionPines),
    enabled: Boolean(anioLectivoIdPines && seccionPines),
  });
  const invalidarPersonas = () => {
    cliente.invalidateQueries({ queryKey: ["personas"] });
  };

  useEffect(() => {
    if (anioLectivoIdPines || !anios.data?.elementos.length) return;
    setAnioLectivoIdPines(anios.data.elementos.find((anio) => anio.vigente)?.id ?? anios.data.elementos[0].id);
  }, [anioLectivoIdPines, anios.data]);

  useEffect(() => {
    const espera = window.setTimeout(() => setBuscarAplicado(buscar), 250);
    return () => window.clearTimeout(espera);
  }, [buscar]);

  const reiniciarPin = useMutation({
    mutationFn: plataformaApi.personas.reiniciarPin,
    onSuccess: (credencial) => {
      setConfirmacionPin(undefined);
      setCredenciales([credencial]);
    },
  });
  const desactivar = useMutation({
    mutationFn: plataformaApi.personas.desactivar,
    onSuccess: () => {
      setMensaje("La persona quedó inactiva y sus sesiones fueron revocadas.");
      invalidarPersonas();
    },
  });
  const reiniciarGrupo = useMutation({
    mutationFn: plataformaApi.personas.reiniciarPinesSeccion,
    onSuccess: (resultado) => {
      setConfirmacionPin(undefined);
      setCredenciales(resultado);
      const anio = anios.data?.elementos.find((item) => item.id === anioLectivoIdPines)?.anio;
      setReportePines(anio ? { anio, seccion: seccionPines } : undefined);
    },
  });

  function enviarGrupo(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    const datos = new FormData(evento.currentTarget);
    const seccion = String(datos.get("seccionPines") || "").trim();
    const anioLectivoId = Number(datos.get("anioLectivoIdPines"));
    if (!seccion || !anioLectivoId) return;
    setOperacionesPinAbiertas(false);
    setConfirmacionPin({ tipo: "seccion", anioLectivoId, seccion });
  }

  const error = reiniciarPin.error || desactivar.error || reiniciarGrupo.error || personas.error || resumen.error || secciones.error || resumenPines.error;
  const elementos = personas.data?.elementos ?? [];
  const filtrosActivos = buscar.length > 0 || estado !== "activos" || tipo !== "";
  const limpiarFiltros = () => {
    setBuscar("");
    setBuscarAplicado("");
    setEstado("activos");
    setTipo("");
    setPagina(1);
  };
  const cambiarOrden = (columna: OrdenPersonas) => {
    if (ordenarPor === columna) setDireccion((valor) => valor === "asc" ? "desc" : "asc");
    else {
      setOrdenarPor(columna);
      setDireccion("asc");
    }
    setPagina(1);
  };
  const confirmarReinicioPin = () => {
    if (!confirmacionPin) return;
    if (confirmacionPin.tipo === "individual") reiniciarPin.mutate(confirmacionPin.persona.id);
    else reiniciarGrupo.mutate({ anioLectivoId: confirmacionPin.anioLectivoId, seccion: confirmacionPin.seccion });
  };
  const reiniciandoPin = reiniciarPin.isPending || reiniciarGrupo.isPending;
  return (
    <section>
      <DialogoCredencialTemporal credenciales={credenciales} reportePines={reportePines} alCerrar={() => { setCredenciales(undefined); setReportePines(undefined); }} />
      <AlertDialog open={Boolean(confirmacionPin)} onOpenChange={(abierto) => !abierto && !reiniciandoPin && setConfirmacionPin(undefined)}>
        <AlertDialogContent aria-busy={reiniciandoPin}>
          <AlertDialogHeader>
            <AlertDialogTitle>{reiniciandoPin ? "Generando credenciales" : "¿Reiniciar el PIN?"}</AlertDialogTitle>
            <AlertDialogDescription>
              {reiniciandoPin
                ? confirmacionPin?.tipo === "seccion"
                  ? `Estamos reiniciando y preparando la lista de PIN para ${resumenPines.data?.estudiantesActivos ?? 0} estudiantes de la sección ${confirmacionPin.seccion}.`
                  : "Estamos generando el PIN temporal."
                : confirmacionPin?.tipo === "individual"
                ? `Se generará un PIN temporal nuevo para ${confirmacionPin.persona.nombres}. El anterior dejará de funcionar.`
                : `Se generará un PIN temporal nuevo para ${resumenPines.data?.estudiantesActivos ?? 0} estudiantes activos de la sección ${confirmacionPin?.seccion ?? ""}. Los PIN anteriores dejarán de funcionar.`}
            </AlertDialogDescription>
          </AlertDialogHeader>
          {reiniciandoPin ? <div className="pin-processing" role="status"><span aria-hidden="true" className="pin-processing-spinner" /><span>Espere un momento. No cierre esta ventana.</span></div> : <AlertDialogFooter>
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <button className="button primary" type="button" onClick={confirmarReinicioPin}>Reiniciar PIN</button>
          </AlertDialogFooter>}
        </AlertDialogContent>
      </AlertDialog>
      <EncabezadoPagina
        titulo="Estudiantes / PIN"
        descripcion="Consulte las personas importadas. Para estudiantes, gestione únicamente la beca de comedor y la ruta de transporte."
      />
      {error && <Aviso tipo="error">{errMsg(error)}</Aviso>}
      {mensaje && <Aviso tipo="exito">{mensaje}</Aviso>}
      {resumen.data && <div className="mb-4 flex flex-wrap gap-2" aria-label="Resumen del curso lectivo vigente">
        <span className="inline-flex items-baseline gap-2 rounded-lg bg-muted px-3 py-2 text-sm text-muted-foreground"><strong className="text-lg font-semibold leading-none text-foreground">{resumen.data.estudiantesActivos}</strong> Estudiantes activos</span>
        <span className="inline-flex items-baseline gap-2 rounded-lg bg-muted px-3 py-2 text-sm text-muted-foreground"><strong className="text-lg font-semibold leading-none text-foreground">{resumen.data.estudiantesInactivos}</strong> Estudiantes inactivos</span>
      </div>}
      <form className="mb-6 grid grid-cols-1 items-end gap-3 sm:grid-cols-2 lg:grid-cols-[minmax(18rem,31rem)_repeat(2,minmax(10.5rem,14rem))_auto]" role="search" onSubmit={(evento) => evento.preventDefault()}>
            <Campo etiqueta="Buscar">
              <span className="flex min-h-11 items-center gap-2 rounded-lg border border-input bg-card px-3 text-muted-foreground focus-within:border-primary focus-within:ring-2 focus-within:ring-primary/20"><MagnifyingGlass aria-hidden="true" size={18} /><input className="min-h-0 w-full border-0 bg-transparent px-1 shadow-none outline-none focus:ring-0" value={buscar} onChange={(e) => { setBuscar(e.target.value); setPagina(1); }} placeholder="Cédula o nombre" /></span>
            </Campo>
            <Campo etiqueta="Estado"><select value={estado} onChange={(e) => { setEstado(e.target.value as EstadoFiltro); setPagina(1); }}><option value="activos">Activos</option><option value="inactivos">Inactivos</option><option value="todos">Todos</option></select></Campo>
            <Campo etiqueta="Tipo"><select value={tipo} onChange={(e) => { setTipo(e.target.value as "" | "estudiante" | "profesor"); setPagina(1); }}><option value="">Todos</option><option value="estudiante">Estudiantes</option><option value="profesor">Profesores</option></select></Campo>
            <button className="button warning pin-operations-trigger self-end whitespace-nowrap" type="button" onClick={() => setOperacionesPinAbiertas(true)}><Key aria-hidden="true" size={18} /> Operaciones PIN</button>
            {filtrosActivos && <button className="button link clear-filters self-end" type="button" onClick={limpiarFiltros}><X aria-hidden="true" size={16} /> Limpiar filtros</button>}
      </form>
      <Dialog open={operacionesPinAbiertas} onOpenChange={setOperacionesPinAbiertas}>
        <DialogContent className="pin-operations-dialog sm:max-w-xl">
          <DialogHeader>
            <DialogTitle>Operaciones PIN por sección</DialogTitle>
            <DialogDescription>Genere nuevas credenciales temporales y prepare el reporte para entregar los PIN de una sección.</DialogDescription>
          </DialogHeader>
          <DialogBody>
            <form id="operaciones-pin" className="pin-operations-form" onSubmit={enviarGrupo}>
              <Campo etiqueta="Año lectivo"><select name="anioLectivoIdPines" required value={anioLectivoIdPines ?? ""} onChange={(evento) => { setAnioLectivoIdPines(Number(evento.target.value) || undefined); setSeccionPines(""); }}>{anios.data?.elementos.map((anio) => <option key={anio.id} value={anio.id}>{anio.anio}{anio.vigente ? " (vigente)" : ""}</option>)}</select></Campo>
              <Campo etiqueta="Sección"><select name="seccionPines" required value={seccionPines} disabled={!anioLectivoIdPines || secciones.isLoading} onChange={(evento) => setSeccionPines(evento.target.value)}><option value="">{secciones.isLoading ? "Cargando secciones…" : "Seleccione una sección"}</option>{secciones.data?.elementos.map((seccion) => <option key={seccion} value={seccion}>{seccion}</option>)}</select></Campo>
              <div className="pin-section-summary" aria-live="polite"><FilePdf aria-hidden="true" size={19} /><span>{seccionPines ? resumenPines.isLoading ? "Verificando sección…" : `${resumenPines.data?.estudiantesActivos ?? 0} estudiantes activos recibirán un PIN nuevo` : "Seleccione una sección para verificar el grupo"}</span></div>
              <p className="pin-warning" role="note"><strong>Atención:</strong> al continuar se reiniciarán los PIN de todos los estudiantes activos de esta sección. Los PIN anteriores dejarán de funcionar.</p>
            </form>
          </DialogBody>
          <DialogFooter>
            <button className="button secondary" type="button" onClick={() => setOperacionesPinAbiertas(false)}>Cancelar</button>
            <button className="button primary" type="submit" form="operaciones-pin" disabled={reiniciarGrupo.isPending || !seccionPines || !resumenPines.data?.estudiantesActivos}>Continuar</button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
      {personas.isLoading ? <EstadoCarga /> : <ListadoPersonas personas={elementos} total={personas.data?.total ?? 0} pagina={pagina} tamano={TAMANO_PAGINA} ordenarPor={ordenarPor} direccion={direccion} alCambiarPagina={setPagina} alOrdenar={cambiarOrden} alEditar={(persona) => navegar(`/admin/panel/estudiantes/expediente/${persona.referenciaPublica}`, { state: { persona } })} alReiniciarPin={(persona) => setConfirmacionPin({ tipo: "individual", persona })} vacio={filtrosActivos ? "No hay personas que coincidan con los filtros actuales." : "No hay personas importadas para mostrar."} />}
    </section>
  );
}
