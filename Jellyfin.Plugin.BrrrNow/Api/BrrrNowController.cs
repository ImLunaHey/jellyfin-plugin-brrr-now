using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.BrrrNow.Configuration;
using Jellyfin.Plugin.BrrrNow.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.BrrrNow.Api;

[ApiController]
[Authorize(Policy = "RequiresElevation")]
[Route("BrrrNow")]
[Produces("application/json")]
public class BrrrNowController : ControllerBase
{
    private readonly BrrrNowClient _client;

    public BrrrNowController(BrrrNowClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Sends a test notification using the supplied configuration overrides,
    /// or the saved plugin configuration if no body is supplied.
    /// </summary>
    [HttpPost("TestNotification")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> SendTestNotification(
        [FromBody] TestNotificationRequest? request,
        CancellationToken cancellationToken)
    {
        var saved = Plugin.Instance?.Configuration ?? new PluginConfiguration();

        var config = new PluginConfiguration
        {
            Secret = request?.Secret ?? saved.Secret,
            Sound = request?.Sound ?? saved.Sound,
            InterruptionLevel = request?.InterruptionLevel ?? saved.InterruptionLevel,
            ThreadId = request?.ThreadId ?? saved.ThreadId,
            IncludeUserName = saved.IncludeUserName,
        };

        var result = await _client.SendAsync(
            config,
            title: "Jellyfin test",
            message: "If you can read this, brrr.now is wired up correctly.",
            openUrl: null,
            cancellationToken).ConfigureAwait(false);

        if (result.Ok)
        {
            return Ok(new { ok = true });
        }

        return BadRequest(new { ok = false, error = result.Error });
    }

    public class TestNotificationRequest
    {
        public string? Secret { get; set; }

        public string? Sound { get; set; }

        public string? InterruptionLevel { get; set; }

        public string? ThreadId { get; set; }
    }
}
