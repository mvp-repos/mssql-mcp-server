# mssql-mcp-server

A .NET 9 [MCP](https://modelcontextprotocol.io/) server that exposes **read-only** MSSQL Server tools to AI hosts (Cursor, Claude Desktop, etc.) over **stdio**.

## Architecture & security

```
  MCP host (Cursor, …)                 mssql-mcp-server                    MSSQL Server
  ┌──────────────────┐                 ┌──────────────────────────┐        ┌──────────┐
  │  agent / tools   │◄──stdio────────►│  JSON-RPC handler        │        │  login   │
  │  (trusted spawn) │   JSON-RPC      │           │              │  TDS   │ (least   │
  └──────────────────┘                 │           ▼              │───────►│ privilege│
                                       │  catalog tools           │        │  SELECT) │
                                       │  (fixed SQL + SqlParam)  │        └──────────┘
                                       │           │              │
                                       │  execute_read_query      │
                                       │       │                  │
                                       │       ▼                  │
                                       │  QueryValidator          │  SELECT-only, fail-closed
                                       │       │                  │
                                       │       ▼                  │
                                       │  SqlExecutor             │  QueryOptions:
                                       │  (shared)                │  MaxRows / MaxCell / timeout
                                       └──────────────────────────┘
```

No HTTP listener — transport is **stdio only**. Only `execute_read_query` goes through `QueryValidator`; catalog tools use fixed SQL (arguments bound with `SqlParameter` where needed). Row/cell/timeout caps apply in `SqlExecutor`. Details: [Project overview](docs/PROJECT_OVERVIEW.md), [Security posture](docs/SECURITY_POSTURE.md).

## Documentation

| Document | Description |
|----------|-------------|
| [Documentation index](docs/index.md) | Master index for all project docs |
| [Project overview](docs/PROJECT_OVERVIEW.md) | Architecture, protocol flow, security, and all MCP tools |
| [Source tree](docs/SOURCE_TREE.md) | Repository layout and file responsibilities |
| [Contributing](CONTRIBUTING.md) | Development setup, conventions, and pull request guidelines |
| [Release notes](RELEASE_NOTES.md) | Version history |
| [Security policy](SECURITY.md) | How to report vulnerabilities |
| [Security posture](docs/SECURITY_POSTURE.md) | Protections, gaps, permissions, and production guidance |
| [Code of conduct](CODE_OF_CONDUCT.md) | Community standards |

## Requirements

- MSSQL Server reachable from the machine running the server.
- An MCP host that can spawn the process.
- **End users (release install):** Windows x64, Linux x64, or macOS (arm64 / x64); no .NET SDK required.
- **Developers:** [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later (.NET 10 SDK recommended for `dotnet test` with Microsoft.Testing.Platform).

## Install from GitHub Release (recommended)

For local use in Cursor without cloning or building:

1. Open **[Releases](https://github.com/mvp-repos/mssql-mcp-server/releases)** and download the archive for your OS from the latest `v*` tag (see [RELEASE_NOTES.md](RELEASE_NOTES.md)):

   | Platform | Asset |
   |----------|--------|
   | Windows x64 | `McpServer-win-x64.zip` |
   | Linux x64 | `McpServer-linux-x64.tar.gz` |
   | macOS Apple Silicon | `McpServer-osx-arm64.tar.gz` |
   | macOS Intel | `McpServer-osx-x64.tar.gz` |

2. Extract to a folder (for example `C:\Tools\McpServer\` or `~/Tools/McpServer/`). On Linux/macOS, ensure the binary is executable (`chmod +x McpServer.Server` if needed).
3. Copy shipped `appsettings.json` to `appsettings.local.json` beside the binary (`McpServer.Server.exe` on Windows, `McpServer.Server` on Linux/macOS).
4. Edit `appsettings.local.json` — replace the `YOUR_*` placeholders for `Database.ConnectionString` and `Serilog.WriteTo[0].Args.path` (use an OS-appropriate log path).
5. Copy the MCP example for your OS into your Cursor MCP config (e.g. `.cursor/mcp.json` or user settings) and set `command` to the full path of the binary:
   - Windows: [mcp.json.release.example](mcp.json.release.example)
   - Linux / macOS: [mcp.json.release.unix.example](mcp.json.release.unix.example)
6. Restart Cursor and enable the **sqlmcp** server.

Do **not** commit `appsettings.local.json`, `.runsettings`, `mcp.json`, publish profiles, or other files with real credentials or machine-specific paths (see [.gitignore](.gitignore)).

## Quick start (developers)

### Clone and build

```powershell
git clone https://github.com/mvp-repos/mssql-mcp-server.git
cd mssql-mcp-server
dotnet build McpServer.sln
```

Default branch is `main`.

### Configure locally

1. Copy [appsettings.json](McpServer.Server/appsettings.json) to `McpServer.Server/appsettings.local.json`.
2. Edit `appsettings.local.json` — replace the `YOUR_*` placeholders with your SQL Server connection string and log file path.
3. Leave [appsettings.json](McpServer.Server/appsettings.json) as the masked shared template (local values override it).

### Run locally (stdio)

```powershell
dotnet run --project McpServer.Server/McpServer.Server.csproj
```

The server reads JSON-RPC from stdin and writes responses to stdout. Startup fails with exit code **1** if the database is unreachable or configuration is missing.

### Run tests

```powershell
# Unit tests only (matches CI)
dotnet test --solution McpServer.sln --filter "TestCategory!=Integration"
```

Or target the test project directly:

```powershell
dotnet test --project McpServer.Test/McpServer.Test.csproj --filter "TestCategory!=Integration"
```

Unit tests use mocks and do not require SQL Server. Integration tests (`[TestCategory("Integration")]`) need a live database:

1. Run [integration-test-db.sql](McpServer.Test/Script/integration-test-db.sql) to create `mcp_test`.
2. Copy [`.runsettings.example`](McpServer.Test/.runsettings.example) → `McpServer.Test/.runsettings`, set `DbConnectionString`, then run with `--settings`.

CI skips integration tests.

### Configure in Cursor (from source)

Copy [mcp.json.example](mcp.json.example) to your Cursor MCP config and fix the project path in `args`. Ensure `appsettings.local.json` exists beside the server project (or in the run output directory).

## Create a release (maintainers)

Pushing a version tag triggers [.github/workflows/release.yml](.github/workflows/release.yml), which runs unit tests (excludes `Integration`), publishes self-contained single-file binaries for **win-x64**, **linux-x64**, **osx-arm64**, and **osx-x64**, and attaches those archives to the GitHub Release.

```powershell
git checkout main
git pull
git tag v1.0.0
git push origin v1.0.0
```

Replace `v1.0.0` with your semver tag. Check the **Actions** tab for the workflow run, then the **Releases** tab for the download.

## Configuration

Settings are loaded from JSON files in the server working directory (`appsettings.json`, then optional `appsettings.local.json` overrides).

| Section | Key | Description |
|---------|-----|-------------|
| `Database` | `ConnectionString` | MSSQL Server connection string (**required**) |
| `Serilog` | `WriteTo[].Args.path` | Log file path (required for file logging) |
| `QueryOptions` | `MaxRows` | Maximum rows returned per query (default: 500) |
| `QueryOptions` | `MaxCellLength` | Maximum string length per cell before truncation (default: 5000) |
| `QueryOptions` | `CommandTimeoutSeconds` | MSSQL command timeout (default: 30) |

See [appsettings.json](McpServer.Server/appsettings.json) for the masked committed template. Copy it to `appsettings.local.json` and replace the `YOUR_*` placeholders (never commit the local file).

## MCP tools

Responses use JSON-RPC **`result`** / **`error`**. `tools/call` puts MCP **`content`** / **`isError`** inside `result`. See [Project overview — Response shape](docs/PROJECT_OVERVIEW.md#response-shape) and [Tool results](docs/PROJECT_OVERVIEW.md#tool-results).

| Tool | Arguments | Description |
|------|-----------|-------------|
| `list_tables` | — | User base tables (`SCHEMANAME`, `TABLENAME`) |
| `list_views` | — | Views (`SCHEMANAME`, `VIEWNAME`) |
| `list_procedures` | — | Stored procedures (`SCHEMANAME`, `PROCEDURENAME`) |
| `list_triggers` | — | Triggers (`PARENTOBJECT`, `TRIGGERNAME`, `ISDISABLED`) |
| `list_functions` | — | User functions (`SCHEMANAME`, `FUNCTIONNAME`) |
| `describe_table` | `tableName` | Column metadata for a table (`schema.table` or name) |
| `get_object_definition` | `objectName` | T-SQL for procedure, view, function, or trigger |
| `search_definitions` | `text` | Objects whose module body contains `text` |
| `find_references` | `objectName` | Objects that reference a table or view |
| `execute_read_query` | `sql` | Run a validated **SELECT-only** query |

## Contributing

Contributions are welcome. See [CONTRIBUTING.md](CONTRIBUTING.md) for setup, coding conventions, testing, and how to submit a pull request.

## License

This project is licensed under the [MIT License](LICENSE.txt).
