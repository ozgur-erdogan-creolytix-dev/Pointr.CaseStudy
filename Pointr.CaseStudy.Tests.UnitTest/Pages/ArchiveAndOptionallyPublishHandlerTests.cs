using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using Pointr.Base.Application.Interfaces;
using Pointr.Base.Domain.Utilities;
using Pointr.CaseStudy.Application.Common;
using Pointr.CaseStudy.Application.Interfaces;
using Pointr.CaseStudy.Application.Pages.ArchiveAndOptionallyPublish;
using Pointr.CaseStudy.Domain.Entities;

namespace Pointr.CaseStudy.Tests.UnitTest.Pages;

public class ArchiveAndOptionallyPublishHandlerTests
{
    // Fields
    private readonly Mock<IGenericRepository> _genericRepository;
    private readonly Mock<IGenericRepositoryUnitOfWork> _unitOfWork;
    private readonly Mock<IDateTimeProvider> _dateTimeProvider;
    private readonly Mock<IPageCache> _pageCache;
    private readonly ArchiveAndOptionallyPublishHandler _sut;

    private static readonly Guid SiteAcme = Guid.Parse("6f4a0001-0000-0000-0000-000000000001");
    private static readonly DateTime FixedUtc = new(2025, 8, 30, 12, 0, 0, DateTimeKind.Utc);

    public ArchiveAndOptionallyPublishHandlerTests()
    {
        _genericRepository = new Mock<IGenericRepository>();
        _unitOfWork = new Mock<IGenericRepositoryUnitOfWork>();
        _dateTimeProvider = new Mock<IDateTimeProvider>();
        _pageCache = new Mock<IPageCache>();

        _sut = new ArchiveAndOptionallyPublishHandler(
            _genericRepository.Object,
            _unitOfWork.Object,
            _dateTimeProvider.Object,
            _pageCache.Object
        );

        _unitOfWork
            .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NoopTx());
        _unitOfWork
            .Setup(u => u.SaveChangesWithSingleRetryAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWork
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _dateTimeProvider.SetupGet(c => c.UtcNow).Returns(FixedUtc);
    }

    // 1) Page missing → NotFound
    [Fact]
    public async Task Handle_ShouldThrow_NotFound_WhenPageMissing()
    {
        _genericRepository
            .Setup(r =>
                r.FirstOrDefaultAsync(
                    It.IsAny<IQueryOptions<Page>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((Page?)null);

        var cmd = new ArchiveAndOptionallyPublishCommand(SiteAcme, "about", null);
        await Assert.ThrowsAsync<NotFoundAppException>(
            () => _sut.HandleAsync(cmd, CancellationToken.None)
        );

        _unitOfWork.Verify(
            u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
        _pageCache.Verify(c => c.Invalidate(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    // 2) Draft does not belong to the page → Validation
    [Fact]
    public async Task Handle_ShouldThrow_Validation_WhenDraftNotBelongToPage()
    {
        var page = new Page(Guid.NewGuid(), SiteAcme, "about", FixedUtc.AddDays(-1));
        _genericRepository
            .Setup(r =>
                r.FirstOrDefaultAsync(
                    It.IsAny<IQueryOptions<Page>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(page);
        _genericRepository
            .Setup(r =>
                r.FirstOrDefaultAsync(
                    It.IsAny<IQueryOptions<PageDraft>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((PageDraft?)null);

        var cmd = new ArchiveAndOptionallyPublishCommand(SiteAcme, "about", 2);
        await Assert.ThrowsAsync<ValidationAppException>(
            () => _sut.HandleAsync(cmd, CancellationToken.None)
        );

        _unitOfWork.Verify(
            u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    // 3) Archive only → IsArchived=true, UpdatedUtc=UtcNow, cache invalidate
    [Fact]
    public async Task Handle_ShouldArchive_Only_WhenNoPublishDraft()
    {
        var page = new Page(Guid.NewGuid(), SiteAcme, "contact", FixedUtc.AddDays(-5));
        _genericRepository
            .Setup(r =>
                r.FirstOrDefaultAsync(
                    It.IsAny<IQueryOptions<Page>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(page);

        var cmd = new ArchiveAndOptionallyPublishCommand(SiteAcme, "contact", null);
        await _sut.HandleAsync(cmd, CancellationToken.None);

        Assert.True(page.IsArchived);
        Assert.Equal(FixedUtc, page.UpdatedUtc);

        _unitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(
            u => u.SaveChangesWithSingleRetryAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);

        _pageCache.Verify(c => c.Invalidate(SiteAcme, "contact"), Times.Once);
    }

    // 4) Archive + publish (insert publication if not exists) → deterministic Id
    [Fact]
    public async Task Handle_ShouldPublish_WhenDraftExists_AndPublicationNotExists()
    {
        var pageId = Guid.NewGuid();
        var draftId = Guid.NewGuid();
        var page = new Page(pageId, SiteAcme, "about", FixedUtc.AddDays(-2));
        var draft = new PageDraft(draftId, pageId, 2, "About v2");

        _genericRepository
            .Setup(r =>
                r.FirstOrDefaultAsync(
                    It.IsAny<IQueryOptions<Page>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(page);
        _genericRepository
            .Setup(r =>
                r.FirstOrDefaultAsync(
                    It.IsAny<IQueryOptions<PageDraft>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(draft);
        _genericRepository
            .Setup(r =>
                r.AnyAsync(
                    It.IsAny<IQueryOptions<PagePublication>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(false);

        PagePublication? captured = null;
        _genericRepository
            .Setup(r => r.AddAsync(It.IsAny<PagePublication>(), It.IsAny<CancellationToken>()))
            .Callback<PagePublication, CancellationToken>((p, _) => captured = p)
            .Returns(Task.CompletedTask);

        var cmd = new ArchiveAndOptionallyPublishCommand(SiteAcme, "about", 2);
        await _sut.HandleAsync(cmd, CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal(pageId, captured!.PageId);
        Assert.Equal(draftId, captured.DraftId);
        Assert.Equal(FixedUtc, captured.PublishedUtc);
        Assert.Equal(DeterministicGuid.Create(pageId, draftId), captured.Id);

        _pageCache.Verify(c => c.Invalidate(SiteAcme, "about"), Times.Once);
    }

    // 5) Publish is idempotent → skip insert when publication already exists
    [Fact]
    public async Task Handle_ShouldNotInsertPublication_WhenAlreadyExists()
    {
        var pageId = Guid.NewGuid();
        var draft = new PageDraft(Guid.NewGuid(), pageId, 3, "Blog v3");
        var page = new Page(pageId, SiteAcme, "blog", FixedUtc);

        _genericRepository
            .Setup(r =>
                r.FirstOrDefaultAsync(
                    It.IsAny<IQueryOptions<Page>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(page);
        _genericRepository
            .Setup(r =>
                r.FirstOrDefaultAsync(
                    It.IsAny<IQueryOptions<PageDraft>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(draft);
        _genericRepository
            .Setup(r =>
                r.AnyAsync(
                    It.IsAny<IQueryOptions<PagePublication>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(true);

        var cmd = new ArchiveAndOptionallyPublishCommand(SiteAcme, "blog", 3);
        await _sut.HandleAsync(cmd, CancellationToken.None);

        _genericRepository.Verify(
            r => r.AddAsync(It.IsAny<PagePublication>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _pageCache.Verify(c => c.Invalidate(SiteAcme, "blog"), Times.Once);
    }

    // 6) Concurrency conflict → throws ConcurrencyAppException; no commit/invalidate
    [Fact]
    public async Task Handle_ShouldThrow_Concurrency_WhenSaveConflicts()
    {
        var page = new Page(Guid.NewGuid(), SiteAcme, "contact", FixedUtc);
        _genericRepository
            .Setup(r =>
                r.FirstOrDefaultAsync(
                    It.IsAny<IQueryOptions<Page>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(page);

        _unitOfWork
            .Setup(u => u.SaveChangesWithSingleRetryAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException("conflict"));

        var cmd = new ArchiveAndOptionallyPublishCommand(SiteAcme, "contact", null);
        await Assert.ThrowsAsync<ConcurrencyAppException>(
            () => _sut.HandleAsync(cmd, CancellationToken.None)
        );

        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _pageCache.Verify(c => c.Invalidate(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    // 7) Cancellation token propagation (ensures token is passed through)
    [Fact]
    public async Task Handle_ShouldPass_CancellationToken()
    {
        var page = new Page(Guid.NewGuid(), SiteAcme, "home", FixedUtc);
        _genericRepository
            .Setup(r =>
                r.FirstOrDefaultAsync(
                    It.IsAny<IQueryOptions<Page>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(page);

        using var cts = new CancellationTokenSource();
        await _sut.HandleAsync(
            new ArchiveAndOptionallyPublishCommand(SiteAcme, "home", null),
            cts.Token
        );

        _unitOfWork.Verify(
            u => u.SaveChangesWithSingleRetryAsync(It.Is<CancellationToken>(t => t == cts.Token)),
            Times.Once
        );
    }

    private sealed class NoopTx : IAsyncDisposable
    {
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
