namespace ei_back.Core.Domain.Entity
{
    public class Play : Base
    {
        public Play(Guid gameId, Guid playerId, string prompt)
        {
            GameId = gameId;
            PlayerId = playerId;
            Prompt = prompt;
        }

        public Play(Game game, Player player, string prompt)
        {
            GameId = game.Id;
            Game = game;
            PlayerId = player.Id;
            Player = player;
            Prompt = prompt;
            SetCreatedDate(DateTime.Now);
        }

        public Game Game { get; private set; }
        public Guid GameId { get; private set; }
        public Player Player { get; private set; }
        public Guid PlayerId { get; private set; }
        public string Prompt { get; private set; }
    }
}
