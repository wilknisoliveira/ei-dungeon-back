using ei_back.Core.Application.Repository;
using ei_back.Core.Domain.Entity;

namespace ei_back.Infrastructure.Context.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        void Commit();
        Task<int> CommitAsync(CancellationToken cancellationToken = default);
        void Rollback();
        IRepository<T> GetRepository<T>() where T : Base;
    }
}
