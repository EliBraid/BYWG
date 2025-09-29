namespace BYWG.Server.Models;

public sealed class DeviceDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "offline"; // online/offline
    public string? Ip { get; set; }
    public string? Protocol { get; set; }
    public string? ConnectionString { get; set; }
}

public sealed class DeviceCreateRequest
{
    public string? Name { get; set; }
    public string? Status { get; set; }
    public string? Ip { get; set; }
    public string? Protocol { get; set; }
    public string? ConnectionString { get; set; }
}

public sealed class TestConnectionRequest
{
    public string? Ip { get; set; }
    public int? Port { get; set; }
    public string? Protocol { get; set; }
    public string? ConnectionString { get; set; }
}

public sealed class TestConnectionResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public long LatencyMs { get; set; }
}

public sealed class NodeDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Config { get; set; } = string.Empty;
}

public sealed class NodeConfigUpdateRequest
{
    public string? Config { get; set; }
}


