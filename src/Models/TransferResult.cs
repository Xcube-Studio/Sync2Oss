namespace Sync2Oss.Models;

public sealed class TransferResult
{
    public required string ObjectKey { get; init; }

    public required string OssUri { get; init; }

    public required string SignedUrl { get; init; }

    public required DateTimeOffset ExpiresAtUtc { get; init; }
}
