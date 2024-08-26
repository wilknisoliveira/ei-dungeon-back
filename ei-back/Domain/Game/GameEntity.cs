using ei_back.Domain.Base;
using ei_back.Domain.Play;
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

        public GameEntity(string name, string systemGame)
        {
            Name = name;
            SystemGame = systemGame;
        }

        public string Name { get; private set; }
        public UserEntity OwnerUser { get; private set; }
        public Guid OwnerUserId { get; private set; }
        public string SystemGame { get; private set; }


        public List<PlayerEntity> Players { get; private set; }
        public List<PlayEntity> Plays { get; private set; }

        public void SetOwnerUser(UserEntity user)
        {
            OwnerUser = user;
            OwnerUserId = user.Id;
        }

        public void SetPlayers(List<PlayerEntity> players)
        {
            Players = players;
        }
    }
}
