namespace ei_back.Core.Domain.Entity
{
    public class PlayEntity : BaseEntity
    {
        public PlayEntity(Guid gameId, Guid playerId, string prompt)
        {
            GameId = gameId;
            PlayerId = playerId;
            Prompt = prompt;
        }

        public PlayEntity(GameEntity game, PlayerEntity player, string prompt)
        {
            GameId = game.Id;
            Game = game;
            PlayerId = player.Id;
            Player = player;
            Prompt = prompt;
            SetCreatedDate(DateTime.Now);
        }

        public GameEntity Game { get; private set; }
        public Guid GameId { get; private set; }
        public PlayerEntity Player { get; private set; }
        public Guid PlayerId { get; private set; }
        public string Prompt { get; private set; }
    }
}
