namespace RevitMcp.Core.Server;

public sealed record HttpRequestContext(
    string? Origin = null,
    string? Authorization = null,
    string? ClientName = null);
