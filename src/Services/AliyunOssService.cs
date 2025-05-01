using Aliyun.OSS;
using Microsoft.Extensions.Logging;

namespace Sync2Oss.Services;

public class AliyunOssService(
    Dictionary<string, string> apikeys, 
    OssClient ossClient,
    ILogger<AliyunOssService> logger)
{
    //private readonly string _accessKeyId = apikeys["accessKeyId"];
    //private readonly string _accessKeySecret = apikeys["accessKeySecret"];
    //private readonly string _endpoint = apikeys["endpoint"];

    private string BucketName => apikeys["bucketName"];

    private string Region => apikeys["region"];

    public void PutSymlink(string destinationObjectName, string symLink)
    {
        //symLink = Path.Combine("https://source.cubestructor.cc", symLink);

        try
        {
            ossClient.SetRegion(Region);
            ossClient.CreateSymlink(BucketName, symLink, destinationObjectName);

            logger.PutSymlinkSucceed(symLink, destinationObjectName);
        }
        catch (Exception ex)
        {
            logger.PutSymlinkFailed(ex, symLink, destinationObjectName);
            throw;
        }
    }

    public void UploadFile(string localFilePath, string remoteFilePath)
    {
        try
        {
            ossClient.SetRegion(Region);

            if (IsRemoteFileExist(remoteFilePath))
            {
                ossClient.DeleteObject(BucketName, remoteFilePath);
                logger.DeletedExistingFile(remoteFilePath);
            }

            ossClient.PutObject(BucketName, remoteFilePath, localFilePath);
            logger.UploadSucceed(localFilePath, remoteFilePath);
        }
        catch (Exception ex)
        {
            logger.UploadFailed(ex, localFilePath, remoteFilePath);
            throw;
        }
    }

    public bool IsRemoteFileExist(string remoteFilePath)
    {
        try
        {
            ossClient.SetRegion(Region);
            return ossClient.DoesObjectExist(BucketName, remoteFilePath);
        }
        catch (Exception ex)
        {
            logger.CheckExistFailed(ex, remoteFilePath);
            return false;
        }
    }
}


public static partial class AliyunOssServiceLoggers
{
    [LoggerMessage(LogLevel.Information, "Put symlink {symLink} to {destinationObjectName} successfully")]
    public static partial void PutSymlinkSucceed(this ILogger logger, string symLink, string destinationObjectName);

    [LoggerMessage(LogLevel.Error, "Put symlink {symLink} to {destinationObjectName} failed")]
    public static partial void PutSymlinkFailed(this ILogger logger, Exception ex, string symLink, string destinationObjectName);

    [LoggerMessage(LogLevel.Error, "Check object {remoteFilePath} exist failed")]
    public static partial void CheckExistFailed(this ILogger logger, Exception ex, string remoteFilePath);

    [LoggerMessage(LogLevel.Information, "Existing file {remoteFilePath} deleted")]
    public static partial void DeletedExistingFile(this ILogger logger, string remoteFilePath);

    [LoggerMessage(LogLevel.Information, "Upload file {localFilePath} to {remoteFilePath} successfully")]
    public static partial void UploadSucceed(this ILogger logger, string localFilePath, string remoteFilePath);

    [LoggerMessage(LogLevel.Error, "Upload file {localFilePath} to {remoteFilePath} failed")]
    public static partial void UploadFailed(this ILogger logger, Exception ex, string localFilePath, string remoteFilePath);
}