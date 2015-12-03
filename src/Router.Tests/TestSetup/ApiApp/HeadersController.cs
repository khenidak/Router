using System.Collections.Generic;
using System.Web.Http;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json;

namespace RouterTests
{
    public class HeadersController : ApiController
    {
        
        public Task<HttpResponseMessage> Get()
        {
            

            var headerList = new Dictionary<string, IEnumerable<string>>();
            foreach (var header in Request.Headers)
                headerList.Add(header.Key, header.Value);
            

            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            response.Content = new StringContent(JsonConvert.SerializeObject(headerList));

            return Task.FromResult(response);

        }
        
    }
}