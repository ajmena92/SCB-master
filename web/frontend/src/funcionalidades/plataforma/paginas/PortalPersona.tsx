import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { plataformaApi } from "../consultas/plataforma";
import { useAutenticacion } from "@/aplicacion/estado/ContextoAutenticacion";
import { Aviso } from "../componentes/ElementosComunes";
import { errMsg } from "@/compartido/consultas/errores_api";
import type { AutenticacionPlataforma } from "../seguridad";

export default function PortalPersona() {
  const { session, logout } = useAutenticacion() as unknown as AutenticacionPlataforma;
  const cliente = useQueryClient();
  const menu = useQuery({ queryKey: ["menu-hoy"], queryFn: plataformaApi.menu.hoy });
  const reservar = useMutation({
    mutationFn: () => plataformaApi.comedor.reservar(new Date().toISOString().slice(0, 10)),
    onSuccess: () => cliente.invalidateQueries(),
  });
  return (
    <main className="portal-shell">
      <header>
        <div>
          <span className="eyebrow">Portal personal</span>
          <h1>
            Hola,{" "}
            {(session && session.usuario?.nombres) ||
              (session && session.usuario?.Nombre) ||
              "bienvenido"}
          </h1>
        </div>
        <button className="button secondary" onClick={logout}>
          Cerrar sesión
        </button>
      </header>
      <section className="menu-today">
        <span className="eyebrow">Menú de hoy</span>
        <h2>{menu.data?.nombre ?? "Menú pendiente de publicación"}</h2>
        <ul>
          {menu.data?.componentes?.map((c) => (
            <li key={c}>{c}</li>
          ))}
        </ul>
      </section>
      {(menu.error || reservar.error) && (
        <Aviso tipo="error">{errMsg(menu.error || reservar.error)}</Aviso>
      )}
      {reservar.isSuccess && (
        <Aviso tipo="exito">
          Reserva registrada. Su tiquete quedó inmovilizado cuando corresponde.
        </Aviso>
      )}
      <button
        className="button primary jumbo"
        disabled={reservar.isPending || !menu.data}
        onClick={() => reservar.mutate()}
      >
        Reservar almuerzo de hoy
      </button>
      <p className="portal-help">
        Si no puede reservar antes del cierre, deberá solicitar autorización al final de la fila.
      </p>
    </main>
  );
}
