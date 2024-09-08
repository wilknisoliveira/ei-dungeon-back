using ei_back.Core.Domain.Entity;

namespace ei_back.Core.Application.Service.Play.Interfaces
{
    public interface IInitialMasterPlayService
    {
        Task<PlayEntity> Handler(GameEntity gameEntity, CancellationToken cancellationToken);
    }
}
