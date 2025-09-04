using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pointr.Base.Application.Interfaces;
using Pointr.Base.Domain.Utilities;
using Pointr.CaseStudy.Application.Common;
using Pointr.CaseStudy.Application.Interfaces;
using Pointr.CaseStudy.Application.Queries.Pages;
using Pointr.CaseStudy.Domain.Entities;

namespace Pointr.CaseStudy.Application.Pages.ArchiveAndOptionallyPublish;

public sealed class ArchiveAndOptionallyPublishHandler
{
    private readonly IGenericRepository _genericRepository;
    private readonly IGenericRepositoryUnitOfWork _genericRepositoryUnitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IPageCache _pageCache;

    public ArchiveAndOptionallyPublishHandler(
        IGenericRepository genericRepository,
        IGenericRepositoryUnitOfWork genericRepositoryUnitOfWork,
        IDateTimeProvider dateTimeProvider,
        IPageCache pageCache
    )
    {
        _genericRepository = genericRepository;
        _genericRepositoryUnitOfWork = genericRepositoryUnitOfWork;
        _dateTimeProvider = dateTimeProvider;
        _pageCache = pageCache;
    }

    public async Task HandleAsync(
        ArchiveAndOptionallyPublishCommand command,
        CancellationToken cancellationToken
    )
    {
        // 1) Resolve Page
        var page = await _genericRepository.FirstOrDefaultAsync(
            new PageBySiteAndSlugSpec(command.SiteId, command.Slug),
            cancellationToken
        );
        if (page is null)
            throw new NotFoundAppException(
                $"Page not found. siteId={command.SiteId}, slug={command.Slug}"
            );

        // 2) Resolve optional Draft to publish (if provided)
        PageDraft? draft = null;
        if (command.PublishDraft is int draftNumber)
        {
            draft = await _genericRepository.FirstOrDefaultAsync(
                new DraftByPageAndNumberSpec(page.Id, draftNumber),
                cancellationToken
            );
            if (draft is null)
                throw new ValidationAppException(
                    $"Draft #{draftNumber} does not belong to the page."
                );
        }

        // 3) Transaction: archive (+ optional publish) with single-retry save
        await using var transaction = await _genericRepositoryUnitOfWork.BeginTransactionAsync(
            cancellationToken
        );

        page.Archive(_dateTimeProvider.UtcNow);
        _genericRepository.Update(page);

        bool insertedPublication = false;
        if (draft is not null)
        {
            // Idempotent publication: ensure (PageId, DraftId) pair hasn't been published already
            var publicationExists = await _genericRepository.AnyAsync(
                new PublicationByPageAndDraftSpec(page.Id, draft.Id),
                cancellationToken
            );

            if (!publicationExists)
            {
                var publication = new PagePublication(
                    id: DeterministicGuid.Create(page.Id, draft.Id),
                    pageId: page.Id,
                    draftId: draft.Id,
                    publishedUtc: _dateTimeProvider.UtcNow
                );
                await _genericRepository.AddAsync(publication, cancellationToken);

                insertedPublication = true;
            }
        }

        try
        {
            await _genericRepositoryUnitOfWork.SaveChangesWithSingleRetryAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // If it still conflicts on the second attempt, upper layer should return HTTP 409
            throw new ConcurrencyAppException("Concurrency conflict while archiving page.", ex);
        }

        await _genericRepositoryUnitOfWork.CommitAsync(cancellationToken);

        // 4) Cache invalidation (deterministic: awaited, not fire and forget)
        await _pageCache.Invalidate(page.SiteId, page.Slug);

        // warm up cache if we inserted a new publication
        if (insertedPublication)
        {
            _ = await _pageCache.GetPublishedPageAsync(page.SiteId, page.Slug, cancellationToken);
        }
    }
}
