using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace News.Core.Contracts
{
    public interface ITextToSpeechService
    {
        byte[] ConvertTextToSpeech(string text, string language = "en-US");
    }
}
