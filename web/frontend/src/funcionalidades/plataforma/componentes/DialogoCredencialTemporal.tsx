import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import { Copy, Download, Printer, ShieldCheck } from "lucide-react";
import { useState } from "react";
import type { CredencialTemporal } from "@/compartido/contratos/plataforma";

interface ReportePines {
  anio: number;
  seccion: string;
}

function escaparHtml(valor: string) {
  return valor.replace(/[&<>"']/g, (caracter) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#039;" })[caracter] ?? caracter);
}

function abrirReportePines(credenciales: CredencialTemporal[], reporte: ReportePines) {
  const ventana = window.open("", "_blank");
  if (!ventana) return false;
  ventana.opener = null;
  const fecha = new Intl.DateTimeFormat("es-CR", { dateStyle: "long", timeStyle: "short" }).format(new Date());
  const filas = credenciales.map((credencial, indice) => `<tr><td>${indice + 1}</td><td>${escaparHtml(credencial.nombre)}</td><td>${escaparHtml(credencial.cedula)}</td><td class="pin">${escaparHtml(credencial.pinTemporal)}</td></tr>`).join("");
  ventana.document.write(`<!doctype html><html lang="es"><head><title>PIN ${reporte.seccion} ${reporte.anio}</title><style>:root{font-family:Karla,system-ui,sans-serif;color:#182052}body{margin:28px;max-width:900px}header{border-bottom:2px solid #747BFF;padding-bottom:14px;margin-bottom:18px}h1,p{margin:0}h1{font-family:Chivo,system-ui,sans-serif;font-size:21px;font-weight:700}small{color:#6B72A8}table{width:100%;border-collapse:collapse;margin-top:18px}th,td{padding:9px;border-bottom:1px solid #DDE1FF;text-align:left}th{background:#EEF0FF;font-size:12px;text-transform:uppercase}.pin{font-size:18px;font-weight:600;letter-spacing:2px}footer{margin-top:22px;padding-top:10px;border-top:1px solid #DDE1FF;font-size:11px;color:#6B72A8}@media print{body{margin:14mm}}</style></head><body><header><p>CTP Platanares · Entrega segura de credenciales</p><h1>PIN temporales · Sección ${escaparHtml(reporte.seccion)} · ${reporte.anio}</h1><small>Generado ${escaparHtml(fecha)} · ${credenciales.length} estudiantes activos</small></header><table><thead><tr><th>#</th><th>Estudiante</th><th>Cédula</th><th>PIN temporal</th></tr></thead><tbody>${filas}</tbody></table><footer>Documento confidencial. Entregar individualmente y destruir cuando los PIN hayan sido cambiados.</footer></body></html>`);
  ventana.document.close();
  ventana.focus();
  ventana.print();
  return true;
}

function descargarCredenciales(credenciales: CredencialTemporal[]) {
  const escapar = (valor: string) => `"${valor.replaceAll('"', '""')}"`;
  const contenido = [
    ["Cédula", "Nombre", "PIN temporal"].map(escapar).join(","),
    ...credenciales.map((credencial) =>
      [credencial.cedula, credencial.nombre, credencial.pinTemporal].map(escapar).join(","),
    ),
  ].join("\n");
  const url = URL.createObjectURL(
    new Blob([`\ufeff${contenido}`], { type: "text/csv;charset=utf-8" }),
  );
  const enlace = document.createElement("a");
  enlace.href = url;
  enlace.download = credenciales.length === 1
    ? `credencial-${credenciales[0].cedula}.csv`
    : "credenciales-temporales.csv";
  enlace.click();
  URL.revokeObjectURL(url);
}

export default function DialogoCredencialTemporal({
  credenciales,
  alCerrar,
  reportePines,
}: {
  credenciales?: CredencialTemporal[];
  alCerrar: () => void;
  reportePines?: ReportePines;
}) {
  const [errorImpresion, setErrorImpresion] = useState<string>();
  async function copiar() {
    if (!credenciales?.length) return;
    await navigator.clipboard.writeText(
      credenciales
        .map((credencial) => `Cédula: ${credencial.cedula}\nNombre: ${credencial.nombre}\nPIN temporal: ${credencial.pinTemporal}`)
        .join("\n\n"),
    );
  }

  return (
    <AlertDialog open={Boolean(credenciales?.length)}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>{credenciales?.length === 1 ? "Credencial temporal creada" : "PIN temporales listos"}</AlertDialogTitle>
          <AlertDialogDescription>{reportePines ? "Genere el reporte para entregar las credenciales de forma segura." : "Entréguela de forma segura. El PIN no se guardará ni volverá a mostrarse después de cerrar este diálogo."}</AlertDialogDescription>
        </AlertDialogHeader>
        {reportePines && credenciales?.length ? (
          <>
            <div className="pin-report-summary">
              <ShieldCheck aria-hidden="true" size={28} />
              <div><span>Sección {reportePines.seccion} · {reportePines.anio}</span><strong>{credenciales.length} PIN temporales generados</strong></div>
            </div>
            <div className="pin-report-actions">
              <button className="button primary pin-report-action" type="button" onClick={() => setErrorImpresion(abrirReportePines(credenciales, reportePines) ? undefined : "El navegador bloqueó la ventana de impresión. Permití ventanas emergentes para este sitio e intentá de nuevo.")}><Printer aria-hidden="true" size={22} /><span>Imprimir o guardar PDF</span></button>
              <button className="button secondary pin-report-action" type="button" onClick={() => descargarCredenciales(credenciales)}><Download aria-hidden="true" size={20} /><span>Descargar CSV</span></button>
              <button className="button secondary pin-report-action" type="button" onClick={copiar}><Copy aria-hidden="true" size={20} /><span>Copiar PIN</span></button>
            </div>
            {errorImpresion && <p className="pin-warning" role="alert">{errorImpresion}</p>}
          </>
        ) : credenciales?.length && (
          <dl className="credential-summary credential-summary--scrollable">
            {credenciales.map((credencial) => <div key={credencial.cedula}>
              <dt>{credencial.nombre} · {credencial.cedula}</dt>
              <dd className="temporary-pin">{credencial.pinTemporal}</dd>
            </div>)}
          </dl>
        )}
        <AlertDialogFooter>
          {!reportePines && <><button className="button secondary" type="button" onClick={copiar}>Copiar</button><button className="button secondary" type="button" onClick={() => credenciales && descargarCredenciales(credenciales)}>Descargar CSV</button></>}
          <AlertDialogAction onClick={alCerrar}>{reportePines ? "Finalizar" : "Ya la guardé"}</AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
