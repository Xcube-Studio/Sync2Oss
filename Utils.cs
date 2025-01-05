using Aliyun.OSS;
using Aliyun.OSS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sync2Oss;

//internal class Utils
//{

//    public static async Task DownloadFileAsync(string url, string outputPath)
//    {
//        using (HttpClient client = new HttpClient())
//        {
//            try
//            {
//                HttpResponseMessage response = await client.GetAsync(url);
//                response.EnsureSuccessStatusCode();

//                using (var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None))
//                {
//                    await response.Content.CopyToAsync(fs);
//                }

//                Console.WriteLine($"[INFO] File downloaded successfully to {outputPath}");
//            }
//            catch (HttpRequestException e)
//            {
//                Console.WriteLine($"[ERROR] Download file failed: {e.Message}");
//            }
//            catch (IOException e)
//            {
//                Console.WriteLine($"[ERROR] File IO error: {e.Message}");
//            }
//        }
//    }

//    public class AliyunOss(string accessKeyId, string accessKeySecret, string endpoint, string bucketName)
//    {
//        private readonly string _accessKeyId = accessKeyId;
//        private readonly string _accessKeySecret = accessKeySecret;
//        private readonly string _endpoint = endpoint;
//        private readonly string _bucketName = bucketName;

//        const string region = "cn-shanghai";

//        private readonly OssClient _client = new OssClient(endpoint, accessKeyId, accessKeySecret, new ClientConfiguration()
//        {
//            SignatureVersion = SignatureVersion.V4
//        });

//        public async Task UploadFileAsync(string localFilePath, string remoteFilePath)
//        {
//            try
//            {
//                _client.SetRegion(region);
//                // 检查文件是否存在
//                if (await IsObjectExist(remoteFilePath))
//                {
//                    // 删除现有文件
//                    await Task.Run(() => _client.DeleteObject(_bucketName, remoteFilePath));
//                    Console.WriteLine($"[INFO] Existing file {remoteFilePath} deleted.");
//                }

//                // 上传新文件
//                await Task.Run(() => _client.PutObject(_bucketName, remoteFilePath, localFilePath));
//                Console.WriteLine($"[INFO] Upload file {localFilePath} to {remoteFilePath} successfully.");
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine($"[ERROR] Upload file {localFilePath} to {remoteFilePath} failed: {e.Message}");
//            }
//        }
//        public async Task<bool> IsObjectExist(string objectName)
//        {

//            try
//            {
//                _client.SetRegion(region);
//                var result = await Task.Run(() => _client.DoesObjectExist(_bucketName, objectName));
//                return result;
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine($"[ERROR] Check object {objectName} exist failed: {e.Message}");
//                return false;
//            }
//        }
//    }

//    public class Deserialization
//    {
//        public class release
//        {
//            public List<asset> assets { set; get; }
//            public required string tag_name { set; get; }
//        }

//        public class asset
//        {
//            public required string name { set; get; }
//            public required string browser_download_url { set; get; }
//        }
//    }
//}
