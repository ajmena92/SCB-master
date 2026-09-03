import { beforeEach, describe, expect, it, vi } from "vitest";
import { api } from "@/compartido/consultas/cliente_http";
import { plataformaApi } from "./plataforma";

vi.mock("@/compartido/consultas/cliente_http", () => ({
  api: { get: vi.fn(), post: vi.fn(), put: vi.fn(), delete: vi.fn() },
}));

describe("plataformaApi", () => {
  beforeEach(() => vi.clearAllMocks());

  it("normaliza respuestas de lista y usa rutas versionadas", async () => {
    vi.mocked(api.get).mockResolvedValueOnce({ data: [{ id: 1, codigo: "E-1" }] });
    const resultado = await plataformaApi.personas.listar({ buscar: "Ana", estado: "activos" });
    expect(api.get).toHaveBeenCalledWith("/v1/personas", { params: { buscar: "Ana", estado: "activos" } });
    expect(resultado).toEqual({ elementos: [{ id: 1, codigo: "E-1" }], total: 1 });
  });

  it("consulta el resumen global del padrón y normaliza sus campos", async () => {
    vi.mocked(api.get).mockResolvedValueOnce({
      data: {
        total: 42,
        estudiantes_activos: 30,
        profesores_activos: 8,
        inactivos: 4,
      },
    });

    await expect(plataformaApi.personas.resumen()).resolves.toEqual({
      total: 42,
      estudiantesActivos: 30,
      profesoresActivos: 8,
      inactivos: 4,
    });
    expect(api.get).toHaveBeenCalledWith("/v1/personas/resumen");
  });

  it("devuelve la credencial temporal creada sin persistirla en el cliente", async () => {
    vi.mocked(api.post).mockResolvedValueOnce({
      data: {
        id: 7,
        codigo: "E-00000018",
        cedula: "1-1111-1111",
        nombres: "Ana Mora",
        tipo: "estudiante",
        activo: true,
        pinTemporal: "123456",
      },
    });

    const persona = await plataformaApi.personas.crear({
      cedula: "1-1111-1111",
      referenciaPublica: "persona-ana-mora",
      nombres: "Ana",
      apellidos: "Mora",
      tipo: "estudiante",
      activo: true,
    });

    expect(persona.pinTemporal).toBe("123456");
    expect(sessionStorage.length).toBe(0);
    expect(localStorage.length).toBe(0);
  });

  it("envía la importación como multipart con el año de destino", async () => {
    vi.mocked(api.post).mockResolvedValueOnce({
      data: {
        huella: "abc",
        total: 0,
        altas: 0,
        cambios: 0,
        desactivaciones: 0,
        errores: [],
        datos: { anio: 2026, filas: [] },
      },
    });
    const archivo = new File(["datos"], "padron.xlsx");
    await plataformaApi.importaciones.previsualizar(archivo, 9);
    const cuerpo = vi.mocked(api.post).mock.calls[0][1] as FormData;
    expect(api.post).toHaveBeenCalledWith("/v1/importaciones/previsualizar", cuerpo);
    expect(cuerpo.get("archivo")).toBe(archivo);
    expect(cuerpo.get("anio")).toBe("9");
  });

  it("crea matrículas con el estado activo reconocido por la operación", async () => {
    vi.mocked(api.post).mockResolvedValueOnce({ data: {} });

    await plataformaApi.matriculas.crear({
      personaId: 7,
      anioLectivoId: 3,
      seccion: "10-2",
      becaComedor: true,
      estado: "activo",
    });

    expect(api.post).toHaveBeenCalledWith("/v1/matriculas", {
      personaId: 7,
      anioLectivoId: 3,
      seccion: "10-2",
      becado: true,
      estado: "activo",
    });
  });

  it("actualiza los beneficios de la matrícula en una sola operación", async () => {
    vi.mocked(api.put).mockResolvedValueOnce({
      data: { matricula_id: 7, becado: true, ruta_id: 12 },
    });

    await expect(
      plataformaApi.matriculas.actualizarBeneficios(7, { becado: true, rutaId: 12 }),
    ).resolves.toEqual({ matriculaId: 7, becado: true, rutaId: 12 });

    expect(api.put).toHaveBeenCalledWith("/v1/matriculas/7/beneficios", {
      becado: true,
      rutaId: 12,
    });
  });

  it("normaliza las credenciales devueltas al confirmar una importación", async () => {
    vi.mocked(api.post).mockResolvedValueOnce({
      data: {
        credenciales: [{ codigo: "E-00000018", nombre: "Ana", pin_temporal: "654321" }],
      },
    });

    const resultado = await plataformaApi.importaciones.confirmar(
      JSON.stringify({ anio: 2026, filas: [], huella: "abc" }),
    );

    expect(resultado.credenciales).toEqual([
      { cedula: "E-00000018", nombre: "Ana", pinTemporal: "654321" },
    ]);
  });

  it("usa la cédula en ventas y escaneos de comedor", async () => {
    vi.mocked(api.post).mockResolvedValue({ data: {} });
    await plataformaApi.tiquetes.vender({
      cedula: "E-00000018",
      cantidad: 2,
      medioPago: "efectivo",
    });
    await plataformaApi.comedor.registrarIngreso("E-00000018");

    expect(api.post).toHaveBeenNthCalledWith(1, "/v1/tiquetes/ventas", {
      cedula: "E-00000018",
      cantidad: 2,
      medioPago: "efectivo",
    });
    expect(api.post).toHaveBeenNthCalledWith(2, "/v1/comedor/operacion", {
      cedula: "E-00000018",
      fecha: expect.stringMatching(/^\d{4}-\d{2}-\d{2}$/),
    });
  });
});
