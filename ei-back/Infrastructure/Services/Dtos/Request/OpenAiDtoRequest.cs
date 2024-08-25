namespace ei_back.Infrastructure.Services.Dtos.Request
{
    public class OpenAiDtoRequest
    {
        public string model { get; set; }
        public List<Message> messages { get; set; }
        public decimal temperature { get; set; }
    }

    public class Message
    {
        public string role { get; set; }
        public string content { get; set; }
        public string refusal { get; set; }
    }
}
