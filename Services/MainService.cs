using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Nodes;
using Sync2Oss.Models;

namespace Sync2Oss.Services;

public class MainService()
{
    public readonly AliyunOssService _aliyunOssService;

    public MainService(AliyunOssService aliyunOssService)
    {
        _aliyunOssService = aliyunOssService;
    }

    public async Task InvokeAsync()
    {
        string currentDirectory = Directory.GetCurrentDirectory();
        string releaseJsonText = await HttpUtils.DefaultClient.GetStringAsync("https://api.github.com/repos/Xcube-Studio/Natsurainko.FluentLauncher/releases");

        if (string.IsNullOrEmpty(releaseInfo))
        {
            Console.WriteLine("[ERROR] Failed to get release information.");
            return;
        }

        var releases = JsonNode.Parse(releaseJsonText)!.Deserialize<List<ReleaseModel>>();
        Console.WriteLine($"[INFO] {releases.Count} releases found.");

        foreach (var release in releases.Take(2))
        {
            Console.WriteLine($"[INFO] Release: {release.TagName}");

            foreach (var assest in release.Assets)
            {
                Console.WriteLine($"[INFO] Asset: {assest.Name}");
                Console.WriteLine($"[INFO] Download URL: {assest.DownloadUrl}");

                string localFilePath = Path.Combine(currentDirectory, release.TagName, assest.Name);
                string remoteFilePath = Path.Combine(release.TagName, assest.Name);

                if (await _aliyunOssService.IsRemoteFileExistAsync(remoteFilePath)) 
                    break;

                await HttpUtils.DownloadFileAsync(assest.DownloadUrl, localFilePath);
                await _aliyunOssService.UploadFileAsync(localFilePath, remoteFilePath);
            }
        }
    }
}