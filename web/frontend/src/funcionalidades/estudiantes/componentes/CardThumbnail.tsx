import { Image as ImageIcon } from "lucide-react";

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
      {tieneFoto ? (
        <img
          src={`/api/v1/estudiantes/${idEstudiante}/foto`}
          alt=""
          loading="lazy"
          decoding="async"
          className="h-full w-full object-cover object-top"
        />
      ) : (
        <ImageIcon className="m-2 h-4 w-4 text-muted-foreground" />
      )}
    </div>
  );
}
