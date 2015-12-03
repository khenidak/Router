using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterTests
{
    public class MessageIn
    {
        public string MessageType { get; set; }
        // acts as  payload
        public Model theModel { get; set; }

        public string GetAsJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
