using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterTests
{
    public class ModelControllerResponseContent
    {
        public List<Model> Models { get; set; } = new List<Model>();
        public string Method { get; set;}
        public string RequestUri { get; set; }
    }
}
