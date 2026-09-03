import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { plataformaApi } from "../consultas/plataforma";
import { useAutenticacion } from "@/aplicacion/estado/ContextoAutenticacion";
import { Aviso } from "../componentes/ElementosComunes";
import { errMsg } from "@/compartido/consultas/errores_api";
import type { AutenticacionPlataforma } from "../seguridad";

export default function PortalPersona() {
  const { session, logout } = useAutenticacion() as unknown as AutenticacionPlataforma;
  const cliente = useQueryClient();
  const [confirmacionSinTiquete, setConfirmacionSinTiquete] = useState(false);
  const menu = useQuery({ queryKey: ["menu-hoy"], queryFn: plataformaApi.menu.hoy });
  const reservar = useMutation({
    mutationFn: () => plataformaApi.comedor.reservar(new Date().toISOString().slice(0, 10)),
    onSuccess: (respuesta) => {
      setConfirmacionSinTiquete(Boolean(respuesta.data?.sin_tiquete));
      return cliente.invalidateQueries();
    },
  });
  const usuario = session && typeof session.usuario === "object" ? session.usuario : undefined;
  return (
    <main className="mx-auto grid max-w-3xl gap-6 px-4 py-6 sm:px-6 sm:py-8">
      <header className="flex flex-col gap-4 border-b border-border pb-5 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <span className="font-body text-xs font-medium uppercase tracking-[0.16em] text-primary">Portal personal</span>
          <h1 className="mt-1 font-heading text-2xl font-bold tracking-tight text-foreground">Hola, {String(usuario?.nombres || usuario?.Nombre || "bienvenido")}</h1>
        </div>
        <button className="button secondary" onClick={logout}>
          Cerrar sesión
        </button>
      </header>
      <section className="rounded-xl border border-border bg-card p-5 shadow-sm sm:p-6">
        <span className="font-body text-xs font-medium uppercase tracking-[0.16em] text-primary">Menú de hoy</span>
        <h2 className="mt-2 font-heading text-xl font-bold text-foreground">{menu.data?.nombre ?? "Menú pendiente de publicación"}</h2>
        <ul className="mt-4 grid gap-2 text-base text-muted-foreground">
          {menu.data?.componentes?.map((c) => (
            <li className="rounded-lg bg-muted px-3 py-2" key={c}>{c}</li>
          ))}
        </ul>
      </section>
      {(menu.error || reservar.error) && (
        <Aviso tipo="error">{errMsg(menu.error || reservar.error)}</Aviso>
      )}
      {reservar.isSuccess && (
        <Aviso tipo="exito">
          {confirmacionSinTiquete
            ? "Asistencia confirmada. No tenés tiquetes disponibles; consultá al operador al ingresar al comedor."
            : "Asistencia confirmada. Su tiquete quedó inmovilizado cuando corresponde."}
        </Aviso>
      )}
      <button
        className="button primary jumbo w-full sm:w-auto"
        disabled={reservar.isPending || !menu.data}
        onClick={() => reservar.mutate()}
      >
        Reservar almuerzo de hoy
      </button>
      <p className="text-sm leading-6 text-muted-foreground">
        Si no puede reservar antes del cierre, deberá solicitar autorización al final de la fila.
      </p>
    </main>
  );
}
