using McpServer.Server.Models;

namespace McpServer.Server.Services.Interfaces;

/// <summary>
/// Read-only catalog queries against a SQL Server database for MCP tool handlers.
/// </summary>
public interface IDatabaseService
{
    /// <summary>
    /// Verifies that the configured database is reachable by opening and closing a connection.
    /// Called once at application startup before the MCP message loop begins.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// <see langword="true"/> if the connection is successfully opened and closed;
    /// <see langword="false"/> if the server is unreachable, authentication fails, or the attempt is cancelled.
    /// </returns>
    Task<bool> ValidateConnectionAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Lists user-defined base tables from <c>sys.tables</c>.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <remarks>
    /// Excludes Microsoft-shipped tables (<c>is_ms_shipped = 1</c>) and temporal history tables
    /// (<c>temporal_type = 2</c>).
    /// <para>
    /// Each result line:<br/>
    /// <c>SchemaName.TableName</c> (for example, <c>dbo.Orders</c>).
    /// </para>
    /// </remarks>
    /// <returns>
    /// A object of <see cref="QueryResult"/> containing the result.
    /// </returns>
    Task<QueryResult> GetTablesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Lists views from <c>sys.views</c>.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <remarks>
    /// Returns all views visible in the catalog view, including system views if present.
    /// <para>
    /// Each result line:<br/>
    /// <c>SchemaName.ViewName</c> (for example, <c>dbo.vwActiveCustomers</c>).
    /// </para>
    /// </remarks>
    /// <returns>
    /// A object of <see cref="QueryResult"/> containing the result.
    /// </returns>
    Task<QueryResult> GetViewsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Lists user-defined stored procedures from <c>sys.procedures</c>.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <remarks>
    /// Excludes Microsoft-shipped procedures (<c>is_ms_shipped = 1</c>) and numbered procedure
    /// variants (<c>parent_object_id &lt;&gt; 0</c>, for example <c>MyProc;2</c>).
    /// <para>
    /// Each result line:<br/>
    /// <c>SchemaName.ProcedureName</c> (for example, <c>dbo.usp_GetOrders</c>).
    /// </para>
    /// </remarks>
    /// <returns>
    /// A object of <see cref="QueryResult"/> containing the result.
    /// </returns>
    Task<QueryResult> GetProceduresAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Lists triggers from <c>sys.triggers</c>.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <remarks>
    /// Excludes Microsoft-shipped triggers (<c>is_ms_shipped = 1</c>).
    /// DML triggers show the parent table or view as <c>schema.object</c>. Database-level DDL
    /// triggers use <c>(database)</c> as the parent.
    /// <para>
    /// Each result line:<br/>
    /// <c>ParentObject.TriggerName.IsDisabled</c> (for example, <c>dbo.Orders.tr_OrderAudit.False</c> or
    /// <c>(database).tr_AuditDDL.True</c>).
    /// </para>
    /// </remarks>
    /// <returns>
    /// A object of <see cref="QueryResult"/> containing the result.
    /// </returns>
    Task<QueryResult> GetTriggersAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Lists user-defined functions from <c>sys.objects</c> (scalar and table-valued).
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <remarks>
    /// Includes SQL scalar (<c>FN</c>), inline table-valued (<c>IF</c>), and
    /// multi-statement table-valued (<c>TF</c>) functions. Excludes Microsoft-shipped objects
    /// (<c>is_ms_shipped = 1</c>).
    /// <para>
    /// Each result line:<br/>
    /// <c>SchemaName.FunctionName</c> (for example, <c>dbo.fn_FormatDate</c>).
    /// </para>
    /// </remarks>
    /// <returns>
    /// A object of <see cref="QueryResult"/> containing the result.
    /// </returns>
    Task<QueryResult> GetFunctionsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Returns column metadata for a table or view from <c>INFORMATION_SCHEMA.COLUMNS</c>.
    /// </summary>
    /// <remarks>
    /// <paramref name="tableName"/> may be a bare name (<c>Orders</c>) or qualified
    /// (<c>dbo.Orders</c>). When qualified, both schema and table name filter the result; when bare,
    /// all schemas with that table name are included.<br/><br/>
    /// <para>
    /// Each result line:<br/>
    /// <c>ColumnName.DataType.IsNullable.CharacterMaximumLength</c> (for example,
    /// <c>OrderId.int.NO.</c> or <c>Notes.nvarchar.YES.500</c>). A null max length appears as an empty
    /// fourth segment.
    /// </para>
    /// </remarks>
    /// <param name="tableName">Table to describe, optionally as <c>schema.table</c>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A object of <see cref="QueryResult"/> containing the result.
    /// </returns>
    Task<QueryResult> DescribeTableAsync(string tableName, CancellationToken cancellationToken);

    /// <summary>
    /// Returns the T-SQL source definition of a schema-scoped object (view, procedure, function, or trigger).
    /// </summary>
    /// <remarks>
    /// <paramref name="objectName"/> may be <c>schema.object</c> (recommended, e.g. <c>dbo.usp_GetOrders</c>)
    /// or a bare object name (resolved via default schema, or the first match when ambiguous).
    /// Uses <c>OBJECT_DEFINITION</c> against <c>sys.objects</c>, with a fallback lookup for database-level
    /// DDL triggers (not resolvable through <c>OBJECT_ID</c> alone).
    /// Returns an explanatory message when no definition exists (unknown name, table, or encrypted module
    /// without <c>VIEW DEFINITION</c> permission).
    /// </remarks>
    /// <param name="objectName">Object to script, preferably as <c>schema.object</c>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A object of <see cref="QueryResult"/> containing the result.
    /// </returns>
    Task<QueryResult> GetObjectDefinitionAsync(string objectName, CancellationToken cancellationToken);

    /// <summary>
    /// Searches module definitions (procedures, views, functions, triggers) for the given text.
    /// </summary>
    /// <remarks>
    /// Queries <c>sys.sql_modules</c> with a case-sensitive <c>LIKE</c> match. Encrypted modules
    /// have a null definition and are not returned.
    /// <para>
    /// Each result line:<br/>
    /// <c>TypeDesc.SchemaName.ObjectName</c> (for example, <c>SQL_STORED_PROCEDURE.dbo.usp_GetOrders</c>).
    /// </para>
    /// </remarks>
    /// <param name="text">Substring to find in object definitions.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A object of <see cref="QueryResult"/> containing the result.
    /// </returns>
    Task<QueryResult> SearchObjectDefinitionsAsync(string text, CancellationToken cancellationToken);

    /// <summary>
    /// Lists objects that reference the given table or view by name.
    /// </summary>
    /// <remarks>
    /// Queries <c>sys.sql_expression_dependencies</c>. <paramref name="objectName"/> may be
    /// <c>schema.object</c> (for example, <c>dbo.Orders</c>) or a bare name (matches any schema).
    /// <para>
    /// Each result line:<br/>
    /// <c>ReferencingSchema.ReferencingObject</c> (for example, <c>dbo.usp_GetOrders</c>).
    /// </para>
    /// </remarks>
    /// <param name="objectName">Referenced table or view name.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A object of <see cref="QueryResult"/> containing the result.
    /// </returns>
    Task<QueryResult> GetObjectReferencesAsync(string objectName, CancellationToken cancellationToken);

    /// <summary>
    /// Executes an arbitrary read-only SQL query and returns the result.
    /// </summary>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A object of <see cref="QueryResult"/> containing the result.
    /// </returns>
    Task<QueryResult> ExecuteReadQueryAsync(string sql, CancellationToken cancellationToken = default);
}