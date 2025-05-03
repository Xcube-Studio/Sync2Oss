using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sync2Oss.Models;
using Sync2Oss.Utils;
using System.CommandLine;

namespace Sync2Oss.Services;

static class Commands
{
    public static readonly Option<string> AccessKeyIdOption = new("--accessKeyId", description: "Aliyun AccessKeyId") { IsRequired = true };

    public static readonly Option<string> AccessKeySecretOption = new("--accessKeySecret", description: "Aliyun AccessKeySecret") { IsRequired = true };

    public static readonly Option<string> EndpointOption = new("--endpoint", description: "Aliyun Endpoint") { IsRequired = true };

    public static readonly Option<string> BucketNameOption = new("--bucketName", description: "Aliyun BucketName") { IsRequired = true };

    public static readonly Option<string> LocalPathOption = new("--localPath", description: "Local file path to upload") { IsRequired = true };

    public static readonly Option<string?> ObjectKeyOption = new("--objectKey", description: "Target OSS object key; defaults to the local file name");

    public static readonly Option<int> ExpiresInSecondsOption = new("--expiresInSeconds", description: "Signed download URL validity in seconds", getDefaultValue: () => 3600);

    public static readonly Option<bool> OverwriteOption = new("--overwrite", description: "Overwrite the remote object if it already exists", getDefaultValue: () => true);

    public static readonly Option<string> OssRegionOption = new("--region", description: "Aliyun OSS region", getDefaultValue: () => "cn-shanghai");
}

internal class ConsoleService(
    string[] args,
    Dictionary<string, string> aliyunOssArgsDictionary,
    IServiceProvider serviceProvider,
    ILogger<ConsoleService> logger)
{
    public async Task ExecuteAsync()
    {
        RootCommand rootCommand = new("Vfiletransfer")
        {
            Commands.AccessKeyIdOption,
            Commands.AccessKeySecretOption,
            Commands.EndpointOption,
            Commands.BucketNameOption,
            Commands.LocalPathOption,
            Commands.ObjectKeyOption,
            Commands.ExpiresInSecondsOption,
            Commands.OverwriteOption,
            Commands.OssRegionOption
        };

        rootCommand.SetHandler(async context =>
        {
            var localPath = context.ParseResult.GetValueForOption(Commands.LocalPathOption)!;
            var objectKey = context.ParseResult.GetValueForOption(Commands.ObjectKeyOption);
            var expiresInSeconds = context.ParseResult.GetValueForOption(Commands.ExpiresInSecondsOption);
            var overwrite = context.ParseResult.GetValueForOption(Commands.OverwriteOption);
            var region = context.ParseResult.GetValueForOption(Commands.OssRegionOption)!;

            aliyunOssArgsDictionary.Add("accessKeyId", context.ParseResult.GetValueForOption(Commands.AccessKeyIdOption)!);
            aliyunOssArgsDictionary.Add("accessKeySecret", context.ParseResult.GetValueForOption(Commands.AccessKeySecretOption)!);
            aliyunOssArgsDictionary.Add("endpoint", NormalizeEndpoint(context.ParseResult.GetValueForOption(Commands.EndpointOption)!));
            aliyunOssArgsDictionary.Add("bucketName", context.ParseResult.GetValueForOption(Commands.BucketNameOption)!);
            aliyunOssArgsDictionary.Add("region", region);

            await InvokeHandle(localPath, objectKey, expiresInSeconds, overwrite);
        });

        await rootCommand.InvokeAsync(args);
    }

    private async Task InvokeHandle(
        string localPath,
        string? objectKey,
        int expiresInSeconds,
        bool overwrite)
    {
        var aliyunOssService = serviceProvider.GetRequiredService<AliyunOssService>();
        logger.Initialized();

        ValidateInputs(localPath, expiresInSeconds);

        var normalizedObjectKey = NormalizeObjectKey(objectKey, localPath);
        var result = aliyunOssService.UploadAndCreateSignedUrl(new TransferRequest
        {
            LocalPath = Path.GetFullPath(localPath),
            ObjectKey = normalizedObjectKey,
            ExpiresInSeconds = expiresInSeconds,
            Overwrite = overwrite
        });

        GitHubActionsOutputWriter.WriteOutput("downloadUrl", result.SignedUrl);
        GitHubActionsOutputWriter.WriteOutput("signedUrl", result.SignedUrl);
        GitHubActionsOutputWriter.WriteOutput("objectKey", result.ObjectKey);
        GitHubActionsOutputWriter.WriteOutput("ossUri", result.OssUri);

        logger.TransferCompleted(result.ObjectKey, result.ExpiresAtUtc);
        await Task.CompletedTask;
    }

    private static void ValidateInputs(string localPath, int expiresInSeconds)
    {
        if (!File.Exists(localPath))
        {
            if (Directory.Exists(localPath))
            {
                throw new ArgumentException("Vfiletransfer only supports a single file. Please pass a file path in --localPath.");
            }

            throw new FileNotFoundException("The local file specified by --localPath does not exist.", localPath);
        }

        if (expiresInSeconds <= 0 || expiresInSeconds > 604800)
        {
            throw new ArgumentOutOfRangeException(nameof(expiresInSeconds), expiresInSeconds, "expiresInSeconds must be between 1 and 604800 seconds.");
        }
    }

    private static string NormalizeObjectKey(string? objectKey, string localPath)
    {
        var resolvedObjectKey = string.IsNullOrWhiteSpace(objectKey)
            ? Path.GetFileName(localPath)
            : objectKey.Trim();

        var normalizedObjectKey = resolvedObjectKey
            .Replace('\\', '/')
            .TrimStart('/');

        if (string.IsNullOrWhiteSpace(normalizedObjectKey))
        {
            throw new ArgumentException("objectKey cannot be empty after normalization.");
        }

        return normalizedObjectKey;
    }

    private static string NormalizeEndpoint(string endpoint)
    {
        var trimmed = endpoint.Trim().TrimEnd('/');

        return trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
               trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            ? trimmed
            : $"https://{trimmed}";
    }
}

internal static partial class ConsoleServiceLoggers
{
    [LoggerMessage(LogLevel.Information, "Vfiletransfer initialized")]
    public static partial void Initialized(this ILogger logger);

    [LoggerMessage(LogLevel.Information, "Vfiletransfer completed for {objectKey}; signed URL expires at {expiresAtUtc}")]
    public static partial void TransferCompleted(this ILogger logger, string objectKey, DateTimeOffset expiresAtUtc);
}
