import { useState } from "react";
import { useDashboard } from "@/funcionalidades/administracion/hooks/useDashboard";
import { urlListaControl } from "@/funcionalidades/administracion/consultas/dashboard";
import { useAutenticacion } from "@/aplicacion/estado/ContextoAutenticacion";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { EstadoPanel } from "@/compartido/componentes/Estados";
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
import {
  Activity,
  Bus,
  Coffee,
  Download,
  FileSpreadsheet,
  GraduationCap,
  Printer,
  RefreshCw,
  Search,
  Users,
  X,
} from "lucide-react";
import {
  CapacidadServicio,
  GroupChart,
  MetricCard,
} from "@/funcionalidades/administracion/componentes/DashboardGraficos";
import { fechaLocalActual } from "@/compartido/utilidades/fecha";
import { EncabezadoPagina } from "@/funcionalidades/plataforma/componentes/ElementosComunes";
const COLORS = [
  "var(--chart-1-color)",
  "var(--chart-2-color)",
  "var(--chart-3-color)",
  "var(--chart-4-color)",
];

function etiquetaAsistencia(row) {
  if (row.historico) return "Registro histórico";
  return row.estadoClave === "presente" ? "Ingresó al comedor" : "Aún sin ingreso";
}

function varianteAsistencia(row) {
  return row.historico || row.estadoClave !== "presente" ? "secondary" : "default";
}

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

function ResumenLinea({ datos, tipo }) {
  if (!datos.length) return null;
  return (
    <details className="mt-3 rounded-lg border border-border bg-muted/30 px-3 py-2 text-sm">
      <summary className="cursor-pointer font-medium text-foreground">
        Ver datos del gráfico
      </summary>
      <ul className="mt-2 space-y-1 text-muted-foreground">
        {datos.map((dato) => (
          <li key={dato.dia}>
            <span className="font-medium text-foreground">{dato.dia}:</span>{" "}
            {tipo === "asistencia"
              ? `${dato.presentes ?? 0} ingresos; ${dato.ausentes ?? 0} sin registro.`
              : `${dato.porcentaje ?? 0}% con ingreso registrado.`}
          </li>
        ))}
      </ul>
    </details>
  );
}

export default function DashboardTab() {
  const { session } = useAutenticacion();
  const [fecha, setFecha] = useState(fechaLocalActual);
  const [busqueda, setBusqueda] = useState("");
  const [ruta, setRuta] = useState("");
  const [beneficioTransporte, setBeneficioTransporte] = useState("");
  const [seccion, setSeccion] = useState("");
  const [estado, setEstado] = useState("");
  const [tipoPersona, setTipoPersona] = useState("estudiante");
  const [servicioExportacion, setServicioExportacion] = useState("comedor");
  const [pagina, setPagina] = useState(1);
  const filtros = {
    ...(busqueda ? { busqueda } : {}),
    ...(ruta ? { ruta } : {}),
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
  const casosAnaliticos = data?.casosAnaliticos || [];
  const esProfesor = tipoPersona === "profesor";
  const tendencia = data?.tendenciaVeinteDias || data?.ultimosCincoDias || [];
  const hayRegistrosHistoricos = tendencia.some((dia) => dia.presentes > 0);
  const beneficiariosConIngreso =
    estadosComedor.find((grupo) => grupo.nombre === "Beneficiarios")?.presentes ?? 0;
  const rutasParaGrafico = resumirGruposParaGrafico(rutas, 10, "Otras rutas");
  const capacidad = data?.capacidad;
  const seccionesActivas = data?.seccionesActivas || [];
  const vistaDocenteSinContrato = esProfesor && data?.tipoPersona !== "profesor";
  const puedeExportar =
    session?.tipo === "administracion" &&
    (session.rol === "administrador" || session.permisos?.includes("reportes.leer"));
  const filtrosExportacion = {
    busqueda,
    ruta,
    seccion,
    estado,
    beneficioTransporte,
  };
  const enlaceExportacion = (formato) =>
    urlListaControl(fecha, servicioExportacion, formato, filtrosExportacion);

  return (
    <div className="space-y-5">
      <EncabezadoPagina
        titulo="Operación del comedor"
        descripcion="Consulte el padrón activo, los registros de ingreso y el seguimiento de beneficios."
        accion={
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
                className="h-11 w-44"
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
                  const nuevoTipo = e.target.value;
                  setTipoPersona(nuevoTipo);
                  if (nuevoTipo === "profesor") {
                    setRuta("");
                    setBeneficioTransporte("");
                    setSeccion("");
                  }
                  setPagina(1);
                }}
                className="h-11 rounded-md border bg-background px-3 text-sm"
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
              className="h-11 w-11"
            >
              <RefreshCw className="h-4 w-4" />
            </Button>
          </div>
        }
      />

      {loading ? (
        <EstadoPanel variante="carga">Cargando el dashboard…</EstadoPanel>
      ) : error ? (
        <EstadoPanel
          variante="error"
          accion={
            <Button
              variant="link"
              className="h-auto p-0 text-destructive"
              onClick={() => refetch()}
            >
              Reintentar
            </Button>
          }
        >
          {mensajeError}{" "}
        </EstadoPanel>
      ) : vistaDocenteSinContrato ? (
        <div
          role="status"
          data-testid="dashboard-profesor-no-disponible"
          className="rounded-xl border border-warning/40 bg-warning/10 p-5 text-sm text-foreground"
        >
          La API actual todavía no publica estadísticas de profesores. No se muestran datos
          estudiantiles en esta vista.
        </div>
      ) : (
        <>
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-4">
            <MetricCard
              label={esProfesor ? "Profesores activos" : "Ingresos registrados"}
              value={esProfesor ? asistencia?.total ?? 0 : `${asistencia?.porcentaje ?? 0}%`}
              detail={
                esProfesor
                  ? "Padrón docente activo"
                  : `${asistencia?.presentes ?? 0} de ${asistencia?.total ?? 0} estudiantes activos`
              }
              icon={Activity}
            />
            <MetricCard
              label={esProfesor ? "Ingresaron al comedor" : "Sin registro"}
              value={esProfesor ? asistencia?.presentes ?? 0 : asistencia?.sinRegistro ?? 0}
              detail={
                esProfesor ? "Con ingreso registrado hoy" : "Sin registro de ingreso al comedor"
              }
              icon={Users}
            />
            <MetricCard
              label={esProfesor ? "Sin ingreso" : "Beneficiarios de comedor"}
              value={esProfesor ? asistencia?.sinRegistro ?? 0 : data?.beneficiariosComedor ?? 0}
              detail={
                esProfesor ? "Aún sin ingreso hoy" : `${data?.noBeneficiarios ?? 0} no beneficiarios`
              }
              icon={GraduationCap}
            />
            <MetricCard
              label={esProfesor ? "Cobertura de hoy" : "Cobertura de beneficiarios"}
              value={
                esProfesor
                  ? `${asistencia?.porcentaje ?? 0}%`
                  : `${beneficiariosConIngreso} de ${data?.beneficiariosComedor ?? 0}`
              }
              detail={esProfesor ? "Del padrón docente activo" : "Beneficiarios con ingreso registrado hoy"}
              icon={Coffee}
            />
          </div>
          {!esProfesor && (
            <p className="rounded-lg border border-primary/15 bg-primary/5 px-3 py-2 text-xs text-muted-foreground">
              Padrón activo {fecha.slice(0, 4)}:{" "}
              <strong className="text-foreground">
                {asistencia?.total ?? 0} estudiantes REGULAR
              </strong>{" "}
              · datos al {fecha}.
            </p>
          )}
          {!esProfesor && alertas.some((alerta) => alerta.cantidad > 0) && (
            <section
              aria-labelledby="dashboard-alertas"
              className="rounded-xl border border-warning/35 bg-warning/10 p-4"
            >
              <h3 id="dashboard-alertas" className="font-display text-sm font-bold">
                Alertas operativas
              </h3>
              <div className="mt-3 grid gap-2 sm:grid-cols-3">
                {alertas
                  .filter((alerta) => alerta.cantidad > 0)
                  .map((alerta) => (
                    <div key={alerta.tipo} className="rounded-lg bg-card/70 p-3 text-sm">
                      <p className="font-semibold">{alerta.titulo}</p>
                      <p className="mt-1 text-2xl font-bold tabular-nums">{alerta.cantidad}</p>
                    </div>
                  ))}
              </div>
            </section>
          )}
          <div className="grid grid-cols-1 gap-4 xl:grid-cols-2">
            <div className="rounded-xl border bg-card p-4">
              <h3 className="mb-4 font-display text-sm font-bold uppercase tracking-wide">
                Últimos 5 días hábiles
              </h3>
              <p className="-mt-2 mb-4 text-xs text-muted-foreground">
                Ingresos registrados y {esProfesor ? "profesores" : "estudiantes"} sin registro de ingreso al comedor.
              </p>
              {!hayRegistrosHistoricos ? (
                <p className="flex h-28 items-center justify-center text-center text-sm text-muted-foreground">
                  Aún no hay registros de ingreso al comedor en este período.
                </p>
              ) : (
                <>
                  <div role="img" aria-label="Gráfico de ingresos y estudiantes sin registro de los últimos cinco días hábiles.">
                    <ResponsiveContainer width="100%" height={250}>
                      <LineChart data={data?.semana || []} margin={{ left: 0, right: 12 }}>
                    <CartesianGrid stroke="rgb(var(--border))" strokeDasharray="3 3" />
                    <XAxis
                      dataKey="dia"
                      tick={{ fontSize: 11, fill: "rgb(var(--muted-foreground))" }}
                      axisLine={{ stroke: "rgb(var(--border))" }}
                      tickLine={{ stroke: "rgb(var(--border))" }}
                    />
                    <YAxis
                      allowDecimals={false}
                      tick={{ fill: "rgb(var(--muted-foreground))" }}
                      axisLine={{ stroke: "rgb(var(--border))" }}
                      tickLine={{ stroke: "rgb(var(--border))" }}
                    />
                    <Tooltip
                      contentStyle={{
                        backgroundColor: "rgb(var(--popover))",
                        border: "1px solid rgb(var(--border))",
                        borderRadius: "0.5rem",
                        color: "rgb(var(--popover-foreground))",
                      }}
                      labelStyle={{ color: "rgb(var(--muted-foreground))" }}
                    />
                    <Line
                      type="monotone"
                      dataKey="presentes"
                      name="Ingresos"
                      stroke={COLORS[1]}
                      strokeWidth={3}
                    />
                    <Line
                      type="monotone"
                      dataKey="ausentes"
                      name="Sin registro"
                      stroke={COLORS[3]}
                      strokeWidth={2}
                    />
                      </LineChart>
                    </ResponsiveContainer>
                  </div>
                  <ResumenLinea datos={data?.semana || []} tipo="asistencia" />
                </>
              )}
            </div>
            <div className="rounded-xl border bg-card p-4">
              <h3 className="mb-4 font-display text-sm font-bold uppercase tracking-wide">
                Tendencia de 20 días lectivos
              </h3>
              <p className="-mt-2 mb-4 text-xs text-muted-foreground">
                Porcentaje del padrón activo con ingreso registrado cada día hábil.
              </p>
              {!hayRegistrosHistoricos ? (
                <p className="flex h-28 items-center justify-center text-center text-sm text-muted-foreground">
                  La tendencia aparecerá cuando existan registros de ingreso.
                </p>
              ) : (
                <>
                  <div role="img" aria-label="Gráfico de porcentaje de ingreso registrado durante los últimos veinte días lectivos.">
                    <ResponsiveContainer width="100%" height={250}>
                      <LineChart data={tendencia} margin={{ left: 0, right: 12 }}>
                    <CartesianGrid stroke="rgb(var(--border))" strokeDasharray="3 3" />
                    <XAxis
                      dataKey="dia"
                      tick={{ fontSize: 11, fill: "rgb(var(--muted-foreground))" }}
                      axisLine={{ stroke: "rgb(var(--border))" }}
                      tickLine={{ stroke: "rgb(var(--border))" }}
                    />
                    <YAxis
                      allowDecimals={false}
                      unit="%"
                      tick={{ fill: "rgb(var(--muted-foreground))" }}
                      axisLine={{ stroke: "rgb(var(--border))" }}
                      tickLine={{ stroke: "rgb(var(--border))" }}
                    />
                    <Tooltip
                      contentStyle={{
                        backgroundColor: "rgb(var(--popover))",
                        border: "1px solid rgb(var(--border))",
                        borderRadius: "0.5rem",
                        color: "rgb(var(--popover-foreground))",
                      }}
                      labelStyle={{ color: "rgb(var(--muted-foreground))" }}
                    />
                    <Line
                      type="monotone"
                      dataKey="porcentaje"
                      name="Ingreso registrado"
                      stroke={COLORS[0]}
                      strokeWidth={3}
                    />
                      </LineChart>
                    </ResponsiveContainer>
                  </div>
                  <ResumenLinea datos={tendencia} tipo="tendencia" />
                </>
              )}
            </div>
          </div>
          {!esProfesor && <div className="grid grid-cols-1 gap-4 xl:grid-cols-3">
              <GroupChart
                title="Beneficio de comedor"
                description="Distribución del padrón activo entre beneficiarios y no beneficiarios."
                data={estadosComedor}
              />
              <GroupChart
                title="Rutas de transporte"
                description="Las 9 rutas principales y el resto agrupado. Mide asignación activa, no viajes realizados."
                data={rutasParaGrafico}
              />
              <CapacidadServicio capacidad={capacidad} fecha={fecha} />
            </div>}
          {!esProfesor && <section
            className="overflow-hidden rounded-xl border bg-card"
            aria-labelledby="casos-analiticos"
          >
            <div className="border-b p-4">
              <h3 id="casos-analiticos" className="font-display font-bold">
                Casos para revisión
              </h3>
              <p className="mt-1 text-sm text-muted-foreground">
                Señales históricas de apoyo; no modifican beneficios automáticamente.
              </p>
            </div>
            {casosAnaliticos.length === 0 ? (
              <p className="p-6 text-sm text-muted-foreground">
                Sin casos con historial suficiente para revisar. Las señales se habilitan después de
                al menos tres días de operación con registros.
              </p>
            ) : (
              <div className="overflow-x-auto">
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Estudiante</TableHead>
                      <TableHead>Sección</TableHead>
                      <TableHead>Señal</TableHead>
                      <TableHead>Asistencia</TableHead>
                      <TableHead>Consumos</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {casosAnaliticos.map((caso) => (
                      <TableRow key={`${caso.idPersona}-${caso.senal}`}>
                        <TableCell className="font-medium">{caso.nombreCompleto}</TableCell>
                        <TableCell>{caso.seccion}</TableCell>
                        <TableCell>{caso.senal}</TableCell>
                        <TableCell>{caso.porcentajeAsistencia}%</TableCell>
                        <TableCell>{caso.consumosComedor}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </div>
            )}
          </section>}
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
          {!esProfesor && puedeExportar && (
            <div className="flex flex-wrap items-end gap-2" aria-label="Exportar lista de control">
              <div>
                <label
                  htmlFor="servicio-exportacion"
                  className="text-xs font-bold uppercase tracking-wide text-muted-foreground"
                >
                  Servicio
                </label>
                <select
                  id="servicio-exportacion"
                  aria-label="Servicio de la lista a exportar"
                  value={servicioExportacion}
                  onChange={(evento) => setServicioExportacion(evento.target.value)}
                  className="mt-1 h-11 rounded-md border bg-background px-3 text-sm"
                >
                  <option value="comedor">Comedor</option>
                  <option value="transporte">Transporte</option>
                </select>
              </div>
              <Button asChild variant="default" size="sm">
                <a href={enlaceExportacion("xlsx")} download>
                  <FileSpreadsheet aria-hidden="true" /> Excel
                </a>
              </Button>
              <Button asChild variant="outline" size="sm">
                <a href={enlaceExportacion("csv")} download>
                  <Download aria-hidden="true" /> CSV
                </a>
              </Button>
              <Button asChild variant="outline" size="sm">
                <a href={enlaceExportacion("pdf")} target="_blank" rel="noopener noreferrer">
                  <Printer aria-hidden="true" /> Imprimir / PDF
                </a>
              </Button>
            </div>
          )}
          <div className="flex flex-wrap gap-2">
            <div className="relative">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                id="nominal-search"
                data-testid="nominal-search"
                aria-label={esProfesor ? "Buscar profesor" : "Buscar estudiante"}
                placeholder={esProfesor ? "Buscar profesor" : "Buscar estudiante"}
                value={busqueda}
                onChange={(event) => {
                  setBusqueda(event.target.value);
                  setPagina(1);
                }}
                className="h-11 w-56 pl-9 pr-12"
              />
              {busqueda && (
                <button
                  type="button"
                  aria-label="Limpiar búsqueda"
                  onClick={() => setBusqueda("")}
                  className="absolute right-1 top-1/2 grid h-11 w-11 -translate-y-1/2 place-items-center rounded-md text-muted-foreground hover:bg-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                >
                  <X className="h-4 w-4" />
                </button>
              )}
            </div>
            {!esProfesor && (
              <select
                aria-label="Filtrar asignación de transporte"
                value={beneficioTransporte}
                onChange={(e) => {
                  setBeneficioTransporte(e.target.value);
                  setRuta("");
                  setPagina(1);
                }}
                className="h-11 rounded-md border bg-background px-3 text-sm"
              >
                <option value="">Toda asignación de transporte</option>
                <option value="beneficiario">Con ruta asignada</option>
                <option value="no_beneficiario">Sin ruta asignada</option>
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
                className="h-11 rounded-md border bg-background px-3 text-sm"
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
                aria-label="Filtrar sección"
                value={seccion}
                onChange={(e) => {
                  setSeccion(e.target.value);
                  setPagina(1);
                }}
                className="h-11 rounded-md border bg-background px-3 text-sm"
              >
                <option value="">Todas las secciones</option>
                {seccionesActivas.map((nivel) => (
                  <optgroup key={nivel.etiqueta} label={nivel.etiqueta}>
                    {nivel.secciones.map((seccionDisponible) => (
                      <option key={seccionDisponible} value={seccionDisponible}>
                        {seccionDisponible}
                      </option>
                    ))}
                  </optgroup>
                ))}
              </select>
            )}
            <select
              aria-label="Filtrar asistencia de hoy"
              value={estado}
              onChange={(e) => {
                setEstado(e.target.value);
                setPagina(1);
              }}
              className="h-11 rounded-md border bg-background px-3 text-sm"
            >
              <option value="">Toda la asistencia de hoy</option>
              <option value="presente">Ingresaron al comedor</option>
              <option value="sin_registro">Aún sin ingreso</option>
            </select>
          </div>
        </div>
        <div className="hidden overflow-x-auto md:block">
          <Table data-testid="nominal-table">
            <TableHeader>
              <TableRow>
                <TableHead>{esProfesor ? "Profesor" : "Estudiante"}</TableHead>
                {!esProfesor && <TableHead>Sección</TableHead>}
                {!esProfesor && <TableHead>Ruta</TableHead>}
                <TableHead>{esProfesor ? "Identificación" : "Beneficio de comedor"}</TableHead>
                <TableHead>Asistencia hoy</TableHead>
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
                    <TableCell>{esProfesor ? row.identificacion : row.beneficioComedor}</TableCell>
                    <TableCell>
                      <Badge variant={varianteAsistencia(row)}>
                        {etiquetaAsistencia(row)}
                      </Badge>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </div>
        <div className="grid gap-3 p-4 md:hidden" data-testid="nominal-cards">
          {nominal.length === 0 ? (
            <p className="py-4 text-center text-sm text-muted-foreground">
              Sin personas para los filtros seleccionados.
            </p>
          ) : (
            nominal.map((row) => (
              <article className="rounded-lg border border-border bg-muted/20 p-3" key={row.idPersona}>
                <div className="flex items-start justify-between gap-3">
                  <p className="font-medium text-foreground">{row.nombreCompleto}</p>
                  <Badge variant={varianteAsistencia(row)}>
                    {etiquetaAsistencia(row)}
                  </Badge>
                </div>
                <dl className="mt-3 grid gap-2 text-sm sm:grid-cols-2">
                  {!esProfesor && (
                    <div>
                      <dt className="text-xs text-muted-foreground">Sección</dt>
                      <dd>{row.seccion}</dd>
                    </div>
                  )}
                  {!esProfesor && (
                    <div>
                      <dt className="text-xs text-muted-foreground">Ruta</dt>
                      <dd className="flex items-center gap-1"><Bus aria-hidden="true" className="h-3.5 w-3.5" />{row.ruta}</dd>
                    </div>
                  )}
                  <div>
                    <dt className="text-xs text-muted-foreground">{esProfesor ? "Identificación" : "Comedor"}</dt>
                    <dd>{esProfesor ? row.identificacion : row.beneficioComedor}</dd>
                  </div>
                </dl>
              </article>
            ))
          )}
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
