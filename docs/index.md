**Author:** Cursor  
**Editor:** Darshana Wijesinghe  
**Created Date:** 25/07/2026  

# Documentation index

Master index for SqlMcpServer project documentation. Use this as the entry point before coding, planning, or reviewing changes.

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
| [appsettings.local.json.example](../SqlMcpServer.Server/appsettings.local.json.example) | Copy → `appsettings.local.json` (gitignored) |
| [mcp.json.example](../mcp.json.example) | Cursor MCP when running from source |
| [mcp.json.release.example](../mcp.json.release.example) | Cursor MCP when using the release exe |
| [.runsettings.example](../SqlMcpServer.Test/.runsettings.example) | Copy → `.runsettings` for integration tests (gitignored) |
