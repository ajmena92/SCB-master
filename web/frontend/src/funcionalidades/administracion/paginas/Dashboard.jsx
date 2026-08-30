import { useState } from "react";
import { useDashboard } from "@/funcionalidades/administracion/hooks/useDashboard";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  CartesianGrid,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import { Activity, Bus, Coffee, GraduationCap, RefreshCw, Search, Users, X } from "lucide-react";
import {
  GroupChart,
  MetricCard,
} from "@/funcionalidades/administracion/componentes/DashboardGraficos";
import { fechaLocalActual } from "@/compartido/utilidades/fecha";
const COLORS = [
  "hsl(var(--chart-1))",
  "hsl(var(--chart-2))",
  "hsl(var(--chart-3))",
  "hsl(var(--chart-4))",
];

export default function DashboardTab() {
  const [fecha, setFecha] = useState(fechaLocalActual);
  const [busqueda, setBusqueda] = useState("");
  const [ruta, setRuta] = useState("");
  const [idEstadoComedor, setIdEstadoComedor] = useState("");
  const [beneficioTransporte, setBeneficioTransporte] = useState("");
  const [seccion, setSeccion] = useState("");
  const [estado, setEstado] = useState("");
  const [horario, setHorario] = useState("");
  const [tipoPersona, setTipoPersona] = useState("estudiante");
  const [pagina, setPagina] = useState(1);
  const filtros = {
    ...(busqueda ? { busqueda } : {}),
    ...(ruta ? { ruta } : {}),
    ...(idEstadoComedor ? { idEstadoComedor } : {}),
    ...(beneficioTransporte ? { beneficioTransporte } : {}),
    ...(seccion ? { seccion } : {}),
    ...(estado ? { estado } : {}),
    ...(horario ? { horario } : {}),
    tipoPersona,
    pagina,
  };
  const {
    data = null,
    error,
    isPending: loading,
    refetch,
    mensajeError,
  } = useDashboard(fecha, filtros);
  const asistencia = data?.asistencia;
  const nominal = data?.nominal?.elementos || [];
  const rutas = data?.porRuta || [];
  const estadosComedor = data?.porEstadoComedor || [];
  const horarios = data?.horarios || [];
  const alertas = data?.alertas || [];
  const esProfesor = tipoPersona === "profesor";
  const vistaDocenteSinContrato = esProfesor && data?.tipoPersona !== "profesor";

  return (
    <div className="space-y-5">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h2 className="font-display text-2xl font-bold tracking-tight">
            Dashboard de asistencia
          </h2>
          <p className="text-sm text-muted-foreground">
            Operación del comedor, asistencia y padrón estudiantil.
          </p>
        </div>
        <div className="flex items-end gap-2">
          <div>
            <label
              htmlFor="dashboard-fecha"
              className="text-xs font-bold uppercase tracking-wide text-muted-foreground"
            >
              Fecha
            </label>
            <Input
              type="date"
              id="dashboard-fecha"
              data-testid="dashboard-fecha"
              value={fecha}
              onChange={(e) => setFecha(e.target.value)}
              className="h-10 w-44"
            />
          </div>
          <div>
            <label
              htmlFor="dashboard-tipo-persona"
              className="text-xs font-bold uppercase tracking-wide text-muted-foreground"
            >
              Vista
            </label>
            <select
              id="dashboard-tipo-persona"
              aria-label="Tipo de persona"
              value={tipoPersona}
              onChange={(e) => {
                setTipoPersona(e.target.value);
                setPagina(1);
              }}
              className="h-10 rounded-md border bg-background px-3 text-sm"
            >
              <option value="estudiante">Estudiantes</option>
              <option value="profesor">Profesores</option>
            </select>
            {!esProfesor && horarios.length > 1 && (
              <select
                aria-label="Filtrar horario"
                value={horario}
                onChange={(e) => {
                  setHorario(e.target.value);
                  setPagina(1);
                }}
                className="h-10 rounded-md border bg-background px-3 text-sm"
              >
                <option value="">Todos los horarios</option>
                {horarios.map((opcion) => (
                  <option key={opcion} value={opcion}>
                    {opcion === "diurno" ? "Diurno" : "Nocturno"}
                  </option>
                ))}
              </select>
            )}
          </div>
          <Button
            variant="outline"
            size="icon"
            data-testid="dashboard-refresh"
            onClick={() => refetch()}
          >
            <RefreshCw className="h-4 w-4" />
          </Button>
        </div>
      </div>

      {loading ? (
        <Skeleton className="h-64 w-full rounded-xl" />
      ) : error ? (
        <div
          role="alert"
          className="rounded-xl border border-destructive/30 bg-destructive/10 p-4 text-sm text-destructive"
        >
          {mensajeError}{" "}
          <Button variant="link" className="h-auto p-0 text-destructive" onClick={() => refetch()}>
            Reintentar
          </Button>
        </div>
      ) : vistaDocenteSinContrato ? (
        <div
          role="status"
          data-testid="dashboard-profesor-no-disponible"
          className="rounded-xl border border-amber-300/50 bg-amber-50 p-5 text-sm text-amber-950"
        >
          La API actual todavía no publica estadísticas de profesores. No se muestran datos
          estudiantiles en esta vista.
        </div>
      ) : (
        <>
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-4">
            <MetricCard
              label={esProfesor ? "Profesores habilitados" : "Asistencia"}
              value={`${asistencia?.porcentaje ?? 0}%`}
              detail={`${asistencia?.presentes ?? 0} de ${asistencia?.total ?? 0} ${esProfesor ? "profesores" : "estudiantes"}`}
              icon={Activity}
            />
            <MetricCard
              label={esProfesor ? "Sin ingreso" : "Sin registro"}
              value={asistencia?.sinRegistro ?? 0}
              detail={`${asistencia?.ausentes ?? 0} ausencias registradas`}
              icon={Users}
            />
            <MetricCard
              label={esProfesor ? "Personas con tiquete" : "Beneficiarios de comedor"}
              value={data?.beneficiariosComedor ?? 0}
              detail={
                esProfesor ? "Requieren tiquete" : `${data?.noBeneficiarios ?? 0} no beneficiarios`
              }
              icon={GraduationCap}
            />
            <MetricCard
              label="Consumo comedor"
              value={data?.consumoComedor ?? 0}
              detail={`Fecha ${fecha}`}
              icon={Coffee}
            />
          </div>
          {alertas.some((alerta) => alerta.cantidad > 0) && (
            <section
              aria-labelledby="dashboard-alertas"
              className="rounded-xl border border-amber-300/60 bg-amber-50/70 p-4"
            >
              <h3 id="dashboard-alertas" className="font-display text-sm font-bold">
                Alertas operativas
              </h3>
              <div className="mt-3 grid gap-2 sm:grid-cols-3">
                {alertas
                  .filter((alerta) => alerta.cantidad > 0)
                  .map((alerta) => (
                    <div key={alerta.tipo} className="rounded-lg bg-white/70 p-3 text-sm">
                      <p className="font-semibold">{alerta.titulo}</p>
                      <p className="mt-1 text-2xl font-black tabular-nums">{alerta.cantidad}</p>
                    </div>
                  ))}
              </div>
            </section>
          )}
          {esProfesor && (
            <section className="grid gap-3 sm:grid-cols-3" aria-label="Tiquetes de profesores">
              <MetricCard
                label="Saldo de tiquetes"
                value={data?.saldoTiquetes ?? 0}
                detail="Saldo disponible acumulado"
                icon={Coffee}
              />
              <MetricCard
                label="Tiquetes reservados"
                value={data?.tiquetesReservados ?? 0}
                detail="Comprometidos para ingreso"
                icon={Activity}
              />
              <MetricCard
                label="Consumo histórico"
                value={data?.tiquetesConsumidos ?? 0}
                detail={`${data?.ingresosHistoricos ?? 0} ingresos registrados`}
                icon={Users}
              />
            </section>
          )}
          <div className="grid grid-cols-1 gap-4 xl:grid-cols-2">
            <div className="rounded-xl border bg-card p-4">
              <h3 className="mb-4 font-display text-sm font-bold uppercase tracking-wide">
                Semana laboral
              </h3>
              <ResponsiveContainer width="100%" height={250}>
                <LineChart data={data?.semana || []} margin={{ left: 0, right: 12 }}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="dia" tick={{ fontSize: 11 }} />
                  <YAxis allowDecimals={false} />
                  <Tooltip />
                  <Line
                    type="monotone"
                    dataKey="presentes"
                    name="Presentes"
                    stroke={COLORS[1]}
                    strokeWidth={3}
                  />
                  <Line
                    type="monotone"
                    dataKey="ausentes"
                    name="Ausentes"
                    stroke={COLORS[3]}
                    strokeWidth={2}
                  />
                </LineChart>
              </ResponsiveContainer>
            </div>
            <div className="rounded-xl border bg-card p-4">
              <h3 className="mb-4 font-display text-sm font-bold uppercase tracking-wide">
                Últimos cinco días laborales
              </h3>
              <ResponsiveContainer width="100%" height={250}>
                <LineChart data={data?.ultimosCincoDias || []} margin={{ left: 0, right: 12 }}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="dia" tick={{ fontSize: 11 }} />
                  <YAxis allowDecimals={false} />
                  <Tooltip />
                  <Line
                    type="monotone"
                    dataKey="porcentaje"
                    name="Asistencia %"
                    stroke={COLORS[0]}
                    strokeWidth={3}
                  />
                </LineChart>
              </ResponsiveContainer>
            </div>
          </div>
          <div className="grid grid-cols-1 gap-4 xl:grid-cols-3">
            <GroupChart title="Por estado de comedor: población" data={estadosComedor} />
            <GroupChart title="Por ruta: población" data={rutas} />
            <GroupChart title="Por estado: asistencia y comedor" data={estadosComedor} stacked />
          </div>
        </>
      )}

      <div className="overflow-hidden rounded-xl border bg-card">
        <div className="flex flex-wrap items-end justify-between gap-3 p-4">
          <div>
            <h3 className="font-display font-bold">Lista nominal</h3>
            <p className="text-sm text-muted-foreground">
              {esProfesor
                ? "Profesores habilitados; solo se muestran en esta vista."
                : "Solo estudiantes; inactivos únicamente con marca histórica."}
            </p>
          </div>
          <div className="flex flex-wrap gap-2">
            <div className="relative">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                id="nominal-search"
                data-testid="nominal-search"
                aria-label="Buscar estudiante"
                placeholder="Buscar estudiante"
                value={busqueda}
                onChange={(event) => {
                  setBusqueda(event.target.value);
                  setPagina(1);
                }}
                className="w-56 pl-9 pr-8"
              />
              {busqueda && (
                <button
                  type="button"
                  aria-label="Limpiar búsqueda"
                  onClick={() => setBusqueda("")}
                  className="absolute right-2 top-1/2 -translate-y-1/2"
                >
                  <X className="h-4 w-4" />
                </button>
              )}
            </div>
            {!esProfesor && (
              <select
                aria-label="Filtrar beneficio de transporte"
                value={beneficioTransporte}
                onChange={(e) => {
                  setBeneficioTransporte(e.target.value);
                  setRuta("");
                  setPagina(1);
                }}
                className="h-10 rounded-md border bg-background px-3 text-sm"
              >
                <option value="">Todo beneficio de transporte</option>
                <option value="beneficiario">Beneficiario</option>
                <option value="no_beneficiario">No beneficiario</option>
              </select>
            )}
            {!esProfesor && (
              <select
                aria-label="Filtrar ruta"
                value={ruta}
                onChange={(e) => {
                  setRuta(e.target.value);
                  setPagina(1);
                }}
                className="h-10 rounded-md border bg-background px-3 text-sm"
              >
                <option value="">Todas las rutas</option>
                {rutas
                  .filter((item) => item.idRuta)
                  .map((item) => (
                    <option key={item.idRuta} value={item.idRuta}>
                      {item.nombre}
                    </option>
                  ))}
              </select>
            )}
            {!esProfesor && (
              <select
                aria-label="Filtrar estado de comedor"
                value={idEstadoComedor}
                onChange={(e) => {
                  setIdEstadoComedor(e.target.value);
                  setPagina(1);
                }}
                className="h-10 rounded-md border bg-background px-3 text-sm"
              >
                <option value="">Todos los estados</option>
                <option value="1">Beneficiario</option>
                <option value="2">No beneficiario</option>
              </select>
            )}
            {!esProfesor && (
              <Input
                aria-label="Filtrar sección"
                placeholder="Sección"
                value={seccion}
                onChange={(e) => {
                  setSeccion(e.target.value);
                  setPagina(1);
                }}
                className="h-10 w-28"
              />
            )}
            <select
              aria-label="Filtrar estado"
              value={estado}
              onChange={(e) => {
                setEstado(e.target.value);
                setPagina(1);
              }}
              className="h-10 rounded-md border bg-background px-3 text-sm"
            >
              <option value="">Todos los estados</option>
              <option value="presente">Presentes</option>
              <option value="ausente">Ausentes</option>
              <option value="tardanza">Tardanzas</option>
              <option value="sin_registro">Sin registro</option>
            </select>
          </div>
        </div>
        <div className="overflow-x-auto">
          <Table data-testid="nominal-table">
            <TableHeader>
              <TableRow>
                <TableHead>{esProfesor ? "Profesor" : "Estudiante"}</TableHead>
                {!esProfesor && <TableHead>Sección</TableHead>}
                {!esProfesor && <TableHead>Ruta</TableHead>}
                <TableHead>{esProfesor ? "Persona" : "Beneficio de comedor"}</TableHead>
                <TableHead>Estado</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {nominal.length === 0 ? (
                <TableRow>
                  <TableCell
                    colSpan={esProfesor ? 3 : 5}
                    className="py-8 text-center text-muted-foreground"
                  >
                    Sin personas para los filtros seleccionados.
                  </TableCell>
                </TableRow>
              ) : (
                nominal.map((row) => (
                  <TableRow key={row.idPersona}>
                    <TableCell className="font-medium">{row.nombreCompleto}</TableCell>
                    {!esProfesor && <TableCell>{row.seccion}</TableCell>}
                    {!esProfesor && (
                      <TableCell>
                        <span className="inline-flex items-center gap-1">
                          <Bus className="h-3 w-3" />
                          {row.ruta}
                        </span>
                      </TableCell>
                    )}
                    <TableCell>{esProfesor ? "Profesor" : row.beneficioComedor}</TableCell>
                    <TableCell>
                      <Badge variant={row.historico ? "secondary" : "default"}>
                        {row.historico ? "Histórico" : row.estado}
                      </Badge>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </div>
        {data?.nominal && (
          <div className="flex items-center justify-between border-t px-4 py-3 text-xs text-muted-foreground">
            <span>
              Mostrando {nominal.length} de {data.nominal.total}{" "}
              {esProfesor ? "profesores" : "estudiantes"}. Página {data.nominal.pagina}.
            </span>
            <div className="flex gap-2">
              <Button
                variant="outline"
                size="sm"
                disabled={pagina <= 1}
                onClick={() => setPagina((actual) => actual - 1)}
              >
                Anterior
              </Button>
              <Button
                variant="outline"
                size="sm"
                disabled={pagina * data.nominal.porPagina >= data.nominal.total}
                onClick={() => setPagina((actual) => actual + 1)}
              >
                Siguiente
              </Button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
