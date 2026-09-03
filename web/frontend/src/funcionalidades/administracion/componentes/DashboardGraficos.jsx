import { Bar, BarChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";
import { Activity, Coffee, GraduationCap, Users } from "lucide-react";

const COLORS = ["hsl(var(--chart-1))", "hsl(var(--chart-2))", "hsl(var(--chart-3))"];

export function MetricCard({ label, value, detail, icon: Icon }) {
  return (
    <div className="rounded-xl border bg-card p-4 shadow-sm">
      <div className="flex items-center justify-between text-xs font-bold uppercase tracking-wide text-muted-foreground">
        <span>{label}</span>
        <Icon className="h-4 w-4 text-primary" />
      </div>
      <p className="mt-3 font-display text-3xl font-black">{value}</p>
      <p className="mt-1 text-xs text-muted-foreground">{detail}</p>
    </div>
  );
}

export function GroupChart({ title, description, data = [], stacked = false }) {
  return (
    <div className="rounded-xl border bg-card p-4">
      <h3 className="mb-4 font-display text-sm font-bold uppercase tracking-wide">{title}</h3>
      {description && <p className="-mt-2 mb-4 text-xs leading-relaxed text-muted-foreground">{description}</p>}
      {data.length === 0 ? (
        <p className="py-8 text-center text-sm text-muted-foreground">Sin datos</p>
      ) : (
        <ResponsiveContainer width="100%" height={240}>
          <BarChart data={data} layout="vertical" margin={{ left: 8, right: 12 }}>
            <XAxis type="number" allowDecimals={false} hide />
            <YAxis type="category" dataKey="nombre" width={92} tick={{ fontSize: 11 }} />
            <Tooltip />
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
      )}
    </div>
  );
}

export const METRIC_ICONS = { Activity, Coffee, GraduationCap, Users };
