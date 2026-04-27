# Proceso-ABB_actualiza_2008

> Documentación del proyecto **ABB_procesa_2008** — proceso de actualización y gestión de registros ABB del ejercicio 2008.

---

## Tabla de contenidos

1. [Descripción general](#1-descripción-general)
2. [Contexto y propósito](#2-contexto-y-propósito)
3. [Arquitectura del proceso](#3-arquitectura-del-proceso)
4. [Estructura del repositorio](#4-estructura-del-repositorio)
5. [Requisitos previos](#5-requisitos-previos)
6. [Instalación y configuración](#6-instalación-y-configuración)
7. [Ejecución del proceso](#7-ejecución-del-proceso)
8. [Descripción detallada de los módulos](#8-descripción-detallada-de-los-módulos)
9. [Flujo de datos](#9-flujo-de-datos)
10. [Manejo de errores y logs](#10-manejo-de-errores-y-logs)
11. [Pruebas](#11-pruebas)
12. [Preguntas frecuentes (FAQ)](#12-preguntas-frecuentes-faq)
13. [Contribución](#13-contribución)
14. [Historial de cambios](#14-historial-de-cambios)
15. [Licencia y contacto](#15-licencia-y-contacto)

---

## 1. Descripción general

**Proceso-ABB_actualiza_2008** es un proceso automatizado diseñado para la **lectura, validación, transformación y actualización** de registros del sistema ABB correspondientes al ejercicio fiscal **2008**.

El proceso toma como entrada los datos originales almacenados en las fuentes de información de ABB (archivos planos, bases de datos o servicios), los transforma conforme a las reglas de negocio establecidas y los carga en el sistema destino, garantizando la integridad y trazabilidad de la información procesada.

### Características principales

- **Lectura de datos de origen**: extracción de registros ABB del periodo 2008 desde múltiples fuentes.
- **Validación y limpieza**: verificación de formatos, rangos de valores y consistencia referencial.
- **Transformación**: aplicación de reglas de negocio para adaptar los datos al esquema destino.
- **Carga (actualización)**: inserción o actualización de registros en el sistema destino.
- **Auditoría y logging**: generación de bitácoras detalladas con estadísticas de procesamiento y errores.

---

## 2. Contexto y propósito

### ¿Qué es ABB en este contexto?

Dentro de este proyecto, **ABB** hace referencia al conjunto de registros, catálogos o entidades de negocio que forman parte del sistema central de la organización. El sufijo **_2008** indica que el alcance del proceso abarca exclusivamente los datos generados o vigentes durante el año **2008**.

### Problema que resuelve

Con el paso del tiempo, los datos históricos de 2008 pueden encontrarse en formatos o estructuras que difieren del esquema actual del sistema. Este proceso cubre la necesidad de:

- Migrar esos registros al formato y estándar vigentes.
- Corregir inconsistencias detectadas en el ejercicio 2008.
- Asegurar que la información sea consultable y utilizable en los sistemas actuales.

### Alcance

| Aspecto | Detalle |
|---|---|
| Periodo de datos | Ejercicio fiscal 2008 |
| Tipo de operación | Actualización / migración |
| Sistema origen | Repositorio legacy ABB 2008 |
| Sistema destino | Sistema ABB actual |
| Modalidad de ejecución | Proceso por lotes (batch) |

---

## 3. Arquitectura del proceso

El proceso sigue una arquitectura **ETL** (Extract → Transform → Load):

```
┌─────────────────────────────────────────────────────────────────┐
│                        PROCESO ABB_actualiza_2008               │
│                                                                 │
│  ┌──────────┐    ┌──────────────┐    ┌──────────────────────┐  │
│  │ EXTRACT  │───▶│  TRANSFORM   │───▶│        LOAD          │  │
│  │          │    │              │    │                      │  │
│  │ • Lectura│    │ • Validación │    │ • Inserción/Update   │  │
│  │   de     │    │ • Limpieza   │    │ • Confirmación       │  │
│  │   origen │    │ • Mapeo de   │    │ • Rollback si error  │  │
│  │          │    │   campos     │    │                      │  │
│  └──────────┘    └──────────────┘    └──────────────────────┘  │
│        │                │                       │               │
│        └────────────────┴───────────────────────┘               │
│                             │                                   │
│                    ┌────────▼────────┐                          │
│                    │   AUDITORÍA /   │                          │
│                    │     LOGGING     │                          │
│                    └─────────────────┘                          │
└─────────────────────────────────────────────────────────────────┘
```

### Componentes

| Componente | Responsabilidad |
|---|---|
| **Extractor** | Lee y deserializa los registros de la fuente de datos 2008 |
| **Validador** | Verifica reglas de integridad, formatos y valores permitidos |
| **Transformador** | Aplica mapeos y reglas de negocio para adaptar los registros |
| **Cargador** | Escribe los registros transformados en el sistema destino |
| **Logger / Auditor** | Registra métricas, errores y trazabilidad de cada registro procesado |

---

## 4. Estructura del repositorio

```
Proceso-ABB_actualiza_2008/
│
├── README.md                  ← Este archivo (documentación principal)
├── CONTRIBUTING.md            ← Guía de contribución al proyecto
│
├── src/                       ← Código fuente del proceso
│   ├── extractor/             ← Módulos de extracción de datos
│   ├── transformer/           ← Módulos de transformación y validación
│   ├── loader/                ← Módulos de carga al sistema destino
│   └── utils/                 ← Utilidades comunes (logging, configuración)
│
├── config/                    ← Archivos de configuración
│   ├── config.example.yaml    ← Plantilla de configuración (sin credenciales)
│   └── mappings/              ← Definición de mapeos de campos
│
├── docs/                      ← Documentación adicional
│   ├── diagrama_flujo.md      ← Diagrama detallado del flujo de datos
│   ├── reglas_negocio.md      ← Descripción de las reglas de transformación
│   └── guia_errores.md        ← Guía de interpretación de errores comunes
│
├── tests/                     ← Pruebas unitarias e integración
│   ├── unit/
│   └── integration/
│
└── logs/                      ← Directorio de salida de logs (generado en runtime)
    └── .gitkeep
```

> **Nota:** La estructura de directorios anterior refleja la organización recomendada para el proyecto. Los directorios vacíos que aún no contienen código serán incorporados en futuras iteraciones del desarrollo.

---

## 5. Requisitos previos

Antes de ejecutar el proceso, asegúrese de contar con:

- **Entorno de ejecución**: según la tecnología del proyecto (se documentará al añadir el código fuente).
- **Acceso a la fuente de datos**: credenciales o conexión al repositorio de datos ABB 2008.
- **Acceso al sistema destino**: permisos de escritura en el sistema ABB actual.
- **Variables de entorno**: configuradas según la plantilla `config/config.example.yaml`.

---

## 6. Instalación y configuración

### 6.1 Clonar el repositorio

```bash
git clone https://github.com/miguel-aguirre-tsoft/Proceso-ABB_actualiza_2008.git
cd Proceso-ABB_actualiza_2008
```

### 6.2 Configuración del entorno

Copie la plantilla de configuración y complete los parámetros según su entorno:

```bash
cp config/config.example.yaml config/config.yaml
# Edite config/config.yaml con los valores correspondientes a su entorno
```

> **⚠️ Importante:** Nunca cometa el archivo `config/config.yaml` al repositorio, ya que puede contener credenciales. Este archivo está incluido en `.gitignore`.

### 6.3 Variables de entorno requeridas

| Variable | Descripción | Ejemplo |
|---|---|---|
| `ABB_SOURCE_DB` | Cadena de conexión a la BD origen | `server=host;db=abb2008` |
| `ABB_TARGET_DB` | Cadena de conexión a la BD destino | `server=host;db=abb_actual` |
| `ABB_LOG_DIR` | Directorio de salida de logs | `./logs` |
| `ABB_BATCH_SIZE` | Número de registros por lote | `1000` |

---

## 7. Ejecución del proceso

### Ejecución estándar

```bash
# Ejecutar el proceso completo
<comando de ejecución según tecnología>

# Ejecutar solo la fase de extracción (modo dry-run)
<comando> --fase extraccion --dry-run

# Ejecutar solo la fase de carga
<comando> --fase carga
```

### Parámetros de línea de comandos

| Parámetro | Descripción | Valor por defecto |
|---|---|---|
| `--fase` | Fase a ejecutar: `extraccion`, `transformacion`, `carga`, `todo` | `todo` |
| `--dry-run` | Ejecuta sin escribir en el destino | `false` |
| `--lote-inicio` | Número de lote desde el cual reanudar | `1` |
| `--verbose` | Activa logging detallado | `false` |
| `--config` | Ruta al archivo de configuración | `config/config.yaml` |

---

## 8. Descripción detallada de los módulos

### 8.1 Módulo Extractor

**Responsabilidad**: Conectarse a la fuente de datos del sistema ABB 2008 y extraer los registros a procesar.

**Proceso interno**:
1. Establece conexión con el origen configurado.
2. Ejecuta la consulta de extracción parametrizada por rango de fechas (año 2008).
3. Serializa los registros en la estructura interna del proceso.
4. Registra en el log el total de registros extraídos.

**Salida**: Colección de registros en formato interno para la fase de transformación.

---

### 8.2 Módulo Validador / Transformador

**Responsabilidad**: Validar la integridad de cada registro y aplicar las transformaciones requeridas.

**Reglas de validación aplicadas**:

| Regla | Descripción |
|---|---|
| `RV-001` | Los campos obligatorios no deben ser nulos o vacíos |
| `RV-002` | Las fechas deben estar en formato `YYYY-MM-DD` y pertenecer al año 2008 |
| `RV-003` | Los importes numéricos deben ser positivos y con máximo 2 decimales |
| `RV-004` | Los códigos de referencia deben existir en el catálogo maestro |

**Transformaciones aplicadas**:

| Transformación | Descripción |
|---|---|
| `TR-001` | Normalización de cadenas de texto (mayúsculas, eliminación de espacios) |
| `TR-002` | Conversión de tipos de datos al esquema destino |
| `TR-003` | Enriquecimiento con datos de catálogos actuales |
| `TR-004` | Cálculo de campos derivados requeridos por el esquema destino |

---

### 8.3 Módulo Cargador

**Responsabilidad**: Persistir los registros transformados en el sistema destino.

**Proceso interno**:
1. Inicia una transacción en el sistema destino.
2. Por cada lote de registros, ejecuta operaciones `INSERT` o `UPDATE` según corresponda.
3. Si todos los registros del lote se procesan correctamente, confirma la transacción (`COMMIT`).
4. En caso de error, revierte la transacción (`ROLLBACK`) y registra el error en el log.

**Política de duplicados**: Si un registro ya existe en el destino (identificado por su clave primaria), se aplica una actualización (`UPSERT`).

---

### 8.4 Módulo de Logging y Auditoría

**Responsabilidad**: Registrar métricas, eventos y errores del proceso para asegurar trazabilidad completa.

**Niveles de log**:

| Nivel | Uso |
|---|---|
| `INFO` | Eventos normales del proceso (inicio, fin de fase, totales) |
| `WARN` | Situaciones anómalas no críticas (registros con datos incompletos opcionales) |
| `ERROR` | Errores que impiden procesar un registro específico |
| `FATAL` | Errores críticos que detienen el proceso completo |

**Archivos generados**:

| Archivo | Contenido |
|---|---|
| `logs/proceso_YYYYMMDD_HHMMSS.log` | Log completo del proceso |
| `logs/errores_YYYYMMDD_HHMMSS.csv` | Detalle de registros con error (clave, motivo) |
| `logs/resumen_YYYYMMDD_HHMMSS.txt` | Resumen ejecutivo con totales y estadísticas |

---

## 9. Flujo de datos

El siguiente diagrama describe el flujo de un registro individual a través del proceso:

```
REGISTRO ORIGEN (ABB 2008)
         │
         ▼
  ┌─────────────┐
  │  EXTRACCIÓN │  ──── ¿Error de conexión? ──▶ FATAL: detener proceso
  └──────┬──────┘
         │
         ▼
  ┌─────────────┐
  │  VALIDACIÓN │  ──── ¿Falla validación? ──▶ ERROR: registrar en log, descartar registro
  └──────┬──────┘
         │ Registro válido
         ▼
  ┌────────────────┐
  │ TRANSFORMACIÓN │  ──── ¿Error de transformación? ──▶ ERROR: registrar en log, descartar
  └──────┬─────────┘
         │ Registro transformado
         ▼
  ┌─────────────┐
  │    CARGA    │  ──── ¿Error de carga? ──▶ ROLLBACK + ERROR: registrar en log
  └──────┬──────┘
         │ Éxito
         ▼
  REGISTRO ACTUALIZADO EN DESTINO
         │
         ▼
  LOG: Registro procesado correctamente
```

---

## 10. Manejo de errores y logs

### Códigos de error

| Código | Fase | Descripción | Acción |
|---|---|---|---|
| `E001` | Extracción | Error de conexión con el origen | Detener proceso (FATAL) |
| `E002` | Extracción | Timeout en la consulta | Reintentar (máx. 3 veces) |
| `E100` | Validación | Campo obligatorio nulo | Descartar registro |
| `E101` | Validación | Formato de fecha inválido | Descartar registro |
| `E102` | Validación | Valor fuera de rango | Descartar registro |
| `E103` | Validación | Código de referencia inexistente | Descartar registro |
| `E200` | Transformación | Error en cálculo de campo derivado | Descartar registro |
| `E300` | Carga | Error de escritura en destino | Rollback del lote |
| `E301` | Carga | Violación de restricción de integridad | Descartar registro |

### Resumen de ejecución

Al finalizar, el proceso genera un resumen con las siguientes métricas:

```
============================================================
 RESUMEN DE EJECUCIÓN - ABB_actualiza_2008
 Fecha/Hora: YYYY-MM-DD HH:MM:SS
============================================================
 Registros extraídos   :   10,000
 Registros válidos     :    9,850
 Registros con error   :      150
 Registros cargados    :    9,850
 Tiempo total          :    00:05:32
============================================================
```

---

## 11. Pruebas

### Tipos de pruebas

| Tipo | Descripción | Ubicación |
|---|---|---|
| **Unitarias** | Prueba de cada módulo de forma aislada | `tests/unit/` |
| **Integración** | Prueba del flujo completo con datos de muestra | `tests/integration/` |

### Ejecución de pruebas

```bash
# Ejecutar todas las pruebas
<comando de pruebas según tecnología>

# Ejecutar solo pruebas unitarias
<comando> --scope unit

# Ejecutar solo pruebas de integración
<comando> --scope integration
```

### Cobertura esperada

- Módulo Validador: ≥ 90%
- Módulo Transformador: ≥ 85%
- Módulo Cargador: ≥ 80%

---

## 12. Preguntas frecuentes (FAQ)

**¿Qué ocurre si el proceso se interrumpe a mitad de la ejecución?**

El proceso es reanudable. Puede reiniciarse desde el último lote confirmado utilizando el parámetro `--lote-inicio`. Consulte el log de la ejecución anterior para identificar el número de lote desde el cual reanudar.

**¿Los datos originales de 2008 son modificados?**

No. El proceso trabaja únicamente sobre el sistema destino. La fuente de datos ABB 2008 se accede en modo de solo lectura.

**¿Qué sucede si un registro ya existe en el destino?**

Se aplica una política de `UPSERT`: si el registro ya existe (identificado por su clave primaria), se actualiza con los nuevos valores transformados. Si no existe, se inserta como nuevo registro.

**¿Cómo puedo verificar qué registros fallaron?**

Revise el archivo `logs/errores_YYYYMMDD_HHMMSS.csv` generado al finalizar el proceso. Este archivo contiene la clave de cada registro con error y el motivo del fallo.

---

## 13. Contribución

Consulte el archivo [CONTRIBUTING.md](CONTRIBUTING.md) para conocer las pautas de contribución al proyecto.

En resumen:
1. Cree una rama desde `main` con el formato `feature/<descripcion>` o `fix/<descripcion>`.
2. Realice los cambios, incluyendo pruebas unitarias.
3. Verifique que todas las pruebas pasan antes de abrir un Pull Request.
4. Describa claramente los cambios realizados en el PR.

---

## 14. Historial de cambios

| Versión | Fecha | Descripción |
|---|---|---|
| 1.0.0 | 2026-04-27 | Creación del repositorio y documentación inicial del proceso |

---

## 15. Licencia y contacto

**Proyecto interno** — uso restringido a los equipos autorizados de la organización.

Para consultas o soporte, contacte al equipo responsable a través de los canales internos establecidos.
