using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace MapAdvertisements.Models
{
    public class PropModel
    {
        public int Id { get; set; }
        public string? modelPath { get; set; }
        public int ModelGroupIndex { get; set; }
        public float posX { get; set; }
        public float posY { get; set; }
        public float posZ { get; set; }
        public float angleX { get; set; }
        public float angleY { get; set; }
        public float angleZ { get; set; }
        public float width { get; set; }
        public float height { get; set; }
        public bool isOnGround { get; set; }
        public int depth { get; set; }

        [JsonIgnore]
        public CBaseEntity? EntityProp { get; set; }
    }
}