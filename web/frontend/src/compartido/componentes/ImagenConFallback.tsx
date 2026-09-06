import { useEffect, useState, type ReactNode } from "react";

type Propiedades = {
  src?: string;
  alt: string;
  className?: string;
  fallback: ReactNode;
};

/** Evita iconos de imagen rota y ofrece un estado claro cuando la foto no existe. */
export function ImagenConFallback({ src, alt, className, fallback }: Propiedades) {
  const [fallo, setFallo] = useState(false);

  useEffect(() => setFallo(false), [src]);

  if (!src || fallo) return <>{fallback}</>;
  return <img src={src} alt={alt} className={className} onError={() => setFallo(true)} />;
}
