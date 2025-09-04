using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Pointr.CaseStudy.Application.Pages.ArchiveAndOptionallyPublish;

namespace Pointr.CaseStudy.Api.Controllers;

[ApiController]
public class PagesController : ControllerBase
{
    private readonly ArchiveAndOptionallyPublishHandler _svc;

    public PagesController(ArchiveAndOptionallyPublishHandler handler)
    {
        _svc = handler;
    }

    // DELETE /api/v1/sites/{siteId}/pages/{slug}?publishDraft=3
    [HttpDelete("api/v1/sites/{siteId}/pages/{slug}")]
    public async Task<IActionResult> Archive(
        [FromRoute] Guid siteId,
        [FromRoute] string slug,
        [FromQuery]
        [Range(1, int.MaxValue, ErrorMessage = "publishDraft must be >= 1")]
            int? publishDraft,
        CancellationToken ct
    )
    {
        var cmd = new ArchiveAndOptionallyPublishCommand(siteId, slug, publishDraft);
        await _svc.HandleAsync(cmd, ct);
        return NoContent(); // 204 (idempotent)
    }
}
