# Cuentas

Administra saldos web y movimientos atómicos de las cuentas de estudiantes.
El módulo depende únicamente de `cuentas.*` y del identificador de estudiante recibido
por contrato; no consulta ni sincroniza datos del sistema local.

Permisos: `cuentas.leer` para consultar saldo y `cuentas.editar` para registrar recargas,
consumos o ajustes. Cada movimiento exige una clave de idempotencia única por estudiante.
