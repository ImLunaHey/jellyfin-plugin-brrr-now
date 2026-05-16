# jellyfin-plugin-brrr-now

A [Jellyfin](https://jellyfin.org) plugin that forwards server notifications to
your Apple devices via [brrr.now](https://brrr.now).

Built for Jellyfin **10.10** (.NET 8).

## What it sends

Each event is independently toggleable in the plugin settings:

| Event | Default | Notes |
|-------|---------|-------|
| Playback started | on | Fires once per playback. |
| Playback stopped | off | |
| Item added to library | off | Noisy during library scans — enable knowingly. |
| Plugin installed | on | |
| Plugin updated | on | |

## Install

### Manual

1. Download `Jellyfin.Plugin.BrrrNow.dll` from the
   [latest release](https://github.com/ImLunaHey/jellyfin-plugin-brrr-now/releases).
2. Drop it into `<jellyfin-data>/plugins/BrrrNow_1.0.0.0/` (create the folder if
   it doesn't exist). On Linux this is usually
   `/var/lib/jellyfin/plugins/`, on macOS
   `~/.local/share/jellyfin/plugins/`, on Docker the path mounted as
   `/config/plugins/`.
3. Restart Jellyfin.
4. Open **Dashboard → Plugins → brrr.now Notifications**, paste your brrr.now
   secret, and hit **Send test notification**.

### Build from source

```sh
dotnet build Jellyfin.Plugin.BrrrNow -c Release
```

The output DLL lands in `Jellyfin.Plugin.BrrrNow/bin/Release/net8.0/`.

## Configure

| Field | What it does |
|-------|--------------|
| **brrr.now secret** | The part of your webhook URL after `https://api.brrr.now/v1/`. Treat like a password. |
| **Thread id** | Groups related notifications on the device. Defaults to `jellyfin`. |
| **Sound** | One of brrr.now's named sounds (see [docs](https://brrr.now/docs/)). |
| **Interruption level** | `passive` / `active` / `time-sensitive` / `critical`. |
| **Append user name to title** | Useful on multi-user servers. |

The **Send test notification** button uses whatever's in the form, so you can
validate a secret before saving it.

## How it works

- `BrrrNowEventListener` (`IHostedService`) subscribes to
  `ISessionManager.PlaybackStart` / `PlaybackStopped` and
  `ILibraryManager.ItemAdded`.
- `PluginInstalledConsumer` / `PluginUpdatedConsumer` (`IEventConsumer<T>`)
  receive Jellyfin's typed plugin lifecycle events.
- All paths funnel through `BrrrNowClient`, which `POST`s to
  `https://api.brrr.now/v1/send` with a `Bearer` token.
- The admin-only `POST /BrrrNow/TestNotification` endpoint powers the test
  button in the plugin settings.

## Plugin GUID

`a4f8c2d7-1e6b-4c8f-9a3d-7e2f5b8c1d9a`
