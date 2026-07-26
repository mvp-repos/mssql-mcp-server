using System.Text.Json.Serialization;

namespace SqlMcpServer.Server.Models;

/// <summary>
/// MCP <c>tools/call</c> result payload (<c>content</c> + <c>isError</c>).
/// </summary>
public sealed class McpCallToolResult
{
    /// <summary>
    /// Gets the MCP content blocks.
    /// </summary>
    [JsonPropertyName("content")]
    public IReadOnlyList<McpContentBlock> Content { get; init; } = [];

    /// <summary>
    /// Gets a value indicating whether the tool execution failed.
    /// </summary>
    [JsonPropertyName("isError")]
    public bool IsError                           { get; init; }

    /// <summary>
    /// Creates a successful tool result with a single text content block.
    /// </summary>
    /// <param name="text">The text to return to the host.</param>
    /// <returns>
    /// An <see cref="McpCallToolResult"/> with <c>isError</c> false.
    /// </returns>
    public static McpCallToolResult Ok(string text) => new()
    {
        IsError = false,
        Content =
        [
            new McpContentBlock
            {
                Type = "text",
                Text = text
            }
        ]
    };

    /// <summary>
    /// Creates a tool-execution error result (JSON-RPC success with <c>isError: true</c>).
    /// </summary>
    /// <param name="message">The error message for the host / model.</param>
    /// <returns>
    /// An <see cref="McpCallToolResult"/> with <c>isError</c> true.
    /// </returns>
    public static McpCallToolResult Error(string message) => new()
    {
        IsError = true,
        Content =
        [
            new McpContentBlock
            {
                Type = "text",
                Text = message
            }
        ]
    };
}
