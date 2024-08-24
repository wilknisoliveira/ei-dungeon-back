using ei_back.Domain.Base;
using ei_back.Domain.Player;
using ei_back.Domain.User;

namespace ei_back.Domain.Game
{
    public class GameEntity : BaseEntity
    {
        public GameEntity(Guid ownerUserId, string systemGame, string name)
        {
            OwnerUserId = ownerUserId;
            SystemGame = systemGame;
            Name = name;
        }

        public string Name { get; set; }
        public UserEntity OwnerUser { get; private set; }
        public Guid OwnerUserId { get; private set; }
        public string SystemGame { get; private set; }
        public List<PlayerEntity> Players { get; private set; }
    }
}
