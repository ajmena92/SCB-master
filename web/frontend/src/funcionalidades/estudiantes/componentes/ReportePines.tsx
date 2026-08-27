import { Button } from "@/components/ui/button";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import type { ReportePinesEstudiantes } from "@/funcionalidades/estudiantes/modelo/contratos";

type ReportePinesProps = { reporte: ReportePinesEstudiantes; alCerrar: () => void };

export function ReportePines({ reporte, alCerrar }: ReportePinesProps) {
  return (
    <div className="pin-reporte min-h-screen bg-background p-6 sm:p-10">
      <div className="pin-reporte-actions mb-8 flex flex-wrap justify-between gap-3">
        <Button variant="outline" onClick={alCerrar}>
          Volver a estudiantes
        </Button>
        <Button onClick={() => window.print()} data-testid="print-pin-reporte">
          Imprimir / Guardar como PDF
        </Button>
      </div>
      <header className="mb-6 border-b pb-5">
        <p className="text-xs font-bold uppercase tracking-[0.2em] text-primary">Comedor SCSC</p>
        <h1 className="mt-2 font-display text-3xl font-bold tracking-tight">
          Reporte de PIN por sección
        </h1>
        <div className="mt-3 flex flex-wrap gap-x-8 gap-y-1 text-sm text-muted-foreground">
          <span>
            <strong>Turno:</strong> {reporte.turno}
          </span>
          <span>
            <strong>Sección:</strong> {reporte.seccion}
          </span>
          <span>
            <strong>Estudiantes:</strong> {reporte.total}
          </span>
          <span>
            <strong>Generado:</strong> {reporte.generadoEn}
          </span>
        </div>
      </header>
      <div className="overflow-x-auto">
        <Table data-testid="pin-reporte-table">
          <TableHeader>
            <TableRow>
              <TableHead>#</TableHead>
              <TableHead>Estudiante</TableHead>
              <TableHead>Cédula</TableHead>
              <TableHead>Horario</TableHead>
              <TableHead className="text-right">PIN temporal</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {reporte.estudiantes.map((estudiante, indice) => (
              <TableRow key={estudiante.idEstudiante}>
                <TableCell>{indice + 1}</TableCell>
                <TableCell className="font-medium">{estudiante.nombreCompleto}</TableCell>
                <TableCell>{estudiante.cedula}</TableCell>
                <TableCell>{estudiante.horario}</TableCell>
                <TableCell className="text-right font-display text-lg font-black tracking-[0.2em]">
                  {estudiante.pin}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>
      <p className="mt-8 text-xs text-muted-foreground">
        Documento confidencial. Entregue cada PIN al estudiante de forma segura. Todos los
        estudiantes deben cambiar este PIN al ingresar.
      </p>
    </div>
  );
}
