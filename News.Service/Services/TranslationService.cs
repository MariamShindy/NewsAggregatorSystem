namespace News.Service.Services
{
    public class TranslationService(AggregateTranslator _translator) : ITranslationService
    {
        //Using GTranslate Pakcage 

        public async Task<object> TranslateText(string text, string targetLanguage)
        {
            var translatedText = await _translator.TranslateAsync(text, targetLanguage, "en");
            return translatedText;
        }
    }
}
