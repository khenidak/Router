using System.Collections.Generic;
using System.Web.Http;
using System.Net.Http;
using System.Threading.Tasks;
using System;

namespace RouterTests
{
    /// <summary>
    /// used to test routing to fastest route 
    /// </summary>
    public class RandomDelayController : ApiController
    {


        // GET api/RandomDelay/5 
        public async Task<HttpResponseMessage> Get(int id)
        {
            await Task.Delay((new Random().Next(0, 100)));



            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            response.Content = new StringContent(Request.RequestUri.ToString());

            return response;

        }
        
    }
}