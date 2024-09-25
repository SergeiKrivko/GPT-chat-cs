using System.Text.Json.Serialization;

namespace Core.Releases;

public class ReleaseAssetReadModel
{
    [JsonPropertyName("version")] public string Version { get; set; }
    [JsonPropertyName("url")] public string Url { get; set; }
    [JsonPropertyName("system")] public string System { get; set; }
}