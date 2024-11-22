using System.ComponentModel;

namespace ei_back.Core.Domain.Entity
{
    public class Player : Base
    {
        public Player(string name, string description, PlayerType type, Guid gameId)
        {
            Name = name;
            Description = description;
            Type = type;
            GameId = gameId;
        }

        public Player(string name, string description, PlayerType type)
        {
            Name = name;
            Description = description;
            Type = type;
        }

        public Player(string name, string description, PlayerType type, Game game) : this(name, description, type)
        {
            Game = game;
            SetCreatedDate(DateTime.Now);
        }

        public string Name { get; private set; }
        public string Description { get; private set; }
        public PlayerType Type { get; private set; }
        public Game Game { get; private set; }
        public Guid GameId { get; private set; }


        public List<Play> Plays { get; private set; }


        public void SetGame(Game game)
        {
            Game = game;
            GameId = game.Id;
        }

        public string InfoToString()
        {
            return $"PlayerId: {Id} \nPlayerName: {Name} \nPlayerDescription: {Description}\n";
        }
    }

    public enum PlayerType : short
    {
        [Description("RealPlayer")] RealPlayer = 0,
        [Description("ArtificialPlayer")] ArtificialPlayer = 1,
        [Description("Master")] Master = 2,
        [Description("System")] System = 3
    }
}
