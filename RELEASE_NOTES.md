# Release Notes

## [v1.0.0] - 2026-07-26

### Summary

Initial release of SqlMcpServer — a read-only MCP server for SQL Server exploration over stdio, with structured query results and validated SELECT execution.

### Added

- MCP server over stdio (JSON-RPC 2.0, protocol `2025-11-25`)
- Generic Host with dependency injection (`Program.cs`, `Startup.cs`)
- Configuration via `appsettings.json` and optional `appsettings.local.json`
- Serilog file logging configured in appsettings
- Ten MCP tools: nine catalog tools plus `execute_read_query` (SELECT-only, ScriptDom-validated)
- Responses use JSON-RPC `result` / `error`; `tools/call` returns MCP `content` / `isError` inside `result` (tabular data as JSON `QueryResult` in `text`)
- Tool execution failures set `result.isError: true`; protocol/stdio failures use JSON-RPC `error` (`-32601`/`-32602`/`-32603`)
- Query limits via `QueryOptions` (`MaxRows`, `MaxCellLength`, `CommandTimeoutSeconds`)
- `SqlExecutor` for shared SQL execution and `QueryValidator` for read-query safety
- Self-contained Windows x64 release (`SqlMcpServer-win-x64.zip`) via GitHub Releases
- Example configs: masked `appsettings.json` (`YOUR_*` placeholders), `mcp.json.example`, `mcp.json.release.example`, `.runsettings.example`
- Integration test database script: `SqlMcpServer.Test/Script/integration-test-db.sql` (creates `mcp_test`)

### Notes

- Real secrets stay in `appsettings.local.json` and `.runsettings` (gitignored); copy from the masked templates and replace placeholders
- Visual Studio publish profiles under `Properties/PublishProfiles/` are gitignored (machine-specific paths)
