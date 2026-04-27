# Diagrama de flujo — Proceso-ABB_actualiza_2008

Este documento describe en detalle el flujo de ejecución del proceso `ABB_actualiza_2008`, desde la lectura de los datos de origen hasta la confirmación de la carga en el sistema destino.

---

## Flujo general del proceso

```
INICIO
  │
  ▼
Cargar configuración (config.yaml / variables de entorno)
  │
  ├── ¿Error en configuración? ──▶ FATAL E000: Configuración inválida → FIN
  │
  ▼
Inicializar logger y archivo de auditoría
  │
  ▼
┌────────────────────────────────────────────┐
│              FASE 1: EXTRACCIÓN            │
│                                            │
│  Conectar a la fuente de datos ABB 2008    │
│    │                                       │
│    ├── ¿Error de conexión? ──▶ FATAL E001  │
│    │                                       │
│  Ejecutar consulta de extracción           │
│    │                                       │
│    ├── ¿Timeout? ──▶ Reintentar (máx. 3)  │
│    │     ├── 3 fallos ──▶ FATAL E002       │
│    │                                       │
│  Leer registros en lotes (BATCH_SIZE)      │
│                                            │
│  Log: "Extracción completada: N registros" │
└────────────────────────────────────────────┘
  │
  ▼
┌────────────────────────────────────────────┐
│           FASE 2: TRANSFORMACIÓN           │
│                                            │
│  Para cada registro:                       │
│    │                                       │
│    ▼                                       │
│  Validar campos obligatorios               │
│    ├── Falla ──▶ ERROR E100, descartar     │
│    │                                       │
│  Validar formato de fechas                 │
│    ├── Falla ──▶ ERROR E101, descartar     │
│    │                                       │
│  Validar rangos de valores numéricos       │
│    ├── Falla ──▶ ERROR E102, descartar     │
│    │                                       │
│  Validar códigos de referencia             │
│    ├── Falla ──▶ ERROR E103, descartar     │
│    │                                       │
│  Aplicar transformaciones (TR-001..TR-004) │
│    ├── Error ──▶ ERROR E200, descartar     │
│    │                                       │
│  Añadir registro a lote transformado       │
│                                            │
│  Log: "Transformación: N válidos, M errores│
└────────────────────────────────────────────┘
  │
  ▼
┌────────────────────────────────────────────┐
│               FASE 3: CARGA               │
│                                            │
│  Conectar al sistema destino               │
│    ├── ¿Error? ──▶ FATAL E300             │
│    │                                       │
│  Para cada lote transformado:              │
│    │                                       │
│    ▼                                       │
│  Iniciar transacción                       │
│    │                                       │
│  UPSERT registro a registro                │
│    ├── ¿Error E301? ──▶ Descartar registro │
│    │                                       │
│  ¿Todos los registros del lote OK?         │
│    ├── SÍ ──▶ COMMIT                       │
│    └── NO ──▶ ROLLBACK + ERROR E300        │
│                                            │
│  Log: "Carga completada: N registros"      │
└────────────────────────────────────────────┘
  │
  ▼
Generar resumen de ejecución
  │
  ▼
Escribir archivos de log:
  • proceso_YYYYMMDD_HHMMSS.log
  • errores_YYYYMMDD_HHMMSS.csv
  • resumen_YYYYMMDD_HHMMSS.txt
  │
  ▼
FIN
```

---

## Diagrama de estados de un registro

Cada registro individual atraviesa los siguientes estados posibles:

```
         EXTRAÍDO
            │
     ┌──────┴──────┐
     │             │
   VÁLIDO      INVÁLIDO ──▶ [Descartado con ERROR E1xx]
     │
     │
  TRANSFORMADO
     │
  ┌──┴──┐
  │     │
CARGADO  ERROR ──▶ [Descartado con ERROR E2xx / E3xx]
```

---

## Notas

- Los registros descartados en cualquier fase son registrados en el archivo `errores_YYYYMMDD_HHMMSS.csv`.
- Un error `FATAL` detiene el proceso completo; los errores `ERROR` solo descartan el registro afectado.
- El proceso puede reanudarse desde el último lote confirmado usando el parámetro `--lote-inicio`.
