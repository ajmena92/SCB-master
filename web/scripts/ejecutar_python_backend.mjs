import { existsSync } from "node:fs";
import { spawnSync } from "node:child_process";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const directorioScripts = dirname(fileURLToPath(import.meta.url));
const directorioWeb = dirname(directorioScripts);
const candidatos = [
  join(directorioWeb, "backend", ".venv", "bin", "python"),
  join(directorioWeb, "backend", ".venv", "Scripts", "python.exe"),
];
const interprete = candidatos.find(existsSync);

if (!interprete) {
  console.error(
    "No se encontró web/backend/.venv. Cree el entorno backend e instale sus dependencias antes de generar el cliente OpenAPI.",
  );
  process.exit(1);
}

const [script, ...argumentos] = process.argv.slice(2);
if (!script) {
  console.error("Debe indicar el script Python que se ejecutará.");
  process.exit(1);
}

const resultado = spawnSync(
  interprete,
  [join(directorioScripts, script), ...argumentos],
  {
    cwd: directorioWeb,
    stdio: "inherit",
  },
);

if (resultado.error) {
  console.error(resultado.error.message);
  process.exit(1);
}

process.exit(resultado.status ?? 1);
