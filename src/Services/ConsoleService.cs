using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sync2Oss.Models;
using Sync2Oss.Utils;
using System.CommandLine;
using System.Text.Json;

namespace Sync2Oss.Services;

static class Commands
{
    public static readonly Option<string> AccessKeyIdOption = new("--accessKeyId", description: "Aliyun AccessKeyId") { IsRequired = true };

    public static readonly Option<string> AccessKeySecretOption = new("--accessKeySecret", description: "Aliyun AccessKeySecret") { IsRequired = true };

    public static readonly Option<string> EndpointOption = new("--endpoint", description: "Aliyun Endpoint") { IsRequired = true };

    public static readonly Option<string> BucketNameOption = new("--bucketName", description: "Aliyun BucketName") { IsRequired = true };

    public static readonly Option<bool> FromReleaseOption = new("--fromRelease", description: "是否从 Release 上传, 默认 true ", getDefaultValue: () => true);

    public static readonly Option<string?> RepoUrlOption = new("--repoUrl", description: "GitHub 仓库地址, 可选");

    public static readonly Option<bool> IsPreOption = new("--isPre", description: "是否上传 PreRelease , 默认 false", getDefaultValue: () => false);

    public static readonly Option<int> KeepCountOption = new("--keepCount", description: "保留最新几个版本, 默认 2", getDefaultValue: () => 2);

    public static readonly Option<string?> RemoteDirOption = new("--remoteDir", description: "上传到远程目录, 可选");

    public static readonly Option<bool> AddSymlinkOption = new("--addSymlink", description: "是否添加软链接, 默认 false", getDefaultValue: () => false);

    public static readonly Option<string?> LocalPathOption = new("--localPath", description: "本地文件 / 文件夹路径 (fromRelease 为 false 时使用)");

    public static readonly Option<string?> OssRegionOption = new("--region", description: "阿里云 OSS 区域");
}

internal class ConsoleService(
    string[] args,
    Dictionary<string, string> aliyunOssArgsDictionary,
    IServiceProvider serviceProvider,
    ILogger<ConsoleService> logger)
{
    public async Task ExecuteAsync()
    {
        RootCommand rootCommand = new("Sync2Oss")
        {
            Commands.AccessKeyIdOption,
            Commands.AccessKeySecretOption,
            Commands.EndpointOption,
            Commands.BucketNameOption,
            Commands.FromReleaseOption,
            Commands.RepoUrlOption,
            Commands.IsPreOption,
            Commands.KeepCountOption,
            Commands.RemoteDirOption,
            Commands.AddSymlinkOption,
            Commands.LocalPathOption,
            Commands.OssRegionOption
        };

        rootCommand.SetHandler(async context =>
        {
            var fromRelease = context.ParseResult.GetValueForOption(Commands.FromReleaseOption);
            var repoUrl = context.ParseResult.GetValueForOption(Commands.RepoUrlOption);
            var isPre = context.ParseResult.GetValueForOption(Commands.IsPreOption);
            var keepCount = context.ParseResult.GetValueForOption(Commands.KeepCountOption);
            var remoteDir = context.ParseResult.GetValueForOption(Commands.RemoteDirOption);
            var addSymlink = context.ParseResult.GetValueForOption(Commands.AddSymlinkOption);
            var localPath = context.ParseResult.GetValueForOption(Commands.LocalPathOption);

            aliyunOssArgsDictionary.Add("accessKeyId", context.ParseResult.GetValueForOption(Commands.AccessKeyIdOption)!);
            aliyunOssArgsDictionary.Add("accessKeySecret", context.ParseResult.GetValueForOption(Commands.AccessKeySecretOption)!);
            aliyunOssArgsDictionary.Add("endpoint", context.ParseResult.GetValueForOption(Commands.EndpointOption)!);
            aliyunOssArgsDictionary.Add("bucketName", context.ParseResult.GetValueForOption(Commands.BucketNameOption)!);
            aliyunOssArgsDictionary.Add("region", context.ParseResult.GetValueForOption(Commands.OssRegionOption)!);

            await InvokeHandle(fromRelease, repoUrl, isPre, keepCount, remoteDir, addSymlink, localPath);
        });

        await rootCommand.InvokeAsync(args);
    }

    async Task InvokeHandle(
        bool fromRelease = true,
        string? repoUrl = null,
        bool isPre = false,
        int keepCount = 2,
        string? remoteDir = null,
        bool addSymlink = false,
        string? localPath = null)
    {
        var aliyunOssService = serviceProvider.GetService<AliyunOssService>()!;
        logger.Initialized();

        List<(string,string,string)> ossFiles = [];

        if (fromRelease)
        {
            #region 获取 Release 列表

            logger.FetchingRepositoryReleases(repoUrl!);

            string repoApi = $"https://api.github.com/repos/{repoUrl!.Replace("https://github.com/", "")}/releases";
            string releaseJsonText = await HttpUtils.DefaultClient.GetStringAsync(repoApi).ContinueWith(task => 
            {
                if (task.IsFaulted)
                {
                    logger.FailedToFetchReleaseInfo(repoUrl);
                    throw task.Exception;
                }

                return task.Result;
            });

            var releases = JsonSerializer.Deserialize(releaseJsonText, SerializerContext.Default.ReleaseModelArray);

            if (releases == null || releases.Length == 0)
            {
                logger.NotReleaseFound();
                return;
            }

            #endregion

            #region 过滤 PreRelease

            IEnumerable<ReleaseModel> filtered = releases
                .Where(r => r.Prerelease == isPre)
                .OrderByDescending(r => r.PublishedAt)
                .Take(keepCount);

            foreach (var release in filtered)
            {
                foreach (var asset in release.Assets)
                {
                    FileInfo localFile = new(Path.Combine(Path.GetTempPath(), "Sync2Oss", release.TagName, asset.Name));
                    string remoteFilePath = remoteDir != null 
                        ? Path.Combine(remoteDir, release.TagName, asset.Name) 
                        : Path.Combine(release.TagName, asset.Name);

                    if (aliyunOssService.IsRemoteFileExist(remoteFilePath))
                    {
                        logger.RemoteExists(remoteFilePath);
                        continue;
                    }

                    await HttpUtils.DownloadFileAsync(asset.DownloadUrl, localFile);
                    ossFiles.Add((localFile.FullName, remoteFilePath, asset.DownloadUrl));
                }
            }

            #endregion
        }
        else
        {
            #region 本地文件 / 文件夹上传

            List<string> files = [];

            if (!string.IsNullOrEmpty(localPath))
            {
                if (File.Exists(localPath))
                    files.Add(localPath);

                if (Directory.Exists(localPath))
                    files.AddRange(Directory.GetFiles(localPath, "*", SearchOption.AllDirectories));
            }

            if (files.Count == 0)
            {
                logger.NoFilesFound();
                return;
            }

            foreach (var file in files)
            {
                string relative = localPath != null
                    ? Path.GetRelativePath(localPath, file)
                    : Path.GetFileName(file);

                string remoteFilePath = remoteDir != null 
                    ? Path.Combine(remoteDir, relative) 
                    : relative;

                if (aliyunOssService.IsRemoteFileExist(remoteFilePath))
                {
                    logger.RemoteExists(remoteFilePath);
                    continue;
                }

                ossFiles.Add((file, remoteFilePath, file));
            }

            #endregion
        }

        foreach ((string localFile, string remoteFilePath, string symlink) in ossFiles)
        {
            aliyunOssService.UploadFile(localFile, remoteFilePath.Replace('\\', '/'));

            if (addSymlink) 
                aliyunOssService.PutSymlink(remoteFilePath.Replace('\\', '/'), symlink.Replace('\\', '/'));
        }

        logger.CleanTempFiles();
        Directory.Delete(Path.Combine(Path.GetTempPath(), "Sync2Oss"), true);
    }
}

internal static partial class ConsoleServiceLoggers
{
    [LoggerMessage(LogLevel.Information, "Sync2Oss Initialized")]
    public static partial void Initialized(this ILogger logger);

    [LoggerMessage(LogLevel.Information, "Fetching information of repository [{url}] releases")]
    public static partial void FetchingRepositoryReleases(this ILogger logger, string url);

    [LoggerMessage(LogLevel.Error, "Failed to information of repository [{url}] releases")]
    public static partial void FailedToFetchReleaseInfo(this ILogger logger, string url);

    [LoggerMessage(LogLevel.Warning, "No release found, Skipped")]
    public static partial void NotReleaseFound(this ILogger logger);

    [LoggerMessage(LogLevel.Warning, "Remote file {remoteFilePath} already exists, skip upload")]
    public static partial void RemoteExists(this ILogger logger, string remoteFilePath);

    [LoggerMessage(LogLevel.Error, "No files found in the specified local path")]
    public static partial void NoFilesFound(this ILogger logger);

    [LoggerMessage(LogLevel.Information, "Cleaning temporary files")]
    public static partial void CleanTempFiles(this ILogger logger);
}
