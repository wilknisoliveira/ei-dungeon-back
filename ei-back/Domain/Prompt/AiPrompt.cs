using ei_back.Domain.Prompt.Interfaces;
using Microsoft.OpenApi.Extensions;
using System.ComponentModel;

namespace ei_back.Domain.Prompt
{
    public class AiPrompt : IAiPrompt
    {
        public AiPrompt(PromptRole role, string content)
        {
            Role = role.GetAttributeOfType<DescriptionAttribute>().Description;
            Content = content;
        }

        public string Role { get; set; }
        public string Content { get; set; }
    }

    public enum PromptRole : short
    {
        [Description("user")] User = 0,
        [Description("assistant")] Assistant = 1,
        [Description("system")] System = 2
    }
}
