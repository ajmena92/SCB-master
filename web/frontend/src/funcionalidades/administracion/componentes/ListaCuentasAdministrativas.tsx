import { KeyRound, Pencil, ShieldCheck } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import type { CuentaAdministrativa } from "@/compartido/contratos/usuarios_administrativos";

interface PropiedadesCuenta {
  cuenta: CuentaAdministrativa;
  alEditar: () => void;
  alRestablecer: () => void;
}

function AccionesCuenta({ alEditar, alRestablecer }: Omit<PropiedadesCuenta, "cuenta">) {
  return (
    <div className="flex justify-end gap-2">
      <Button variant="outline" size="sm" className="min-h-10 gap-1.5" onClick={alEditar}>
        <Pencil className="h-4 w-4" /> Editar
      </Button>
      <Button variant="ghost" size="sm" className="min-h-10 gap-1.5" onClick={alRestablecer}>
        <KeyRound className="h-4 w-4" /> Restablecer
      </Button>
    </div>
  );
}

function EstadoCuenta({ cuenta }: Pick<PropiedadesCuenta, "cuenta">) {
  if (cuenta.vinculacionPendiente) return <Badge variant="outline">Vinculación pendiente</Badge>;
  if (!cuenta.activo) return <Badge variant="secondary">Inactiva</Badge>;
  if (cuenta.cambioContrasenaObligatorio)
    return (
      <Badge className="bg-amber-100 text-amber-900 hover:bg-amber-100">Cambio pendiente</Badge>
    );
  return <Badge className="bg-emerald-100 text-emerald-900 hover:bg-emerald-100">Activa</Badge>;
}

function FilaCuenta({ cuenta, alEditar, alRestablecer }: PropiedadesCuenta) {
  return (
    <tr className="border-t">
      <td className="px-4 py-3">
        <p className="font-bold">{cuenta.persona?.nombres ?? "Sin vincular"}</p>
        <p className="text-xs text-muted-foreground">{cuenta.persona?.cedula ?? "Sin cédula"}</p>
      </td>
      <td className="px-4 py-3 font-semibold">{cuenta.usuario}</td>
      <td className="px-4 py-3 capitalize">{cuenta.rol}</td>
      <td className="px-4 py-3">
        <EstadoCuenta cuenta={cuenta} />
      </td>
      <td className="px-4 py-3">
        <AccionesCuenta alEditar={alEditar} alRestablecer={alRestablecer} />
      </td>
    </tr>
  );
}

function TarjetaCuenta({ cuenta, alEditar, alRestablecer }: PropiedadesCuenta) {
  return (
    <article className="rounded-2xl border bg-card p-4">
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <p className="truncate font-bold">{cuenta.persona?.nombres ?? "Sin vincular"}</p>
          <p className="truncate text-sm text-muted-foreground">
            {cuenta.usuario} · {cuenta.rol}
          </p>
        </div>
        <ShieldCheck className="h-5 w-5 shrink-0 text-primary" />
      </div>
      <div className="mt-3">
        <EstadoCuenta cuenta={cuenta} />
      </div>
      <div className="mt-4 border-t pt-3">
        <AccionesCuenta alEditar={alEditar} alRestablecer={alRestablecer} />
      </div>
    </article>
  );
}

export default function ListaCuentasAdministrativas({
  cuentas,
  alEditar,
  alRestablecer,
}: {
  cuentas: CuentaAdministrativa[];
  alEditar: (cuenta: CuentaAdministrativa) => void;
  alRestablecer: (cuenta: CuentaAdministrativa) => void;
}) {
  return (
    <>
      <div className="hidden overflow-hidden rounded-2xl border bg-card md:block">
        <table className="w-full text-left text-sm">
          <thead className="bg-muted/60 text-xs uppercase tracking-wide text-muted-foreground">
            <tr>
              <th className="px-4 py-3">Profesor</th>
              <th className="px-4 py-3">Usuario</th>
              <th className="px-4 py-3">Rol</th>
              <th className="px-4 py-3">Estado</th>
              <th className="px-4 py-3 text-right">Acciones</th>
            </tr>
          </thead>
          <tbody>
            {cuentas.map((cuenta) => (
              <FilaCuenta
                key={cuenta.id}
                cuenta={cuenta}
                alEditar={() => alEditar(cuenta)}
                alRestablecer={() => alRestablecer(cuenta)}
              />
            ))}
          </tbody>
        </table>
      </div>
      <div className="grid gap-3 md:hidden">
        {cuentas.map((cuenta) => (
          <TarjetaCuenta
            key={cuenta.id}
            cuenta={cuenta}
            alEditar={() => alEditar(cuenta)}
            alRestablecer={() => alRestablecer(cuenta)}
          />
        ))}
      </div>
    </>
  );
}
