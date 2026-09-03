import { useMemo, useState } from "react";
import type { FormEvent } from "react";
import { useQuery } from "@tanstack/react-query";
import { errMsg } from "@/compartido/consultas/errores_api";
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
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
import { Map, Pencil, Plus, Route as IconoRuta, Search } from "lucide-react";
import { toast } from "sonner";
import { EditorRuta } from "@/funcionalidades/rutas/EditorRuta";
import {
  actualizarRuta,
  crearRuta,
  obtenerDatosRutas,
  validarRuta,
} from "@/funcionalidades/rutas/consultas/rutas";
import type { FormularioRuta, Ruta } from "@/funcionalidades/rutas/consultas/rutas";

const EMPTY: FormularioRuta = {
  idRuta: null,
  codigo: "",
  descripcion: "",
  activo: true,
  colorHex: "#EF4444",
};

export {
  normalizeRuta,
  validarRuta as validateRuta,
} from "@/funcionalidades/rutas/consultas/rutas";

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
    setForm({ ...EMPTY, colorHex: data.palette[0]?.hex || "#EF4444" });
    setError("");
    setDrawerOpen(true);
  };
  const abrirEdicion = (ruta: Ruta) => {
    setForm({ ...ruta, colorHex: ruta.colorCarnetHex || "#CBD5E1" });
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
        colorHex: rutaPorConfirmar.colorCarnetHex || "#CBD5E1",
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
          <p className="text-xs uppercase tracking-[0.16em] font-semibold text-primary">Catálogo operativo</p>
          <h2 id="rutas-title" className="font-display text-xl font-semibold tracking-tight">Rutas de transporte</h2>
          <p className="mt-1 max-w-2xl text-sm text-muted-foreground">
            Administrá la nomenclatura MEP, la descripción y el color usado en el carné digital.
          </p>
        </div>
        <Button onClick={abrirNueva} data-testid="ruta-nueva">
          <Plus className="mr-2 h-4 w-4" /> Nueva ruta
        </Button>
      </div>
      {(error || loadError) && !drawerOpen && (
        <Alert ref={undefined} className="" variant="destructive" data-testid="rutas-error">
          <AlertTitle ref={undefined} className="">
            No se pudo completar la operación
          </AlertTitle>
          <AlertDescription ref={undefined} className="">
            {error || errMsg(loadError)}
          </AlertDescription>
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
      <aside className="flex flex-wrap items-center gap-2 rounded-xl border border-blue-100 bg-blue-50/60 px-4 py-3 text-xs text-slate-600" aria-label="Guía de colores TE-01">
        <strong className="mr-1 text-slate-800">Guía TE-01</strong>
        {data.palette.map((color) => <span key={color.clave} className="inline-flex items-center gap-1.5"><i className="h-3.5 w-3.5 rounded-full border border-slate-300" style={{ backgroundColor: color.hex }} />{color.nombre}</span>)}
      </aside>
      {loading ? (
        <Skeleton className="h-64 w-full rounded-2xl" data-testid="rutas-loading" />
      ) : visible.length === 0 ? (
        <div
          className="rounded-2xl border border-dashed bg-card p-8 text-center"
          data-testid="rutas-empty"
        >
          <Map className="mx-auto mb-3 h-8 w-8 text-muted-foreground" />
          <h3 className="font-display text-lg font-semibold">No hay rutas para mostrar</h3>
          <p className="mt-1 text-sm text-muted-foreground">
            Probá otra búsqueda o agregá una nueva ruta.
          </p>
        </div>
      ) : (
        <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3" data-testid="rutas-list">
          {visible.map((ruta) => (
            <article
              key={ruta.idRuta}
              className={`rounded-xl border bg-card p-4 shadow-[0_4px_14px_rgb(15_72_131_/_0.06)] transition-colors duration-200 hover:border-primary/35 ${ruta.activo ? "" : "opacity-65"}`}
              data-testid={`ruta-${ruta.idRuta}`}
            >
              <div className="flex items-start justify-between gap-3">
                <div className="flex min-w-0 items-start gap-3">
                  <span
                    className="mt-1 flex h-9 w-9 shrink-0 items-center justify-center rounded-full border-2"
                    style={{
                      backgroundColor: ruta.colorCarnetHex || "#CBD5E1",
                      borderColor:
                        ruta.colorCarnetHex === "#FFFFFF"
                          ? "#CBD5E1"
                          : ruta.colorCarnetHex || "#CBD5E1",
                    }}
                    aria-label={`Color de la ruta ${ruta.codigo}`}
                  >
                    <IconoRuta className="h-4 w-4 text-slate-900" />
                  </span>
                  <div className="min-w-0">
                    <p className="text-xs font-semibold tracking-wide text-primary">Ruta {ruta.codigo}</p>
                    <p className="text-sm font-medium leading-relaxed text-foreground">
                      {ruta.descripcion}
                    </p>
                  </div>
                </div>
                <Badge className="text-[11px] font-medium" variant={ruta.activo ? "secondary" : "outline"}>
                  {ruta.activo ? "Activa" : "Inactiva"}
                </Badge>
              </div>
              <div className="mt-4 flex items-center justify-between border-t pt-3 text-xs text-muted-foreground">
                <span>
                  {ruta.estudiantesAsignados} estudiante{ruta.estudiantesAsignados === 1 ? "" : "s"}
                </span>
                <span className="hidden items-center gap-1 sm:inline-flex"><i className="h-3 w-3 rounded-full border" style={{ backgroundColor: ruta.colorCarnetHex ?? undefined }} />Color TE-01</span>
                <div className="flex gap-2">
                  <Button
                    ref={undefined}
                    className=""
                    variant="ghost"
                    size="sm"
                    onClick={() => abrirEdicion(ruta)}
                    disabled={ruta.codigo === "0"}
                    data-testid={`ruta-editar-${ruta.idRuta}`}
                  >
                  <Pencil className="mr-1 h-3.5 w-3.5" /> Editar ruta
                  </Button>
                  {ruta.activo && ruta.codigo !== "0" && (
                    <Button
                      ref={undefined}
                      variant="ghost"
                      size="sm"
                      className="text-destructive hover:text-destructive"
                      onClick={() => setRutaPorConfirmar(ruta)}
                      data-testid={`ruta-desactivar-${ruta.idRuta}`}
                    >
                      Desactivar
                    </Button>
                  )}
                </div>
              </div>
            </article>
          ))}
        </div>
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
        <AlertDialogContent ref={undefined} className="">
          <AlertDialogHeader className="">
            <AlertDialogTitle ref={undefined} className="">
              ¿Desactivar la ruta {rutaPorConfirmar?.codigo}?
            </AlertDialogTitle>
            <AlertDialogDescription ref={undefined} className="">
              La ruta dejará de estar disponible para nuevas asignaciones. Sus estudiantes y
              registros históricos no se eliminarán.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter ref={undefined} className="">
            <AlertDialogCancel ref={undefined} className="" disabled={saving}>
              Cancelar
            </AlertDialogCancel>
            <AlertDialogAction
              ref={undefined}
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
