using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Nodes;
using Sync2Oss.Models;

namespace Sync2Oss.Services;

public class MainService
{
    public readonly AliyunOssService _aliyunOssService;
    private readonly bool _fromRelease;
    private readonly string? _repoUrl;
    private readonly bool _isPre;
    private readonly int _keepCount;
    private readonly string? _remoteDir;
    private readonly bool _addSymlink;
    private readonly string? _localPath;

    public MainService(
        AliyunOssService aliyunOssService,
        bool fromRelease = true,
        string? repoUrl = null,
        bool isPre = false,
        int keepCount = 2,
        string? remoteDir = null,
        bool addSymlink = false,
        string? localPath = null)
    {
        _aliyunOssService = aliyunOssService;
        _fromRelease = fromRelease;
        _repoUrl = repoUrl;
        _isPre = isPre;
        _keepCount = keepCount;
        _remoteDir = remoteDir;
        _addSymlink = addSymlink;
        _localPath = localPath;
    }

    public async Task InvokeAsync()
    {
        if (_fromRelease)
        {
            // 1. 获取 Release 列表
            string repoApi = $"https://api.github.com/repos/{_repoUrl.Replace("https://github.com/", "")}/releases";
            string releaseJsonText = await HttpUtils.DefaultClient.GetStringAsync(repoApi);
            if (string.IsNullOrEmpty(releaseJsonText))
            {
                Console.WriteLine("[ERROR] Failed to get release information.");
                return;
            }
            var releases = JsonSerializer.Deserialize(releaseJsonText, SerializerContext.Default.ReleaseModelArray);
            if (releases == null || !releases.Any())
            {
                Console.WriteLine("[ERROR] No releases found.");
                return;
            }
            // 2. 过滤 PreRelease
            var filtered = _isPre ? releases.Where(r => r.Prerelease) : releases.Where(r => !r.Prerelease);
            filtered = filtered.OrderByDescending(r => r.PublishedAt).Take(_keepCount);
            foreach (var release in filtered)
            {
                foreach (var asset in release.Assets)
                {
                    string localFilePath = Path.Combine(Path.GetTempPath(), release.TagName, asset.Name);
                    string remoteFilePath = _remoteDir != null ? Path.Combine(_remoteDir, release.TagName, asset.Name) : Path.Combine(release.TagName, asset.Name);
                    remoteFilePath = remoteFilePath.Replace('\\', '/');
                    Directory.CreateDirectory(Path.GetDirectoryName(localFilePath)!);
                    await HttpUtils.DownloadFileAsync(asset.DownloadUrl, localFilePath);
                    if (await _aliyunOssService.IsRemoteFileExistAsync(remoteFilePath))
                    {
                        Console.WriteLine($"[INFO] Remote file {remoteFilePath} already exists, skip upload.");
                        continue;
                    }
                    await _aliyunOssService.UploadFileAsync(localFilePath, remoteFilePath);
                    if (_addSymlink)
                        await _aliyunOssService.PutSymlink(remoteFilePath, asset.DownloadUrl);
                    
                }
            }
        }
        else
        {
            // 本地文件/文件夹上传
            if (string.IsNullOrEmpty(_localPath) || !File.Exists(_localPath) && !Directory.Exists(_localPath))
            {
                Console.WriteLine("[ERROR] localPath 未指定或不存在");
                return;
            }
            List<string> files = new();
            if (File.Exists(_localPath))
                files.Add(_localPath);
            else
                files.AddRange(Directory.GetFiles(_localPath, "*", SearchOption.AllDirectories));
            foreach (var file in files)
            {
                string relative = File.Exists(_localPath) ? Path.GetFileName(file) : Path.GetRelativePath(_localPath, file);
                string remoteFilePath = _remoteDir != null ? Path.Combine(_remoteDir, relative) : relative;
                if (await _aliyunOssService.IsRemoteFileExistAsync(remoteFilePath))
                {
                    Console.WriteLine($"[INFO] Remote file {remoteFilePath} already exists, skip upload.");
                    continue;
                }
                await _aliyunOssService.UploadFileAsync(file, remoteFilePath);
                if (_addSymlink)
                    await _aliyunOssService.PutSymlink(remoteFilePath, file);
            }
        }
    }
}
