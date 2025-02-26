using GTranslate.Results;
using GTranslate.Translators;
using Microsoft.Extensions.Configuration;
using News.Core.Contracts;
using System.Text;
using System.Text.Json;
using System.Web;

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
