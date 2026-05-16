using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.BrrrNow.Configuration;

public class PluginConfiguration : BasePluginConfiguration
{
    public PluginConfiguration()
    {
        Secret = string.Empty;
        Sound = "default";
        InterruptionLevel = "active";
        ThreadId = "jellyfin";
        IncludeUserName = true;
        NotifyOnPlaybackStart = true;
        NotifyOnPlaybackStop = false;
        NotifyOnItemAdded = false;
        NotifyOnPluginInstalled = true;
        NotifyOnPluginUpdated = true;
    }

    public string Secret { get; set; }

    public string Sound { get; set; }

    public string InterruptionLevel { get; set; }

    public string ThreadId { get; set; }

    public bool IncludeUserName { get; set; }

    public bool NotifyOnPlaybackStart { get; set; }

    public bool NotifyOnPlaybackStop { get; set; }

    public bool NotifyOnItemAdded { get; set; }

    public bool NotifyOnPluginInstalled { get; set; }

    public bool NotifyOnPluginUpdated { get; set; }
}
