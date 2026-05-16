using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Events.Updates;

namespace Jellyfin.Plugin.BrrrNow.Notifications;

public class PluginUpdatedConsumer : IEventConsumer<PluginUpdatedEventArgs>
{
    private readonly BrrrNowClient _client;

    public PluginUpdatedConsumer(BrrrNowClient client)
    {
        _client = client;
    }

    public async Task OnEvent(PluginUpdatedEventArgs eventArgs)
    {
        var config = Plugin.Instance?.Configuration;
        if (config is null || !config.NotifyOnPluginUpdated || string.IsNullOrWhiteSpace(config.Secret))
        {
            return;
        }

        var info = eventArgs.Argument;
        await _client.SendAsync(
            config,
            title: "Plugin updated",
            message: $"{info.Name} updated to {info.Version}.",
            openUrl: null,
            CancellationToken.None).ConfigureAwait(false);
    }
}
