using Microsoft.IdentityModel.Tokens;

namespace ei_back.Core.Domain.Entity
{
    public class Game : Base
    {
        public Game(Guid ownerUserId, string systemGame, string name)
        {
            OwnerUserId = ownerUserId;
            SystemGame = systemGame;
            Name = name;
        }

        public Game(string name, string systemGame)
        {
            Name = name;
            SystemGame = systemGame;
        }

        public string Name { get; private set; }
        public User OwnerUser { get; private set; }
        public Guid OwnerUserId { get; private set; }
        public string SystemGame { get; private set; }


        public List<Player> Players { get; private set; }
        public List<Play> Plays { get; private set; }

        public void SetOwnerUser(User user)
        {
            OwnerUser = user;
            OwnerUserId = user.Id;
        }

        public void SetPlayers(List<Player> players)
        {
            Players = players;
        }

        public void AddPlay(Play play)
        {
            if (Plays == null)
                Plays = [];

            Plays.Add(play);
        }
    }
}
