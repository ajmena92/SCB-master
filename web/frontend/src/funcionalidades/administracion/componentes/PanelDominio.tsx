import { useQuery } from "@tanstack/react-query";
import { AlertCircle, RefreshCw, Search } from "lucide-react";
import { useMemo, useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
import { errMsg } from "@/lib/api";
import type { DefinicionDominio, Registro } from "../consultas/dominios";

const mostrar = (valor: unknown): string => {
  if (valor == null || valor === "") return "—";
  if (typeof valor === "object") return JSON.stringify(valor);
  return String(valor);
};

export default function PanelDominio({ definicion }: { definicion: DefinicionDominio }) {
  const [busqueda, setBusqueda] = useState("");
  const consulta = useQuery({
    queryKey: ["dominio", definicion.clave],
    queryFn: definicion.cargar,
  });
  const filas = useMemo(() => {
    const filtro = busqueda.trim().toLocaleLowerCase();
    if (!filtro) return consulta.data ?? [];
    return (consulta.data ?? []).filter((fila: Registro) =>
      JSON.stringify(fila).toLocaleLowerCase().includes(filtro),
    );
  }, [busqueda, consulta.data]);

  return (
    <section className="space-y-6" aria-labelledby={`${definicion.clave}-titulo`}>
      <div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <h2 id={`${definicion.clave}-titulo`} className="font-display text-2xl font-black">
            {definicion.titulo}
          </h2>
          <p className="mt-1 text-sm text-muted-foreground">{definicion.descripcion}</p>
        </div>
        <div className="relative w-full sm:w-72">
          <Search
            className="absolute left-3 top-2.5 h-4 w-4 text-muted-foreground"
            aria-hidden="true"
          />
          <Input
            className="pl-9"
            value={busqueda}
            onChange={(e) => setBusqueda(e.target.value)}
            placeholder="Buscar…"
            aria-label={`Buscar en ${definicion.titulo}`}
          />
        </div>
      </div>
      {consulta.isPending && (
        <div aria-label={`Cargando ${definicion.titulo}`}>
          <Skeleton className="h-64 w-full rounded-2xl" />
        </div>
      )}
      {consulta.isError && (
        <div role="alert" className="rounded-2xl border border-destructive/30 bg-card p-6">
          <div className="flex items-center gap-3">
            <AlertCircle className="h-5 w-5 text-destructive" />
            <p className="text-sm">{errMsg(consulta.error)}</p>
            <Button variant="outline" size="sm" onClick={() => consulta.refetch()}>
              <RefreshCw className="mr-2 h-4 w-4" />
              Reintentar
            </Button>
          </div>
        </div>
      )}
      {consulta.isSuccess && (
        <div className="overflow-x-auto rounded-2xl border bg-card">
          <table className="w-full text-sm">
            <thead className="border-b bg-muted/40">
              <tr>
                {definicion.columnas.map((columna) => (
                  <th key={columna} className="px-4 py-3 text-left font-semibold">
                    {columna}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {filas.length === 0 ? (
                <tr>
                  <td
                    colSpan={definicion.columnas.length}
                    className="py-12 text-center text-muted-foreground"
                  >
                    No hay registros para mostrar.
                  </td>
                </tr>
              ) : (
                filas.map((fila, indice) => (
                  <tr
                    key={String(fila.id ?? fila.idEstudiante ?? fila.idEvento ?? indice)}
                    className="border-b last:border-0 hover:bg-muted/30"
                  >
                    {definicion.columnas.map((columna) => (
                      <td key={columna} className="max-w-xs truncate px-4 py-3">
                        {mostrar(fila[columna])}
                      </td>
                    ))}
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
}
