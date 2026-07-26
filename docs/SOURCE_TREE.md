# Source tree

Repository layout (build artifacts `bin/`, `obj/`, and `.vs/` are gitignored).

```
sql-mcp-server/                            # Repository root
├── .gitignore
├── .gitattributes
├── global.json                            # .NET test runner configuration
├── LICENSE.txt
├── README.md
├── CONTRIBUTING.md                        # Contribution guidelines
├── CODE_OF_CONDUCT.md                     # Community standards
├── SECURITY.md                            # Vulnerability reporting
├── RELEASE_NOTES.md                       # Version history
├── mcp.json.example                       # MCP config when running from source (dotnet run)
├── mcp.json.release.example               # MCP config when using downloaded exe from Releases
├── SqlMcpServer.sln
├── .github/
│   ├── ISSUE_TEMPLATE/
│   │   ├── bug_report.md
│   │   └── feature_request.md
│   └── workflows/
│       ├── build.yml                      # PR build + unit tests (main, dev; skips Integration)
│       └── release.yml                    # Unit tests + win-x64 zip on v* tag push
├── docs/
│   ├── index.md                           # Documentation master index
│   ├── PROJECT_OVERVIEW.md
│   └── SOURCE_TREE.md                     # This file
│
├── SqlMcpServer.Server/                   # MCP server (net8.0 console)
│   ├── SqlMcpServer.Server.csproj
│   ├── Program.cs                         # class Program — Generic Host, DI, Serilog, configuration
│   ├── Startup.cs                         # DB validation, stdio JSON-RPC loop
│   ├── appsettings.json                   # Masked shared template (YOUR_* placeholders; committed)
│   │                                      # Copy → appsettings.local.json for real values (gitignored)
│   │
│   ├── Models/
│   │   ├── AppSettings.cs                 # Database, QueryOptions, Log binding
│   │   ├── QueryResult.cs                 # Tool result DTO (columns, rows, text)
│   │   ├── JsonRpcRequest.cs              # Inbound: jsonrpc, id, method, params
│   │   ├── JsonRpcResponse.cs             # Outbound: jsonrpc, id, result, error
│   │   ├── McpCallToolResult.cs           # tools/call result: content + isError
│   │   ├── McpContentBlock.cs             # content[] item: type, text
│   │   ├── JsonRpcError.cs                # JSON-RPC error object
│   │   └── ErrorCodes.cs                  # Error codes (-32601, -32602, -32603)
│   │
│   ├── Services/
│   │   ├── McpMessageHandler.cs           # MCP protocol + tool dispatch (10 tools)
│   │   ├── DatabaseService.cs             # Catalog SQL + execute_read_query
│   │   ├── SqlExecutor.cs                 # Shared SQL execution, row limits
│   │   └── Interfaces/
│   │       ├── IDatabaseService.cs        # Database contract for handler and tests
│   │       └── ISqlExecutor.cs            # Low-level SQL execution contract
│   │
│   ├── Utils/
│   │   ├── JsonHelper.cs                  # JsonElement id → CLR type for responses
│   │   └── QueryValidator.cs              # SELECT-only validation via ScriptDom
│   │
│   └── Properties/                        # May hold local PublishProfiles (gitignored)
│
└── SqlMcpServer.Test/                     # Unit and integration tests (MSTest)
    ├── SqlMcpServer.Test.csproj
    ├── .runsettings.example               # Template for integration test DB connection (copy → .runsettings)
    ├── MSTestSettings.cs                  # Parallel test execution (method level)
    ├── McpMessageHandlerTests.cs          # Handler protocol and tool-call coverage
    ├── DatabaseServiceTests.cs            # DatabaseService unit tests (mocked executor)
    ├── DatabaseServiceIntegrationTests.cs # Live SQL Server integration tests
    ├── Script/
    │   └── integration-test-db.sql        # Creates mcp_test DB + objects for integration tests
    └── Helpers/
        ├── McpTestHelper.cs               # JSON-RPC request builders and assertions
        └── TestDatabaseService.cs         # In-memory IDatabaseService for handler tests
```

## File reference

### Root

| Path | Role |
|------|------|
| `.gitignore` | Excludes build output, IDE state, secrets, `appsettings.local.json`, `mcp.json`, `.runsettings`, `PublishProfiles/` |
| `.gitattributes` | Git line-ending and diff settings |
| `global.json` | Configures Microsoft.Testing.Platform as the test runner |
| `LICENSE.txt` | MIT license |
| `README.md` | Entry point, quick start, release install, configuration, and tool summary |
| `CONTRIBUTING.md` | Development setup, conventions, and pull request process |
| `CODE_OF_CONDUCT.md` | Contributor Covenant community standards |
| `SECURITY.md` | Private vulnerability reporting and disclosure policy |
| `RELEASE_NOTES.md` | Version history and release summaries |
| `mcp.json.example` | MCP template for `dotnet run` (developers) |
| `mcp.json.release.example` | MCP template for published `SqlMcpServer.Server.exe` (end users) |
| `.github/workflows/build.yml` | CI: restore, build, and unit tests on PRs to `main`/`dev` (excludes `Integration`) |
| `.github/workflows/release.yml` | CI: unit tests (excludes `Integration`), publish, zip, GitHub Release on `v*` tags |
| `.github/ISSUE_TEMPLATE/` | GitHub issue templates for bugs and feature requests |
| `SqlMcpServer.sln` | Solution file (Server + Test projects) |
| `docs/index.md` | Master documentation index |

### `SqlMcpServer.Server/Program.cs`

`class Program` with `Main`: builds the Generic Host, clears console logging providers (stdout is MCP-only), loads appsettings, configures Serilog file logging, registers `ISqlExecutor`, `IDatabaseService`, and `McpMessageHandler`, then runs `Startup`.

### `SqlMcpServer.Server/Startup.cs`

Validates database connectivity (15s timeout), reads stdin lines, deserializes JSON-RPC, calls `McpMessageHandler`, writes one JSON line per response. Exits when stdin closes. On unexpected exceptions after a request is parsed, replies with JSON-RPC `-32603` so the host does not hang. Serialization omits null properties (`DefaultIgnoreCondition.WhenWritingNull`).

### Configuration

| File | Role |
|------|------|
| `appsettings.json` | Masked committed template (`YOUR_*` placeholders for connection string and log path); copied to output |
| `appsettings.local.json` | Machine-specific secrets (gitignored) — copy from `appsettings.json` and replace placeholders |

### `Models/`

| File | Role |
|------|------|
| `AppSettings.cs` | Options binding for database, query limits, log settings |
| `QueryResult.cs` | Structured tool output (`Columns`, `Rows`, `RowCount`, `Truncated`, `Text`) |
| `JsonRpcRequest.cs` | Incoming message DTO (`JsonElement` for `id` and `params`) |
| `JsonRpcResponse.cs` | Outgoing message DTO (`id`, `result`, `error`) |
| `McpCallToolResult.cs` | `tools/call` payload (`content`, `isError`) |
| `McpContentBlock.cs` | Content block (`type`, `text`) |
| `JsonRpcError.cs` | JSON-RPC error payload (`code`, `message`, optional `data`) |
| `ErrorCodes.cs` | `MethodNotFound` (-32601), `InvalidParams` (-32602), `InternalError` (-32603) |

### `Services/`

| File | Role |
|------|------|
| `McpMessageHandler.cs` | `initialize`, `ping`, `tools/list`, `tools/call`; MCP `content` inside `result`; JSON-RPC `error` for protocol failures |
| `DatabaseService.cs` | Catalog queries and `ExecuteReadQueryAsync` |
| `SqlExecutor.cs` | Executes SQL with row/cell limits and timeouts |
| `Interfaces/IDatabaseService.cs` | Public contract implemented by `DatabaseService` |
| `Interfaces/ISqlExecutor.cs` | Execution contract implemented by `SqlExecutor` |

### `Utils/`

| File | Role |
|------|------|
| `JsonHelper.cs` | `ConvertId` extension for JSON-RPC response ids |
| `QueryValidator.cs` | Parses and validates SELECT-only SQL for `execute_read_query` |

### `SqlMcpServer.Server.csproj`

- Target: `net8.0` executable
- Packages: `Microsoft.Data.SqlClient`, `Microsoft.Extensions.Hosting`, Serilog, ScriptDom
- `GenerateDocumentationFile` enabled
- Copies `appsettings.json` to output directory

### `SqlMcpServer.Test/`

| Path | Role |
|------|------|
| `McpMessageHandlerTests.cs` | Handler protocol and tool-call coverage |
| `DatabaseServiceTests.cs` | `DatabaseService` with mocked `ISqlExecutor` |
| `DatabaseServiceIntegrationTests.cs` | Live SQL Server tests (`[TestCategory("Integration")]`) |
| `Script/integration-test-db.sql` | Creates `mcp_test` with tables/views/procs/functions/trigger + seed data |
| `.runsettings.example` | Template — copy to `.runsettings` and set `DbConnectionString` |
| `.runsettings` | Local integration DB credentials (gitignored) |
| `Helpers/McpTestHelper.cs` | Builds `JsonRpcRequest` payloads; reads tool `content` / `isError`; asserts protocol errors |
| `Helpers/TestDatabaseService.cs` | Test double for `IDatabaseService` |
| `MSTestSettings.cs` | `[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]` |

### Not committed (gitignored)

| Pattern | Reason |
|---------|--------|
| `bin/`, `obj/` | Build output |
| `.vs/`, `*.user` | IDE machine state |
| `appsettings.local.json`, `appsettings.*.local.json` | May contain connection strings |
| `mcp.json`, `.env` | May contain secrets |
| `**/.runsettings` | Integration test connection strings |
| `**/Properties/PublishProfiles/` | Local Visual Studio publish profiles (machine-specific paths) |

## Dependency flow

```
Program.cs
    ├── Configuration (appsettings.json + appsettings.local.json)
    ├── Serilog
    ├── ISqlExecutor ← SqlExecutor
    ├── IDatabaseService ← DatabaseService
    └── Startup
            ├── McpMessageHandler
            │       ├── IDatabaseService
            │       └── ILogger<McpMessageHandler>
            └── IDatabaseService (connection validation)

DatabaseService
    └── ISqlExecutor

execute_read_query path
    DatabaseService → QueryValidator → ISqlExecutor

SqlMcpServer.Test
    ├── TestDatabaseService : IDatabaseService
    ├── McpMessageHandlerTests → McpMessageHandler
    ├── DatabaseServiceTests → DatabaseService (mock ISqlExecutor)
    └── DatabaseServiceIntegrationTests → DatabaseService (live SQL)
```

## Namespaces

| Namespace | Contents |
|-----------|----------|
| `SqlMcpServer.Server` | `Startup` |
| `SqlMcpServer.Server.Models` | DTOs, settings, error codes, query visitor |
| `SqlMcpServer.Server.Services` | Handler, database access, SQL executor |
| `SqlMcpServer.Server.Services.Interfaces` | `IDatabaseService`, `ISqlExecutor` |
| `SqlMcpServer.Server.Utils` | JSON helpers and query validation |
| *(file-scoped / global)* | `class Program` in `Program.cs` |
