namespace ei_back.Infrastructure.Services.Dtos.Response
{
    public class FunTranslate
    {
        public SuccessInfo success { get; set; }
        public TranslationContent contents { get; set; }
    }

    public class SuccessInfo
    {
        public int total { get; set; }
    }

    public class TranslationContent
    {
        public string translated { get; set; }
        public string text { get; set; }
        public string translation { get; set; }
    }
}
