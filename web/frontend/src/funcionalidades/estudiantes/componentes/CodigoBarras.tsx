import { useMemo } from "react";

const PATRONES_CODE128 =
  "212222 222122 222221 121223 121322 131222 122213 122312 132212 221213 221312 231212 112232 122132 122231 113222 123122 123221 223211 221132 221231 213212 223112 312131 311222 321122 321221 312212 322112 322211 212123 212321 232121 111323 131123 131321 112313 132113 132311 211313 231113 231311 112133 112331 132131 113123 113321 133121 313121 211331 231131 213113 213311 213131 311123 311321 331121 312113 312311 332111 314111 221411 431111 111224 111422 121124 121421 141122 141221 112214 112412 122114 122411 142112 142211 241211 221114 413111 241112 134111 111242 121142 121241 114212 124112 124211 411212 421112 421211 212141 214121 412121 111143 111341 131141 114113 114311 411113 411311 113141 114131 311141 411131 211412 211214 211232 2331112".split(
    " ",
  );

type Barra = { x: number; ancho: number };

function simbolosCodigo128(valor: string): number[] {
  const simbolos = [
    104,
    ...String(valor || "")
      .split("")
      .map((caracter) => caracter.charCodeAt(0) - 32),
  ];
  const suma =
    simbolos[0] +
    simbolos.slice(1).reduce((total, simbolo, indice) => total + simbolo * (indice + 1), 0);
  return [...simbolos, suma % 103, 106];
}

export function CodigoBarras({ valor }: { valor?: string | null }) {
  const barras = useMemo<Barra[]>(() => {
    if (!valor || /[^\x20-\x7E]/.test(String(valor))) return [];
    let x = 0;
    return simbolosCodigo128(String(valor)).flatMap((simbolo) => {
      let oscuro = true;
      return PATRONES_CODE128[simbolo]
        .split("")
        .map((parte) => {
          const ancho = Number(parte) * 2;
          const barra = oscuro ? { x, ancho } : null;
          x += ancho;
          oscuro = !oscuro;
          return barra;
        })
        .filter((barra): barra is Barra => barra !== null);
    });
  }, [valor]);

  if (!valor || /[^\x20-\x7E]/.test(String(valor))) {
    return (
      <p className="py-5 text-center text-sm font-semibold text-muted-foreground">
        Código de barras no disponible
      </p>
    );
  }
  const anchoTotal = barras.length
    ? Math.max(...barras.map((barra) => barra.x + barra.ancho)) + 16
    : 160;
  return (
    <svg
      className="h-20 w-full"
      viewBox={`0 0 ${anchoTotal} 72`}
      role="img"
      aria-label={`Código de barras ${valor}`}
      preserveAspectRatio="none"
    >
      {barras.map((barra) => (
        <rect
          key={`${barra.x}-${barra.ancho}`}
          x={barra.x + 8}
          y="0"
          width={barra.ancho}
          height="72"
          fill="currentColor"
        />
      ))}
    </svg>
  );
}
