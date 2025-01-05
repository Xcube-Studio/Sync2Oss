using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Aliyun.OSS;
using Aliyun.OSS.Common;
using Sync2Oss.Services;

namespace Sync2Oss;

internal class Program
{
    public static string[] Arguments { get; set; } = null!;

    public static IServiceProvider Services { get; private set; } = null!;

    public static void BuildServices()
    {
        string accessKeyId = Arguments[0];
        string accessKeySecret = Arguments[1];
        string endpoint = Arguments[2];
        string bucketName = Arguments[3];

        ServiceCollection serviceDescriptors = new();

        serviceDescriptors.AddSingleton<OssClient>(_ => new OssClient(endpoint, accessKeyId, accessKeySecret, 
            new ClientConfiguration() { SignatureVersion = SignatureVersion.V4 }));
        serviceDescriptors.AddSingleton<Dictionary<string, string>>(_ => new()
        {
            { "accessKeyId", accessKeyId },
            { "accessKeySecret", accessKeySecret },
            { "endpoint", endpoint },
            { "bucketName", bucketName },
        });
        serviceDescriptors.AddSingleton<AliyunOssService>();
        serviceDescriptors.AddSingleton<MainService>();

        Services = serviceDescriptors.BuildServiceProvider();
    }

    static async Task Main(string[] args)
    {
        Arguments = args;
        Console.WriteLine("Hello, World! Sync2Oss");

        BuildServices();

        await Services.GetService<MainService>()!.InvokeAsync();
    }
}
