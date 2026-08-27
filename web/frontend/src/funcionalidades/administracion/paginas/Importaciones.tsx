import { Button } from "@/components/ui/button";
import { useImportaciones } from "@/funcionalidades/administracion/hooks/useImportaciones";
export default function Importaciones() {
  const { archivo, setArchivo, vista, lote, error, cargando, previsualizar, importar } =
    useImportaciones();
  return (
    <main className="space-y-6 p-6">
      <h1 className="text-2xl font-semibold">Importaciones</h1>
      <input
        type="file"
        accept=".csv"
        onChange={(e) => setArchivo(e.target.files?.[0])}
        aria-label="Archivo CSV"
      />
      {error && <p role="alert">{error}</p>}
      <div className="flex gap-2">
        <Button onClick={() => void previsualizar()} disabled={!archivo || cargando}>
          Previsualizar
        </Button>
        <Button onClick={() => void importar()} disabled={!archivo || cargando}>
          Ejecutar importación
        </Button>
      </div>
      {vista && (
        <section aria-label="Resultado de previsualización">
          <p>
            {vista.totalFilas} filas; {vista.valida ? "válida" : "con errores"}.
          </p>
          <ul>
            {vista.errores.map((e) => (
              <li key={`${e.fila}-${e.mensaje}`}>
                Fila {e.fila}: {e.mensaje}
              </li>
            ))}
          </ul>
        </section>
      )}
      {lote && (
        <p role="status">
          Lote #{lote.idLote}: {lote.estado} ({lote.totalFilas} filas)
        </p>
      )}
    </main>
  );
}
