using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.BrrrNow.Notifications;

public class BrrrNowEventListener : IHostedService
{
    private readonly ISessionManager _sessionManager;
    private readonly ILibraryManager _libraryManager;
    private readonly BrrrNowClient _client;
    private readonly ILogger<BrrrNowEventListener> _logger;

    public BrrrNowEventListener(
        ISessionManager sessionManager,
        ILibraryManager libraryManager,
        BrrrNowClient client,
        ILogger<BrrrNowEventListener> logger)
    {
        _sessionManager = sessionManager;
        _libraryManager = libraryManager;
        _client = client;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _sessionManager.PlaybackStart += OnPlaybackStart;
        _sessionManager.PlaybackStopped += OnPlaybackStopped;
        _libraryManager.ItemAdded += OnItemAdded;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _sessionManager.PlaybackStart -= OnPlaybackStart;
        _sessionManager.PlaybackStopped -= OnPlaybackStopped;
        _libraryManager.ItemAdded -= OnItemAdded;
        return Task.CompletedTask;
    }

    private void OnPlaybackStart(object? sender, PlaybackProgressEventArgs e)
        => Fire(config => config.NotifyOnPlaybackStart, () =>
        {
            var who = FormatUsers(e);
            var what = e.Item?.Name ?? "an item";
            return ("Playback started", $"{who} started {what}");
        });

    private void OnPlaybackStopped(object? sender, PlaybackStopEventArgs e)
        => Fire(config => config.NotifyOnPlaybackStop, () =>
        {
            var who = FormatUsers(e);
            var what = e.Item?.Name ?? "an item";
            var suffix = e.PlayedToCompletion ? " (finished)" : string.Empty;
            return ("Playback stopped", $"{who} stopped {what}{suffix}");
        });

    private void OnItemAdded(object? sender, ItemChangeEventArgs e)
    {
        if (!IsLibraryMedia(e.Item))
        {
            return;
        }

        Fire(config => config.NotifyOnItemAdded, () =>
        {
            var name = e.Item!.Name ?? "an item";
            var kind = e.Item.GetType().Name;
            return ($"New {kind}", $"{name} was added to the library.");
        });
    }

    private static bool IsLibraryMedia(BaseItem? item) =>
        item is not null
        && item is not Person
        && item is not Genre
        && item is not Studio
        && item is not Year
        && item is not MusicGenre;

    private void Fire(Func<Configuration.PluginConfiguration, bool> enabled, Func<(string Title, string Message)> build)
    {
        var config = Plugin.Instance?.Configuration;
        if (config is null || !enabled(config) || string.IsNullOrWhiteSpace(config.Secret))
        {
            return;
        }

        var (title, message) = build();
        _ = Task.Run(() => _client.SendAsync(config, title, message, openUrl: null, CancellationToken.None));
    }

    private static string FormatUsers(PlaybackProgressEventArgs e)
    {
        var names = e.Users?
            .Select(u => u.Username)
            .Where(n => !string.IsNullOrEmpty(n))
            .ToArray();
        return names is { Length: > 0 } ? string.Join(", ", names) : "Someone";
    }
}
