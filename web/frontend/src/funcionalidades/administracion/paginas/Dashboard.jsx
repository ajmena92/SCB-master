import { useState } from "react";
import { useDashboard } from "@/funcionalidades/administracion/hooks/useDashboard";
import { Skeleton } from "@/components/ui/skeleton";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { BarChart, Bar, XAxis, YAxis, ResponsiveContainer, Cell, Tooltip } from "recharts";
import { Users, Clock, GraduationCap, RefreshCw, Download, Search, X } from "lucide-react";
import { downloadCSV } from "@/lib/csv";

const COLORS = [
  "hsl(var(--chart-1))",
  "hsl(var(--chart-2))",
  "hsl(var(--chart-3))",
  "hsl(var(--chart-4))",
  "hsl(var(--chart-5))",
];

export function filterNominal(rows, query) {
  const term = query.trim().toLocaleLowerCase();
  if (!term) return rows;

  return rows.filter((row) =>
    [row.NombreCompleto, row.Cedula, row.Seccion].some((value) =>
      String(value || "")
        .toLocaleLowerCase()
        .includes(term),
    ),
  );
}

function Chart({ title, data, icon: Icon }) {
  return (
    <div className="bg-card border rounded-lg p-6">
      <div className="flex items-center gap-2 mb-4">
        <Icon className="h-4 w-4 text-primary" />
        <h3 className="font-display font-bold text-sm uppercase tracking-wide">{title}</h3>
      </div>
      {data.length === 0 ? (
        <p className="text-sm text-muted-foreground py-8 text-center">Sin datos</p>
      ) : (
        <ResponsiveContainer width="100%" height={200}>
          <BarChart data={data} layout="vertical" margin={{ left: 10, right: 20 }}>
            <XAxis type="number" allowDecimals={false} hide />
            <YAxis type="category" dataKey="nombre" width={90} tick={{ fontSize: 12 }} />
            <Tooltip cursor={{ fill: "hsl(var(--muted))" }} />
            <Bar dataKey="total" radius={[0, 6, 6, 0]}>
              {data.map((item, index) => (
                <Cell key={item.nombre} fill={COLORS[index % COLORS.length]} />
              ))}
            </Bar>
          </BarChart>
        </ResponsiveContainer>
      )}
    </div>
  );
}

export default function DashboardTab() {
  const [fecha, setFecha] = useState(() => new Date().toISOString().slice(0, 10));
  const [busqueda, setBusqueda] = useState("");
  const { data = null, error, isPending: loading, refetch, mensajeError } = useDashboard(fecha);
  const nominal = data?.nominal || [];

  const nominalFiltrado = filterNominal(nominal, busqueda);

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h2 className="font-display text-2xl font-bold tracking-tight">
            Dashboard de asistencia
          </h2>
          <p className="text-sm text-muted-foreground">
            Confirmaciones en tiempo real por horario, sección y beca.
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
          <button
            data-testid="dashboard-refresh"
            onClick={() => refetch()}
            className="h-10 px-3 rounded-md border bg-card hover:bg-muted transition-colors"
          >
            <RefreshCw className="h-4 w-4" />
          </button>
        </div>
      </div>

      {loading ? (
        <Skeleton className="h-32 w-full rounded-lg" />
      ) : error ? (
        <div
          role="alert"
          className="rounded-lg border border-destructive/30 bg-destructive/10 p-4 text-sm text-destructive"
        >
          {mensajeError}{" "}
          <Button variant="link" className="h-auto p-0 text-destructive" onClick={() => refetch()}>
            Reintentar
          </Button>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
          <div className="bg-secondary text-white rounded-lg p-6 flex flex-col justify-between">
            <div className="flex items-center gap-2 text-white/70 text-xs uppercase tracking-widest font-bold">
              <Users className="h-4 w-4" /> Total confirmado
            </div>
            <p className="font-display text-5xl font-black mt-4" data-testid="total-confirmado">
              {data?.totalConfirmado ?? 0}
            </p>
          </div>
          <div className="md:col-span-3 grid grid-cols-1 lg:grid-cols-3 gap-6">
            <Chart title="Por horario" data={data?.porHorario || []} icon={Clock} />
            <Chart title="Por sección" data={data?.porSeccion || []} icon={GraduationCap} />
            <Chart title="Por beca" data={data?.porBeca || []} icon={GraduationCap} />
          </div>
        </div>
      )}

      <div className="bg-card border rounded-lg overflow-hidden">
        <div className="p-6 pb-3 flex flex-wrap items-center justify-between gap-4">
          <div>
            <h3 className="font-display font-bold">Lista nominal</h3>
            <p className="text-sm text-muted-foreground">
              {busqueda ? `${nominalFiltrado.length} de ${nominal.length}` : nominal.length}{" "}
              registro(s) el {fecha}
            </p>
          </div>
          <div className="flex flex-wrap items-center gap-2">
            <div className="relative w-full sm:w-72">
              <Search
                aria-hidden="true"
                className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground"
              />
              <Input
                id="nominal-search"
                data-testid="nominal-search"
                type="search"
                aria-label="Buscar estudiante en la lista nominal"
                placeholder="Buscar por nombre, cédula o sección"
                value={busqueda}
                onChange={(event) => setBusqueda(event.target.value)}
                className="pl-9 pr-9"
              />
              {busqueda && (
                <button
                  type="button"
                  data-testid="nominal-search-clear"
                  aria-label="Limpiar búsqueda"
                  onClick={() => setBusqueda("")}
                  className="absolute right-2 top-1/2 -translate-y-1/2 rounded-sm p-1 text-muted-foreground hover:bg-muted hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                >
                  <X className="h-4 w-4" />
                </button>
              )}
            </div>
            <Button
              variant="outline"
              size="sm"
              data-testid="export-nominal-csv"
              disabled={nominalFiltrado.length === 0}
              onClick={() =>
                downloadCSV(
                  `nominal_${fecha}.csv`,
                  [
                    { key: "NombreCompleto", label: "Estudiante" },
                    { key: "Cedula", label: "Cédula" },
                    { key: "Horario", label: "Horario" },
                    { key: "Seccion", label: "Sección" },
                    { key: "TipoBeca", label: "Beca" },
                    { key: "Estado", label: "Estado" },
                    { key: "Origen", label: "Origen" },
                    { key: "MotivoCorreccion", label: "Motivo" },
                  ],
                  nominalFiltrado,
                )
              }
            >
              <Download className="h-4 w-4 mr-1" /> Exportar CSV
            </Button>
          </div>
        </div>
        <div className="overflow-x-auto">
          <Table data-testid="nominal-table">
            <TableHeader>
              <TableRow>
                <TableHead>Estudiante</TableHead>
                <TableHead>Cédula</TableHead>
                <TableHead>Horario</TableHead>
                <TableHead>Sección</TableHead>
                <TableHead>Beca</TableHead>
                <TableHead>Estado</TableHead>
                <TableHead>Origen</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {nominal.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={7} className="text-center text-muted-foreground py-8">
                    Sin confirmaciones
                  </TableCell>
                </TableRow>
              ) : nominalFiltrado.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={7} className="text-center text-muted-foreground py-8">
                    No se encontraron estudiantes para la búsqueda.
                  </TableCell>
                </TableRow>
              ) : (
                nominalFiltrado.map((r) => (
                  <TableRow key={`${r.IdUsuario}-${r.Fecha}`} className="hover:bg-muted/40">
                    <TableCell className="font-medium">{r.NombreCompleto}</TableCell>
                    <TableCell>{r.Cedula}</TableCell>
                    <TableCell>{r.Horario}</TableCell>
                    <TableCell>{r.Seccion}</TableCell>
                    <TableCell>{r.TipoBeca}</TableCell>
                    <TableCell>
                      <Badge
                        variant={r.Estado === "Cancelada" ? "secondary" : "default"}
                        className={
                          r.Estado === "Confirmada"
                            ? "bg-success text-white"
                            : r.Estado === "Corregida"
                              ? "bg-primary text-white"
                              : ""
                        }
                      >
                        {r.Estado}
                      </Badge>
                    </TableCell>
                    <TableCell>{r.Origen}</TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </div>
      </div>
    </div>
  );
}
