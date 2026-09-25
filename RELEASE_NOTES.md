# Release Notes

## [v1.1.0] - 2026-09-25

### Summary

Cross-platform release binaries, .NET 9 target, and a fail-closed `QueryValidator` table-source allow-list for `execute_read_query`.

### Added

- Self-contained release archives for **linux-x64**, **osx-arm64**, and **osx-x64** alongside existing **win-x64** (`McpServer-linux-x64.tar.gz`, `McpServer-osx-arm64.tar.gz`, `McpServer-osx-x64.tar.gz`, `McpServer-win-x64.zip`).
- [mcp.json.release.unix.example](mcp.json.release.unix.example) for Linux / macOS MCP host configuration.
- [docs/SECURITY_POSTURE.md](docs/SECURITY_POSTURE.md) — protections, gaps, SQL permissions, prompt injection, and production guidance.
- Unit tests for `QueryValidator` (`McpServer.Test/QueryValidatorTests.cs`), including allow/deny coverage for joins, derived tables, TVFs, `OPENJSON`, and unrecognized table sources.

### Changed

- Target framework updated from `net8.0` to `net9.0` (server and tests).
- `QueryValidator` table-source checks are **fail-closed**: only named tables/views, derived tables, and joins are allowed; TVFs, `OPENJSON`, and unrecognized ScriptDom sources are rejected (previously unknown sources were silently allowed).
- Release workflow publishes a matrix of RIDs (native OS runners), then attaches all archives to one GitHub Release.
- PR build CI (`build.yml`) runs on `ubuntu-latest`.
- Docs, README, CONTRIBUTING, and bug report template updated for multi-OS install and .NET 9.

### Notes

- End users: download the archive for your platform from [Releases](https://github.com/mvp-repos/mssql-mcp-server/releases); no .NET SDK required.
- Developers: [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later (.NET 10 SDK recommended for `dotnet test` with Microsoft.Testing.Platform).
- Publish this release with: `git tag v1.1.0` then `git push origin v1.1.0` (runs `.github/workflows/release.yml`).

## [v1.0.1] - 2026-07-26

### Fixed

- Serilog no longer fails in the single-file Windows release (`No Serilog:Using configuration section…`). The File sink assembly is registered via `ConfigurationReaderOptions`, and `appsettings.json` includes `"Using": [ "Serilog.Sinks.File" ]`.

## [v1.0.0] - 2026-07-26

### Summary

Initial release of mssql-mcp-server — a read-only MCP server for MSSQL Server exploration over stdio, with structured query results and validated SELECT execution.

### Added

- MCP server over stdio (JSON-RPC 2.0, protocol `2025-11-25`).
- Generic Host with dependency injection (`Program.cs`, `Startup.cs`).
- Configuration via `appsettings.json` and optional `appsettings.local.json`.
- Serilog **file** logging (console providers cleared so stdout stays MCP JSON-RPC only).
- Ten MCP tools: nine catalog tools plus `execute_read_query` (SELECT-only, ScriptDom-validated via `QueryValidator`).
- Responses use JSON-RPC `result` / `error`; `tools/call` returns MCP `content` / `isError` inside `result` (tabular data as JSON `QueryResult` in `text`).
- Tool execution failures set `result.isError: true`; protocol/stdio failures use JSON-RPC `error` (`-32601`/`-32602`/`-32603`).
- Startup database validation returns `false` on failure and is capped at **15 seconds**.
- Query limits via `QueryOptions` (`MaxRows`, `MaxCellLength`, `CommandTimeoutSeconds`).
- `SqlExecutor` for shared SQL execution.
- Release pipeline publishes self-contained Windows x64 zip (`McpServer-win-x64.zip`) when a `v*` tag is pushed (see Notes).
- Example configs: masked `appsettings.json` (`YOUR_*` placeholders), `mcp.json.example`, `mcp.json.release.example`, `.runsettings.example`.
- Integration test database script: `McpServer.Test/Script/integration-test-db.sql` (creates `mcp_test`).

### Notes

- Real secrets stay in `appsettings.local.json` and `.runsettings` (gitignored); copy from the masked templates and replace placeholders.
- Visual Studio publish profiles under `Properties/PublishProfiles/` are gitignored (machine-specific paths).
- To publish the GitHub Release asset: `git tag v1.0.0` then `git push origin v1.0.0` (runs `.github/workflows/release.yml`).
- Prefer `v1.1.0` (or later) for multi-platform binaries, .NET 9, and the fail-closed query validator.
