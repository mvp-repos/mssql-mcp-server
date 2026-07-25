using System.Text.Json.Serialization;

namespace SqlMcpServer.Server.Models
{
    /// <summary>
    /// Represents a JSON-RPC error object.
    /// </summary>
    public sealed class JsonRpcError
    {
        [JsonPropertyName("code")]
        public int Code { get; init; }

        [JsonPropertyName("message")]
        public string Message { get; init; } = string.Empty;

        [JsonPropertyName("data")]
        public object? Data { get; init; }
    }
}
