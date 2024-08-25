using ei_back.Domain.Game;

namespace ei_back.Domain.Player.Interfaces
{
    public interface IPlayerFactory
    {
        Task<List<PlayerEntity>> BuildArtificialPlayersAndMaster(int numberOfArtificalPlayers, GameEntity game, CancellationToken cancellationToken);
    }
}
