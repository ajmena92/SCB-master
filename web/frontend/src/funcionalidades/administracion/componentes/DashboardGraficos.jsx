import { Bar, BarChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";
import { Activity, Coffee, GraduationCap, Users } from "lucide-react";

const COLORS = [
  "var(--chart-1-color)",
  "var(--chart-2-color)",
  "var(--chart-3-color)",
];

export function MetricCard({ label, value, detail, icon: Icon }) {
  return (
    <div className="rounded-xl border bg-card p-4 shadow-sm">
      <div className="flex items-center justify-between text-xs font-bold uppercase tracking-wide text-muted-foreground">
        <span>{label}</span>
        <Icon className="h-4 w-4 text-primary" />
      </div>
      <p className="mt-3 font-display text-3xl font-bold">{value}</p>
      <p className="mt-1 text-xs text-muted-foreground">{detail}</p>
    </div>
  );
}

export function CapacidadServicio({ capacidad, fecha }) {
  const total = capacidad?.totalEstudiantes ?? 0;
  const confirmados = capacidad?.confirmados ?? 0;
  const asistieron = capacidad?.asistieron ?? 0;
  const pendientes = capacidad?.pendientes ?? 0;
  const resumenPendientes =
    pendientes > 0
      ? `${pendientes} confirmación${pendientes === 1 ? "" : "es"} aún no registra ingreso.`
      : "No hay confirmaciones pendientes de ingreso.";

  return (
    <section className="rounded-xl border bg-card p-4" aria-labelledby="capacidad-servicio">
      <h3 id="capacidad-servicio" className="font-display text-sm font-bold uppercase tracking-wide">
        Capacidad estimada
      </h3>
      <p className="mt-1 text-xs leading-relaxed text-muted-foreground">
        Padrón, confirmaciones y asistencia del servicio del {fecha}.
      </p>
      <dl className="mt-4 grid grid-cols-3 gap-2">
        <div className="rounded-lg bg-muted/50 p-3">
          <dt className="text-xs text-muted-foreground">Estudiantes</dt>
          <dd className="mt-1 font-display text-2xl font-bold tabular-nums">{total}</dd>
        </div>
        <div className="rounded-lg bg-primary/10 p-3">
          <dt className="text-xs text-muted-foreground">Confirmaron</dt>
          <dd className="mt-1 font-display text-2xl font-bold tabular-nums text-primary">
            {confirmados}
          </dd>
        </div>
        <div className="rounded-lg bg-success/10 p-3">
          <dt className="text-xs text-muted-foreground">Asistieron</dt>
          <dd className="mt-1 font-display text-2xl font-bold tabular-nums text-success">
            {asistieron}
          </dd>
        </div>
      </dl>
      <p className="mt-3 text-sm text-muted-foreground">{resumenPendientes}</p>
    </section>
  );
}

export function GroupChart({ title, description, data = [], stacked = false }) {
  return (
    <div className="rounded-xl border bg-card p-4">
      <h3 className="mb-4 font-display text-sm font-bold uppercase tracking-wide">{title}</h3>
      {description && (
        <p className="-mt-2 mb-4 text-xs leading-relaxed text-muted-foreground">{description}</p>
      )}
      {data.length === 0 ? (
        <p className="py-8 text-center text-sm text-muted-foreground">Sin datos</p>
      ) : (
        <>
          <div role="img" aria-label={`${title}. ${description || "Distribución de datos."}`}>
            <ResponsiveContainer width="100%" height={240}>
              <BarChart data={data} layout="vertical" margin={{ left: 8, right: 12 }}>
            <XAxis type="number" allowDecimals={false} hide />
            <YAxis
              type="category"
              dataKey="nombre"
              width={92}
              tick={{ fontSize: 11, fill: "rgb(var(--muted-foreground))" }}
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
            {stacked ? (
              <>
                <Bar dataKey="presentes" name="Presentes" stackId="a" fill={COLORS[1]} />
                <Bar dataKey="consumo" name="Comedor" stackId="b" fill={COLORS[2]} />
              </>
            ) : (
              <Bar dataKey="total" name="Estudiantes" fill={COLORS[0]} radius={[0, 5, 5, 0]} />
            )}
              </BarChart>
            </ResponsiveContainer>
          </div>
          <details className="mt-3 rounded-lg border border-border bg-muted/30 px-3 py-2 text-sm">
            <summary className="cursor-pointer font-medium text-foreground">Ver datos del gráfico</summary>
            <ul className="mt-2 space-y-1 text-muted-foreground">
              {data.map((grupo) => (
                <li key={grupo.nombre}>
                  <span className="font-medium text-foreground">{grupo.nombre}:</span>{" "}
                  {stacked
                    ? `${grupo.presentes ?? 0} presentes; ${grupo.consumo ?? 0} consumos.`
                    : `${grupo.total ?? 0} estudiantes.`}
                </li>
              ))}
            </ul>
          </details>
        </>
      )}
    </div>
  );
}

export const METRIC_ICONS = { Activity, Coffee, GraduationCap, Users };
