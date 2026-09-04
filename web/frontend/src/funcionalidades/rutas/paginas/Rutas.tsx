import { useMemo, useState } from "react";
import type { FormEvent } from "react";
import { useQuery } from "@tanstack/react-query";
import { Plus, Search } from "lucide-react";
import { toast } from "sonner";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { errMsg } from "@/compartido/consultas/errores_api";
import { EstadoPanel } from "@/compartido/componentes/Estados";
import { EditorRuta } from "../EditorRuta";
import { ListadoRutas } from "../componentes/ListadoRutas";
import { actualizarRuta, crearRuta, obtenerDatosRutas, validarRuta } from "../consultas/rutas";
import type { FormularioRuta, Ruta } from "../consultas/rutas";

const EMPTY: FormularioRuta = {
  idRuta: null,
  codigo: "",
  descripcion: "",
  activo: true,
  colorHex: "",
};
export { normalizeRuta, validarRuta as validateRuta } from "../consultas/rutas";

export default function RutasTab() {
  const [query, setQuery] = useState("");
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");
  const [form, setForm] = useState<FormularioRuta>(EMPTY);
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [rutaPorConfirmar, setRutaPorConfirmar] = useState<Ruta | null>(null);
  const {
    data = { rows: [], palette: [] },
    error: loadError,
    isPending: loading,
    refetch,
  } = useQuery({ queryKey: ["admin", "rutas"], queryFn: obtenerDatosRutas });
  const visible = useMemo(() => {
    const term = query.trim().toLocaleLowerCase();
    return term
      ? data.rows.filter((ruta) =>
          `${ruta.codigo} ${ruta.descripcion}`.toLocaleLowerCase().includes(term),
        )
      : data.rows;
  }, [data.rows, query]);
  const abrirNueva = () => {
    setForm({ ...EMPTY, colorHex: data.palette[0]?.hex ?? "" });
    setError("");
    setDrawerOpen(true);
  };
  const abrirEdicion = (ruta: Ruta) => {
    setForm({ ...ruta, colorHex: ruta.colorCarnetHex ?? "" });
    setError("");
    setDrawerOpen(true);
  };
  const guardar = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const validacion = validarRuta(form);
    if (validacion) {
      setError(validacion);
      return;
    }
    setSaving(true);
    setError("");
    try {
      const datos = {
        codigo: form.codigo.trim(),
        descripcion: form.descripcion.trim(),
        colorHex: form.colorHex,
        activo: form.activo,
      };
      if (form.idRuta) await actualizarRuta(form.idRuta, datos);
      else await crearRuta(datos);
      toast.success(form.idRuta ? "Ruta actualizada" : "Ruta creada");
      setDrawerOpen(false);
      await refetch();
    } catch (e) {
      setError(errMsg(e));
    } finally {
      setSaving(false);
    }
  };
  const desactivar = async () => {
    if (!rutaPorConfirmar) return;
    setSaving(true);
    setError("");
    try {
      await actualizarRuta(rutaPorConfirmar.idRuta, {
        codigo: rutaPorConfirmar.codigo,
        descripcion: rutaPorConfirmar.descripcion,
        colorHex: rutaPorConfirmar.colorCarnetHex ?? "",
        activo: false,
      });
      toast.success("Ruta desactivada");
      setRutaPorConfirmar(null);
      await refetch();
    } catch (e) {
      setError(errMsg(e));
    } finally {
      setSaving(false);
    }
  };
  return (
    <section className="space-y-6" aria-labelledby="rutas-title">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <p className="text-xs uppercase tracking-[0.16em] font-semibold text-primary">
            Catálogo operativo
          </p>
          <h2 id="rutas-title" className="font-display text-xl font-semibold tracking-tight">
            Rutas de transporte
          </h2>
          <p className="mt-1 max-w-2xl text-sm text-muted-foreground">
            Administrá la nomenclatura MEP, la descripción y el color usado en el carné digital.
          </p>
        </div>
        <Button className="w-full sm:w-auto" onClick={abrirNueva} data-testid="ruta-nueva">
          <Plus className="mr-2 h-4 w-4" /> Nueva ruta
        </Button>
      </div>
      {(error || loadError) && !drawerOpen && (
        <Alert variant="destructive" data-testid="rutas-error">
          <AlertTitle>No se pudo completar la operación</AlertTitle>
          <AlertDescription>{error || errMsg(loadError)}</AlertDescription>
        </Alert>
      )}
      <div className="flex items-center gap-3 border-b pb-4">
        <Search className="h-4 w-4 text-muted-foreground" />
        <Input
          aria-label="Buscar rutas"
          data-testid="rutas-busqueda"
          className="max-w-xl border-0 px-0 shadow-none focus-visible:ring-0"
          placeholder="Buscar por código o recorrido"
          value={query}
          onChange={(event) => setQuery(event.target.value)}
        />
        <span className="ml-auto whitespace-nowrap text-xs text-muted-foreground">
          {visible.length} rutas
        </span>
      </div>
      <aside
        className="flex flex-wrap items-center gap-2 rounded-xl border border-primary/20 bg-primary/5 px-4 py-3 text-xs text-muted-foreground"
        aria-label="Guía de colores TE-01"
      >
        <strong className="mr-1 text-foreground">Guía TE-01</strong>
        {data.palette.map((color) => (
          <span key={color.clave} className="inline-flex items-center gap-1.5">
            <i
              className="h-3.5 w-3.5 rounded-full border border-border"
              style={{ backgroundColor: color.hex }}
            />
            {color.nombre}
          </span>
        ))}
      </aside>
      {loading ? (
        <EstadoPanel variante="carga">Cargando rutas…</EstadoPanel>
      ) : (
        <ListadoRutas rutas={visible} onEditar={abrirEdicion} onDesactivar={setRutaPorConfirmar} />
      )}
      <EditorRuta
        open={drawerOpen}
        onOpenChange={setDrawerOpen}
        form={form}
        setForm={setForm}
        palette={data.palette}
        saving={saving}
        error={error}
        onGuardar={guardar}
      />
      <AlertDialog
        open={Boolean(rutaPorConfirmar)}
        onOpenChange={(open) => !open && setRutaPorConfirmar(null)}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>¿Desactivar la ruta {rutaPorConfirmar?.codigo}?</AlertDialogTitle>
            <AlertDialogDescription>
              La ruta dejará de estar disponible para nuevas asignaciones. Sus estudiantes y
              registros históricos no se eliminarán.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel disabled={saving}>Cancelar</AlertDialogCancel>
            <AlertDialogAction
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
              disabled={saving}
              onClick={desactivar}
            >
              Desactivar
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </section>
  );
}
