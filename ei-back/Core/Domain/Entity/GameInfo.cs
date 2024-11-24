using ei_back.Core.Domain.Enums;

namespace ei_back.Core.Domain.Entity
{
    public class GameInfo : Base
    {
        public InfoType Type { get; set; }
        public string Value { get; set; }

        public GameInfo(InfoType type, string value)
        {
            Type = type;
            Value = value;
        }
    }
}
