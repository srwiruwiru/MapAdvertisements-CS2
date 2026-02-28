using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace MapAdvertisements.Config
{
    public class PluginConfig : BasePluginConfig
    {
        [JsonPropertyName("Admin Flag")]
        public string AdminFlag { get; set; } = "@css/root";

        [JsonPropertyName("Props Path")]
        public string[] Props { get; set; } = [];

        [JsonPropertyName("Custom Position Values")]
        public float[] CustomPositionValues { get; set; } = [0.25f, 0.5f, 1f, 5f, 10f];

        [JsonPropertyName("Custom Angle Values")]
        public float[] CustomAngleValues { get; set; } = [0.25f, 0.5f, 1f, 5f, 10f];

        [JsonPropertyName("Enable commands")]
        public bool EnableCMD { get; set; } = true;

        [JsonPropertyName("Debug Mode")]
        public bool Debug { get; set; } = false;
    }
}