using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Nodes;
using Sync2Oss.Models;

namespace Sync2Oss.Services;

public class MainService
{
    public readonly AliyunOssService _aliyunOssService;

    public MainService(AliyunOssService aliyunOssService)
    {
        _aliyunOssService = aliyunOssService;
    }

    public async Task InvokeAsync()
    {
        string currentDirectory = Directory.GetCurrentDirectory();

        //处理Fluent Launcher主仓库同步
        string releaseJsonText = await HttpUtils.DefaultClient.GetStringAsync("https://api.github.com/repos/Xcube-Studio/Natsurainko.FluentLauncher/releases");

        if (string.IsNullOrEmpty(releaseJsonText))
        {
            Console.WriteLine("[ERROR] Failed to get release information.");
            return;
        }

        var releases = JsonSerializer.Deserialize(releaseJsonText, SerializerContext.Default.ReleaseModelArray);
        Console.WriteLine($"[INFO] {releases.Count()} releases found in Natsurainko.FluentLauncher.");

        foreach (var release in releases.Take(2))
        {
            Console.WriteLine($"[INFO] Release: {release.TagName}");

            foreach (var assest in release.Assets)
            {
                Console.WriteLine($"[INFO] Asset: {assest.Name}");


                string localFilePath = Path.Combine(currentDirectory, release.TagName, assest.Name);
                string remoteFilePath = Path.Combine(release.TagName, assest.Name);

                if (await _aliyunOssService.IsRemoteFileExistAsync(remoteFilePath))
                {
                    Console.WriteLine($"[WRAN] Remote file {remoteFilePath} already exists. Skipped {release.TagName}.");
                    break;
                }

                await HttpUtils.DownloadFileAsync(assest.DownloadUrl, localFilePath);
                Console.WriteLine($"[INFO] Download URL: {assest.DownloadUrl}");

                await _aliyunOssService.UploadFileAsync(localFilePath, remoteFilePath);
               
                _aliyunOssService.PutSymlink(remoteFilePath, assest.DownloadUrl);
            }
        }
        //处理Fluent Launcher安装器同步
        string installerJsonText = await HttpUtils.DefaultClient.GetStringAsync("https://api.github.com/repos/Xcube-Studio/FluentLauncher.PreviewChannel.PackageInstaller/releases");
        var installerReleases = JsonSerializer.Deserialize(installerJsonText, SerializerContext.Default.ReleaseModelArray);
        Console.WriteLine($"[INFO] {installerReleases.Count()} releases found in FluentLauncher.PreviewChannel.PackageInstaller.");

        if (installerReleases == null || !installerReleases.Any())
        {
            Console.WriteLine("[ERROR] Failed to get installer release information.");
            return;
        }

        ReleaseModel latestReleaseInstaller = installerReleases.First();
        Console.WriteLine($"[INFO] Release: {latestReleaseInstaller.TagName}");

        foreach (var assest in latestReleaseInstaller.Assets)
        {
            Console.WriteLine($"[INFO] Asset: {assest.Name}");


            string localFilePath = Path.Combine(currentDirectory, latestReleaseInstaller.TagName, assest.Name);
            string remoteFilePath = Path.Combine(latestReleaseInstaller.TagName, assest.Name);

            if (await _aliyunOssService.IsRemoteFileExistAsync(remoteFilePath))
            {
                Console.WriteLine($"[WRAN] Remote file {remoteFilePath} already exists. Skipped {latestReleaseInstaller.TagName}.");
                break;
            }

            await HttpUtils.DownloadFileAsync(assest.DownloadUrl, localFilePath);
            Console.WriteLine($"[INFO] Download URL: {assest.DownloadUrl}");

            await _aliyunOssService.UploadFileAsync(localFilePath, remoteFilePath);

            _aliyunOssService.PutSymlink(remoteFilePath, assest.DownloadUrl);
        }
    }
}
