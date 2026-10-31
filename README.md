# modulo-cicd-2026

Repositorio de la materia de Integración Continua. Contiene un servicio de inventario en .NET
(Clean Architecture + DDD), un cliente React y la configuración de hooks de Git usada en clase.

> ¿Eres un agente de IA? Lee primero [`AGENTS.md`](AGENTS.md).

## Stack

| Componente | Tecnología |
|---|---|
| API | .NET 8 (`net8.0`), ASP.NET Core, Swagger (Swashbuckle) |
| Aplicación | MediatR (CQRS), `Joseco.DDD.Core` (agregados, eventos, `Result`) |
| Persistencia | EF Core 9 + Npgsql, PostgreSQL 16 (Docker) |
| Tests .NET | xUnit, Moq, `Microsoft.AspNetCore.Mvc.Testing`, coverlet |
| Cliente | React 19, Vite 8, Axios, ESLint, Prettier |
| Contratos | Pact (`@pact-foundation/pact`) con Mocha + Chai |
| Hooks | Husky 9 + lint-staged |

## Estructura

```
.
├── inventory-project/           # Servicio de inventario (.NET)
│   ├── src/
│   │   ├── Inventory.Domain/          # Agregados, value objects, eventos de dominio
│   │   ├── Inventory.Application/     # Commands, queries y handlers (MediatR)
│   │   ├── Inventory.Infrastructure/  # EF Core, repositorios, migraciones
│   │   └── Inventory.WebApi/          # Controllers y Program.cs
│   ├── Inventory.Tests/         # Tests unitarios + script de cobertura
│   ├── Inventory.IntTests/      # Tests de integración
│   ├── project/                 # Reglas de testing
│   └── Inventory.sln
├── react-client/                # Cliente React + tests de contrato Pact
├── odd/tasks/                   # Documentos de tareas por feature
├── .husky/                      # Hooks de Git
├── project/                     # Reglas de todo el repositorio (trunk-based, worktrees, Git)
├── docker-compose.yml           # PostgreSQL
└── AGENTS.md                    # Reglas para agentes de IA
```

## Requisitos previos

- .NET SDK 8.x
- Herramienta `dotnet-ef` (para aplicar migraciones)
- Docker con Docker Compose
- Node.js, pnpm (raíz) y Yarn 1.22.22 (`react-client`)
- PowerShell (solo para el reporte HTML de cobertura)

## Puesta en marcha

```bash
# 1. Dependencias de la raíz: instala Husky y activa los hooks
pnpm install

# 2. Base de datos (desde la raíz, donde está docker-compose.yml)
docker compose up -d postgres

# 3. Servicio .NET
cd inventory-project
dotnet restore
dotnet ef database update --project src/Inventory.Infrastructure --startup-project src/Inventory.WebApi --context PersistenceDbContext

# 4. Cliente React
cd ../react-client
yarn install
```

Base de datos local: `localhost:5432`, base `StoreInventory`, usuario `postgres`,
contraseña `postgresspassword` (solo para desarrollo).

## Ejecutar

| Qué | Dónde | Comando |
|---|---|---|
| PostgreSQL | raíz del repositorio | `docker compose up -d postgres` |
| API | `inventory-project/` | `dotnet run --project src/Inventory.WebApi` |
| Cliente | `react-client/` | `yarn dev` |

La API escucha en `http://localhost:5094` (perfil `http`) y abre Swagger en `/swagger`.
En entorno Development aplica las migraciones al arrancar.

Para detener la base de datos, desde la raíz: `docker compose stop` (conserva datos) o
`docker compose down -v` (borra el volumen).

## Tests

| Tipo | Dónde | Comando |
|---|---|---|
| Todos (.NET) | `inventory-project/` | `dotnet test` |
| Unitarios | `inventory-project/` | `dotnet test Inventory.Tests` |
| Integración | `inventory-project/` | `dotnet test Inventory.IntTests` |
| Cobertura | `inventory-project/` | `dotnet test --collect:"XPlat Code Coverage"` |
| Reporte HTML de cobertura | `inventory-project/Inventory.Tests/` (PowerShell) | `./ExecCodeCoverage.ps1` |
| Contratos Pact | `react-client/` | `yarn test:pact` |

Reglas de cada tipo de test:

- [Tests unitarios](inventory-project/project/unit-testing-rules.md)
- [Tests de integración](inventory-project/project/integration-tests-rules.md)
- [Tests de contrato Pact](react-client/project/pact-rules.md)

## Hooks de Git

| Momento | Qué se ejecuta |
|---|---|
| `pre-commit` | `npx lint-staged`: `dotnet format` sobre los `*.cs` y `prettier --write` en `react-client` |
| `pre-push` | `dotnet test inventory-project/Inventory.Tests` |

Los hooks solo funcionan después de `pnpm install` en la raíz. No los omitas con `--no-verify`.
Junto con los tests relevantes ejecutados en local, son la condición para integrar en `main`.

## Flujo de trabajo

Las reglas de todo el repositorio están en [`project/`](project/README.md).

- **Trunk-based development**: `main` es el tronco y debe estar siempre en verde y listo para
  publicar. Solo se usan ramas cortas desde `main` que se integran en uno o dos días como máximo,
  mediante pull requests pequeños; sin ramas `develop` ni de release. Reglas completas:
  [`project/trunk-based-development.md`](project/trunk-based-development.md).
- **Cada feature se desarrolla en su propio worktree de Git**, nunca en el checkout principal.
  Ubicación, comandos, preparación del worktree y recursos compartidos (PostgreSQL, puertos):
  [`project/worktree-workflow.md`](project/worktree-workflow.md).
- Ramas, commits (Conventional Commits), hooks y pull requests:
  [`project/git-conventions.md`](project/git-conventions.md).

Resumen:

```bash
git fetch origin
git worktree add --no-track -b feat/<nombre> ../repo-worktrees/<nombre> origin/main
cd ../repo-worktrees/<nombre>
pnpm install   # sin este paso los hooks no se ejecutan en el worktree
```

Para features de varios pasos, documenta el plan en `odd/tasks/<nombre>.md` dentro del worktree.
Implementa respetando la dirección de dependencias, agrega los tests junto con el cambio, rebasa
con frecuencia (`git fetch origin && git rebase origin/main`) y abre un pull request pequeño hacia
`main`. Tras el merge, elimina la rama y el worktree.
