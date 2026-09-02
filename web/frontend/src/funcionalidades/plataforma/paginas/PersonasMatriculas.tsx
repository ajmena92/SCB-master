import { useEffect, useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Key, MagnifyingGlass, X } from "@phosphor-icons/react";
import { useNavigate } from "react-router-dom";
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle } from "@/components/ui/alert-dialog";
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
  const [estado, setEstado] = useState<EstadoFiltro>("activos");
  const [tipo, setTipo] = useState<"" | "estudiante" | "profesor">("");
  const [pagina, setPagina] = useState(1);
  const [ordenarPor, setOrdenarPor] = useState<OrdenPersonas>("nombres");
  const [direccion, setDireccion] = useState<"asc" | "desc">("asc");
  const [anioLectivoIdPines, setAnioLectivoIdPines] = useState<number>();
  const [credenciales, setCredenciales] = useState<CredencialTemporal[]>();
  const [mensaje, setMensaje] = useState<string>();
  const [confirmacionPin, setConfirmacionPin] = useState<ConfirmacionPin>();

  const personas = useQuery({
    queryKey: ["personas", buscar, estado, tipo, pagina, ordenarPor, direccion],
    queryFn: () => plataformaApi.personas.listar({ buscar, estado, tipo: tipo || undefined, pagina, tamano: TAMANO_PAGINA, ordenar_por: ordenarPor, direccion }),
  });
  const resumen = useQuery({ queryKey: ["personas", "resumen"], queryFn: plataformaApi.personas.resumen });
  const anios = useQuery({ queryKey: ["anios"], queryFn: plataformaApi.anios.listar });
  const secciones = useQuery({
    queryKey: ["anios", anioLectivoIdPines, "secciones"],
    queryFn: () => plataformaApi.anios.secciones(anioLectivoIdPines!),
    enabled: Boolean(anioLectivoIdPines),
  });
  const invalidarPersonas = () => {
    cliente.invalidateQueries({ queryKey: ["personas"] });
  };

  useEffect(() => {
    if (anioLectivoIdPines || !anios.data?.elementos.length) return;
    setAnioLectivoIdPines(anios.data.elementos.find((anio) => anio.vigente)?.id ?? anios.data.elementos[0].id);
  }, [anioLectivoIdPines, anios.data]);

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
    },
  });

  function enviarGrupo(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    const datos = new FormData(evento.currentTarget);
    const seccion = String(datos.get("seccionPines") || "").trim();
    const anioLectivoId = Number(datos.get("anioLectivoIdPines"));
    if (!seccion || !anioLectivoId) return;
    setConfirmacionPin({ tipo: "seccion", anioLectivoId, seccion });
  }

  const error = reiniciarPin.error || desactivar.error || reiniciarGrupo.error || personas.error || resumen.error || secciones.error;
  const elementos = personas.data?.elementos ?? [];
  const filtrosActivos = buscar.length > 0 || estado !== "activos" || tipo !== "";
  const limpiarFiltros = () => {
    setBuscar("");
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
      <DialogoCredencialTemporal credenciales={credenciales} alCerrar={() => setCredenciales(undefined)} />
      <AlertDialog open={Boolean(confirmacionPin)} onOpenChange={(abierto) => !abierto && setConfirmacionPin(undefined)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>¿Reiniciar el PIN?</AlertDialogTitle>
            <AlertDialogDescription>
              {confirmacionPin?.tipo === "individual"
                ? `Se generará un PIN temporal nuevo para ${confirmacionPin.persona.nombres}. El anterior dejará de funcionar.`
                : `Se generará un PIN temporal nuevo para todos los estudiantes activos de la sección ${confirmacionPin?.seccion ?? ""}. Los PIN anteriores dejarán de funcionar.`}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel disabled={reiniciandoPin}>Cancelar</AlertDialogCancel>
            <AlertDialogAction disabled={reiniciandoPin} onClick={confirmarReinicioPin}>
              {reiniciandoPin ? "Reiniciando…" : "Reiniciar PIN"}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
      <EncabezadoPagina
        titulo="Estudiantes / PIN"
        descripcion="Consulte las personas importadas. Para estudiantes, gestione únicamente la beca de comedor y la ruta de transporte."
      />
      {error && <Aviso tipo="error">{errMsg(error)}</Aviso>}
      {mensaje && <Aviso tipo="exito">{mensaje}</Aviso>}
      {resumen.data && <div className="person-metrics" aria-label="Resumen del curso lectivo vigente">
        <span><strong>{resumen.data.estudiantesActivos}</strong> Estudiantes activos</span>
        <span><strong>{resumen.data.estudiantesInactivos}</strong> Estudiantes inactivos</span>
      </div>}
      <form className="filters person-filters" onSubmit={(evento) => evento.preventDefault()}>
            <Campo etiqueta="Buscar">
              <span className="search-input"><MagnifyingGlass aria-hidden="true" size={18} /><input value={buscar} onChange={(e) => { setBuscar(e.target.value); setPagina(1); }} placeholder="Cédula o nombre" /></span>
            </Campo>
            <Campo etiqueta="Estado"><select value={estado} onChange={(e) => { setEstado(e.target.value as EstadoFiltro); setPagina(1); }}><option value="activos">Activos</option><option value="inactivos">Inactivos</option><option value="todos">Todos</option></select></Campo>
            <Campo etiqueta="Tipo"><select value={tipo} onChange={(e) => { setTipo(e.target.value as "" | "estudiante" | "profesor"); setPagina(1); }}><option value="">Todos</option><option value="estudiante">Estudiantes</option><option value="profesor">Profesores</option></select></Campo>
            {filtrosActivos && <button className="button link clear-filters" type="button" onClick={limpiarFiltros}><X aria-hidden="true" size={16} /> Limpiar filtros</button>}
      </form>
      {personas.isLoading ? <EstadoCarga /> : <ListadoPersonas personas={elementos} total={personas.data?.total ?? 0} pagina={pagina} tamano={TAMANO_PAGINA} ordenarPor={ordenarPor} direccion={direccion} alCambiarPagina={setPagina} alOrdenar={cambiarOrden} alEditar={(persona) => navegar(`/admin/panel/estudiantes/${persona.id}`, { state: { persona } })} alReiniciarPin={(persona) => setConfirmacionPin({ tipo: "individual", persona })} vacio={filtrosActivos ? "No hay personas que coincidan con los filtros actuales." : "No hay personas importadas para mostrar."} />}
      <section className="action-panel pin-operations" aria-labelledby="operaciones-pin">
        <div className="panel-title"><Key aria-hidden="true" size={22} /><div><h2 id="operaciones-pin">Operaciones PIN</h2><p>Esta acción reinicia el PIN temporal de todos los estudiantes activos de la sección indicada.</p></div></div>
      <form className="form-grid" onSubmit={enviarGrupo}>
            <Campo etiqueta="Año lectivo"><select name="anioLectivoIdPines" required value={anioLectivoIdPines ?? ""} onChange={(evento) => setAnioLectivoIdPines(Number(evento.target.value) || undefined)}>{anios.data?.elementos.map((anio) => <option key={anio.id} value={anio.id}>{anio.anio}{anio.vigente ? " (vigente)" : ""}</option>)}</select></Campo>
            <Campo etiqueta="Sección"><select key={anioLectivoIdPines} name="seccionPines" required disabled={!anioLectivoIdPines || secciones.isLoading}><option value="">{secciones.isLoading ? "Cargando secciones…" : "Seleccione una sección"}</option>{secciones.data?.elementos.map((seccion) => <option key={seccion} value={seccion}>{seccion}</option>)}</select></Campo>
            <button className="button secondary" disabled={reiniciarGrupo.isPending || !secciones.data?.elementos.length}>Reiniciar PIN de sección</button>
      </form>
      </section>
    </section>
  );
}
