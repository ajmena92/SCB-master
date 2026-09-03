import type { Dispatch, FormEvent, SetStateAction } from "react";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import {
  Drawer,
  DrawerClose,
  DrawerContent,
  DrawerDescription,
  DrawerFooter,
  DrawerHeader,
  DrawerTitle,
} from "@/components/ui/drawer";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { X } from "lucide-react";
import type { ColorRuta, FormularioRuta } from "./consultas/rutas";

interface PropiedadesEditorRuta {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  form: FormularioRuta;
  setForm: Dispatch<SetStateAction<FormularioRuta>>;
  palette: ColorRuta[];
  saving: boolean;
  error: string;
  onGuardar: (event: FormEvent<HTMLFormElement>) => void | Promise<void>;
}

export function EditorRuta({
  open,
  onOpenChange,
  form,
  setForm,
  palette,
  saving,
  error,
  onGuardar,
}: PropiedadesEditorRuta) {
  const bloqueado = form.codigo === "0" || saving;
  return (
    <Drawer open={open} onOpenChange={onOpenChange}>
      <DrawerContent ref={undefined} className="" data-testid="ruta-drawer">
        <DrawerHeader className="">
          <DrawerTitle ref={undefined} className="">
            {form.idRuta ? "Editar ruta" : "Nueva ruta"}
          </DrawerTitle>
          <DrawerDescription ref={undefined} className="">
            El recorrido se verá en el carnet digital y en la operación del transporte.
          </DrawerDescription>
        </DrawerHeader>
        <form onSubmit={onGuardar} className="space-y-5 overflow-y-auto px-4 pb-4 sm:px-6">
          <div className="space-y-2">
            <Label htmlFor="ruta-codigo">Código</Label>
            <Input
              id="ruta-codigo"
              data-testid="ruta-codigo"
              value={form.codigo}
              disabled={bloqueado}
              onChange={(event) =>
                setForm((current) => ({ ...current, codigo: event.target.value }))
              }
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="ruta-descripcion">Descripción del recorrido</Label>
            <textarea
              id="ruta-descripcion"
              data-testid="ruta-descripcion"
              className="flex min-h-28 w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm outline-none focus:ring-1 focus:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
              value={form.descripcion}
              disabled={bloqueado}
              onChange={(event) =>
                setForm((current) => ({ ...current, descripcion: event.target.value }))
              }
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="ruta-color">Color identificador</Label>
            <div className="flex gap-3">
              <Input
                id="ruta-color"
                data-testid="ruta-color"
                type="color"
                className="h-10 w-16 cursor-pointer p-1"
                value={form.colorHex}
                disabled={bloqueado}
                onChange={(event) =>
                  setForm((current) => ({ ...current, colorHex: event.target.value.toUpperCase() }))
                }
              />
              <Input
                aria-label="Código HEX del color"
                data-testid="ruta-color-hex"
                value={form.colorHex}
                pattern="^#[0-9A-Fa-f]{6}$"
                disabled={bloqueado}
                onChange={(event) =>
                  setForm((current) => ({ ...current, colorHex: event.target.value.toUpperCase() }))
                }
              />
            </div>
            {palette.length > 0 && (
              <div className="flex flex-wrap gap-2">
                {palette.map((item) => (
                  <button
                    type="button"
                    key={item.clave}
                    title={item.nombre}
                    aria-label={`Usar color ${item.nombre}`}
                    data-testid={`ruta-paleta-${item.clave}`}
                    className="h-8 w-8 rounded-full border-2 border-slate-300"
                    style={{ backgroundColor: item.hex }}
                    onClick={() => setForm((current) => ({ ...current, colorHex: item.hex }))}
                  />
                ))}
              </div>
            )}
            <p className="text-xs text-muted-foreground">
              Elegí un color de la guía TE-01. El código HEX se conserva para compatibilidad visual.
            </p>
          </div>
          <label className="flex items-center gap-3 text-sm">
            <input
              type="checkbox"
              data-testid="ruta-activo"
              checked={form.activo}
              disabled={bloqueado}
              onChange={(event) =>
                setForm((current) => ({ ...current, activo: event.target.checked }))
              }
            />{" "}
            Ruta activa
          </label>
          {error && (
            <Alert ref={undefined} className="" variant="destructive" data-testid="ruta-form-error">
              <AlertDescription ref={undefined} className="">
                {error}
              </AlertDescription>
            </Alert>
          )}
          <DrawerFooter className="px-0">
            <Button
              className=""
              type="submit"
              disabled={saving || form.codigo === "0"}
              data-testid="ruta-guardar"
            >
              {saving ? "Guardando…" : "Guardar ruta"}
            </Button>
            <DrawerClose asChild>
              <Button
                className=""
                type="button"
                variant="outline"
                disabled={saving}
                data-testid="ruta-cancelar"
              >
                <X className="mr-2 h-4 w-4" /> Cancelar
              </Button>
            </DrawerClose>
          </DrawerFooter>
        </form>
      </DrawerContent>
    </Drawer>
  );
}
