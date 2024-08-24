using ei_back.Domain.Base;
using ei_back.Domain.Game;

namespace ei_back.Domain.Player
{
    public class PlayerEntity : BaseEntity
    {
        public PlayerEntity(string name, string description, PlayerType type, Guid gameId)
        {
            Name = name;
            Description = description;
            Type = type;
            GameId = gameId;
        }

        public string Name { get; private set; }
        public string Description { get; private set; }
        public PlayerType Type { get; private set; }
        public GameEntity Game { get; private set; }
        public Guid GameId { get; private set; }

    }

    public enum PlayerType
    {
        RealPlayer = 0,
        ArtificialPlayer = 1
    }
}
