import { useMemo, useState, type FormEvent } from "react";
import { AlertCircle, BadgeCheck, UserPlus } from "lucide-react";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetFooter,
  SheetHeader,
  SheetTitle,
} from "@/components/ui/sheet";
import { Switch } from "@/components/ui/switch";
import type {
  CuentaAdministrativa,
  CuentaCrearEntrada,
  CuentaEditarEntrada,
  PermisoAdministrativo,
  ProfesorDisponible,
  RolAdministrativo,
} from "@/compartido/contratos/usuarios_administrativos";
import SelectorPermisosCuenta from "./SelectorPermisosCuenta";

type DatosGuardar = CuentaCrearEntrada | CuentaEditarEntrada;

export default function EditorCuentaAdministrativa({
  abierto,
  alCambiarAbierto,
  cuenta,
  profesores,
  permisos,
  guardando,
  error,
  esPropia,
  alGuardar,
}: {
  abierto: boolean;
  alCambiarAbierto: (abierto: boolean) => void;
  cuenta?: CuentaAdministrativa;
  profesores: ProfesorDisponible[];
  permisos: PermisoAdministrativo[];
  guardando: boolean;
  error: string;
  esPropia: boolean;
  alGuardar: (datos: DatosGuardar) => void;
}) {
  const [personaId, setPersonaId] = useState(String(cuenta?.persona?.id ?? ""));
  const [usuario, setUsuario] = useState(cuenta?.usuario ?? "");
  const [rol, setRol] = useState<RolAdministrativo>(cuenta?.rol ?? "operador");
  const [activo, setActivo] = useState(cuenta?.activo ?? true);
  const [seleccionados, setSeleccionados] = useState<string[]>(cuenta?.permisos ?? []);

  const grupos = useMemo(() => {
    const resultado = new Map<string, PermisoAdministrativo[]>();
    permisos.forEach((permiso) => {
      const actual = resultado.get(permiso.modulo) ?? [];
      actual.push(permiso);
      resultado.set(permiso.modulo, actual);
    });
    return [...resultado.entries()];
  }, [permisos]);

  const profesoresSeleccionables = useMemo(() => {
    const actual = cuenta?.persona;
    if (!actual || profesores.some((profesor) => profesor.id === actual.id)) return profesores;
    return [{ id: actual.id, cedula: actual.cedula, nombres: actual.nombres }, ...profesores];
  }, [cuenta?.persona, profesores]);

  function alternarPermiso(clave: string, marcado: boolean) {
    setSeleccionados((actual) =>
      marcado ? [...new Set([...actual, clave])] : actual.filter((item) => item !== clave),
    );
  }

  function enviar(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    const permisosCuenta = rol === "administrador" ? [] : seleccionados;
    if (cuenta) {
      alGuardar({
        rol,
        activo,
        permisos: permisosCuenta,
        ...(Number(personaId) !== cuenta.persona?.id ? { personaId: Number(personaId) } : {}),
      });
      return;
    }
    alGuardar({ usuario: usuario.trim(), rol, permisos: permisosCuenta, personaId: Number(personaId) });
  }

  return (
    <Sheet open={abierto} onOpenChange={alCambiarAbierto}>
      <SheetContent className="flex h-full w-full flex-col overflow-hidden p-0 sm:max-w-xl">
        <SheetHeader className="border-b px-5 pb-4 pt-6 text-left sm:px-6">
          <span className="mb-1 flex h-10 w-10 items-center justify-center rounded-xl bg-primary/10 text-primary">
            <UserPlus className="h-5 w-5" aria-hidden="true" />
          </span>
          <SheetTitle>{cuenta ? "Editar cuenta" : "Crear cuenta administrativa"}</SheetTitle>
          <SheetDescription>
            {cuenta
              ? "Actualizá el rol, los accesos y el estado de la cuenta."
              : "Toda cuenta debe quedar vinculada a una persona registrada como profesor."}
          </SheetDescription>
        </SheetHeader>
        <form className="flex min-h-0 flex-1 flex-col" onSubmit={enviar}>
          <div className="min-h-0 flex-1 space-y-6 overflow-y-auto px-5 py-5 sm:px-6">
            {error && (
              <Alert variant="destructive">
                <AlertCircle className="h-4 w-4" />
                <AlertDescription>{error}</AlertDescription>
              </Alert>
            )}

            <section className="space-y-4" aria-labelledby="vinculacion-cuenta">
              <h3 id="vinculacion-cuenta" className="font-display text-base font-bold">
                Profesor vinculado
              </h3>
              <div className="space-y-2">
                <Label htmlFor="cuenta-profesor">
                  {cuenta ? "Profesor que usará esta cuenta" : "Profesor disponible"}
                </Label>
                <select
                  id="cuenta-profesor"
                  required
                  value={personaId}
                  onChange={(evento) => setPersonaId(evento.target.value)}
                  className="min-h-11 w-full rounded-md border border-input bg-card px-3 text-sm"
                >
                  <option value="">Seleccioná un profesor…</option>
                  {profesoresSeleccionables.map((profesor) => (
                    <option key={profesor.id} value={profesor.id}>
                      {profesor.nombres} — {profesor.cedula}
                    </option>
                  ))}
                </select>
                <p className="text-xs text-muted-foreground">
                  {cuenta
                    ? "Al cambiarlo, se cerrarán las sesiones activas de esta cuenta."
                    : "Solo se muestran profesores activos importados que todavía no tienen cuenta."}
                </p>
                {!profesoresSeleccionables.length && (
                  <p className="text-xs text-muted-foreground">
                    No hay profesores disponibles en el padrón actual.
                  </p>
                )}
              </div>
              {!cuenta && (
                <div className="space-y-2">
                  <Label htmlFor="cuenta-usuario">Nombre de usuario</Label>
                  <Input
                    id="cuenta-usuario"
                    required
                    minLength={3}
                    autoComplete="off"
                    value={usuario}
                    onChange={(evento) => setUsuario(evento.target.value)}
                    placeholder="Ej. mrojas"
                  />
                </div>
              )}
            </section>

            <section className="space-y-4" aria-labelledby="acceso-cuenta">
              <h3 id="acceso-cuenta" className="font-display text-base font-bold">
                Rol y accesos
              </h3>
              <div className="space-y-2">
                <Label htmlFor="cuenta-rol">Rol</Label>
                <select
                  id="cuenta-rol"
                  value={rol}
                  disabled={esPropia}
                  onChange={(evento) => setRol(evento.target.value as RolAdministrativo)}
                  className="min-h-11 w-full rounded-md border border-input bg-card px-3 text-sm disabled:opacity-60"
                >
                  <option value="operador">Operador</option>
                  <option value="administrador">Administrador</option>
                </select>
                {esPropia && (
                  <p className="text-xs text-muted-foreground">No podés cambiar tu propio rol.</p>
                )}
              </div>

              {rol === "administrador" ? (
                <div className="flex gap-3 rounded-xl border bg-primary/5 p-4 text-sm">
                  <BadgeCheck className="h-5 w-5 shrink-0 text-primary" aria-hidden="true" />
                  <div>
                    <p className="font-bold">Acceso total</p>
                    <p className="mt-1 text-muted-foreground">
                      Los administradores pueden gestionar todos los módulos y las cuentas.
                    </p>
                  </div>
                </div>
              ) : (
                <SelectorPermisosCuenta
                  grupos={grupos}
                  seleccionados={seleccionados}
                  alAlternar={alternarPermiso}
                />
              )}
            </section>

            {cuenta && (
              <div className="flex min-h-12 items-center justify-between gap-4 rounded-xl border p-3">
                <div>
                  <Label htmlFor="cuenta-activa" className="font-bold">
                    Cuenta activa
                  </Label>
                  <p className="text-xs text-muted-foreground">Permite iniciar sesión.</p>
                </div>
                <Switch
                  id="cuenta-activa"
                  checked={activo}
                  disabled={esPropia}
                  onCheckedChange={setActivo}
                  aria-label="Cuenta activa"
                />
              </div>
            )}
          </div>
          <SheetFooter className="gap-2 border-t bg-card px-5 py-4 sm:px-6">
            <Button type="button" variant="outline" onClick={() => alCambiarAbierto(false)}>
              Cancelar
            </Button>
            <Button type="submit" disabled={guardando} className="font-bold">
              {guardando ? "Guardando…" : cuenta ? "Guardar cambios" : "Crear cuenta"}
            </Button>
          </SheetFooter>
        </form>
      </SheetContent>
    </Sheet>
  );
}
