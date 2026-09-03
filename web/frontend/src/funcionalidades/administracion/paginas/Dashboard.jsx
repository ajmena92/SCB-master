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

function resumirGruposParaGrafico(grupos, limite, etiquetaResto) {
  if (grupos.length <= limite) return grupos;

  const visibles = grupos.slice(0, limite - 1);
  const resto = grupos.slice(limite - 1);
  return [
    ...visibles,
    {
      nombre: etiquetaResto,
      total: resto.reduce((acumulado, grupo) => acumulado + grupo.total, 0),
    },
  ];
}

export default function DashboardTab() {
  const [fecha, setFecha] = useState(fechaLocalActual);
  const [busqueda, setBusqueda] = useState("");
  const [ruta, setRuta] = useState("");
  const [idEstadoComedor, setIdEstadoComedor] = useState("");
  const [beneficioTransporte, setBeneficioTransporte] = useState("");
  const [seccion, setSeccion] = useState("");
  const [estado, setEstado] = useState("");
  const [tipoPersona, setTipoPersona] = useState("estudiante");
  const [pagina, setPagina] = useState(1);
  const filtros = {
    ...(busqueda ? { busqueda } : {}),
    ...(ruta ? { ruta } : {}),
    ...(idEstadoComedor ? { idEstadoComedor } : {}),
    ...(beneficioTransporte ? { beneficioTransporte } : {}),
    ...(seccion ? { seccion } : {}),
    ...(estado ? { estado } : {}),
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
  const alertas = data?.alertas || [];
  const secciones = data?.porSeccion || [];
  const casosAnaliticos = data?.casosAnaliticos || [];
  const esProfesor = tipoPersona === "profesor";
  const tendencia = data?.tendenciaVeinteDias || data?.ultimosCincoDias || [];
  const hayRegistrosHistoricos = tendencia.some((dia) => dia.presentes > 0);
  const beneficiariosConIngreso = estadosComedor.find(
    (grupo) => grupo.nombre === "Beneficiarios",
  )?.presentes ?? 0;
  const rutasParaGrafico = resumirGruposParaGrafico(rutas, 10, "Otras rutas");
  const seccionesParaGrafico = resumirGruposParaGrafico(secciones, 10, "Otras secciones");
  const vistaDocenteSinContrato = esProfesor && data?.tipoPersona !== "profesor";

  return (
    <div className="space-y-5">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h2 className="font-display text-2xl font-bold tracking-tight">
            Operación del comedor
          </h2>
          <p className="text-sm text-muted-foreground">
            Padrón activo, registros de ingreso y seguimiento de beneficios.
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
          </div>
          <Button
            variant="outline"
            size="icon"
            aria-label="Actualizar dashboard"
            title="Actualizar dashboard"
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
              label={esProfesor ? "Profesores habilitados" : "Ingresos registrados"}
              value={`${asistencia?.porcentaje ?? 0}%`}
              detail={`${asistencia?.presentes ?? 0} de ${asistencia?.total ?? 0} ${esProfesor ? "profesores" : "estudiantes activos"}`}
              icon={Activity}
            />
            <MetricCard
              label={esProfesor ? "Sin ingreso" : "Sin registro"}
              value={asistencia?.sinRegistro ?? 0}
              detail={esProfesor ? "Sin ingreso registrado" : "Sin registro de ingreso al comedor"}
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
              label={esProfesor ? "Consumo comedor" : "Cobertura de beneficiarios"}
              value={esProfesor ? data?.consumoComedor ?? 0 : `${beneficiariosConIngreso} de ${data?.beneficiariosComedor ?? 0}`}
              detail={esProfesor ? `Fecha ${fecha}` : "Beneficiarios con ingreso registrado hoy"}
              icon={Coffee}
            />
          </div>
          {!esProfesor && (
            <p className="rounded-lg border border-primary/15 bg-primary/5 px-3 py-2 text-xs text-muted-foreground">
              Padrón activo 2026: <strong className="text-foreground">{asistencia?.total ?? 0} estudiantes REGULAR</strong> · datos al {fecha}.
            </p>
          )}
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
                Últimos 5 días hábiles
              </h3>
              <p className="-mt-2 mb-4 text-xs text-muted-foreground">Ingresos registrados y estudiantes sin registro de ingreso al comedor.</p>
              {!hayRegistrosHistoricos ? (
                <p className="flex h-28 items-center justify-center text-center text-sm text-muted-foreground">Aún no hay registros de ingreso al comedor en este período.</p>
              ) : (
                <ResponsiveContainer width="100%" height={250}>
                  <LineChart data={data?.semana || []} margin={{ left: 0, right: 12 }}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="dia" tick={{ fontSize: 11 }} />
                    <YAxis allowDecimals={false} />
                    <Tooltip />
                    <Line type="monotone" dataKey="presentes" name="Ingresos" stroke={COLORS[1]} strokeWidth={3} />
                    <Line type="monotone" dataKey="ausentes" name="Sin registro" stroke={COLORS[3]} strokeWidth={2} />
                  </LineChart>
                </ResponsiveContainer>
              )}
            </div>
            <div className="rounded-xl border bg-card p-4">
              <h3 className="mb-4 font-display text-sm font-bold uppercase tracking-wide">
                Tendencia de 20 días lectivos
              </h3>
              <p className="-mt-2 mb-4 text-xs text-muted-foreground">Porcentaje del padrón activo con ingreso registrado cada día hábil.</p>
              {!hayRegistrosHistoricos ? (
                <p className="flex h-28 items-center justify-center text-center text-sm text-muted-foreground">La tendencia aparecerá cuando existan registros de ingreso.</p>
              ) : (
                <ResponsiveContainer width="100%" height={250}>
                  <LineChart data={tendencia} margin={{ left: 0, right: 12 }}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="dia" tick={{ fontSize: 11 }} />
                    <YAxis allowDecimals={false} unit="%" />
                    <Tooltip />
                    <Line type="monotone" dataKey="porcentaje" name="Ingreso registrado" stroke={COLORS[0]} strokeWidth={3} />
                  </LineChart>
                </ResponsiveContainer>
              )}
            </div>
          </div>
          <div className="grid grid-cols-1 gap-4 xl:grid-cols-3">
            <GroupChart title="Beneficio de comedor" description="Distribución del padrón activo entre beneficiarios y no beneficiarios." data={estadosComedor} />
            <GroupChart title="Rutas de transporte" description="Las 9 rutas principales y el resto agrupado. Mide asignación activa, no viajes realizados." data={rutasParaGrafico} />
            <GroupChart title="Secciones" description="Las 9 secciones con mayor población y el resto agrupado." data={seccionesParaGrafico} />
          </div>
          <section className="overflow-hidden rounded-xl border bg-card" aria-labelledby="casos-analiticos">
            <div className="border-b p-4">
              <h3 id="casos-analiticos" className="font-display font-bold">Casos para revisión</h3>
              <p className="mt-1 text-sm text-muted-foreground">
                Señales históricas de apoyo; no modifican beneficios automáticamente.
              </p>
            </div>
            {casosAnaliticos.length === 0 ? (
              <p className="p-6 text-sm text-muted-foreground">Sin casos con historial suficiente para revisar. Las señales se habilitan después de al menos tres días de operación con registros.</p>
            ) : (
              <div className="overflow-x-auto">
                <Table>
                  <TableHeader><TableRow><TableHead>Estudiante</TableHead><TableHead>Sección</TableHead><TableHead>Señal</TableHead><TableHead>Asistencia</TableHead><TableHead>Consumos</TableHead></TableRow></TableHeader>
                  <TableBody>{casosAnaliticos.map((caso) => (
                    <TableRow key={`${caso.idPersona}-${caso.senal}`}>
                      <TableCell className="font-medium">{caso.nombreCompleto}</TableCell>
                      <TableCell>{caso.seccion}</TableCell>
                      <TableCell>{caso.senal}</TableCell>
                      <TableCell>{caso.porcentajeAsistencia}%</TableCell>
                      <TableCell>{caso.consumosComedor}</TableCell>
                    </TableRow>
                  ))}</TableBody>
                </Table>
              </div>
            )}
          </section>
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
