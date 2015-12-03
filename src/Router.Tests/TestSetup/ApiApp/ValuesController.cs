using System.Collections.Generic;
using System.Web.Http;
using System.Net.Http;

namespace RouterTests
{
    public class ValuesController : ApiController
    {
        

        // GET api/values/5 
        public HttpResponseMessage Get(int id)
        {
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            
            response.Content = new StringContent(Request.RequestUri.ToString());

            return response;

        }

        /*
        // GET api/values 
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }
        // POST api/values 
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5 
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5 
        public void Delete(int id)
        {
        }
        */
    }
}