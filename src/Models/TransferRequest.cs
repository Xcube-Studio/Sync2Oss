namespace Sync2Oss.Models;

public sealed class TransferRequest
{
    public required string LocalPath { get; init; }

    public required string ObjectKey { get; init; }

    public int ExpiresInSeconds { get; init; } = 3600;

    public bool Overwrite { get; init; } = true;
}
