**Author:** Cursor  
**Editor:** Darshana Wijesinghe  
**Created Date:** 25/07/2026  

# Contributing to SqlMcpServer

Thank you for your interest in contributing. This guide covers how to set up a development environment, follow project conventions, and submit changes.

## Before you start

- Read the [documentation index](docs/index.md) and [project overview](docs/PROJECT_OVERVIEW.md) for architecture, MCP protocol flow, and security constraints.
- Review the [source tree](docs/SOURCE_TREE.md) to understand where code lives.
- MCP tools are **read-only** — catalog queries plus `execute_read_query` (SELECT-only, validated via ScriptDom).
- Follow the [code of conduct](CODE_OF_CONDUCT.md). Report security issues per [SECURITY.md](SECURITY.md), not public issues.

## Development setup

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later (CI uses .NET 10 SDK for Microsoft.Testing.Platform via `global.json`)
- SQL Server reachable from your machine (optional for unit tests; required for integration tests and manual checks)
- An MCP host such as Cursor (optional, for end-to-end testing)

### Clone and build

```powershell
git clone https://github.com/mvp-repos/sql-mcp-server.git
cd sql-mcp-server
dotnet build SqlMcpServer.sln
```

Default branch is `main`.

### Configure appsettings

1. Copy [appsettings.local.json.example](SqlMcpServer.Server/appsettings.local.json.example) to `SqlMcpServer.Server/appsettings.local.json`.
2. Set `Database.ConnectionString` and your Serilog log path in the local file.
3. Never commit `appsettings.local.json` (see [.gitignore](.gitignore)).

### Run tests

Unit and handler tests use mocks and do **not** require SQL Server. Prefer the same filter CI uses:

```powershell
dotnet test --solution SqlMcpServer.sln --filter "TestCategory!=Integration"
```

Or target the test project directly:

```powershell
dotnet test --project SqlMcpServer.Test/SqlMcpServer.Test.csproj --filter "TestCategory!=Integration"
```

`DatabaseServiceIntegrationTests` (`[TestCategory("Integration")]`) require a live SQL Server. Copy [`.runsettings.example`](SqlMcpServer.Test/.runsettings.example) to `SqlMcpServer.Test/.runsettings`, set `DbConnectionString`, and never commit `.runsettings`. They are excluded from GitHub Actions.

### Run locally (stdio)

```powershell
dotnet run --project SqlMcpServer.Server/SqlMcpServer.Server.csproj
```

For Cursor integration from source, copy [mcp.json.example](mcp.json.example), adjust the project path in `args`, and ensure `appsettings.local.json` is configured.

## What to contribute

Good contributions include:

- Bug fixes with tests
- New read-only catalog tools (with handler, service, executor, test double, and tests)
- Improvements to `QueryValidator` / `SafeQueryVisitor` safety rules
- Documentation improvements
- Test coverage for existing behavior

Out of scope unless discussed with maintainers first:

- HTTP or non-stdio transports
- Write SQL (INSERT, UPDATE, DELETE, DDL) or bypassing query validation
- Breaking changes to existing tool output formats without a version bump plan

## Code conventions

### C# style

- Match existing patterns in the file you are editing (namespaces, naming, structure).
- Keep changes focused; avoid unrelated refactors in the same pull request.
- `GenerateDocumentationFile` is enabled — public APIs need XML documentation.

When you **modify** an existing method, class, or interface, add or update a `<remarks>` block describing what changed and why:

```csharp
/// <remarks>
/// This method has been updated to [clear explanation of what changed and why].
/// <para>
/// Author: Your Name<br/>
/// Last Updated: dd/MM/yyyy
/// </para>
/// </remarks>
```

Include XML `<summary>`, `<param>`, `<returns>`, and `<exception>` tags on public members. Add inline comments only for non-obvious logic.

### Adding a new MCP tool

Follow the extension points documented in [Project overview — Extension points](docs/PROJECT_OVERVIEW.md#extension-points):

1. Add a method to `IDatabaseService` and implement it in `DatabaseService` (read-only SQL only).
2. Register the tool in `McpMessageHandler` (`HandleToolsList` and `HandleToolCallAsync`).
3. Update `TestDatabaseService` in `SqlMcpServer.Test/Helpers/` with test data for the new method.
4. Add unit tests in `McpMessageHandlerTests` and/or `DatabaseServiceTests`.
5. Update [README.md](README.md) tool table and [docs/PROJECT_OVERVIEW.md](docs/PROJECT_OVERVIEW.md) if behavior is user-facing.

### Project layout

| Project | Purpose |
|---------|---------|
| `SqlMcpServer.Server` | MCP server executable (Generic Host, DI, Serilog) |
| `SqlMcpServer.Test` | MSTest unit and integration tests |

Do not commit build output (`bin/`, `obj/`), IDE state, publish profiles (`Properties/PublishProfiles/`), or files that may contain secrets (`appsettings.local.json`, `mcp.json`, `.runsettings`, `.env`).

## Security

- Never commit connection strings, passwords, `appsettings.local.json`, `.runsettings`, or real `mcp.json` configs.
- Do not commit Visual Studio publish profiles with machine-specific paths.
- Use a SQL login with least privilege (metadata read access; avoid `sa` in shared environments).
- Treat MCP hosts as trusted only when you control them.
- Any new SQL surface must go through validation (`QueryValidator`) or use fixed, parameterized catalog SQL.

## Commit messages

Use [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<scope>): <short summary>
```

Types: `feat`, `fix`, `chore`, `docs`, `refactor`, `test`, `perf`, `ci`, `build`, `revert`.

Examples:

```
feat(tools): add list_indexes catalog tool

fix(handler): return InvalidParams when tableName is missing

docs(readme): document appsettings configuration
```

Keep commits small and focused. Each commit should build and pass tests.

## Pull request process

1. Fork the repository and create a feature branch from `main`.
2. Make your changes and add or update tests as needed.
3. Run `dotnet test --solution SqlMcpServer.sln --filter "TestCategory!=Integration"` locally.
4. Open a pull request targeting `main` with:
   - A clear summary of what changed and why
   - Steps to verify the change (test commands, manual checks)
   - Notes on any documentation updates
5. Address review feedback. Maintainers may squash or rebase before merge.

CI (`.github/workflows/build.yml`) runs on pull requests targeting `main` or `dev`.

## Releases (maintainers)

Pushing a semver tag matching `v*` triggers [.github/workflows/release.yml](.github/workflows/release.yml):

```powershell
git checkout main
git pull
git tag v1.0.0
git push origin v1.0.0
```

The workflow runs unit tests (excludes `Integration`), publishes a self-contained Windows x64 executable, and attaches **SqlMcpServer-win-x64.zip** to the GitHub Release. End users must add their own `appsettings.local.json` beside the exe.

## Questions

Open an issue for bugs, feature ideas, or questions before large changes. For small fixes, a pull request with a short description is fine.

## License

By contributing, you agree that your contributions will be licensed under the [MIT License](LICENSE.txt).
