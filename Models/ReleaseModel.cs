using System.Linq;
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
    public required List<AssetModel> Assets { get; set; }
}

[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(AssetModel))]
[JsonSerializable(typeof(List<AssetModel>))]
[JsonSerializable(typeof(ReleaseModel))]
[JsonSerializable(typeof(ReleaseModel[]))]
public partial class SerializerContext : JsonSerializerContext { }