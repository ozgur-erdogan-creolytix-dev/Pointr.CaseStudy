using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pointr.CaseStudy.Application.Pages.ArchiveAndOptionallyPublish;

public sealed record ArchiveAndOptionallyPublishCommand(
    Guid SiteId,
    string Slug,
    int? PublishDraft
);
