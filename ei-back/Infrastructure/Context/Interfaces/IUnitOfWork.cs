using ei_back.Domain.Base.Interfaces;
using ei_back.Domain.Base;

namespace ei_back.Infrastructure.Context.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        void Commit();
        Task<int> CommitAsync(CancellationToken cancellationToken = default);
        void Rollback();
        IRepository<T> GetRepository<T>() where T : BaseEntity;
    }
}
