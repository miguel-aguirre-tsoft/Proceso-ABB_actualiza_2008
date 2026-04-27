# Guía de errores — Proceso-ABB_actualiza_2008

Este documento describe los errores que puede generar el proceso `ABB_actualiza_2008`, su significado y las acciones recomendadas para resolverlos.

---

## Tabla de contenidos

1. [Errores fatales (detienen el proceso)](#1-errores-fatales-detienen-el-proceso)
2. [Errores de extracción](#2-errores-de-extracción)
3. [Errores de validación](#3-errores-de-validación)
4. [Errores de transformación](#4-errores-de-transformación)
5. [Errores de carga](#5-errores-de-carga)
6. [Cómo interpretar el archivo de errores CSV](#6-cómo-interpretar-el-archivo-de-errores-csv)
7. [Procedimiento de reanudación](#7-procedimiento-de-reanudación)

---

## 1. Errores fatales (detienen el proceso)

Los errores con nivel `FATAL` detienen la ejecución completa del proceso. El proceso debe ser corregido y re-ejecutado desde el principio (o desde el último lote confirmado).

| Código | Descripción | Causa probable | Acción recomendada |
|---|---|---|---|
| `E000` | Configuración inválida | Falta variable de entorno o parámetro obligatorio | Revisar `config.yaml` y variables de entorno |
| `E001` | Error de conexión con el origen | Credenciales incorrectas, servidor no disponible | Verificar conectividad y credenciales de la BD origen |
| `E002` | Timeout persistente en extracción | BD origen lenta o saturada, query ineficiente | Revisar estado del servidor origen; considerar ajustar `BATCH_SIZE` |
| `E300` | Error crítico de carga | BD destino no disponible o sin permisos de escritura | Verificar conectividad y permisos de la BD destino |

---

## 2. Errores de extracción

Errores que ocurren durante la fase de extracción, pero que no necesariamente detienen el proceso.

| Código | Descripción | Causa probable | Acción recomendada |
|---|---|---|---|
| `E002` | Timeout en consulta (< 3 intentos) | BD origen lenta | El proceso reintenta automáticamente hasta 3 veces |

---

## 3. Errores de validación

Los errores de validación descartan el registro afectado. El proceso continúa con el siguiente registro.

| Código | Descripción | Causa probable | Acción recomendada |
|---|---|---|---|
| `E100` | Campo obligatorio nulo o vacío | Dato faltante en el origen | Revisar la fuente de datos; si es correcto, ajustar regla |
| `E101` | Formato o rango de fecha inválido | Fecha fuera de 2008 o mal formateada | Verificar campo `fecha_operacion` en el origen |
| `E102` | Valor numérico fuera de rango | Importe negativo, tasa > 100, etc. | Revisar campo numérico indicado en el log |
| `E103` | Código de referencia no existe en catálogo | Dato desactualizado en el catálogo o error en el origen | Actualizar catálogo o corregir el dato en el origen |

---

## 4. Errores de transformación

Los errores de transformación descartan el registro afectado. El proceso continúa con el siguiente registro.

| Código | Descripción | Causa probable | Acción recomendada |
|---|---|---|---|
| `E200` | Error en cálculo de campo derivado | División por cero, overflow numérico, dato inesperado | Revisar los valores del registro indicado en el log |

---

## 5. Errores de carga

| Código | Descripción | Causa probable | Acción recomendada |
|---|---|---|---|
| `E300` | Error de escritura en destino | BD destino caída o sin espacio | Verificar estado de la BD destino |
| `E301` | Violación de restricción de integridad | FK inexistente, dato duplicado con conflicto | Revisar restricciones del esquema destino; corregir dato |

---

## 6. Cómo interpretar el archivo de errores CSV

Al finalizar el proceso, se genera el archivo `logs/errores_YYYYMMDD_HHMMSS.csv` con el siguiente formato:

```
id_registro,fase,codigo_error,descripcion_error,valor_campo,nombre_campo
12345,VALIDACION,E100,"El campo 'importe' es obligatorio y está vacío","","importe"
67890,VALIDACION,E103,"El código 'XYZ' no existe en el catálogo de tipos de operación","XYZ","tipo_operacion"
11111,TRANSFORMACION,E200,"Error al calcular 'importe_con_tasa': tasa inválida","abc","tasa"
```

### Descripción de columnas

| Columna | Descripción |
|---|---|
| `id_registro` | Identificador del registro con error |
| `fase` | Fase en la que ocurrió el error (`EXTRACCION`, `VALIDACION`, `TRANSFORMACION`, `CARGA`) |
| `codigo_error` | Código del error (ver tablas anteriores) |
| `descripcion_error` | Descripción legible del error |
| `valor_campo` | Valor del campo que causó el error |
| `nombre_campo` | Nombre del campo que causó el error |

---

## 7. Procedimiento de reanudación

Si el proceso se interrumpe antes de completarse (error fatal, caída del sistema, etc.), puede reanudarse desde el último lote confirmado:

### Pasos:

1. **Identificar el último lote confirmado**: revise el archivo `logs/proceso_YYYYMMDD_HHMMSS.log` y busque la última línea con el mensaje `COMMIT lote N confirmado`.

2. **Calcular el lote de inicio**: el lote a partir del cual reanudar es `N + 1`.

3. **Ejecutar el proceso con `--lote-inicio`**:

   ```bash
   <comando de ejecución> --lote-inicio <N+1>
   ```

4. **Verificar el resumen final**: al terminar, revise el archivo `logs/resumen_YYYYMMDD_HHMMSS.txt` para confirmar que los totales son coherentes.

> **⚠️ Importante:** No reinicie el proceso desde el lote 1 si ya se han confirmado lotes anteriores, ya que los registros ya cargados serían procesados nuevamente (aunque la política `UPSERT` evitaría duplicados, generaría carga innecesaria).
