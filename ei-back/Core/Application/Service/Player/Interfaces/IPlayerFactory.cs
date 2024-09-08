using ei_back.Core.Domain.Entity;

namespace ei_back.Core.Application.Service.Player.Interfaces
{
    public interface IPlayerFactory
    {
        Task<List<PlayerEntity>> BuildArtificialPlayersAndMaster(int numberOfArtificalPlayers, GameEntity game, CancellationToken cancellationToken);
    }
}
