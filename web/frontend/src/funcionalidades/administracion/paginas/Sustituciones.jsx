import { useSustituciones } from "@/funcionalidades/administracion/hooks/useSustituciones";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { Skeleton } from "@/components/ui/skeleton";
import { Badge } from "@/components/ui/badge";
import { Plus, Trash2, Loader2, Replace } from "lucide-react";

const TIPOS = ["Principal", "Acompañamiento", "Bebida", "Postre", "Otro"];

export default function SustitucionesTab() {
  const {
    open,
    setOpen,
    saving,
    form,
    subs,
    loading,
    abrir,
    setForm,
    setComp,
    addComp,
    delComp,
    guardar,
  } = useSustituciones();

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="font-display text-2xl font-bold tracking-tight">
            Sustituciones por fecha
          </h2>
          <p className="text-sm text-muted-foreground">
            Un menú por fecha exacta que prevalece sobre la plantilla semanal.
          </p>
        </div>
        <Button data-testid="new-sustitucion-btn" onClick={() => abrir(null)}>
          <Plus className="h-4 w-4 mr-1" /> Nueva sustitución
        </Button>
      </div>

      {loading ? (
        <Skeleton className="h-40 w-full rounded-lg" />
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {subs.length === 0 && (
            <p className="text-sm text-muted-foreground">No hay sustituciones registradas.</p>
          )}
          {subs.map((s) => (
            <div
              key={s.IdMenuSustitucion}
              data-testid={`sustitucion-${s.Fecha}`}
              className="bg-card border rounded-lg p-4 hover:shadow-md transition-shadow"
            >
              <div className="flex items-center justify-between">
                <Badge className="bg-primary text-white">{s.Fecha}</Badge>
                <Replace className="h-4 w-4 text-muted-foreground" />
              </div>
              <p className="font-display font-bold mt-2">{s.Titulo}</p>
              <p className="text-xs text-muted-foreground mt-1">
                {s.Componentes.length} componentes
              </p>
              <Button
                variant="ghost"
                size="sm"
                className="mt-2 px-0 text-primary"
                onClick={() => abrir(s)}
              >
                Editar
              </Button>
            </div>
          ))}
        </div>
      )}

      <Dialog open={open} onOpenChange={setOpen}>
        <DialogContent className="max-w-lg max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle className="font-display">Sustitución de menú</DialogTitle>
          </DialogHeader>
          {form && (
            <div className="space-y-4">
              <div>
                <Label>Fecha</Label>
                <Input
                  type="date"
                  data-testid="sust-fecha"
                  value={form.Fecha}
                  onChange={(e) => setForm({ ...form, Fecha: e.target.value })}
                />
              </div>
              <div>
                <Label>Título</Label>
                <Input
                  data-testid="sust-titulo"
                  value={form.Titulo}
                  onChange={(e) => setForm({ ...form, Titulo: e.target.value })}
                />
              </div>
              <div>
                <Label>Observaciones</Label>
                <Textarea
                  data-testid="sust-obs"
                  value={form.Observaciones}
                  onChange={(e) => setForm({ ...form, Observaciones: e.target.value })}
                />
              </div>
              <div>
                <div className="flex items-center justify-between mb-2">
                  <Label>Componentes</Label>
                  <Button type="button" variant="outline" size="sm" onClick={addComp}>
                    <Plus className="h-3 w-3 mr-1" /> Agregar
                  </Button>
                </div>
                <div className="space-y-2">
                  {form.Componentes.map((c, i) => (
                    <div key={c.claveEdicion} className="flex gap-2 items-center">
                      <Input
                        placeholder="Nombre"
                        value={c.Nombre}
                        onChange={(e) => setComp(i, "Nombre", e.target.value)}
                        className="flex-1"
                      />
                      <Select
                        value={c.TipoComponente}
                        onValueChange={(v) => setComp(i, "TipoComponente", v)}
                      >
                        <SelectTrigger className="w-40">
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                          {TIPOS.map((t) => (
                            <SelectItem key={t} value={t}>
                              {t}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                      <Button type="button" variant="ghost" size="icon" onClick={() => delComp(i)}>
                        <Trash2 className="h-4 w-4 text-destructive" />
                      </Button>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          )}
          <DialogFooter>
            <Button variant="outline" onClick={() => setOpen(false)}>
              Cancelar
            </Button>
            <Button data-testid="save-sustitucion" onClick={guardar} disabled={saving}>
              {saving ? <Loader2 className="h-4 w-4 animate-spin" /> : "Guardar"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
