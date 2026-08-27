# Frontend del Portal de Comedor

Cliente React para estudiantes y personal administrativo. Se publica junto a la API bajo el mismo dominio; el navegador solo llama a la ruta relativa `/api`.

El conjunto de herramientas usa Vite 8, React 19 y Vitest 4. Requiere Node `>=24.19.0 <25`; la versión fijada para el proyecto es Node `24.19.0` LTS, indicada en `.nvmrc`. El administrador de paquetes oficial es npm `12.0.2` y las versiones directas están fijadas exactamente en `package.json` y `package-lock.json`.

## Desarrollo

```bash
cp .env.example .env
nvm use
npm ci
npm start
```

Para desarrollo local, configure `API_PROXY_TARGET` en `.env` (por ejemplo, el puerto de la API). El navegador continúa llamando a `/api`, por lo que las cookies se comportan como en producción.

```bash
npm run build
```

Las comprobaciones locales son:

```bash
npm test
npm run typecheck
npm run lint
npm run build
```

Las pruebas unitarias se ejecutan mediante Vitest y el código nuevo se mantiene en TypeScript estricto. Los contratos OpenAPI generados por dominio se documentan en [Contratos de la API](../docs/CONTRATOS_API.md). Se usa TypeScript `6.0.3`, máxima versión admitida por el analizador vigente de ESLint/React; TypeScript 7 no se fuerza mientras ese contrato declare una versión menor a 6.1.

## Puertas de calidad

`npm run test:coverage` aplica umbrales mínimos de 80 % para líneas, funciones y sentencias, y 75 % para ramas; genera reportes en `coverage/`. Requiere el adaptador V8 de Vitest fijado en el entorno CI.

`npm run test:e2e` ejecuta `e2e/rutas-canonicas.spec.ts` contra el preview local. Las pruebas comprueban acceso a las rutas públicas y etiquetas de controles; los flujos autenticados requieren credenciales de staging.

La auditoría WCAG automatizable parte de contratos de título, descripción, permiso y columnas en `accesibilidad.test.ts`; las pruebas E2E verifican además nombres accesibles de controles. La revisión con axe debe ejecutarse en CI cuando el runner Playwright tenga el adaptador fijado.

Para el despliegue institucional use exclusivamente `web/ops/compose.production.yml`, `web/ops/Dockerfile.frontend` y `web/ops/nginx/default.conf`. El contenedor escucha en el puerto `8080` y sirve la SPA; Nginx reenvía `/api/` a FastAPI. No use Dockerfiles o Nginx alternos dentro de `frontend/`.

## Seguridad y contrato de API

- No configurar una URL absoluta pública: `VITE_API_BASE_URL` debe ser una ruta relativa y en producción queda como `/api`.
- No se guardan credenciales, JWT ni datos de sesión en `localStorage`.
- Las sesiones son cookies `HttpOnly`, `Secure`, `SameSite=Strict`; las solicitudes que modifican datos usan una cookie CSRF separada y un encabezado coincidente.
- La API y el cliente deben mantenerse sincronizados mediante [Contratos de la API](../docs/CONTRATOS_API.md). No desplegar cambios de rutas o esquemas sin ejecutar `npm run verificar:cliente`.
