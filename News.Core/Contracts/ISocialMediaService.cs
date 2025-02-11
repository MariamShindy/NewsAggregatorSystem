using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace News.Core.Contracts
{
    public interface ISocialMediaService
    {
         Dictionary<string, string> GenerateShareLinks(string newsId, string platform = null!);
    }
}
