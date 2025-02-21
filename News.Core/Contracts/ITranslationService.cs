namespace News.Core.Contracts
{
    public interface ITranslationService
    {
        Task<string> TranslateText(string text, string targetLanguage);
    }
}
