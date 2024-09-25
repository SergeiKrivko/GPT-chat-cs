using System.Text.Json.Serialization;

namespace Core.Releases;

public class ReleaseReadModel
{
    [JsonPropertyName("version")] public string Version { get; set; }
    [JsonPropertyName("description")] public string Description { get; set; }
    [JsonPropertyName("assets")] public List<ReleaseAssetReadModel> Assets;
}