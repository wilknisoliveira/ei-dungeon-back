using ei_back.Core.Domain.Entity;

namespace ei_back.Core.Application.Service.Play.Interfaces
{
    public interface IPlayFactory
    {
        Task<PlayEntity> BuildInitialMasterPlay(GameEntity gameEntity, CancellationToken cancellationToken);
    }
}
