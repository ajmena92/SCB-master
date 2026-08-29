import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/components/ui/alert-dialog";
import { IdCard, KeyRound } from "lucide-react";
import { CardThumbnail } from "@/funcionalidades/estudiantes/componentes/CardThumbnail";
import type { EstudianteAdministrativo } from "@/funcionalidades/estudiantes/modelo/contratos";

type Props = {
  estudiantes: EstudianteAdministrativo[];
  cargando: boolean;
  alAbrirPerfil: (estudiante: EstudianteAdministrativo) => void;
  alReiniciar: (id: number) => void;
};

export function TablaEstudiantes({ estudiantes, cargando, alAbrirPerfil, alReiniciar }: Props) {
  if (cargando) return <div className="h-64 w-full animate-pulse rounded-lg bg-muted" />;
  return (
    <div className="overflow-x-auto rounded-lg border bg-card">
      <Table data-testid="estudiantes-table">
        <TableHeader>
          <TableRow>
            <TableHead>Estudiante</TableHead>
            <TableHead>Carnet</TableHead>
            <TableHead>Cédula</TableHead>
            <TableHead>Horario</TableHead>
            <TableHead>Sección</TableHead>
            <TableHead>Ruta</TableHead>
            <TableHead>Comedor</TableHead>
            <TableHead>Estado PIN</TableHead>
            <TableHead className="text-right">Acción</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {estudiantes.map((estudiante) => (
            <TableRow key={estudiante.idEstudiante} className="hover:bg-muted/40">
              <TableCell className="font-medium">
                {[estudiante.nombre, estudiante.primerApellido, estudiante.segundoApellido]
                  .filter(Boolean)
                  .join(" ")}
              </TableCell>
              <TableCell>
                <CardThumbnail
                  idEstudiante={estudiante.idEstudiante}
                  tieneFoto={estudiante.tieneFoto}
                />
              </TableCell>
              <TableCell>{estudiante.cedula || "Pendiente"}</TableCell>
              <TableCell>{estudiante.turno || "—"}</TableCell>
              <TableCell>{estudiante.seccion || "Sin sección"}</TableCell>
              <TableCell>
                {estudiante.rutaCodigo || estudiante.rutaDescripcion || "Sin ruta"}
              </TableCell>
              <TableCell>
                {estudiante.beneficioComedor || "No beneficiario"}
              </TableCell>
              <TableCell>
                {estudiante.bloqueado ? (
                  <Badge variant="destructive">Bloqueado</Badge>
                ) : estudiante.debeCambiarPin ? (
                  <Badge variant="secondary">Debe cambiar</Badge>
                ) : (
                  <Badge className="bg-success text-white">Activo</Badge>
                )}
              </TableCell>
              <TableCell className="text-right">
                <Button
                  variant="outline"
                  size="sm"
                  className="mr-2"
                  onClick={() => alAbrirPerfil(estudiante)}
                  data-testid={`manage-card-${estudiante.idEstudiante}`}
                >
                  <IdCard className="mr-1 h-3 w-3" /> Carnet
                </Button>
                <AlertDialog>
                  <AlertDialogTrigger asChild>
                    <Button
                      variant="outline"
                      size="sm"
                      data-testid={`reset-pin-${estudiante.idEstudiante}`}
                    >
                      <KeyRound className="mr-1 h-3 w-3" /> Reiniciar PIN
                    </Button>
                  </AlertDialogTrigger>
                  <AlertDialogContent>
                    <AlertDialogHeader>
                      <AlertDialogTitle>Reiniciar PIN de {estudiante.nombre}</AlertDialogTitle>
                      <AlertDialogDescription>
                        Se generará un PIN temporal de 6 dígitos y el estudiante deberá cambiarlo al
                        ingresar.
                      </AlertDialogDescription>
                    </AlertDialogHeader>
                    <AlertDialogFooter>
                      <AlertDialogCancel>Cancelar</AlertDialogCancel>
                      <AlertDialogAction
                        data-testid={`confirm-reset-${estudiante.idEstudiante}`}
                        onClick={() => alReiniciar(estudiante.idEstudiante)}
                      >
                        Reiniciar
                      </AlertDialogAction>
                    </AlertDialogFooter>
                  </AlertDialogContent>
                </AlertDialog>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}
