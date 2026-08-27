# Excepciones de los verificadores arquitectónicos

El comando `npm run verificar:arquitectura`, ejecutado desde `web/frontend`, aplica los límites de Fase 0. Su configuración legible por máquina está en [`../scripts/configuracion_verificadores.json`](../scripts/configuracion_verificadores.json).

## Alcance intencional

Los controles se aplican al backend activo (`backend/aplicacion`) y a la estructura frontend objetivo (`src/aplicacion`, `src/funcionalidades` y `src/compartido`). El verificador de HTTP también revisa los componentes React existentes en `src/components`, excepto los componentes de biblioteca en `src/components/ui` y pruebas. Documentación y código legado fuera de esos límites no se tratan como dependencias de ejecución nuevas.

## Archivos mayores a 300 líneas

Estas excepciones son transitorias del frontend existente. Cada una deberá retirarse al dividir el comportamiento durante su corte vertical:

| Archivo | Motivo | Revisión |
| --- | --- | --- |
| `frontend/src/compartido/contratos/api.ts` | Contrato TypeScript generado desde OpenAPI; se reemplaza al regenerar el cliente. | Al dividir el cliente generado |

## Inglés propio permitido

No hay términos propios del proyecto permitidos. Las excepciones técnicas generales están documentadas en [CONVENCIONES_NOMBRES.md](CONVENCIONES_NOMBRES.md). Para agregar una excepción propia se debe justificar primero en esa convención y después registrarla, con alcance mínimo, en `terminos_ingleses_propios_permitidos`.

## Límites comprobados

- referencias desde el código objetivo hacia `escritorio`;
- SQL o `execute(...)` en backend fuera de `repositorio.py`;
- `fetch` o `axios` directo en componentes React;
- archivos de código por encima de 300 líneas sin excepción;
- vocabulario inglés propio prohibido en módulos y funcionalidad nueva.
