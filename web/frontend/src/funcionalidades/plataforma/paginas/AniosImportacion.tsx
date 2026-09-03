import { useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { plataformaApi } from "../consultas/plataforma";
import {
  Aviso,
  Campo,
  EncabezadoPagina,
  EstadoCarga,
  Tabla,
} from "../componentes/ElementosComunes";
import { errMsg } from "@/compartido/consultas/errores_api";
import type { CredencialTemporal, ResumenImportacion } from "@/compartido/contratos/plataforma";

function descargarCredenciales(credenciales: CredencialTemporal[]) {
  const escapar = (valor: string) => `"${valor.replaceAll('"', '""')}"`;
  const contenido = [
    ["Código", "Nombre", "PIN temporal"].map(escapar).join(","),
    ...credenciales.map((fila) =>
      [fila.cedula, fila.nombre, fila.pinTemporal].map(escapar).join(","),
    ),
  ].join("\n");
  const url = URL.createObjectURL(
    new Blob([`\ufeff${contenido}`], { type: "text/csv;charset=utf-8" }),
  );
  const enlace = document.createElement("a");
  enlace.href = url;
  enlace.download = "credenciales-importacion.csv";
  enlace.click();
  URL.revokeObjectURL(url);
}

export default function AniosImportacion() {
  const cliente = useQueryClient();
  const [archivo, setArchivo] = useState<File>();
  const [anio, setAnio] = useState(0);
  const [resumen, setResumen] = useState<ResumenImportacion>();
  const [credenciales, setCredenciales] = useState<CredencialTemporal[]>([]);
  const anios = useQuery({ queryKey: ["anios"], queryFn: plataformaApi.anios.listar });
  const crear = useMutation({
    mutationFn: plataformaApi.anios.crear,
    onSuccess: () => cliente.invalidateQueries({ queryKey: ["anios"] }),
  });
  const activar = useMutation({
    mutationFn: plataformaApi.anios.activar,
    onSuccess: () => cliente.invalidateQueries({ queryKey: ["anios"] }),
  });
  const previsualizar = useMutation({
    mutationFn: ({ f, a }: { f: File; a: number }) =>
      plataformaApi.importaciones.previsualizar(f, a),
    onSuccess: setResumen,
  });
  const confirmar = useMutation({
    mutationFn: plataformaApi.importaciones.confirmar,
    onSuccess: (resultado) => {
      setResumen(undefined);
      setCredenciales(resultado.credenciales);
      cliente.invalidateQueries();
    },
  });

  function nuevoAnio(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    const datos = new FormData(evento.currentTarget);
    crear.mutate({ anio: Number(datos.get("anio")), vigente: datos.get("vigente") === "on" });
  }
  const error =
    anios.error || crear.error || activar.error || previsualizar.error || confirmar.error;
  return (
    <section>
      <EncabezadoPagina
        titulo="Años lectivos e importación"
        descripcion="Cree o seleccione el año lectivo, importe el padrón anual y confirme únicamente cuando el resumen sea correcto. Becas y rutas se gestionan después de la importación."
      />
      {error && <Aviso tipo="error">{errMsg(error)}</Aviso>}
      {confirmar.isSuccess && (
        <Aviso tipo="exito">
          Importación confirmada.
          {credenciales.length > 0 && (
            <>
              {" "}
              Descargue ahora las {credenciales.length} credenciales temporales; no se almacenan en
              el navegador.
              <button
                className="button secondary credentials-download"
                type="button"
                onClick={() => descargarCredenciales(credenciales)}
              >
                Descargar credenciales CSV
              </button>
            </>
          )}
        </Aviso>
      )}
      <div className="split-layout">
        <div>
          <h2>Años lectivos</h2>
          <form className="action-panel form-grid" onSubmit={nuevoAnio}>
            <Campo etiqueta="Año">
              <input name="anio" type="number" min="2020" max="2100" required />
            </Campo>
            <label className="check">
              <input name="vigente" type="checkbox" /> Marcar vigente
            </label>
            <button className="button primary">Crear año</button>
          </form>
          {anios.isLoading ? (
            <EstadoCarga />
          ) : (
            <Tabla
              columnas={["Año", "Estado", "Acción"]}
              filas={(anios.data?.elementos ?? []).map((a) => [
                a.anio,
                a.cerrado ? "Cerrado" : a.vigente ? "Vigente" : "Preparación",
                a.vigente || a.cerrado ? (
                  "—"
                ) : (
                  <button
                    key={`activar-${a.id}`}
                    className="button link"
                    onClick={() => activar.mutate(a.id)}
                  >
                    Activar
                  </button>
                ),
              ])}
            />
          )}
        </div>
        <div>
          <h2>Importar padrón Excel</h2>
          <div className="action-panel stack">
            <Campo etiqueta="Año de destino">
              <select value={anio} onChange={(e) => setAnio(Number(e.target.value))}>
                <option value={0}>Seleccione…</option>
                {anios.data?.elementos
                  .filter((a) => !a.cerrado)
                  .map((a) => (
                    <option value={a.anio} key={a.id}>
                      {a.anio}
                    </option>
                  ))}
              </select>
            </Campo>
            <Campo etiqueta="Archivo .xlsx">
              <input type="file" accept=".xlsx" onChange={(e) => setArchivo(e.target.files?.[0])} />
            </Campo>
            <Aviso>El archivo debe contener cédula, nombres, tipo y, para estudiantes, sección. No incluya beca ni ruta.</Aviso>
            <button
              className="button primary"
              disabled={!archivo || !anio || previsualizar.isPending}
              onClick={() => archivo && previsualizar.mutate({ f: archivo, a: anio })}
            >
              Previsualizar sin guardar
            </button>
          </div>
        </div>
      </div>
      {resumen && (
        <div className="preview-panel">
          <h2>Vista previa</h2>
          <div className="metrics">
            <span>
              <b>{resumen.filas}</b> filas
            </span>
            <span>
              <b>{resumen.altas}</b> altas
            </span>
            <span>
              <b>{resumen.cambios}</b> cambios
            </span>
            <span>
              <b>{resumen.desactivaciones}</b> desactivaciones
            </span>
            <span>
              <b>{resumen.errores}</b> errores
            </span>
          </div>
          <Tabla
            columnas={["Fila", "Estado", "Detalle"]}
            filas={resumen.detalle.map((d) => [d.fila, d.estado, d.mensaje])}
          />
          <button
            className="button primary"
            disabled={resumen.errores > 0 || confirmar.isPending}
            onClick={() => confirmar.mutate(resumen.token)}
          >
            Confirmar importación
          </button>
        </div>
      )}
    </section>
  );
}
