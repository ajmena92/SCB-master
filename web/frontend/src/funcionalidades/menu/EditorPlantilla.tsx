import type { Dispatch, SetStateAction } from "react";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { Loader2, Plus, Trash2 } from "lucide-react";
import { prepararComponente } from "./componentesMenu";
import type { ComponenteMenuEditable } from "./componentesMenu";

export const DIAS_MENU = ["", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes"];
const TIPOS_COMPONENTE = ["Principal", "Acompañamiento", "Bebida", "Postre", "Otro"];

export interface FormularioPlantilla {
  IdMenuPlantilla?: number;
  SemanaMes: number;
  DiaSemana: number;
  Titulo: string;
  Observaciones: string;
  Activo: boolean;
  Componentes: ComponenteMenuEditable[];
}

interface PropiedadesEditorPlantilla {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  form: FormularioPlantilla | null;
  setForm: Dispatch<SetStateAction<FormularioPlantilla | null>>;
  saving: boolean;
  onGuardar: () => void | Promise<void>;
}

export function EditorPlantilla({
  open,
  onOpenChange,
  form,
  setForm,
  saving,
  onGuardar,
}: PropiedadesEditorPlantilla) {
  const actualizarComponente = (
    indice: number,
    campo: "Nombre" | "TipoComponente",
    valor: string,
  ) => {
    if (!form) return;
    const componentes = [...form.Componentes];
    componentes[indice] = { ...componentes[indice], [campo]: valor };
    setForm({ ...form, Componentes: componentes });
  };

  const agregarComponente = () => {
    if (!form) return;
    setForm({
      ...form,
      Componentes: [
        ...form.Componentes,
        prepararComponente({
          Orden: form.Componentes.length + 1,
          Nombre: "",
          TipoComponente: "Acompañamiento",
        }),
      ],
    });
  };

  const eliminarComponente = (indice: number) => {
    if (!form) return;
    setForm({
      ...form,
      Componentes: form.Componentes.filter((_, posicion) => posicion !== indice).map(
        (componente, posicion) => ({ ...componente, Orden: posicion + 1 }),
      ),
    });
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-lg max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="font-display">Plantilla de menú</DialogTitle>
          <DialogDescription>
            Definí la semana, el día y los componentes que se aplicarán automáticamente en el menú.
          </DialogDescription>
        </DialogHeader>
        {form && (
          <div className="space-y-4">
            <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
              <div>
                <Label>Semana del mes</Label>
                <Select
                  value={String(form.SemanaMes)}
                  onValueChange={(valor: string) => setForm({ ...form, SemanaMes: Number(valor) })}
                >
                  <SelectTrigger data-testid="form-semana">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {[1, 2, 3, 4, 5].map((numero) => (
                      <SelectItem key={numero} value={String(numero)}>
                        Semana {numero}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <div>
                <Label>Día</Label>
                <Select
                  value={String(form.DiaSemana)}
                  onValueChange={(valor: string) => setForm({ ...form, DiaSemana: Number(valor) })}
                >
                  <SelectTrigger data-testid="form-dia">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {[1, 2, 3, 4, 5].map((numero) => (
                      <SelectItem key={numero} value={String(numero)}>
                        {DIAS_MENU[numero]}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>
            <div>
              <Label>Título</Label>
              <Input
                data-testid="form-titulo"
                value={form.Titulo}
                onChange={(event) => setForm({ ...form, Titulo: event.target.value })}
              />
            </div>
            <div>
              <Label>Observaciones</Label>
              <Textarea
                data-testid="form-obs"
                value={form.Observaciones}
                onChange={(event) => setForm({ ...form, Observaciones: event.target.value })}
              />
            </div>
            <div>
              <div className="flex items-center justify-between mb-2">
                <Label>Componentes</Label>
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  data-testid="add-comp"
                  onClick={agregarComponente}
                >
                  <Plus className="h-3 w-3 mr-1" /> Agregar
                </Button>
              </div>
              <div className="space-y-2">
                {form.Componentes.map((componente, indice) => (
                  <div
                    key={componente.claveEdicion}
                    className="grid min-w-0 grid-cols-[minmax(0,1fr)_auto] gap-2 sm:grid-cols-[minmax(0,1fr)_10rem_auto] sm:items-center"
                  >
                    <Input
                      placeholder="Nombre"
                      value={componente.Nombre}
                      onChange={(event) =>
                        actualizarComponente(indice, "Nombre", event.target.value)
                      }
                      className="flex-1"
                      data-testid={`comp-nombre-${indice}`}
                    />
                    <Select
                      value={componente.TipoComponente}
                      onValueChange={(valor: string) =>
                        actualizarComponente(indice, "TipoComponente", valor)
                      }
                    >
                      <SelectTrigger className="col-span-2 w-full sm:col-span-1">
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        {TIPOS_COMPONENTE.map((tipo) => (
                          <SelectItem key={tipo} value={tipo}>
                            {tipo}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <Button
                      type="button"
                      variant="ghost"
                      size="icon"
                      className="col-start-2 row-start-1 sm:col-start-3"
                      aria-label={`Eliminar componente ${indice + 1}`}
                      onClick={() => eliminarComponente(indice)}
                    >
                      <Trash2 className="h-4 w-4 text-destructive" />
                    </Button>
                  </div>
                ))}
              </div>
            </div>
          </div>
        )}
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancelar
          </Button>
          <Button data-testid="save-plantilla" onClick={onGuardar} disabled={saving}>
            {saving ? <Loader2 className="h-4 w-4 animate-spin" /> : "Publicar"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
