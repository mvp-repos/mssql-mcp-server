# Release Notes

## [v1.0.0] - 2026-06-06

### Summary

Initial release of SqlMcpServer — a read-only MCP server for SQL Server exploration over stdio, with structured query results and validated SELECT execution.

### Added

- MCP server over stdio (JSON-RPC 2.0, protocol `2025-11-25`)
- Generic Host with dependency injection (`Program.cs`, `Startup.cs`)
- Configuration via `appsettings.json` and optional `appsettings.local.json`
- Serilog file logging configured in appsettings
- Ten MCP tools: nine catalog tools plus `execute_read_query` (SELECT-only, ScriptDom-validated)
- Structured `QueryResult` responses (columns, rows, row count, truncation)
- Query limits via `QueryOptions` (`MaxRows`, `MaxCellLength`, `CommandTimeoutSeconds`)
- `SqlExecutor` for shared SQL execution and `QueryValidator` for read-query safety
- Self-contained Windows x64 release (`SqlMcpServer-win-x64.zip`) via GitHub Releases
- Example configs: `appsettings.local.json.example`, `mcp.json.example`, `mcp.json.release.example`
