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
        public int[] CustomPositionValues { get; set; } = [1, 5, 10];

        [JsonPropertyName("Custom Angle Values")]
        public int[] CustomAngleValues { get; set; } = [1, 5, 10];

        [JsonPropertyName("Enable commands")]
        public bool EnableCMD { get; set; } = true;

        [JsonPropertyName("Debug Mode")]
        public bool Debug { get; set; } = false;
    }
}