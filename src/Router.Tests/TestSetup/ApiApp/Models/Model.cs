using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterTests
{
    public class Model
    {
        public string ModelName { get; set; }
        public string ModelId { get; set; }


        public Stream asJsonStream()
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this)));
        }


        public static Model getRandomModel()
        {
            var now = DateTime.UtcNow.Ticks.ToString();

            return new Model() { ModelId = now, ModelName = now };

        }

   }
}
