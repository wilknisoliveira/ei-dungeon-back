using System.ComponentModel;
using ei_back.Core.Domain.DomainExceptions.Player;
using ei_back.Core.Domain.Enums;
using ei_back.Infrastructure.Extensions;

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

        public Player(
            string name, 
            string description, 
            CharacterRace race, 
            PlayerType type) : this(name, description, type)
        {
            Race = race;
            SetCreatedDate(DateTime.Now);
        }

        public string Name { get; private set; }
        public string Description { get; private set; }
        public PlayerType Type { get; private set; }
        public Game Game { get; private set; }
        public Guid GameId { get; private set; }
        public CharacterRace Race { get; private set; } = CharacterRace.Human;
        public int Strength { get; private set; } = 8;
        public int Dexterity { get; private set; } = 8;
        public int Intelligence { get; private set; } = 8;
        public int Constitution { get; private set; } = 8;
        public int Charisma { get; private set; } = 8;
        public int Wisdom { get; private set; } = 8;
        
        public List<Play> Plays { get; private set; } = [];

        public string InfoToString()
        {
            return $"PlayerName: {Name} \n" +
                   $"PlayerDescription: {Description}\n" +
                   $"Race: {Race.GetEnumDescription()}\n" +
                   "Skills:\n" +
                   $"- Strength: {Strength}\n" +
                   $"- Dexterity: {Dexterity}\n" +
                   $"- Intelligence: {Intelligence}\n" +
                   $"- Constitution: {Constitution}\n" +
                   $"- Charisma: {Charisma}\n" +
                   $"- Wisdom: {Wisdom}";
        }

        public void SetSkillPoints(
            int strength, 
            int dexterity, 
            int intelligence, 
            int constitution, 
            int charisma, 
            int wisdom)
        {
            var numberOfSkills = 6;
            var skillBase = 8;
            var maxOfAllocatedPoints = 30;
            var sumOfSkills = strength + dexterity + intelligence + constitution + charisma + wisdom;
            
            if (sumOfSkills != (maxOfAllocatedPoints + numberOfSkills * skillBase))
            {
                throw new AttributePointsNotValidException();
            }

            Strength = strength;
            Dexterity = dexterity;
            Intelligence = intelligence;
            Constitution = constitution;
            Charisma = charisma;
            Wisdom = wisdom;

            switch (Race)
            {
                case CharacterRace.Human:
                    Strength += 1;
                    Dexterity += 1;
                    Intelligence += 1;
                    Constitution += 1;
                    Charisma += 1;
                    Wisdom += 1;
                    break;
                case CharacterRace.Elf:
                    Dexterity += 2;
                    break;
                case CharacterRace.Dwarf:
                    Constitution += 2;
                    break;
                case CharacterRace.HalfElf:
                    Charisma += 2;
                    Wisdom += 1;
                    Intelligence += 1;
                    break;
                case CharacterRace.Halfling:
                    Dexterity += 2;
                    break;
                case CharacterRace.Tiefling:
                    Charisma += 2;
                    Intelligence += 1;
                    break;
                case CharacterRace.Dragonborn:
                    Strength += 2;
                    Charisma += 1;
                    break;
                case CharacterRace.HalfOrc:
                    Strength += 2;
                    Constitution += 1;
                    break;
                case CharacterRace.Gnome:
                    Intelligence += 2;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Race), Race, null);
            }
        }

        public int GetModifier(Skill skill)
        {
            var skillValue = skill switch
            {
                Skill.Strength => Strength,
                Skill.Dexterity => Dexterity,
                Skill.Intelligence => Intelligence,
                Skill.Constitution => Constitution,
                Skill.Charisma => Charisma,
                Skill.Wisdom => Wisdom,
                _ => throw new ArgumentOutOfRangeException(nameof(skill), skill, null)
            };
            
            return (int)((skillValue - 10) / 2.0);
        }
    }

    public enum PlayerType : short
    {
        [Description("RealPlayer")] RealPlayer = 0,
        [Description("ArtificialPlayer")] ArtificialPlayer = 1, // Legacy, don't use it.
        [Description("Master")] Master = 2,
        [Description("System")] System = 3
    }
}
