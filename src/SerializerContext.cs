using System.Text.Json.Serialization;

namespace Sync2Oss.Models;

[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(DateTime))]
[JsonSerializable(typeof(AssetModel))]
[JsonSerializable(typeof(AssetModel[]))]
[JsonSerializable(typeof(ReleaseModel))]
[JsonSerializable(typeof(ReleaseModel[]))]
public partial class SerializerContext : JsonSerializerContext { }
