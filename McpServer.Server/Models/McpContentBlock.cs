using System.Text.Json.Serialization;

namespace McpServer.Server.Models;

/// <summary>
/// A single MCP content block returned inside a tools/call result.
/// </summary>
public sealed class McpContentBlock
{
    /// <summary>
    /// Gets or sets the content type (this server uses <c>text</c>).
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; init; } = "text";

    /// <summary>
    /// Gets or sets the text payload shown to the MCP host / agent.
    /// </summary>
    [JsonPropertyName("text")]
    public string Text { get; init; } = string.Empty;
}
