using System;
using System.Text.Json.Serialization;

namespace Nirvana.WPFLauncher.Entities.MPay;

public class EntitySmsTicket {
    [JsonPropertyName("guide_text")]
    public string GuideText { get; set; } = string.Empty;

    [JsonPropertyName("related_emails")]
    public string[] RelatedEmails { get; set; } = Array.Empty<string>();

    [JsonPropertyName("ticket")]
    public string Ticket { get; set; } = string.Empty;

    [JsonPropertyName("related_accounts")]
    public string[] RelatedAccounts { get; set; } = Array.Empty<string>();
}