import { useMemo, useState } from "react";
import { Download, IdCard, Image as ImageIcon } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";

const CODE128_PATTERNS =
  "212222 222122 222221 121223 121322 131222 122213 122312 132212 221213 221312 231212 112232 122132 122231 113222 123122 123221 223211 221132 221231 213212 223112 312131 311222 321122 321221 312212 322112 322211 212123 212321 232121 111323 131123 131321 112313 132113 132311 211313 231113 231311 112133 112331 132131 113123 113321 133121 313121 211331 231131 213113 213311 213131 311123 311321 331121 312113 312311 332111 314111 221411 431111 111224 111422 121124 121421 141122 141221 112214 112412 122114 122411 142112 142211 241211 221114 413111 241112 134111 111242 121142 121241 114212 124112 124211 411212 421112 421211 212141 214121 412121 111143 111341 131141 114113 114311 411113 411311 113141 114131 311141 411131 211412 211214 211232 2331112".split(
    " ",
  );

function barcodeSymbols(value) {
  const symbols = [
    104,
    ...String(value || "")
      .split("")
      .map((char) => char.charCodeAt(0) - 32),
  ];
  const checksum =
    symbols[0] + symbols.slice(1).reduce((sum, symbol, index) => sum + symbol * (index + 1), 0);
  return [...symbols, checksum % 103, 106];
}

function Code128Barcode({ value }) {
  const bars = useMemo(() => {
    if (!value || /[^\x20-\x7E]/.test(String(value))) return [];
    let x = 0;
    return barcodeSymbols(value).flatMap((symbol) => {
      let dark = true;
      return CODE128_PATTERNS[symbol]
        .split("")
        .map((part) => {
          const width = Number(part) * 2;
          const bar = dark ? { x, width } : null;
          x += width;
          dark = !dark;
          return bar;
        })
        .filter(Boolean);
    });
  }, [value]);
  if (!value || /[^\x20-\x7E]/.test(String(value))) {
    return (
      <p className="py-5 text-center text-sm font-semibold text-muted-foreground">
        Código de barras no disponible
      </p>
    );
  }
  const width = bars.length ? Math.max(...bars.map((bar) => bar.x + bar.width)) + 16 : 160;

  return (
    <svg
      className="h-12 w-full"
      viewBox={`0 0 ${width} 48`}
      role="img"
      aria-label={`Código de barras ${value}`}
      preserveAspectRatio="none"
    >
      {bars.map((bar) => (
        <rect
          key={`${bar.x}-${bar.width}`}
          x={bar.x + 8}
          y="0"
          width={bar.width}
          height="48"
          fill="currentColor"
        />
      ))}
    </svg>
  );
}

function safeRouteColor(value) {
  return typeof value === "string" && /^#[0-9a-f]{6}$/i.test(value) ? value : "#CBD5E1";
}

function routeTextColor(color) {
  const rgb = color
    .slice(1)
    .match(/../g)
    .map((part) => parseInt(part, 16));
  return (rgb[0] * 299 + rgb[1] * 587 + rgb[2] * 114) / 1000 > 160 ? "#252653" : "#FFFFFF";
}

function HtmlStudentCard({ data, hasPhoto }) {
  const student = data || {};
  const routeColor = safeRouteColor(student.rutaColor);
  const headerText = routeTextColor(routeColor);
  const fullName = [student.nombre, student.primerApellido, student.segundoApellido]
    .filter(Boolean)
    .join(" ");
  const photoUrl = `/api/v1/estudiantes/carnet/foto?v=${student.idEstudiante || "student"}`;

  return (
    <div
      className="mx-auto w-full max-w-[23rem] overflow-hidden rounded-[1.75rem] border border-white/80 bg-white shadow-[0_20px_55px_rgb(64_68_170_/_0.2)]"
      data-testid="html-student-card"
    >
      <div
        className="relative overflow-hidden px-6 pb-7 pt-7"
        style={{ backgroundColor: routeColor, color: headerText }}
      >
        <div className="absolute -right-16 -top-20 h-48 w-48 rounded-full border-[22px] border-current opacity-15" />
        <div className="relative flex items-center justify-between">
          <div>
            <p className="text-[0.65rem] font-black uppercase tracking-[0.25em] opacity-75">
              Comedor SCSC
            </p>
            <h3 className="mt-2 font-display text-2xl font-black tracking-tight">Mi carnet</h3>
          </div>
          <IdCard className="h-8 w-8 opacity-90" aria-hidden="true" />
        </div>
        <div className="relative mt-6 flex items-end gap-4">
          <div className="h-28 w-24 shrink-0 overflow-hidden rounded-2xl border-4 border-white/70 bg-white/25 shadow-lg">
            {hasPhoto ? (
              <img
                src={photoUrl}
                alt={`Fotografía de ${fullName}`}
                className="h-full w-full object-cover object-top"
              />
            ) : (
              <div className="flex h-full items-center justify-center text-center text-[0.6rem] font-black uppercase leading-tight opacity-80">
                Foto
                <br />
                pendiente
              </div>
            )}
          </div>
          <div className="min-w-0 pb-1">
            <p className="text-[0.62rem] font-black uppercase tracking-[0.18em] opacity-70">
              Estudiante
            </p>
            <p className="mt-1 line-clamp-3 font-display text-lg font-black leading-tight">
              {fullName || "Sin nombre"}
            </p>
          </div>
        </div>
      </div>
      <div className="space-y-5 p-6">
        <div className="grid grid-cols-2 gap-4 text-sm">
          <div>
            <p className="text-[0.62rem] font-black uppercase tracking-wider text-muted-foreground">
              Cédula
            </p>
            <p className="mt-1 font-bold">{student.cedula || "Pendiente"}</p>
          </div>
          <div>
            <p className="text-[0.62rem] font-black uppercase tracking-wider text-muted-foreground">
              Sección
            </p>
            <p className="mt-1 font-bold">{student.seccion || "Sin sección"}</p>
          </div>
          <div>
            <p className="text-[0.62rem] font-black uppercase tracking-wider text-muted-foreground">
              Ruta
            </p>
            <p className="mt-1 font-bold">
              {student.rutaDescripcion || student.rutaCodigo || "Sin ruta"}
            </p>
          </div>
          <div>
            <p className="text-[0.62rem] font-black uppercase tracking-wider text-muted-foreground">
              Beneficio
            </p>
            <p className="mt-1 font-bold">{student.tipoBeca || "Sin beca"}</p>
          </div>
        </div>
        <div
          className="rounded-2xl bg-primary/5 p-3 text-secondary"
          data-testid="student-card-barcode"
        >
          <Code128Barcode value={data.barcode} />
        </div>
        <p className="text-center text-xs font-semibold text-muted-foreground">
          Presentá este código ante el lector del comedor.
        </p>
      </div>
    </div>
  );
}

export function StudentCardPreview({
  studentId,
  hasPhoto,
  cardData = null,
  loading = false,
  error = "",
  onRetry,
  className = "",
}) {
  const [version] = useState(() => Date.now());
  const photoAvailable = studentId ? hasPhoto : (hasPhoto ?? Boolean(cardData?.tieneFoto));
  const base = studentId ? `/api/v1/estudiantes/${studentId}` : "/api/v1/estudiantes";
  // El contrato canónico entrega la fotografía como imagen y el carnet como PDF.
  const png = studentId ? `${base}/foto?v=${version}` : null;
  const pdf = studentId ? `${base}/carnet.pdf` : "/api/v1/estudiantes/carnet.pdf";

  return (
    <section
      className={`rounded-2xl border bg-card p-5 shadow-[0_8px_30px_rgb(70_73_180_/_0.12)] ${className}`}
      data-testid="student-card-panel"
    >
      <div className="mb-4 flex flex-wrap items-start justify-between gap-3">
        <div className="flex items-start gap-3">
          <div className="rounded-xl bg-primary/10 p-3 text-primary">
            <IdCard className="h-5 w-5" />
          </div>
          <div>
            <h2 className="font-display text-xl font-bold">Mi carnet digital</h2>
            <p className="text-sm text-muted-foreground">
              Presentalo desde tu teléfono para leer el código en el comedor.
            </p>
          </div>
        </div>
        {photoAvailable === false && <Badge variant="secondary">Carnet provisional</Badge>}
      </div>
      {!studentId && loading && (
        <div className="space-y-3">
          <Skeleton className="mx-auto h-[31rem] w-full max-w-[23rem] rounded-[1.75rem]" />
          <p className="text-center text-sm font-medium text-muted-foreground">
            Generando tu carnet digital…
          </p>
        </div>
      )}
      {error && (
        <div
          role="alert"
          className="space-y-3 rounded-xl bg-destructive/10 p-4 text-sm font-medium text-destructive"
        >
          <p>{error}</p>
          {onRetry && (
            <Button type="button" variant="outline" size="sm" onClick={onRetry}>
              Reintentar
            </Button>
          )}
        </div>
      )}
      {studentId && (
        <div className="overflow-hidden rounded-xl border bg-primary/5 p-3">
          <img
            src={png}
            alt="Carnet digital del estudiante"
            className="mx-auto w-full max-w-[280px]"
          />
        </div>
      )}
      {!studentId && cardData && !error && (
        <HtmlStudentCard data={cardData} hasPhoto={photoAvailable} />
      )}
      <div className="mt-4 flex flex-wrap gap-2">
        {png && <Button asChild className="rounded-full">
          <a href={png} download>
            <Download className="mr-2 h-4 w-4" /> Descargar PNG
          </a>
        </Button>}
        <Button asChild variant="outline" className="rounded-full">
          <a href={pdf} download>
            <Download className="mr-2 h-4 w-4" /> Descargar PDF
          </a>
        </Button>
        {photoAvailable === false && (
          <p className="basis-full text-xs text-muted-foreground">
            El administrador todavía debe cargar tu fotografía.
          </p>
        )}
      </div>
    </section>
  );
}

export function CardThumbnail({ studentId, hasPhoto }) {
  return (
    <div
      className="relative h-10 w-8 overflow-hidden rounded border bg-accent/30"
      title={hasPhoto ? "Fotografía cargada" : "Foto pendiente"}
    >
      {hasPhoto ? (
        <img
          src={`/api/v1/estudiantes/${studentId}/foto`}
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
