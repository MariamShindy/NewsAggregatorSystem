using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace News.Core.Entities
{
    public class TranslateRequest
    {
        public string Text { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
    }
}
