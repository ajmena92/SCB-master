import { useState, type FormEvent } from "react";
import { useMutation } from "@tanstack/react-query";
import { plataformaApi } from "../consultas/plataforma";
import { Aviso, Campo, EncabezadoPagina, Tabla } from "../componentes/ElementosComunes";
import { errMsg } from "@/compartido/consultas/errores_api";
import type { ReporteFila } from "@/compartido/contratos/plataforma";

function csv(filas: ReporteFila[]) {
  const columnas = [...new Set(filas.flatMap(Object.keys))];
  const valor = (v: unknown) => `"${String(v ?? "").replaceAll('"', '""')}"`;
  return [
    columnas.map(valor).join(","),
    ...filas.map((f) => columnas.map((c) => valor(f[c])).join(",")),
  ].join("\n");
}

export default function ReportesOperativos() {
  const [filas, setFilas] = useState<ReporteFila[]>([]);
  const [tipo, setTipo] = useState<"comedor" | "transporte" | "ventas">("comedor");
  const consulta = useMutation({
    mutationFn: ({ t, d, h }: { t: typeof tipo; d: string; h: string }) =>
      plataformaApi.reportes.obtener(t, d, h),
    onSuccess: setFilas,
  });
  function consultar(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    const datos = new FormData(evento.currentTarget);
    consulta.mutate({ t: tipo, d: String(datos.get("desde")), h: String(datos.get("hasta")) });
  }
  function descargar() {
    const enlace = document.createElement("a");
    enlace.href = URL.createObjectURL(
      new Blob([`\ufeff${csv(filas)}`], { type: "text/csv;charset=utf-8" }),
    );
    enlace.download = `reporte-${tipo}.csv`;
    enlace.click();
    URL.revokeObjectURL(enlace.href);
  }
  const columnas = filas.length ? Object.keys(filas[0]) : [];
  return (
    <section>
      <EncabezadoPagina
        titulo="Reportes"
        descripcion="Consulte comedor, transporte o ventas por rango y exporte exactamente las filas mostradas."
        accion={
          <button className="button secondary" disabled={!filas.length} onClick={descargar}>
            Exportar CSV
          </button>
        }
      />
      {consulta.error && <Aviso tipo="error">{errMsg(consulta.error)}</Aviso>}
      <form className="grid grid-cols-1 items-end gap-4 rounded-xl border border-border bg-card p-5 sm:grid-cols-2 lg:grid-cols-4" onSubmit={consultar}>
        <Campo etiqueta="Reporte">
          <select value={tipo} onChange={(e) => setTipo(e.target.value as typeof tipo)}>
            <option value="comedor">Comedor</option>
            <option value="transporte">Transporte</option>
            <option value="ventas">Ventas</option>
          </select>
        </Campo>
        <Campo etiqueta="Desde">
          <input name="desde" type="date" required />
        </Campo>
        <Campo etiqueta="Hasta">
          <input name="hasta" type="date" required />
        </Campo>
        <button className="button primary">Consultar</button>
      </form>
      <Tabla
        columnas={columnas}
        filas={filas.map((f) => columnas.map((c) => String(f[c] ?? "")))}
        vacio={consulta.isPending ? "Consultando…" : "Defina un rango para consultar."}
      />
    </section>
  );
}
