using ei_back.Core.Domain.Enums;

namespace ei_back.Core.Domain.Entity
{
    public class Game : Base
    {
        public Game(Guid ownerUserId, string name)
        {
            OwnerUserId = ownerUserId;
            Name = name;
        }
        
        public Game(User ownerUser, string name)
        {
            SetOwnerUser(ownerUser);
            Name = name;
        }

        public Game(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
        public User OwnerUser { get; private set; }
        public Guid OwnerUserId { get; private set; }
        public string WorldInfo { get; private set; } = "";
        public GameStatus GameStatus { get; private set; } = GameStatus.Active;

        public List<Player> Players { get; private set; }
        public List<Play> Plays { get; private set; } = [];

        public void SetOwnerUser(User user)
        {
            OwnerUser = user;
            OwnerUserId = user.Id;
        }

        public void SetWorldInfo(string worldInfo)
        {
            WorldInfo = worldInfo;
        }

        public void SetPlayers(List<Player> players)
        {
            Players = players;
        }

        public void AddPlay(Play play)
        {
            Plays.Add(play);
        }

        public void KillPlayer()
        {
            GameStatus = GameStatus.PlayerDied;
        }
    }
}
