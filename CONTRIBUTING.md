# Guía de Contribución — Proceso-ABB_actualiza_2008

Gracias por su interés en contribuir a este proyecto. Esta guía describe el proceso y las convenciones que debe seguir para realizar cambios de forma ordenada y segura.

---

## Tabla de contenidos

1. [Flujo de trabajo con Git](#1-flujo-de-trabajo-con-git)
2. [Convenciones de nomenclatura de ramas](#2-convenciones-de-nomenclatura-de-ramas)
3. [Convenciones de mensajes de commit](#3-convenciones-de-mensajes-de-commit)
4. [Estándares de código](#4-estándares-de-código)
5. [Proceso de revisión (Pull Request)](#5-proceso-de-revisión-pull-request)
6. [Reportar errores o proponer mejoras](#6-reportar-errores-o-proponer-mejoras)

---

## 1. Flujo de trabajo con Git

Se utiliza el modelo **GitHub Flow**:

1. Cree su rama de trabajo a partir de `main`.
2. Realice sus cambios en commits pequeños y descriptivos.
3. Abra un Pull Request (PR) hacia `main` cuando los cambios estén listos para revisión.
4. Espere la aprobación de al menos un revisor antes de hacer merge.
5. El merge a `main` lo realiza el revisor aprobador.

```
main
  │
  ├── feature/descripcion-cambio
  ├── fix/descripcion-corrección
  └── docs/descripcion-documentacion
```

---

## 2. Convenciones de nomenclatura de ramas

| Prefijo | Uso | Ejemplo |
|---|---|---|
| `feature/` | Nueva funcionalidad | `feature/validacion-importes` |
| `fix/` | Corrección de error | `fix/error-conexion-origen` |
| `docs/` | Cambios en documentación | `docs/actualizar-readme` |
| `refactor/` | Refactorización sin cambio funcional | `refactor/modulo-extractor` |
| `test/` | Adición o corrección de pruebas | `test/pruebas-transformador` |

---

## 3. Convenciones de mensajes de commit

Se sigue el estándar **Conventional Commits**:

```
<tipo>(<alcance>): <descripción corta en imperativo>

[cuerpo opcional — explicación del cambio]

[pie opcional — referencias a issues, breaking changes]
```

### Tipos permitidos

| Tipo | Descripción |
|---|---|
| `feat` | Nueva funcionalidad |
| `fix` | Corrección de error |
| `docs` | Cambios en documentación |
| `refactor` | Refactorización de código sin cambio funcional |
| `test` | Adición o modificación de pruebas |
| `chore` | Tareas de mantenimiento (dependencias, configuración) |
| `perf` | Mejora de rendimiento |

### Ejemplos válidos

```
feat(validador): añadir regla de validación de importes negativos
fix(cargador): corregir rollback incorrecto en lotes grandes
docs(readme): actualizar instrucciones de instalación
test(extractor): añadir prueba de timeout de conexión
```

---

## 4. Estándares de código

- Todo código nuevo debe incluir **pruebas unitarias** con una cobertura mínima del 80%.
- Los nombres de variables, funciones y clases deben estar en **español** siguiendo la convención del proyecto existente.
- Se deben agregar comentarios en español para lógica de negocio compleja.
- No deben incluirse credenciales, contraseñas ni información sensible en el código.
- El archivo `config/config.yaml` (con credenciales reales) nunca debe ser commiteado.

---

## 5. Proceso de revisión (Pull Request)

Al abrir un PR, asegúrese de:

- [ ] La descripción del PR explica claramente **qué** cambia y **por qué**.
- [ ] Todas las pruebas existentes pasan sin errores.
- [ ] Se han añadido pruebas para la nueva funcionalidad o corrección.
- [ ] El código sigue los estándares definidos en esta guía.
- [ ] No se incluyen archivos de configuración con credenciales.

El PR debe ser revisado y aprobado por al menos un integrante del equipo antes de hacer merge.

---

## 6. Reportar errores o proponer mejoras

Use la sección **Issues** del repositorio para:

- Reportar errores encontrados en el proceso.
- Proponer nuevas funcionalidades o mejoras.
- Documentar comportamientos inesperados.

Al crear un issue, incluya:

1. **Título** claro y descriptivo.
2. **Descripción** del problema o propuesta.
3. **Pasos para reproducir** (en caso de error).
4. **Comportamiento esperado** vs. **comportamiento actual** (en caso de error).
5. **Entorno** donde se reproduce (versión del sistema, configuración relevante).
