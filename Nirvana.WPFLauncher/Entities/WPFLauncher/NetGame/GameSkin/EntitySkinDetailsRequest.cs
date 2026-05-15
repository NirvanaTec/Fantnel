using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nirvana.WPFLauncher.Entities.WPFLauncher.NetGame.GameSkin;

public class EntitySkinDetailsRequest {
    [JsonPropertyName("channel_id")]
    public required int ChannelId { get; set; }

    [JsonPropertyName("entity_ids")]
    public required List<string> EntityIds { get; set; }

    [JsonPropertyName("is_has")]
    public required bool IsHas { get; set; }

    [JsonPropertyName("with_price")]
    public required bool WithPrice { get; set; }

    [JsonPropertyName("with_title_image")]
    public required bool WithTitleImage { get; set; }
}