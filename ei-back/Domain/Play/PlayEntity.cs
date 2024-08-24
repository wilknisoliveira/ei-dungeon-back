using ei_back.Domain.Base;
using ei_back.Domain.Game;
using ei_back.Domain.Player;

namespace ei_back.Domain.Play
{
    public class PlayEntity : BaseEntity
    {
        public PlayEntity(Guid gameId, Guid playerId, string prompt)
        {
            GameId = gameId;
            PlayerId = playerId;
            Prompt = prompt;
        }

        public GameEntity Game { get; private set; }
        public Guid GameId { get; private set; }
        public PlayerEntity Player { get; private set; }
        public Guid PlayerId { get; private set; }
        public string Prompt { get; private set; }
    }
}
