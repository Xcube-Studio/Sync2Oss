using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Aliyun.OSS;
using Aliyun.OSS.Common;
using Sync2Oss.Services;
using System.CommandLine;

namespace Sync2Oss;

internal class Program
{
    public static IServiceProvider Services { get; private set; } = null!;

    public static void BuildServices(string accessKeyId, string accessKeySecret, string endpoint, string bucketName,string ossRegion)
    {
        ServiceCollection serviceDescriptors = new();

        serviceDescriptors.AddSingleton<OssClient>(_ => new OssClient(endpoint, accessKeyId, accessKeySecret, 
            new ClientConfiguration() { SignatureVersion = SignatureVersion.V4}));
        serviceDescriptors.AddSingleton<Dictionary<string, string>>(_ => new()
        {
            { "accessKeyId", accessKeyId },
            { "accessKeySecret", accessKeySecret },
            { "endpoint", endpoint },
            { "bucketName", bucketName },
            { "region", ossRegion }
        });
        serviceDescriptors.AddSingleton<AliyunOssService>();
        serviceDescriptors.AddSingleton<MainService>();

        Services = serviceDescriptors.BuildServiceProvider();
    }

    static async Task Main(string[] args)
    {
        var accessKeyIdOption = new Option<string>(
            "--accessKeyId",
            description: "Aliyun AccessKeyId"
        ) { IsRequired = true };
        var accessKeySecretOption = new Option<string>(
            "--accessKeySecret",
            description: "Aliyun AccessKeySecret"
        ) { IsRequired = true };
        var endpointOption = new Option<string>(
            "--endpoint",
            description: "Aliyun Endpoint"
        ) { IsRequired = true };
        var bucketNameOption = new Option<string>(
            "--bucketName",
            description: "Aliyun BucketName"
        ) { IsRequired = true };
        var fromReleaseOption = new Option<bool>(
            "--fromRelease",
            description: "是否从Release上传，默认true",
            getDefaultValue: () => true
        );
        var repoUrlOption = new Option<string?>(
            "--repoUrl",
            description: "GitHub仓库地址，可选"
        );
        var isPreOption = new Option<bool>(
            "--isPre",
            description: "是否上传PreRelease，默认false",
            getDefaultValue: () => false
        );
        var keepCountOption = new Option<int>(
            "--keepCount",
            description: "保留最新几个版本，默认2",
            getDefaultValue: () => 2
        );
        var remoteDirOption = new Option<string?>(
            "--remoteDir",
            description: "上传到远程目录，可选"
        );
        var addSymlinkOption = new Option<bool>(
            "--addSymlink",
            description: "是否添加软链接，默认false",
            getDefaultValue: () => false
        );
        var localPathOption = new Option<string?>(
            "--localPath",
            description: "本地文件/文件夹路径（fromRelease为false时使用）"
        );
        var ossRegionOption = new Option<string?>(
            "--region",
            description: "阿里云OSS区域（缺省值：cn-shanghai）"
        );

        var rootCommand = new RootCommand("Sync2Oss")
        {
            accessKeyIdOption,
            accessKeySecretOption,
            endpointOption,
            bucketNameOption
        };

        rootCommand.AddOption(fromReleaseOption);
        rootCommand.AddOption(repoUrlOption);
        rootCommand.AddOption(isPreOption);
        rootCommand.AddOption(keepCountOption);
        rootCommand.AddOption(remoteDirOption);
        rootCommand.AddOption(addSymlinkOption);
        rootCommand.AddOption(localPathOption);
        rootCommand.AddOption(ossRegionOption);

        rootCommand.SetHandler(async (context) =>
        {
            var accessKeyId = context.ParseResult.GetValueForOption(accessKeyIdOption);
            var accessKeySecret = context.ParseResult.GetValueForOption(accessKeySecretOption);
            var endpoint = context.ParseResult.GetValueForOption(endpointOption);
            var bucketName = context.ParseResult.GetValueForOption(bucketNameOption);
            var fromRelease = context.ParseResult.GetValueForOption(fromReleaseOption);
            var repoUrl = context.ParseResult.GetValueForOption(repoUrlOption);
            var isPre = context.ParseResult.GetValueForOption(isPreOption);
            var keepCount = context.ParseResult.GetValueForOption(keepCountOption);
            var remoteDir = context.ParseResult.GetValueForOption(remoteDirOption);
            var addSymlink = context.ParseResult.GetValueForOption(addSymlinkOption);
            var localPath = context.ParseResult.GetValueForOption(localPathOption);
            var ossRegion = context.ParseResult.GetValueForOption(ossRegionOption);
            
            Console.WriteLine("Hello,World! Sync2Oss");

            BuildServices(accessKeyId, accessKeySecret, endpoint, bucketName, ossRegion);
            var mainService = new MainService(
                Services.GetRequiredService<AliyunOssService>(),
                fromRelease, repoUrl, isPre, keepCount, remoteDir, addSymlink, localPath
            );
            await mainService.InvokeAsync();
        });

        await rootCommand.InvokeAsync(args);
    }
}
