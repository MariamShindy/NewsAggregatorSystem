namespace News.Core.Contracts.Interfaces
{
    public interface ITranslationService
    {
		//Task<object> TranslateText(string text, string targetLanguage);
		Task<TranslationResponse> TranslateTextAsync(TranslationRequest request);

	}
}
