namespace News.Service.Services
{
    public class TranslationService 
    {
        private readonly HttpClient _httpClient;

        public TranslationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<TranslationResponse> TranslateTextAsync(TranslationRequest request)
        {
            var payload = System.Text.Json.JsonSerializer.Serialize(new
            {
                text = request.Text,
                source_lang = request.SourceLang,
                target_lang = request.TargetLang
            });

            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("http://localhost:8000/translate", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Translation failed: {response.StatusCode}");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var responseJson = JsonDocument.Parse(responseBody);

            var translation = responseJson.RootElement.GetProperty("translation").GetString();

            return new TranslationResponse { Translation = translation };
        }
    }
}


//namespace News.Service.Services
//{
//    public class TranslationService(AggregateTranslator _translator) : ITranslationService
//    {
//        //Using GTranslate Pakcage 

//        public async Task<object> TranslateText(string text, string targetLanguage)
//        {
//            var translatedText = await _translator.TranslateAsync(text, targetLanguage, "en");
//            return translatedText;
//        }
//    }
//}
