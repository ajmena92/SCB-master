import { useState, type FormEvent } from "react";
import { useMutation, useQuery } from "@tanstack/react-query";
import { Check, Copy, Link2, Loader2, LogOut, ShieldCheck } from "lucide-react";
import { useNavigate } from "react-router-dom";
import { useAutenticacion } from "@/aplicacion/estado/ContextoAutenticacion";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { errMsg } from "@/compartido/consultas/errores_api";
import { usuariosAdministrativosApi } from "../consultas/usuarios";
import type { AutenticacionPlataforma } from "@/funcionalidades/plataforma/seguridad";

export default function VinculacionInicial() {
  const { loadMe, logout } = useAutenticacion() as unknown as AutenticacionPlataforma;
  const navigate = useNavigate();
  const [modo, setModo] = useState<"existente" | "nuevo">("existente");
  const [personaId, setPersonaId] = useState("");
  const [cedula, setCedula] = useState("");
  const [nombres, setNombres] = useState("");
  const [pinTemporal, setPinTemporal] = useState<string>();
  const [vinculada, setVinculada] = useState(false);
  const [copiado, setCopiado] = useState(false);
  const profesores = useQuery({
    queryKey: ["profesores-disponibles", "vinculacion"],
    queryFn: usuariosAdministrativosApi.profesores,
  });
  const vincular = useMutation({
    mutationFn: usuariosAdministrativosApi.vincular,
    onSuccess: (respuesta) => {
      setPinTemporal(respuesta.pinTemporal);
      setVinculada(true);
    },
  });

  function enviar(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    vincular.mutate(
      modo === "existente"
        ? { personaId: Number(personaId) }
        : { profesorNuevo: { cedula: cedula.trim(), nombres: nombres.trim() } },
    );
  }

  async function continuar() {
    const autenticacion = await loadMe();
    navigate(autenticacion.session ? "/" : "/admin", { replace: true });
  }

  if (vinculada)
    return (
      <main className="flex min-h-[100dvh] items-center justify-center bg-background p-4">
        <section className="w-full max-w-lg rounded-3xl border bg-card p-6 shadow-[0_16px_40px_rgb(45_54_150_/_0.10)] sm:p-8">
          <span className="flex h-12 w-12 items-center justify-center rounded-2xl bg-emerald-100 text-emerald-800">
            <Check className="h-6 w-6" />
          </span>
          <h1 className="mt-5 font-display text-2xl font-bold">Cuenta vinculada</h1>
          <p className="mt-2 text-sm text-muted-foreground">
            Tu cuenta administrativa ya pertenece a un profesor registrado.
          </p>
          {pinTemporal && (
            <div className="mt-6 rounded-2xl border bg-muted/40 p-5">
              <p className="text-xs font-bold uppercase tracking-wide text-muted-foreground">
                PIN temporal del portal docente
              </p>
              <p className="mt-2 font-mono text-2xl font-bold tracking-[0.2em]">{pinTemporal}</p>
              <p className="mt-2 text-xs text-muted-foreground">
                Guardalo ahora: se muestra una sola vez.
              </p>
              <Button
                type="button"
                variant="outline"
                className="mt-4 min-h-11 gap-2"
                onClick={async () => {
                  await navigator.clipboard.writeText(pinTemporal);
                  setCopiado(true);
                }}
              >
                {copiado ? <Check className="h-4 w-4" /> : <Copy className="h-4 w-4" />}
                {copiado ? "Copiado" : "Copiar PIN"}
              </Button>
            </div>
          )}
          <Button className="mt-6 min-h-11 w-full font-bold" onClick={continuar}>
            Continuar
          </Button>
        </section>
      </main>
    );

  return (
    <main className="flex min-h-[100dvh] items-center justify-center bg-background p-4 sm:p-6">
      <section
        className="w-full max-w-xl rounded-3xl border bg-card p-6 shadow-[0_16px_40px_rgb(45_54_150_/_0.10)] sm:p-8"
        aria-labelledby="vinculacion-titulo"
      >
        <div className="flex items-start justify-between gap-4">
          <span className="flex h-12 w-12 items-center justify-center rounded-2xl bg-primary/10 text-primary">
            <Link2 className="h-6 w-6" />
          </span>
          <Button variant="ghost" size="icon" onClick={logout} aria-label="Cerrar sesión">
            <LogOut className="h-5 w-5" />
          </Button>
        </div>
        <p className="mt-5 text-xs font-bold uppercase tracking-[0.16em] text-primary">
          Configuración inicial
        </p>
        <h1 id="vinculacion-titulo" className="mt-1 font-display text-2xl font-bold">
          Vinculá la cuenta con un profesor
        </h1>
        <p className="mt-2 text-sm text-muted-foreground">
          Este paso se realiza una sola vez y no cambia tu contraseña actual.
        </p>
        {vincular.error && (
          <Alert variant="destructive" className="mt-5">
            <AlertDescription>{errMsg(vincular.error)}</AlertDescription>
          </Alert>
        )}
        <form onSubmit={enviar} className="mt-6 space-y-5">
          <div className="grid grid-cols-2 gap-2 rounded-xl bg-muted p-1" role="radiogroup">
            {(["existente", "nuevo"] as const).map((opcion) => (
              <button
                key={opcion}
                type="button"
                role="radio"
                aria-checked={modo === opcion}
                onClick={() => setModo(opcion)}
                className={`min-h-11 rounded-lg px-3 text-sm font-bold ${modo === opcion ? "bg-card shadow-sm" : "text-muted-foreground"}`}
              >
                {opcion === "existente" ? "Profesor existente" : "Registrar profesor"}
              </button>
            ))}
          </div>
          {modo === "existente" ? (
            <div className="space-y-2">
              <Label htmlFor="profesor-vinculacion">Profesor disponible</Label>
              <select
                id="profesor-vinculacion"
                required
                value={personaId}
                onChange={(evento) => setPersonaId(evento.target.value)}
                className="min-h-11 w-full rounded-md border border-input bg-card px-3 text-sm"
              >
                <option value="">Seleccioná un profesor…</option>
                {profesores.data?.map((profesor) => (
                  <option key={profesor.id} value={profesor.id}>
                    {profesor.nombres} — {profesor.cedula}
                  </option>
                ))}
              </select>
              {profesores.isLoading && (
                <p className="text-xs text-muted-foreground">Cargando profesores…</p>
              )}
              {profesores.error && (
                <p className="text-sm text-destructive">{errMsg(profesores.error)}</p>
              )}
            </div>
          ) : (
            <div className="grid gap-4">
              <div className="space-y-2">
                <Label htmlFor="vinculacion-cedula">Cédula</Label>
                <Input
                  id="vinculacion-cedula"
                  required
                  inputMode="numeric"
                  value={cedula}
                  onChange={(evento) => setCedula(evento.target.value)}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="vinculacion-nombres">Nombre completo</Label>
                <Input
                  id="vinculacion-nombres"
                  required
                  value={nombres}
                  onChange={(evento) => setNombres(evento.target.value)}
                />
              </div>
            </div>
          )}
          <div className="flex gap-3 rounded-xl bg-primary/5 p-4 text-sm">
            <ShieldCheck className="h-5 w-5 shrink-0 text-primary" />
            <p>
              Solo se permiten personas activas registradas como profesores y sin otra cuenta
              administrativa.
            </p>
          </div>
          <Button
            type="submit"
            disabled={vincular.isPending || profesores.isLoading}
            className="min-h-11 w-full font-bold"
          >
            {vincular.isPending ? (
              <>
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                Vinculando…
              </>
            ) : (
              "Vincular cuenta"
            )}
          </Button>
        </form>
      </section>
    </main>
  );
}
