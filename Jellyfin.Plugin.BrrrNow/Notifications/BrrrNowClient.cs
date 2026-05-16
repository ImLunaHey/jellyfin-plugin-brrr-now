using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.BrrrNow.Configuration;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.BrrrNow.Notifications;

public class BrrrNowClient
{
    private const string Endpoint = "https://api.brrr.now/v1/send";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<BrrrNowClient> _logger;

    public BrrrNowClient(IHttpClientFactory httpClientFactory, ILogger<BrrrNowClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<BrrrNowSendResult> SendAsync(
        PluginConfiguration config,
        string title,
        string message,
        string? openUrl,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(config.Secret))
        {
            return BrrrNowSendResult.Failure("brrr.now secret is not configured.");
        }

        var payload = new BrrrPayload
        {
            Title = NullIfBlank(title),
            Message = string.IsNullOrWhiteSpace(message) ? title : message,
            ThreadId = NullIfBlank(config.ThreadId),
            Sound = NullIfBlank(config.Sound),
            InterruptionLevel = NullIfBlank(config.InterruptionLevel),
            OpenUrl = NullIfBlank(openUrl),
        };

        using var client = _httpClientFactory.CreateClient();
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, Endpoint)
        {
            Content = JsonContent.Create(payload),
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", config.Secret);

        try
        {
            using var response = await client.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                return BrrrNowSendResult.Success();
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogError(
                "brrr.now returned HTTP {Status}: {Body}",
                (int)response.StatusCode,
                body);
            return BrrrNowSendResult.Failure($"HTTP {(int)response.StatusCode}: {body}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification via brrr.now");
            return BrrrNowSendResult.Failure(ex.Message);
        }
    }

    private static string? NullIfBlank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    private sealed class BrrrPayload
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Title { get; set; }

        [JsonPropertyName("subtitle")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Subtitle { get; set; }

        [JsonPropertyName("thread_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ThreadId { get; set; }

        [JsonPropertyName("sound")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Sound { get; set; }

        [JsonPropertyName("interruption_level")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? InterruptionLevel { get; set; }

        [JsonPropertyName("open_url")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? OpenUrl { get; set; }
    }
}

public readonly record struct BrrrNowSendResult(bool Ok, string? Error)
{
    public static BrrrNowSendResult Success() => new(true, null);

    public static BrrrNowSendResult Failure(string error) => new(false, error);
}
