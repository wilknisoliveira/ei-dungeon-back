using ei_back.Core.Domain.Entity;

namespace ei_back.Core.Application.Service.Play.Interfaces
{
    public interface IInitialMasterPlayService
    {
        Task<Domain.Entity.Play> Handler(Domain.Entity.Game gameEntity, CancellationToken cancellationToken);
    }
}
