import { Button } from "@/components/ui/button";

type Props = {
  total: number;
  pagina: number;
  totalPaginas: number;
  alCambiarPagina: (pagina: number) => void;
};

export function PaginacionEstudiantes({ total, pagina, totalPaginas, alCambiarPagina }: Props) {
  return (
    <div className="flex items-center justify-between text-sm text-muted-foreground">
      <span>
        {total} estudiante(s) · Página {pagina} de {totalPaginas}
      </span>
      <div className="flex gap-2">
        <Button
          variant="outline"
          size="sm"
          disabled={pagina === 1}
          onClick={() => alCambiarPagina(pagina - 1)}
        >
          Anterior
        </Button>
        <Button
          variant="outline"
          size="sm"
          disabled={pagina >= totalPaginas}
          onClick={() => alCambiarPagina(pagina + 1)}
        >
          Siguiente
        </Button>
      </div>
    </div>
  );
}
