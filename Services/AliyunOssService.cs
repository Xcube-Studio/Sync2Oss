﻿using Aliyun.OSS;
using Aliyun.OSS.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sync2Oss.Services;

public class AliyunOssService
{
    private readonly string _accessKeyId;
    private readonly string _accessKeySecret;
    private readonly string _endpoint;
    private readonly string _bucketName;

    private readonly OssClient _ossClient;

    public const string Region = "cn-shanghai";

    public AliyunOssService(Dictionary<string, string> apikeys, OssClient ossClient)
    {
        _accessKeyId = apikeys["accessKeyId"];
        _accessKeySecret = apikeys["accessKeySecret"];
        _endpoint = apikeys["endpoint"];
        _bucketName = apikeys["bucketName"];

        _ossClient = ossClient;
    }

    public async Task UploadFileAsync(string localFilePath, string remoteFilePath)
    {
        try
        {
            _ossClient.SetRegion(region);

            if (await IsRemoteFileExistAsync(remoteFilePath))
            {
                _ossClient.DeleteObject(_bucketName, remoteFilePath);
                Console.WriteLine($"[INFO] Existing file {remoteFilePath} deleted.");
            }

            _ossClient.PutObject(_bucketName, remoteFilePath, localFilePath)
            Console.WriteLine($"[INFO] Upload file {localFilePath} to {remoteFilePath} successfully.");
        }
        catch (Exception e)
        {
            Console.WriteLine($"[ERROR] Upload file {localFilePath} to {remoteFilePath} failed: {e.Message}");
        }
    }

    public async Task<bool> IsRemoteFileExistAsync(string remoteFilePath)
    {
        try
        {
            _ossClient.SetRegion(region);
            return _ossClient.DoesObjectExist(_bucketName, objectName);
        }
        catch (Exception e)
        {
            Console.WriteLine($"[ERROR] Check object {objectName} exist failed: {e.Message}");
            return false;
        }
    }
}