using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace News.Core.Entities
{
    public class SourcesResponse
    {
        public ICollection<Source> Sources { get; set; }
    }
}
