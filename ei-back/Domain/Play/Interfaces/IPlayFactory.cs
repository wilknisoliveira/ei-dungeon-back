using ei_back.Domain.Game;

namespace ei_back.Domain.Play.Interfaces
{
    public interface IPlayFactory
    {
        Task<PlayEntity> BuildInitialMasterPlay(GameEntity gameEntity, CancellationToken cancellationToken);
    }
}
