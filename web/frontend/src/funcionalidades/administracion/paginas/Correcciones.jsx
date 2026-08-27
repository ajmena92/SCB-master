import { useEffect, useState, useCallback } from "react";
import { api, errMsg } from "@/lib/api";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Skeleton } from "@/components/ui/skeleton";
import { toast } from "sonner";
import { Wrench, Loader2, ShieldAlert } from "lucide-react";

export default function CorreccionesTab() {
  const [estudiantes, setEstudiantes] = useState([]);
  const [buscar, setBuscar] = useState("");
  const [loading, setLoading] = useState(true);
  const [idUsuario, setIdUsuario] = useState("");
  const [fecha, setFecha] = useState(() => new Date().toISOString().slice(0, 10));
  const [accion, setAccion] = useState("agregar");
  const [motivo, setMotivo] = useState("");
  const [saving, setSaving] = useState(false);

  const cargar = useCallback(async (texto) => {
    if (texto.trim().length < 2) {
      setEstudiantes([]);
      setLoading(false);
      return;
    }
    setLoading(true);
    try {
      const { data } = await api.get(
        `/v1/estudiantes?pagina=1&tamano=50&buscar=${encodeURIComponent(texto)}`,
      );
      setEstudiantes(data.items);
    } catch (e) {
      toast.error(errMsg(e));
    } finally {
      setLoading(false);
    }
  }, []);
  useEffect(() => {
    const timer = setTimeout(() => cargar(buscar), 250);
    return () => clearTimeout(timer);
  }, [buscar, cargar]);

  const enviar = async () => {
    if (!idUsuario) return toast.error("Seleccioná un estudiante");
    if (!motivo.trim()) return toast.error("El motivo es obligatorio");
    setSaving(true);
    try {
      await api.put(`/v1/asistencia/marcas/${Number(idUsuario)}/correccion`, {
        estado: accion === "agregar" ? "presente" : "ausente",
        motivo,
      });
      toast.success("Corrección aplicada y auditada");
      setMotivo("");
    } catch (e) {
      toast.error(errMsg(e));
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="space-y-6 max-w-2xl">
      <div>
        <h2 className="font-display text-2xl font-bold tracking-tight">
          Correcciones fuera de horario
        </h2>
        <p className="text-sm text-muted-foreground">
          Solo Administrador. Toda corrección exige un motivo y queda auditada.
        </p>
      </div>

      <div className="flex items-start gap-3 bg-primary/10 border border-primary/30 rounded-lg p-4">
        <ShieldAlert className="h-5 w-5 text-primary shrink-0 mt-0.5" />
        <p className="text-sm">
          Agregar crea una marca en el registro canónico de asistencia retirar elimina únicamente la
          marca del portal y conserva la auditoría.
        </p>
      </div>

      {loading ? (
        <Skeleton className="h-64 w-full rounded-lg" />
      ) : (
        <div className="bg-card border rounded-lg p-6 space-y-5">
          <div>
            <Label>Estudiante</Label>
            <Input
              value={buscar}
              onChange={(event) => {
                setBuscar(event.target.value);
                setIdUsuario("");
              }}
              placeholder="Escriba al menos 2 caracteres"
              className="mb-2"
            />
            <Select value={idUsuario} onValueChange={setIdUsuario}>
              <SelectTrigger
                data-testid="correccion-estudiante"
                disabled={buscar.trim().length < 2}
              >
                <SelectValue placeholder="Seleccionar estudiante" />
              </SelectTrigger>
              <SelectContent>
                {estudiantes.map((e) => (
                  <SelectItem key={e.IdUsuario} value={String(e.IdUsuario)}>
                    {e.NombreCompleto} · {e.Cedula}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <Label>Fecha de servicio</Label>
              <Input
                type="date"
                data-testid="correccion-fecha"
                value={fecha}
                onChange={(e) => setFecha(e.target.value)}
              />
            </div>
            <div>
              <Label>Acción</Label>
              <Select value={accion} onValueChange={setAccion}>
                <SelectTrigger data-testid="correccion-accion">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="agregar">Agregar confirmación</SelectItem>
                  <SelectItem value="retirar">Retirar confirmación</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>
          <div>
            <Label>Motivo (obligatorio)</Label>
            <Textarea
              data-testid="correccion-motivo"
              value={motivo}
              onChange={(e) => setMotivo(e.target.value)}
              placeholder="Justificación de la corrección…"
            />
          </div>
          <Button
            data-testid="correccion-submit"
            onClick={enviar}
            disabled={saving}
            className="w-full"
          >
            {saving ? (
              <Loader2 className="h-4 w-4 animate-spin" />
            ) : (
              <>
                <Wrench className="h-4 w-4 mr-2" /> Aplicar corrección
              </>
            )}
          </Button>
        </div>
      )}
    </div>
  );
}
