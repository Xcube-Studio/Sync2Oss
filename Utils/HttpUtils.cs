using System.Net;
using System.IO;
using System;
using System.Buffers;

public static class HttpUtils
{
    public readonly static HttpClient DefaultClient = new();
    private const int DownloadBufferSize = 4096; // 4 KB

    static HttpUtils()
    {
        DefaultClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");
    }

    public static async Task DownloadFileAsync(string url, string path)
    {
        using var responseStream = await DefaultClient.GetStreamAsync(url);
        using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);

        byte[] downloadBufferArr = ArrayPool<byte>.Shared.Rent(DownloadBufferSize);
        Memory<byte> downloadBuffer = downloadBufferArr.AsMemory(0, DownloadBufferSize);

        int bytesRead = 0;
        while ((bytesRead = await responseStream.ReadAsync(downloadBuffer)) > 0)
            await fileStream.WriteAsync(downloadBuffer[0..bytesRead]);

        ArrayPool<byte>.Shared.Return(downloadBufferArr);
    }
}