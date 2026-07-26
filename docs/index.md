# Documentation index

Master index for SqlMcpServer project documentation. Use this as the entry point before coding, planning, or reviewing changes.

Wire format note: JSON-RPC `result` / `error`; tool results use `result.content` / `result.isError` — see [PROJECT_OVERVIEW.md — Response shape](PROJECT_OVERVIEW.md#response-shape).

## Project docs

| Document | Description |
|----------|-------------|
| [PROJECT_OVERVIEW.md](PROJECT_OVERVIEW.md) | Purpose, architecture, MCP methods/tools, configuration, security, testing, releases |
| [SOURCE_TREE.md](SOURCE_TREE.md) | Repository layout, file responsibilities, dependency flow |

## Root docs

| Document | Description |
|----------|-------------|
| [README.md](../README.md) | Quick start, release install, configuration, tool summary |
| [CONTRIBUTING.md](../CONTRIBUTING.md) | Development setup, conventions, PR process |
| [SECURITY.md](../SECURITY.md) | Vulnerability reporting and disclosure |
| [RELEASE_NOTES.md](../RELEASE_NOTES.md) | Version history |
| [CODE_OF_CONDUCT.md](../CODE_OF_CONDUCT.md) | Community standards |

## Example configs (templates only)

| File | Use |
|------|-----|
| [appsettings.json](../SqlMcpServer.Server/appsettings.json) | Masked template — copy → `appsettings.local.json` (gitignored) |
| [mcp.json.example](../mcp.json.example) | Cursor MCP when running from source |
| [mcp.json.release.example](../mcp.json.release.example) | Cursor MCP when using the release exe |
| [.runsettings.example](../SqlMcpServer.Test/.runsettings.example) | Copy → `.runsettings` for integration tests (gitignored) |
| [integration-test-db.sql](../SqlMcpServer.Test/Script/integration-test-db.sql) | Creates `mcp_test` schema/data for integration tests |
