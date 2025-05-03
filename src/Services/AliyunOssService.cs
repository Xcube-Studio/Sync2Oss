using Aliyun.OSS;
using Microsoft.Extensions.Logging;
using Sync2Oss.Models;

namespace Sync2Oss.Services;

public class AliyunOssService(
    Dictionary<string, string> apikeys,
    OssClient ossClient,
    ILogger<AliyunOssService> logger)
{
    private string BucketName => apikeys["bucketName"];

    private string Region => apikeys["region"];

    public TransferResult UploadAndCreateSignedUrl(TransferRequest request)
    {
        try
        {
            ossClient.SetRegion(Region);

            if (!request.Overwrite && IsRemoteFileExist(request.ObjectKey))
            {
                throw new InvalidOperationException($"Remote object already exists: {request.ObjectKey}");
            }

            ossClient.PutObject(BucketName, request.ObjectKey, request.LocalPath);
            logger.UploadSucceeded(request.LocalPath, request.ObjectKey);

            var expiresAtUtc = DateTimeOffset.UtcNow.AddSeconds(request.ExpiresInSeconds);
            var signedUriRequest = new GeneratePresignedUriRequest(BucketName, request.ObjectKey, SignHttpMethod.Get)
            {
                Expiration = expiresAtUtc.UtcDateTime
            };

            var signedUri = ossClient.GeneratePresignedUri(signedUriRequest);
            var signedUrl = signedUri.ToString().Replace("+", "%2B", StringComparison.Ordinal);

            logger.GeneratedSignedUrl(request.ObjectKey, expiresAtUtc);

            return new TransferResult
            {
                ObjectKey = request.ObjectKey,
                OssUri = $"oss://{BucketName}/{request.ObjectKey}",
                SignedUrl = signedUrl,
                ExpiresAtUtc = expiresAtUtc
            };
        }
        catch (Exception ex)
        {
            logger.UploadFailed(ex, request.LocalPath, request.ObjectKey);
            throw;
        }
    }

    public bool IsRemoteFileExist(string objectKey)
    {
        try
        {
            ossClient.SetRegion(Region);
            return ossClient.DoesObjectExist(BucketName, objectKey);
        }
        catch (Exception ex)
        {
            logger.CheckExistFailed(ex, objectKey);
            return false;
        }
    }
}

public static partial class AliyunOssServiceLoggers
{
    [LoggerMessage(LogLevel.Error, "Check object {objectKey} existence failed")]
    public static partial void CheckExistFailed(this ILogger logger, Exception ex, string objectKey);

    [LoggerMessage(LogLevel.Information, "Uploaded file {localFilePath} to {objectKey} successfully")]
    public static partial void UploadSucceeded(this ILogger logger, string localFilePath, string objectKey);

    [LoggerMessage(LogLevel.Error, "Failed to upload file {localFilePath} to {objectKey}")]
    public static partial void UploadFailed(this ILogger logger, Exception ex, string localFilePath, string objectKey);

    [LoggerMessage(LogLevel.Information, "Generated signed download URL for {objectKey}, expires at {expiresAtUtc}")]
    public static partial void GeneratedSignedUrl(this ILogger logger, string objectKey, DateTimeOffset expiresAtUtc);
}
