using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pointr.Base.Application.Interfaces;
using Pointr.CaseStudy.Application.Common;

namespace Pointr.Base.Infrastructure.Persistence.UnitOfWork;

public sealed class EfUnitOfWork<TDbContext> : IGenericRepositoryUnitOfWork
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;
    private readonly ILogger<EfUnitOfWork<TDbContext>> _logger;

    public EfUnitOfWork(TDbContext dbContext, ILogger<EfUnitOfWork<TDbContext>> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IAsyncDisposable> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        return transaction;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChangesWithSingleRetryAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex1)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken); // single retry
            }
            catch (DbUpdateConcurrencyException ex2)
            {
                throw new ConcurrencyAppException("Concurrency conflict. Please retry.", ex2); // → 409
            }
        }
    }

    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        // If a transaction exists, commit; otherwise no-op
        var currentTransaction = _dbContext.Database.CurrentTransaction;
        if (currentTransaction is not null)
            await currentTransaction.CommitAsync(cancellationToken);
    }
}
