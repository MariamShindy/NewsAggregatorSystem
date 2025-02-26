namespace News.Core.Contracts
{
    public interface ITranslationService
    {
        Task<object> TranslateText(string text, string targetLanguage);
    }
}
