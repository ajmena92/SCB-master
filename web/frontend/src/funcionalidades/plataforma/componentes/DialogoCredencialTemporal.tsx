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

function descargarCredenciales(credenciales: CredencialTemporal[]) {
  const escapar = (valor: string) => `"${valor.replaceAll('"', '""')}"`;
  const contenido = [
    ["Cédula", "Nombre", "PIN temporal"].map(escapar).join(","),
    ...credenciales.map((credencial) =>
      [credencial.codigo, credencial.nombre, credencial.pinTemporal].map(escapar).join(","),
    ),
  ].join("\n");
  const url = URL.createObjectURL(
    new Blob([`\ufeff${contenido}`], { type: "text/csv;charset=utf-8" }),
  );
  const enlace = document.createElement("a");
  enlace.href = url;
  enlace.download = credenciales.length === 1
    ? `credencial-${credenciales[0].codigo}.csv`
    : "credenciales-temporales.csv";
  enlace.click();
  URL.revokeObjectURL(url);
}

export default function DialogoCredencialTemporal({
  credenciales,
  alCerrar,
}: {
  credenciales?: CredencialTemporal[];
  alCerrar: () => void;
}) {
  async function copiar() {
    if (!credenciales?.length) return;
    await navigator.clipboard.writeText(
      credenciales
        .map((credencial) => `Cédula: ${credencial.codigo}\nNombre: ${credencial.nombre}\nPIN temporal: ${credencial.pinTemporal}`)
        .join("\n\n"),
    );
  }

  return (
    <AlertDialog open={Boolean(credenciales?.length)}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>{credenciales?.length === 1 ? "Credencial temporal creada" : "Credenciales temporales creadas"}</AlertDialogTitle>
          <AlertDialogDescription>
            Entréguela de forma segura. El PIN no se guardará ni volverá a mostrarse después de
            cerrar este diálogo.
          </AlertDialogDescription>
        </AlertDialogHeader>
        {credenciales?.length && (
          <dl className="credential-summary">
            {credenciales.map((credencial) => <div key={credencial.codigo}>
              <dt>{credencial.nombre} · {credencial.codigo}</dt>
              <dd className="temporary-pin">{credencial.pinTemporal}</dd>
            </div>)}
          </dl>
        )}
        <AlertDialogFooter>
          <button className="button secondary" type="button" onClick={copiar}>
            Copiar
          </button>
          <button
            className="button secondary"
            type="button"
            onClick={() => credenciales && descargarCredenciales(credenciales)}
          >
            Descargar CSV
          </button>
          <AlertDialogAction onClick={alCerrar}>Ya la guardé</AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
