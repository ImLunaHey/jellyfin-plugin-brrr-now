using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Events.Updates;

namespace Jellyfin.Plugin.BrrrNow.Notifications;

public class PluginInstalledConsumer : IEventConsumer<PluginInstalledEventArgs>
{
    private readonly BrrrNowClient _client;

    public PluginInstalledConsumer(BrrrNowClient client)
    {
        _client = client;
    }

    public async Task OnEvent(PluginInstalledEventArgs eventArgs)
    {
        var config = Plugin.Instance?.Configuration;
        if (config is null || !config.NotifyOnPluginInstalled || string.IsNullOrWhiteSpace(config.Secret))
        {
            return;
        }

        var info = eventArgs.Argument;
        await _client.SendAsync(
            config,
            title: "Plugin installed",
            message: $"{info.Name} {info.Version} was installed.",
            openUrl: null,
            CancellationToken.None).ConfigureAwait(false);
    }
}
