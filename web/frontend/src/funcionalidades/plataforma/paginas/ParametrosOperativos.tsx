import type { FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Gear, Tag, Buildings } from "@phosphor-icons/react";
import { plataformaApi } from "../consultas/plataforma";
import { Aviso, Campo, EncabezadoPagina, EstadoCarga, Tabla } from "../componentes/ElementosComunes";
import { errMsg } from "@/compartido/consultas/errores_api";
import { toast } from "sonner";

export default function ParametrosOperativos() {
  const cliente = useQueryClient();
  const tarifas = useQuery({ queryKey: ["tarifas"], queryFn: plataformaApi.tiquetes.tarifas });
  const horarios = useQuery({ queryKey: ["horarios-reserva"], queryFn: plataformaApi.tiquetes.horariosReserva });
  const institucion = useQuery({ queryKey: ["institucion"], queryFn: plataformaApi.tiquetes.institucion });
  const crear = useMutation({ mutationFn: plataformaApi.tiquetes.crearTarifa, onSuccess: () => { toast.success("Tarifa programada"); cliente.invalidateQueries({ queryKey: ["tarifas"] }); } });
  const guardarHorario = useMutation({ mutationFn: plataformaApi.tiquetes.actualizarHorarioReserva, onSuccess: () => { toast.success("Hora límite actualizada"); cliente.invalidateQueries({ queryKey: ["horarios-reserva"] }); } });
  const guardarInstitucion = useMutation({ mutationFn: plataformaApi.tiquetes.actualizarInstitucion, onSuccess: () => { toast.success("Datos institucionales actualizados"); cliente.invalidateQueries({ queryKey: ["institucion"] }); } });
  const error = tarifas.error || horarios.error || institucion.error || crear.error || guardarHorario.error || guardarInstitucion.error;
  function nuevaTarifa(evento: FormEvent<HTMLFormElement>) { evento.preventDefault(); const datos = new FormData(evento.currentTarget); crear.mutate({ tipoPersona: datos.get("tipoPersona") === "profesor" ? "profesor" : "estudiante", montoColones: Number(datos.get("monto")), vigenteDesde: String(datos.get("desde")), vigenteHasta: null }); }
  return <section className="stack">
    <EncabezadoPagina titulo="Parámetros operativos" descripcion="Configure tarifas de tiquetes y la hora límite de marca de comedor." />
    {error && <Aviso tipo="error">{errMsg(error)}</Aviso>}
    <section className="action-panel"><div className="panel-title"><Tag size={22} aria-hidden="true" /><div><h2>Tarifas de tiquetes</h2><p>La venta aplica automáticamente la tarifa vigente según el tipo de persona.</p></div></div><form className="form-grid" onSubmit={nuevaTarifa}><Campo etiqueta="Persona"><select name="tipoPersona"><option value="estudiante">Estudiante</option><option value="profesor">Profesor</option></select></Campo><Campo etiqueta="Precio por tiquete (₡)"><input name="monto" type="number" min="0" required /></Campo><Campo etiqueta="Vigente desde"><input name="desde" type="date" required /></Campo><button className="button primary" disabled={crear.isPending}>{crear.isPending ? "Guardando…" : "Programar tarifa"}</button></form>{tarifas.isLoading ? <EstadoCarga /> : <Tabla columnas={["Tipo", "Monto", "Desde", "Hasta"]} filas={(tarifas.data?.elementos ?? []).map((tarifa) => [tarifa.tipoPersona, `₡${tarifa.montoColones.toLocaleString("es-CR")}`, tarifa.vigenteDesde, tarifa.vigenteHasta ?? "Vigente"])} />}</section>
    <section className="action-panel"><div className="panel-title"><Gear size={22} aria-hidden="true" /><div><h2>Hora límite de marca de comedor</h2><p>Después de esta hora, la reserva del horario correspondiente queda cerrada.</p></div></div>{horarios.isLoading ? <EstadoCarga /> : <div className="settings-hours">{horarios.data?.map((horario) => <form key={horario.turno} onSubmit={(evento) => { evento.preventDefault(); guardarHorario.mutate({ turno: horario.turno, horaLimite: String(new FormData(evento.currentTarget).get("hora")) }); }}><span>{horario.turno}</span><input name="hora" type="time" defaultValue={horario.horaLimite} required /><button className="button secondary" disabled={guardarHorario.isPending}>Guardar</button></form>)}</div>}</section>
    <section className="action-panel"><div className="panel-title"><Buildings size={22} aria-hidden="true" /><div><h2>Identidad institucional en reportes</h2><p>Estos datos aparecen en comprobantes y reportes generados por el sistema.</p></div></div>{institucion.isLoading ? <EstadoCarga /> : <form className="form-grid" onSubmit={(evento) => { evento.preventDefault(); const datos = new FormData(evento.currentTarget); guardarInstitucion.mutate({ nombreColegio: String(datos.get("nombreColegio")), subtituloReportes: String(datos.get("subtituloReportes")) }); }}><Campo etiqueta="Nombre del colegio"><input name="nombreColegio" defaultValue={institucion.data?.nombreColegio} required maxLength={180} /></Campo><Campo etiqueta="Subtítulo de reportes"><input name="subtituloReportes" defaultValue={institucion.data?.subtituloReportes} required maxLength={220} /></Campo><button className="button primary" disabled={guardarInstitucion.isPending}>{guardarInstitucion.isPending ? "Guardando…" : "Guardar identidad"}</button></form>}</section>
  </section>;
}
