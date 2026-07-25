**Author:** Cursor  
**Editor:** Darshana Wijesinghe  
**Created Date:** 25/07/2026  

# Release Notes

## [v1.0.0] - 2026-07-25

### Summary

Initial release of SqlMcpServer — a read-only MCP server for SQL Server exploration over stdio, with structured query results and validated SELECT execution.

### Added

- MCP server over stdio (JSON-RPC 2.0, protocol `2025-11-25`)
- Generic Host with dependency injection (`Program.cs`, `Startup.cs`)
- Configuration via `appsettings.json` and optional `appsettings.local.json`
- Serilog file logging configured in appsettings
- Ten MCP tools: nine catalog tools plus `execute_read_query` (SELECT-only, ScriptDom-validated)
- Structured `QueryResult` responses (`Columns`, `Rows`, `RowCount`, `Truncated`, optional `Text`)
- Query limits via `QueryOptions` (`MaxRows`, `MaxCellLength`, `CommandTimeoutSeconds`)
- `SqlExecutor` for shared SQL execution and `QueryValidator` for read-query safety
- Self-contained Windows x64 release (`SqlMcpServer-win-x64.zip`) via GitHub Releases
- Example configs: masked `appsettings.json` (`YOUR_*` placeholders), `mcp.json.example`, `mcp.json.release.example`, `.runsettings.example`

### Notes

- Real secrets stay in `appsettings.local.json` and `.runsettings` (gitignored); copy from the masked templates and replace placeholders
- Visual Studio publish profiles under `Properties/PublishProfiles/` are gitignored (machine-specific paths)
