# SqlMcpServer

A .NET 8 [MCP](https://modelcontextprotocol.io/) server that exposes **read-only** SQL Server tools to AI hosts (Cursor, Claude Desktop, etc.) over **stdio**.

## Documentation

| Document | Description |
|----------|-------------|
| [Project overview](docs/PROJECT_OVERVIEW.md) | Architecture, protocol flow, security, and all MCP tools |
| [Source tree](docs/SOURCE_TREE.md) | Repository layout and file responsibilities |
| [Contributing](CONTRIBUTING.md) | Development setup, conventions, and pull request guidelines |
| [Release notes](RELEASE_NOTES.md) | Version history |
| [Security policy](SECURITY.md) | How to report vulnerabilities |
| [Code of conduct](CODE_OF_CONDUCT.md) | Community standards |

## Requirements

- SQL Server reachable from the machine running the server
- An MCP host that can spawn the process
- **End users (release install):** Windows x64 only; no .NET SDK required
- **Developers:** [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Install from GitHub Release (recommended)

For local use in Cursor without cloning or building:

1. Open **[Releases](https://github.com/darwijesinghe/sql-mcp-server/releases)** and download **SqlMcpServer-win-x64.zip** from the latest `v*` tag.
2. Unzip to a folder, for example `C:\Tools\SqlMcpServer\`.
3. Copy [appsettings.local.json.example](SqlMcpServer.Server/appsettings.local.json.example) to `appsettings.local.json` in the same folder as `SqlMcpServer.Server.exe`.
4. Edit `appsettings.local.json` — set `Database.ConnectionString` and `Serilog.WriteTo[0].Args.path` (log file path).
5. Copy [mcp.json.release.example](mcp.json.release.example) into your Cursor MCP config (e.g. `.cursor/mcp.json` or user settings). Set `command` to the full path of `SqlMcpServer.Server.exe`.
6. Restart Cursor and enable the **sqlmcp** server.

Do **not** commit `appsettings.local.json`, `mcp.json`, or other files with real credentials (see [.gitignore](.gitignore)).

## Quick start (developers)

### Clone and build

```bash
git clone https://github.com/darwijesinghe/sql-mcp-server.git
cd sql-mcp-server
dotnet build SqlMcpServer.sln
```

### Configure locally

1. Copy [appsettings.local.json.example](SqlMcpServer.Server/appsettings.local.json.example) to `SqlMcpServer.Server/appsettings.local.json`.
2. Edit `appsettings.local.json` with your SQL Server connection string and log file path.
3. Leave [appsettings.json](SqlMcpServer.Server/appsettings.json) as the shared defaults template (local values override it).

### Run locally (stdio)

```powershell
dotnet run --project SqlMcpServer.Server/SqlMcpServer.Server.csproj
```

The server reads JSON-RPC from stdin and writes responses to stdout. Startup fails with exit code **1** if the database is unreachable or configuration is missing.

### Run tests

```bash
dotnet test SqlMcpServer.sln
```

Or target the test project directly:

```bash
dotnet test --project SqlMcpServer.Test/SqlMcpServer.Test.csproj
```

Unit tests use mocks and do not require SQL Server. Integration tests in `DatabaseServiceIntegrationTests` require a configured database.

### Configure in Cursor (from source)

Copy [mcp.json.example](mcp.json.example) to your Cursor MCP config and fix the project path in `args`. Ensure `appsettings.local.json` exists beside the server project (or in the run output directory).

## Create a release (maintainers)

Pushing a version tag triggers [.github/workflows/release.yml](.github/workflows/release.yml), which runs tests, publishes a self-contained Windows x64 exe, and attaches **SqlMcpServer-win-x64.zip** to the GitHub Release.

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
| `Database` | `ConnectionString` | SQL Server connection string (**required**) |
| `Serilog` | `WriteTo[].Args.path` | Log file path (required for file logging) |
| `QueryOptions` | `MaxRows` | Maximum rows returned per query (default: 500) |
| `QueryOptions` | `MaxCellLength` | Maximum string length per cell before truncation (default: 5000) |
| `QueryOptions` | `CommandTimeoutSeconds` | SQL command timeout (default: 30) |

See [appsettings.json](SqlMcpServer.Server/appsettings.json) for the committed defaults and [appsettings.local.json.example](SqlMcpServer.Server/appsettings.local.json.example) for a local template.

## MCP tools

All tools return a structured **`QueryResult`** in the JSON-RPC `result` (columns, rows, row count, truncation flag, and optional `text` for definition-style output). See [Project overview — Tool results](docs/PROJECT_OVERVIEW.md#tool-results).

| Tool | Arguments | Description |
|------|-----------|-------------|
| `list_tables` | — | User base tables |
| `list_views` | — | Views |
| `list_procedures` | — | Stored procedures |
| `list_triggers` | — | Triggers |
| `list_functions` | — | User functions |
| `describe_table` | `tableName` | Column metadata for a table (`schema.table` or name) |
| `get_object_definition` | `objectName` | T-SQL for procedure, view, function, or trigger |
| `search_definitions` | `text` | Objects whose module body contains `text` |
| `find_references` | `objectName` | Objects that reference a table or view |
| `execute_read_query` | `sql` | Run a validated **SELECT-only** query |

## Contributing

Contributions are welcome. See [CONTRIBUTING.md](CONTRIBUTING.md) for setup, coding conventions, testing, and how to submit a pull request.

## License

This project is licensed under the [MIT License](LICENSE.txt).
