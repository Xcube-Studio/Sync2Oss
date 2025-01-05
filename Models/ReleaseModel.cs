using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sync2Oss.Models;

public class AssetModel
{
    [JsonPropertyName("name")]
    public required string Name { set; get; }

    [JsonPropertyName("browser_download_url")]
    public required string DownloadUrl { set; get; }
}

public class ReleaseModel
{
    [JsonPropertyName("tag_name")]
    public required string TagName { get; set; }

    [JsonPropertyName("assets")]
    public required string List<AssetModel> { get; set; }
}