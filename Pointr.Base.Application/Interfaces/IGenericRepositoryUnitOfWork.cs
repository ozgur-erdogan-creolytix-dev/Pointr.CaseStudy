using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pointr.Base.Application.Interfaces;

public interface IGenericRepositoryUnitOfWork
{
    Task<IAsyncDisposable> BeginTransactionAsync(CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);

    Task SaveChangesWithSingleRetryAsync(CancellationToken cancellationToken);

    Task CommitAsync(CancellationToken cancellationToken);
}
