import { useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Plus, Search, UserCog } from "lucide-react";
import { useAutenticacion } from "@/aplicacion/estado/ContextoAutenticacion";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
import { errMsg } from "@/compartido/consultas/errores_api";
import type {
  CredencialesParaMostrar,
  CuentaAdministrativa,
  CuentaCrearEntrada,
  CuentaEditarEntrada,
} from "@/compartido/contratos/usuarios_administrativos";
import DialogoCredencialesAdministrativas from "../componentes/DialogoCredencialesAdministrativas";
import EditorCuentaAdministrativa from "../componentes/EditorCuentaAdministrativa";
import ListaCuentasAdministrativas from "../componentes/ListaCuentasAdministrativas";
import { usuariosAdministrativosApi } from "../consultas/usuarios";
import type { AutenticacionPlataforma } from "@/funcionalidades/plataforma/seguridad";

export default function UsuariosAdministrativos() {
  const { session } = useAutenticacion() as unknown as AutenticacionPlataforma;
  const cliente = useQueryClient();
  const [buscar, setBuscar] = useState("");
  const [editorAbierto, setEditorAbierto] = useState(false);
  const [seleccionada, setSeleccionada] = useState<CuentaAdministrativa>();
  const [credenciales, setCredenciales] = useState<CredencialesParaMostrar>();

  const cuentas = useQuery({
    queryKey: ["cuentas-administrativas"],
    queryFn: usuariosAdministrativosApi.listar,
  });
  const permisos = useQuery({
    queryKey: ["permisos-administrativos"],
    queryFn: usuariosAdministrativosApi.permisos,
  });
  const profesores = useQuery({
    queryKey: ["profesores-disponibles"],
    queryFn: usuariosAdministrativosApi.profesores,
  });
  const refrescar = () => {
    cliente.invalidateQueries({ queryKey: ["cuentas-administrativas"] });
    cliente.invalidateQueries({ queryKey: ["profesores-disponibles"] });
  };
  const crear = useMutation({
    mutationFn: usuariosAdministrativosApi.crear,
    onSuccess: ({ cuenta, credencialesTemporales }) => {
      setEditorAbierto(false);
      setCredenciales({
        nombres: cuenta.persona?.nombres ?? "Profesor",
        usuario: cuenta.usuario,
        contrasena: credencialesTemporales.contrasena,
        pin: credencialesTemporales.pin,
      });
      refrescar();
    },
  });
  const actualizar = useMutation({
    mutationFn: ({ id, datos }: { id: number; datos: CuentaEditarEntrada }) =>
      usuariosAdministrativosApi.actualizar(id, datos),
    onSuccess: () => {
      setEditorAbierto(false);
      setSeleccionada(undefined);
      refrescar();
    },
  });
  const restablecer = useMutation({
    mutationFn: usuariosAdministrativosApi.restablecer,
    onSuccess: (respuesta, id) => {
      const cuenta = cuentas.data?.find((item) => item.id === id);
      if (cuenta)
        setCredenciales({
          nombres: cuenta.persona?.nombres ?? "Profesor",
          usuario: cuenta.usuario,
          contrasena: respuesta.contrasenaTemporal,
        });
      refrescar();
    },
  });

  const filtradas = useMemo(() => {
    const termino = buscar.trim().toLocaleLowerCase();
    if (!termino) return cuentas.data ?? [];
    return (cuentas.data ?? []).filter((cuenta) =>
      [cuenta.usuario, cuenta.persona?.nombres, cuenta.persona?.cedula, cuenta.rol]
        .filter(Boolean)
        .some((valor) => String(valor).toLocaleLowerCase().includes(termino)),
    );
  }, [buscar, cuentas.data]);
  const error =
    cuentas.error ||
    permisos.error ||
    profesores.error ||
    crear.error ||
    actualizar.error ||
    restablecer.error;

  function editar(cuenta: CuentaAdministrativa) {
    setSeleccionada(cuenta);
    setEditorAbierto(true);
  }

  function guardar(datos: CuentaCrearEntrada | CuentaEditarEntrada) {
    if (seleccionada) actualizar.mutate({ id: seleccionada.id, datos });
    else crear.mutate(datos as CuentaCrearEntrada);
  }

  return (
    <section className="space-y-6" aria-labelledby="usuarios-titulo">
      <DialogoCredencialesAdministrativas
        credenciales={credenciales}
        alCerrar={() => setCredenciales(undefined)}
      />
      {editorAbierto && (
        <EditorCuentaAdministrativa
          abierto
          alCambiarAbierto={(abierto) => {
            setEditorAbierto(abierto);
            if (!abierto) setSeleccionada(undefined);
          }}
          cuenta={seleccionada}
          profesores={profesores.data ?? []}
          permisos={permisos.data ?? []}
          guardando={crear.isPending || actualizar.isPending}
          error={crear.error || actualizar.error ? errMsg(crear.error || actualizar.error) : ""}
          esPropia={Boolean(session && seleccionada?.id === session.cuentaId)}
          alGuardar={guardar}
        />
      )}

      <div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <p className="text-sm font-semibold text-primary">Acceso administrativo</p>
          <h2 id="usuarios-titulo" className="font-display text-2xl font-black tracking-tight">
            Usuarios y permisos
          </h2>
          <p className="mt-2 max-w-2xl text-sm text-muted-foreground">
            Cada cuenta pertenece a un profesor. Los operadores solo ven los módulos que les
            asignés.
          </p>
        </div>
        <Button
          className="min-h-11 gap-2 font-bold"
          onClick={() => {
            setSeleccionada(undefined);
            setEditorAbierto(true);
          }}
        >
          <Plus className="h-4 w-4" /> Crear cuenta
        </Button>
      </div>

      {error && (
        <Alert variant="destructive">
          <AlertDescription>{errMsg(error)}</AlertDescription>
        </Alert>
      )}

      <div className="relative max-w-lg">
        <Search
          className="pointer-events-none absolute left-3 top-3 h-5 w-5 text-muted-foreground"
          aria-hidden="true"
        />
        <Input
          value={buscar}
          onChange={(evento) => setBuscar(evento.target.value)}
          className="min-h-11 pl-10"
          placeholder="Buscar por profesor, cédula o usuario"
          aria-label="Buscar cuentas"
        />
      </div>

      {cuentas.isLoading ? (
        <div className="space-y-3" role="status" aria-label="Cargando cuentas">
          <Skeleton className="h-16 w-full" />
          <Skeleton className="h-16 w-full" />
          <Skeleton className="h-16 w-full" />
        </div>
      ) : filtradas.length === 0 ? (
        <div className="rounded-2xl border border-dashed p-8 text-center">
          <UserCog className="mx-auto h-8 w-8 text-muted-foreground" aria-hidden="true" />
          <p className="mt-3 font-bold">
            {buscar ? "No hay coincidencias" : "Todavía no hay cuentas"}
          </p>
          <p className="mt-1 text-sm text-muted-foreground">
            {buscar
              ? "Probá con otro nombre, cédula o usuario."
              : "Creá la primera cuenta vinculada a un profesor."}
          </p>
        </div>
      ) : (
        <ListaCuentasAdministrativas
          cuentas={filtradas}
          alEditar={editar}
          alRestablecer={(cuenta) => restablecer.mutate(cuenta.id)}
        />
      )}
    </section>
  );
}
