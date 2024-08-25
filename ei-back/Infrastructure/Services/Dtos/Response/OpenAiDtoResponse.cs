using ei_back.Infrastructure.Services.Dtos.Request;

namespace ei_back.Infrastructure.Services.Dtos.Response
{
    public class OpenAiDtoResponse
    {
        public string id { get; set; }
        //public string object { get; set; }
        public int created { get; set; }
        public string model { get; set; }
        public List<Choice> choices { get; set; }
        public Usage usage { get; set; }
        public string system_fingerprint {  get; set; }
    }

    public class Choice
    {
        public int index { get; set; }
        public Message message { get; set; }
        public string logprobs { get; set; }
        public string finish_reason { get; set; }
    }

    public class Usage
    {
        public int prompt_tokens { get; set; }
        public int completion_tokens { get; set; }
        public int total_tokens { get; set; }
    }
}
