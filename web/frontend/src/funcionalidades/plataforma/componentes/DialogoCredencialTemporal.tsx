import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import type { CredencialTemporal } from "@/compartido/contratos/plataforma";

function descargarCredencial(credencial: CredencialTemporal) {
  const escapar = (valor: string) => `"${valor.replaceAll('"', '""')}"`;
  const contenido = [
    ["Código", "Nombre", "PIN temporal"].map(escapar).join(","),
    [credencial.codigo, credencial.nombre, credencial.pinTemporal].map(escapar).join(","),
  ].join("\n");
  const url = URL.createObjectURL(
    new Blob([`\ufeff${contenido}`], { type: "text/csv;charset=utf-8" }),
  );
  const enlace = document.createElement("a");
  enlace.href = url;
  enlace.download = `credencial-${credencial.codigo}.csv`;
  enlace.click();
  URL.revokeObjectURL(url);
}

export default function DialogoCredencialTemporal({
  credencial,
  alCerrar,
}: {
  credencial?: CredencialTemporal;
  alCerrar: () => void;
}) {
  async function copiar() {
    if (!credencial) return;
    await navigator.clipboard.writeText(
      `Código: ${credencial.codigo}\nNombre: ${credencial.nombre}\nPIN temporal: ${credencial.pinTemporal}`,
    );
  }

  return (
    <AlertDialog open={Boolean(credencial)}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Credencial temporal creada</AlertDialogTitle>
          <AlertDialogDescription>
            Entréguela de forma segura. El PIN no se guardará ni volverá a mostrarse después de
            cerrar este diálogo.
          </AlertDialogDescription>
        </AlertDialogHeader>
        {credencial && (
          <dl className="credential-summary">
            <div>
              <dt>Persona</dt>
              <dd>{credencial.nombre}</dd>
            </div>
            <div>
              <dt>Código</dt>
              <dd>{credencial.codigo}</dd>
            </div>
            <div>
              <dt>PIN temporal</dt>
              <dd className="temporary-pin">{credencial.pinTemporal}</dd>
            </div>
          </dl>
        )}
        <AlertDialogFooter>
          <button className="button secondary" type="button" onClick={copiar}>
            Copiar
          </button>
          <button
            className="button secondary"
            type="button"
            onClick={() => credencial && descargarCredencial(credencial)}
          >
            Descargar CSV
          </button>
          <AlertDialogAction onClick={alCerrar}>Ya la guardé</AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
