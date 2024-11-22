using ei_back.Core.Domain.Entity;

namespace ei_back.Core.Application.Service.Player.Interfaces
{
    public interface IPlayerFactory
    {
        Task<List<Domain.Entity.Player>> BuildArtificialPlayersAndMaster(int numberOfArtificalPlayers, Domain.Entity.Game game, CancellationToken cancellationToken);
    }
}
