using ei_back.Domain.Base;

namespace ei_back.Domain.Player
{
    public class PlayerEntity(string name, string description, PlayerType type) : BaseEntity
    {
        public string Name { get; private set; } = name;
        public string Description { get; private set; } = description;
        public PlayerType Type { get; private set; } = type;

    }

    public enum PlayerType
    {
        RealPlayer = 0,
        ArtificialPlayer = 1
    }
}
