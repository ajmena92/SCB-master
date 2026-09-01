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
  const [modo, setModo] = useState<"existente" | "nuevo">("existente");
  const [personaId, setPersonaId] = useState("");
  const [cedula, setCedula] = useState("");
  const [nombres, setNombres] = useState("");
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

  function alternarPermiso(clave: string, marcado: boolean) {
    setSeleccionados((actual) =>
      marcado ? [...new Set([...actual, clave])] : actual.filter((item) => item !== clave),
    );
  }

  function enviar(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    const permisosCuenta = rol === "administrador" ? [] : seleccionados;
    if (cuenta) {
      alGuardar({ rol, activo, permisos: permisosCuenta });
      return;
    }
    const credencial = { usuario: usuario.trim(), rol, permisos: permisosCuenta };
    alGuardar(
      modo === "existente"
        ? { ...credencial, personaId: Number(personaId) }
        : {
            ...credencial,
            profesorNuevo: {
              cedula: cedula.trim(),
              nombres: nombres.trim(),
            },
          },
    );
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

            {cuenta ? (
              <div className="rounded-xl border bg-muted/40 p-4">
                <p className="font-bold">{cuenta.persona?.nombres || "Vinculación pendiente"}</p>
                <p className="mt-1 text-sm text-muted-foreground">
                  {cuenta.persona?.cedula || "Sin cédula vinculada"} · {cuenta.usuario}
                </p>
                <p className="mt-2 text-xs text-muted-foreground">
                  La identidad vinculada no puede cambiarse. Para usar otro profesor, desactivá esta
                  cuenta y creá una nueva.
                </p>
              </div>
            ) : (
              <section className="space-y-4" aria-labelledby="vinculacion-cuenta">
                <h3 id="vinculacion-cuenta" className="font-display text-base font-bold">
                  Profesor vinculado
                </h3>
                <div className="grid grid-cols-2 gap-2 rounded-xl bg-muted p-1" role="radiogroup">
                  {(["existente", "nuevo"] as const).map((opcion) => (
                    <button
                      key={opcion}
                      type="button"
                      role="radio"
                      aria-checked={modo === opcion}
                      className={`min-h-11 rounded-lg px-3 text-sm font-bold ${modo === opcion ? "bg-card text-foreground shadow-sm" : "text-muted-foreground"}`}
                      onClick={() => setModo(opcion)}
                    >
                      {opcion === "existente" ? "Profesor existente" : "Registrar profesor"}
                    </button>
                  ))}
                </div>
                {modo === "existente" ? (
                  <div className="space-y-2">
                    <Label htmlFor="cuenta-profesor">Profesor disponible</Label>
                    <select
                      id="cuenta-profesor"
                      required
                      value={personaId}
                      onChange={(evento) => setPersonaId(evento.target.value)}
                      className="min-h-11 w-full rounded-md border border-input bg-card px-3 text-sm"
                    >
                      <option value="">Seleccioná un profesor…</option>
                      {profesores.map((profesor) => (
                        <option key={profesor.id} value={profesor.id}>
                          {profesor.nombres} — {profesor.cedula}
                        </option>
                      ))}
                    </select>
                    {!profesores.length && (
                      <p className="text-xs text-muted-foreground">
                        No hay profesores sin cuenta. Podés registrar uno nuevo.
                      </p>
                    )}
                  </div>
                ) : (
                  <div className="grid gap-4">
                    <div className="space-y-2">
                      <Label htmlFor="cuenta-cedula">Cédula</Label>
                      <Input
                        id="cuenta-cedula"
                        required
                        inputMode="numeric"
                        value={cedula}
                        onChange={(evento) => setCedula(evento.target.value)}
                      />
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="cuenta-nombres">Nombre completo</Label>
                      <Input
                        id="cuenta-nombres"
                        required
                        value={nombres}
                        onChange={(evento) => setNombres(evento.target.value)}
                      />
                    </div>
                  </div>
                )}
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
              </section>
            )}

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
