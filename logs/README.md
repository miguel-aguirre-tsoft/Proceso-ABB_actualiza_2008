# Logs

Este directorio contiene los archivos de log generados en tiempo de ejecución por el proceso `ABB_actualiza_2008`.

## Archivos generados

| Archivo | Descripción |
|---|---|
| `proceso_YYYYMMDD_HHMMSS.log` | Log completo de la ejecución |
| `errores_YYYYMMDD_HHMMSS.csv` | Registros descartados con detalle del error |
| `resumen_YYYYMMDD_HHMMSS.txt` | Resumen ejecutivo con totales y estadísticas |

> **Nota:** Este directorio está incluido en `.gitignore` para evitar que los logs de ejecución sean commiteados al repositorio. El archivo `.gitkeep` existe únicamente para preservar el directorio en el repositorio.
