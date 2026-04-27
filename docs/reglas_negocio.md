# Reglas de negocio — Proceso-ABB_actualiza_2008

Este documento describe en detalle las reglas de validación y transformación aplicadas a cada registro durante el procesamiento de los datos ABB del ejercicio 2008.

---

## Tabla de contenidos

1. [Reglas de validación (RV)](#1-reglas-de-validación-rv)
2. [Reglas de transformación (TR)](#2-reglas-de-transformación-tr)
3. [Catálogos de referencia](#3-catálogos-de-referencia)
4. [Casos especiales](#4-casos-especiales)

---

## 1. Reglas de validación (RV)

Las reglas de validación se aplican **en el orden indicado**. Si un registro falla cualquiera de ellas, es descartado y registrado en el log de errores.

### RV-001 — Campos obligatorios no nulos

**Descripción**: Verifica que todos los campos marcados como obligatorios en el esquema de origen tengan un valor distinto de nulo o vacío.

**Campos obligatorios**:

| Campo | Tipo | Descripción |
|---|---|---|
| `id_registro` | Entero | Identificador único del registro |
| `codigo_abb` | Cadena | Código de referencia ABB |
| `fecha_operacion` | Fecha | Fecha de la operación |
| `importe` | Decimal | Importe de la operación |
| `tipo_operacion` | Cadena | Código del tipo de operación |

**Error generado si falla**: `E100`

---

### RV-002 — Formato y rango de fechas

**Descripción**: Verifica que el campo `fecha_operacion` cumpla con las siguientes condiciones:
- Formato válido: `YYYY-MM-DD`.
- La fecha debe pertenecer al año 2008 (entre `2008-01-01` y `2008-12-31` inclusive).

**Error generado si falla**: `E101`

---

### RV-003 — Rangos de valores numéricos

**Descripción**: Verifica que los campos numéricos cumplan con los rangos definidos:

| Campo | Rango permitido | Decimales máximos |
|---|---|---|
| `importe` | > 0 | 2 |
| `cantidad` | ≥ 0 | 0 (entero) |
| `tasa` | 0.0 – 100.0 | 4 |

**Error generado si falla**: `E102`

---

### RV-004 — Existencia de códigos de referencia

**Descripción**: Verifica que los campos que hacen referencia a catálogos maestros contengan valores existentes en dichos catálogos.

**Campos con referencia a catálogos**:

| Campo | Catálogo |
|---|---|
| `tipo_operacion` | Catálogo de tipos de operación |
| `codigo_producto` | Catálogo de productos |
| `codigo_entidad` | Catálogo de entidades |

**Error generado si falla**: `E103`

---

## 2. Reglas de transformación (TR)

Las transformaciones se aplican únicamente a los registros que han superado todas las validaciones.

### TR-001 — Normalización de cadenas de texto

**Descripción**: Limpia y normaliza todos los campos de tipo texto.

**Acciones aplicadas**:
- Eliminar espacios al inicio y al final (`trim`).
- Convertir a mayúsculas todos los campos de código.
- Eliminar caracteres especiales no permitidos en el esquema destino.

**Campos afectados**: `codigo_abb`, `tipo_operacion`, `codigo_producto`, `codigo_entidad`, `descripcion`.

---

### TR-002 — Conversión de tipos de datos

**Descripción**: Convierte los tipos de datos del esquema origen al esquema destino cuando difieren.

| Campo origen | Tipo origen | Tipo destino | Notas |
|---|---|---|---|
| `importe` | `VARCHAR` | `DECIMAL(15,2)` | Eliminar separadores de miles antes de convertir |
| `fecha_operacion` | `VARCHAR` | `DATE` | Formato esperado: `YYYY-MM-DD` |
| `cantidad` | `FLOAT` | `INTEGER` | Truncar decimales (no redondear) |

---

### TR-003 — Enriquecimiento con datos de catálogos actuales

**Descripción**: Agrega al registro información proveniente de los catálogos maestros vigentes, necesaria en el esquema destino pero ausente en el origen.

**Campos añadidos**:

| Campo nuevo | Origen | Descripción |
|---|---|---|
| `descripcion_tipo` | Catálogo de tipos de operación | Descripción textual del tipo de operación |
| `categoria_producto` | Catálogo de productos | Categoría del producto en el esquema actual |
| `nombre_entidad` | Catálogo de entidades | Nombre completo de la entidad |

---

### TR-004 — Cálculo de campos derivados

**Descripción**: Calcula campos adicionales requeridos por el esquema destino que no existen en el origen.

| Campo calculado | Fórmula | Descripción |
|---|---|---|
| `importe_con_tasa` | `importe × (1 + tasa / 100)` | Importe con la tasa aplicada |
| `anio_operacion` | `YEAR(fecha_operacion)` | Año extraído de la fecha (siempre 2008) |
| `trimestre` | `QUARTER(fecha_operacion)` | Trimestre fiscal (1–4) |
| `flag_actualizado` | `1` (constante) | Indicador de que el registro fue procesado por este ETL |

---

## 3. Catálogos de referencia

Los catálogos se cargan en memoria al inicio del proceso para optimizar las validaciones y el enriquecimiento.

| Catálogo | Fuente | Frecuencia de actualización |
|---|---|---|
| Tipos de operación | Tabla `CAT_TIPO_OPERACION` en BD destino | Actualización manual |
| Productos | Tabla `CAT_PRODUCTO` en BD destino | Actualización mensual |
| Entidades | Tabla `CAT_ENTIDAD` en BD destino | Actualización trimestral |

---

## 4. Casos especiales

### 4.1 Registros con importe cero

Los registros con `importe = 0` **no** son válidos según la regla `RV-003` y son descartados. Si en el futuro se requiere procesar este tipo de registros, se deberá crear una regla de validación alternativa.

### 4.2 Fechas en días festivos

El proceso no aplica ninguna restricción adicional por días festivos. Las fechas son válidas siempre y cuando cumplan con `RV-002`.

### 4.3 Registros duplicados en el origen

El proceso no deduplica en la fase de extracción. Si el origen contiene registros duplicados (mismo `id_registro`), ambos serán procesados. En la fase de carga, el segundo registro actualizará al primero (política `UPSERT` por `id_registro`).

### 4.4 Valores nulos en campos opcionales

Los campos no listados en `RV-001` son opcionales. Si están nulos o vacíos, el registro continúa siendo procesado y el campo se carga como `NULL` en el destino.
