import { Checkbox } from "@/components/ui/checkbox";
import type { PermisoAdministrativo } from "@/compartido/contratos/usuarios_administrativos";

export default function SelectorPermisosCuenta({
  grupos,
  seleccionados,
  alAlternar,
}: {
  grupos: [string, PermisoAdministrativo[]][];
  seleccionados: string[];
  alAlternar: (clave: string, marcado: boolean) => void;
}) {
  return (
    <div className="space-y-4">
      <p className="text-sm text-muted-foreground">
        Elegí únicamente los módulos necesarios para su trabajo.
      </p>
      {grupos.map(([modulo, opciones]) => (
        <fieldset key={modulo} className="space-y-2 rounded-xl border p-3">
          <legend className="px-1 text-sm font-bold">{modulo}</legend>
          {opciones.map((permiso) => (
            <label
              key={permiso.clave}
              htmlFor={`permiso-${permiso.clave}`}
              className="flex min-h-11 cursor-pointer items-start gap-3 rounded-lg p-2 hover:bg-muted"
            >
              <Checkbox
                id={`permiso-${permiso.clave}`}
                className="mt-0.5 h-5 w-5"
                checked={seleccionados.includes(permiso.clave)}
                onCheckedChange={(valor) => alAlternar(permiso.clave, valor === true)}
              />
              <span className="min-w-0">
                <span className="block text-sm font-semibold">{permiso.descripcion}</span>
                <span className="block break-all text-xs text-muted-foreground">
                  {permiso.clave}
                </span>
              </span>
            </label>
          ))}
        </fieldset>
      ))}
    </div>
  );
}
