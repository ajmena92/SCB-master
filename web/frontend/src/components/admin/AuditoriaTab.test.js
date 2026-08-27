import { auditEventLabel } from "./AuditoriaTab";

describe("auditEventLabel", () => {
  it("presents portal setting changes with the requested audit label", () => {
    expect(auditEventLabel("Parámetros del portal")).toBe("Parámetros del portal");
    expect(auditEventLabel("ParametrosPortal")).toBe("Parámetros del portal");
  });
});
