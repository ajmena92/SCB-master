import { Image as ImageIcon } from "lucide-react";
import { ImagenConFallback } from "@/compartido/componentes/ImagenConFallback";

export function CardThumbnail({
  idEstudiante,
  tieneFoto,
}: {
  idEstudiante: number;
  tieneFoto?: boolean;
}) {
  return (
    <div
      className="relative h-10 w-8 overflow-hidden rounded border bg-accent/30"
      title={tieneFoto ? "Fotografía cargada" : "Foto pendiente"}
    >
      <ImagenConFallback
        src={tieneFoto ? `/api/v1/estudiantes/${idEstudiante}/foto` : undefined}
        alt=""
        className="h-full w-full object-cover object-top"
        fallback={<ImageIcon className="m-2 h-4 w-4 text-muted-foreground" aria-hidden="true" />}
      />
    </div>
  );
}
