using Microsoft.OpenApi.Extensions;
using System.ComponentModel;

namespace ei_back.Infrastructure.ExternalAPIs.Dtos.Request
{
    public class AiPromptRequest : IAiPromptRequest
    {
        public AiPromptRequest(PromptRole role, string content)
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
