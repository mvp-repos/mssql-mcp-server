using SqlMcpServer.Server.Models;
using System.Text.Json;

namespace SqlMcpServer.Test.Helpers
{
    /// <summary>
    /// Provides helper methods and utilities for unit tests.
    /// </summary>
    internal static class McpTestHelper
    {
        /// <summary>
        /// Creates a JsonRpcRequest object with the specified id, method, and optional paramsJson.
        /// </summary>
        /// <param name="id">The id of the JSON-RPC request.</param>
        /// <param name="method">The method name of the JSON-RPC request.</param>
        /// <param name="paramsJson">The optional JSON string representing the parameters of the JSON-RPC request.</param>
        /// <returns>
        /// A <see cref="JsonRpcRequest"/> object constructed from the provided parameters.
        /// </returns>
        public static JsonRpcRequest Request(int id, string method, string? paramsJson = null)
        {
            var json = paramsJson is null
                ? $$"""{"jsonrpc":"2.0","id":{{id}},"method":"{{method}}"}"""
                : $$"""{"jsonrpc":"2.0","id":{{id}},"method":"{{method}}","params":{{paramsJson}}}""";

            return JsonSerializer.Deserialize<JsonRpcRequest>(json)!;
        }

        /// <summary>
        /// Creates a JsonRpcRequest object for calling a tool with the specified id, tool name, and optional arguments in JSON format.
        /// </summary>
        /// <param name="id">The id of the JSON-RPC request.</param>
        /// <param name="toolName">The name of the tool to call (for example, "describe_table").</param>
        /// <param name="argumentsJson">The optional JSON string representing the arguments to pass to the tool. If not provided, it defaults to an empty JSON object ("{}").</param>
        /// <returns>
        /// A <see cref="JsonRpcRequest"/> object constructed for calling the specified tool with the provided arguments.
        /// </returns>
        public static JsonRpcRequest ToolCall(int id, string toolName, string? argumentsJson = "{}")
            => Request(id, "tools/call", $$"""{"name":"{{toolName}}","arguments":{{argumentsJson}}}""");

        /// <summary>
        /// Extracts the text content from the result of a JSON-RPC response that is expected to contain a tool call result.
        /// </summary>
        /// <param name="response">The <see cref="JsonRpcResponse"/> object from which to extract the tool text.</param>
        /// <returns>
        /// The text content extracted from the tool call result in the JSON-RPC response; otherwise, <see langword="null"/>.
        /// </returns>
        public static string? GetToolText(JsonRpcResponse response)
        {
            var queryResult = (QueryResult?)response.Result;
            return queryResult?.Text ?? null;
        }

        /// <summary>
        /// Asserts that the actualId (which may be a long, int, or null) matches the 
        /// expectedId (an int), by converting expectedId to long for comparison.
        /// </summary>
        /// <param name="actualId">The actual id value from the <see cref="JsonRpcRequest"/>, which may be a long, int, or null.</param>
        /// <param name="expectedId">The expected id value as an int, which will be converted to long for comparison.</param>
        public static void AssertId(object? actualId, int expectedId) => Assert.AreEqual((double)expectedId, actualId);

        /// <summary>
        /// Asserts that the given <see cref="JsonRpcResponse"/> contains an error with the specified code and a 
        /// message that contains the specified substring.
        /// </summary>
        /// <param name="response">The <see cref="JsonRpcResponse"/> object to check for the expected error code and message content.</param>
        /// <param name="code">The expected error code that should be present in the error object of the JSON-RPC response.</param>
        /// <param name="messageContains">The substring that should be contained in the error message of the JSON-RPC response.</param>
        public static void AssertError(JsonRpcResponse response, ErrorCodes code, string messageContains)
        {
            Assert.AreEqual((int)code, response.Error.Code);
            StringAssert.Contains(response.Error.Message, messageContains);
        }

        /// <summary>
        /// Gets the value of a specified column from a row represented as a dictionary. If the column does not exist, returns null.
        /// </summary>
        /// <param name="row">The row represented as a dictionary.</param>
        /// <param name="columnName">The name of the column to retrieve the value for.</param>
        /// <returns>
        /// The value of the specified column, or null if the column does not exist.
        /// </returns>
        public static object? GetValue(this Dictionary<string, object?> row, string columnName)
        {
            return row.TryGetValue(columnName, out var value)
                ? value
                : null;
        }
    }
}
