# ADR-0003: sesión en cookie y protección CSRF vinculada a sesión

- Estado: Aceptada
- Fecha: 2026-09-03
- Responsables: por designar

## Contexto

La composición actual conserva una sesión Bearer cuya representación se guarda en
`sessionStorage`. A la vez, existen referencias de cliente a CSRF que no forman
parte de un contrato completo y verificable con la API. Esta combinación deja dos
modelos de seguridad implícitos, dificulta las pruebas negativas y expone el
portador de sesión a código JavaScript que llegue a ejecutarse en el origen.

El producto necesita un único contrato para portal y administración, coherente con
la autoridad de permisos de la API y aplicable en los despliegues web aprobados.

## Decisión

Adoptar sesiones opacas transmitidas exclusivamente mediante una cookie de sesión
con los atributos `HttpOnly`, `Secure`, `SameSite=Lax` y `Path` explícito, junto
con protección CSRF firmada o vinculada criptográficamente a la sesión para las
solicitudes que cambian estado.

- La cookie de sesión es emitida, renovada, vencida y revocada solo por la API; el
  frontend no recibe ni almacena su valor en `sessionStorage`, `localStorage` ni
  otra ubicación accesible a JavaScript.
- La cookie será de host exclusivo: se omite el atributo `Domain`, sin dominios
  comodín ni un dominio padre amplio. `Path` se emite explícitamente con el
  prefijo mínimo común de las rutas API que necesitan la sesión, definido tras el
  inventario de rutas; no se deja al valor implícito del navegador.
- La cookie tiene vencimiento absoluto configurable y documentado. El
  vencimiento por inactividad queda **diferido**: requiere persistir actividad y
  una migración DDL aprobada antes de diseñarlo e implementarlo. El identificador
  de sesión rota al autenticarse y al renovarse; la renovación no extiende más
  allá del vencimiento absoluto. Logout, cambio de credencial y revocación
  invalidan la sesión en servidor y limpian las cookies.
- La API emite un valor CSRF legible por el cliente para que este lo copie en
  `X-CSRF-Token`. El valor debe ser firmado por la API o estar vinculado
  criptográficamente a la sesión y a su vigencia; una igualdad de dos valores
  independientes no satisface esta decisión. La validación verifica firma o
  vínculo, sesión, expiración y coincidencia antes de ejecutar la operación.
- La protección CSRF cubre `POST`, `PUT`, `PATCH` y `DELETE`. Las rutas de lectura
  no requieren cabecera CSRF. Login requiere un CSRF anónimo, firmado y obtenido
  desde un endpoint de inicio, además de `Origin` válido, para mitigar login CSRF.
  Logout y renovación requieren el CSRF vinculado a la sesión y `Origin` válido;
  no se exceptúan. Cualquier excepción futura exige justificación de seguridad,
  actualización de esta ADR y pruebas negativas antes de publicarse.
- Toda mutación valida `Origin` contra la lista exacta de orígenes aprobados,
  incluso cuando el CSRF sea válido. Las solicitudes sin `Origin` o con un origen
  no aprobado se rechazan sin cambiar estado, salvo una excepción no navegador
  documentada y autorizada por ruta.
- La API mantiene la autoridad de autenticación, autorización, vencimiento,
  bloqueo de intentos y revocación. CORS, credenciales y cabeceras permitidas se
  configuran únicamente para los orígenes aprobados y se verifican en preflight:
  deben incluir `PATCH` y `X-CSRF-Token`, además de los métodos y cabeceras
  estrictamente necesarios.
- El login exitoso no devuelve un token de sesión en cuerpo, cabecera ni objeto
  JavaScript; únicamente emite las cookies definidas por este contrato.
- `Secure` no se desactiva en producción. Cualquier excepción de desarrollo local
  debe estar aislada por configuración no versionada y no puede convertirse en el
  comportamiento predeterminado de despliegue.

Esta decisión define el contrato objetivo; no autoriza por sí sola cambios de
cookies, CORS, secretos, DDL ni el retiro inmediato de código existente.

## Límites

- No se usan tokens Bearer de sesión como alternativa productiva ni se mantienen
  flujos paralelos de autenticación después de la transición.
- La cookie no sustituye la validación de permisos en endpoints ni las protecciones
  contra fuerza bruta.
- El CSRF vinculado a sesión no sustituye defensas contra XSS, validación de origen,
  límites CORS, TLS ni prácticas de manejo de secretos.
- Esta ADR no determina el proveedor de almacenamiento de sesiones ni modifica el
  modelo de datos; esas decisiones requieren evidencia y aprobación separadas.

## Alternativas consideradas

### Bearer sin CSRF y sin almacenamiento accesible a JavaScript

Descartada para este corte. Evitar un almacenamiento accesible a JavaScript exige
un mecanismo adicional de custodia del portador y mantiene un contrato distinto
del modelo de sesión web documentado. No reduce la necesidad de retirar el flujo
Bearer actual de manera verificable.

### Mantener Bearer en `sessionStorage`

Descartada porque el portador queda disponible para JavaScript y prolonga la
incoherencia con la protección CSRF documentada.

### Cookie sin CSRF

Descartada porque las cookies se envían automáticamente y las operaciones que
cambian estado necesitan una prueba adicional de intención del cliente.

## Consecuencias

Positivas:

- el portador de sesión deja de estar expuesto al código JavaScript de la página;
- existe un único contrato comprobable para navegador, API, OpenAPI y operación;
- las pruebas pueden comprobar de manera precisa cookies, CSRF, CORS y revocación.

Costos y restricciones:

- se requiere un corte coordinado entre frontend, API, configuración, pruebas y
  documentación;
- el cliente debe incluir credenciales y la cabecera CSRF solo donde corresponda;
- configuraciones de dominios, HTTPS, `SameSite` y CORS requieren pruebas de
  navegador en los entornos aprobados;
- se debe retirar de forma controlada el código Bearer y el uso de
  `token_sesion.ts`, sin aliases o compatibilidad permanente.

## Transición desde Bearer y `sessionStorage`

1. Caracterizar y probar login, portal, administración, bloqueo, vencimiento,
   logout, revocación y errores de autorización antes de modificar el contrato.
2. Implementar emisión, rotación, expiración, validación y limpieza de las cookies
   de sesión y CSRF, con el cliente configurado para enviar credenciales y copiar
   `X-CSRF-Token` solo en mutaciones.
3. Actualizar contratos API, CORS y documentación del despliegue; probar los casos
   positivos y negativos en navegador contra un origen autorizado y uno rechazado,
   incluido `PATCH`, `X-CSRF-Token`, login, logout y renovación.
4. Retirar el encabezado Bearer, `token_sesion.ts`, persistencia asociada y pruebas
   o código CSRF no conectados al contrato definitivo. No se mantiene doble vía.
5. Invalidar las sesiones Bearer preexistentes dentro de una ventana operativa
   aprobada y comunicar la necesidad de iniciar sesión nuevamente. La ventana,
   responsables y reversión operativa se registran antes del despliegue.

## Criterios de cumplimiento

La decisión se considera implementada cuando:

- login exitoso emite una cookie de sesión `HttpOnly`, `Secure`, `SameSite=Lax`,
  `Path` explícito y sin `Domain` amplio, sin exponer el portador a JavaScript ni
  devolverlo en la respuesta;
- `SameSite=Lax` se conserva deliberadamente para permitir navegación de nivel
  superior desde enlaces institucionales sin habilitar envío de cookies en la
  mayoría de solicitudes cross-site; no sustituye CSRF ni validación de `Origin`;
- la sesión y el CSRF rotan, expiran y se invalidan según el contrato; el límite
  de expiración absoluta está documentado y cubierto por pruebas. El control de
  inactividad no se declara implementado hasta que cuente con DDL aprobado y
  pruebas propias;
- las mutaciones válidas requieren credenciales, `Origin` aprobado y CSRF firmado
  o vinculado a sesión correcto. Un valor ausente, inválido, vencido o no vinculado
  recibe `403` sin cambiar estado;
- login exige CSRF anónimo firmado y `Origin` aprobado; logout y renovación exigen
  CSRF de sesión y `Origin` aprobado, con pruebas positivas y negativas;
- CORS permite solo orígenes aprobados, credenciales, `PATCH` y `X-CSRF-Token`; el
  preflight no habilita orígenes, métodos o cabeceras no autorizados;
- sesión ausente, vencida, revocada o inválida recibe `401`; permisos insuficientes
  reciben `403`; los rechazos `401` y `403` no mutan estado;
- no queda autenticación Bearer productiva ni persistencia de sesión accesible a
  JavaScript;
- pruebas automatizadas y recorridos de navegador cubren los casos anteriores, y
  la documentación de API y despliegue describe exactamente el flujo aplicado.
