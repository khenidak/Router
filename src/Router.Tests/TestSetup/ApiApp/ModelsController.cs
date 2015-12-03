using System.Collections.Generic;
using System.Web.Http;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json;

namespace RouterTests
{
    public class ModelsController : ApiController
    {

        [HttpGet]
        public Task<HttpResponseMessage> Get()
        {

            var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            var responseContent = new ModelControllerResponseContent();
            responseContent.Method = HttpMethod.Get.ToString();
            responseContent.Models.Add(Model.getRandomModel());
            responseContent.RequestUri = Request.RequestUri.ToString();
            responseMessage.Content = new StringContent(JsonConvert.SerializeObject(responseContent));

            return Task.FromResult(responseMessage);

            /*

            var headerList = new Dictionary<string, IEnumerable<string>>();
            foreach (var header in Request.Headers)
                headerList.Add(header.Key, header.Value);
            

            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            response.Content = new StringContent(JsonConvert.SerializeObject(headerList));

            return Task.FromResult(response);
            */
        }



        // POST api/model
        [HttpPost] 
        public Task<HttpResponseMessage> Post([FromBody]Model value)
        {

            var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            var responseContent = new ModelControllerResponseContent();
            responseContent.Method = HttpMethod.Post.ToString();
            responseContent.Models.Add(value);
            responseContent.RequestUri = Request.RequestUri.ToString();

            responseMessage.Content = new StringContent(JsonConvert.SerializeObject(responseContent));

            return Task.FromResult(responseMessage);


        }



        //in a typical web api you won't return on a put 
        // but this is done to validate that whatever reached the API is correct
        [HttpPut]
        public Task<HttpResponseMessage> Put(long id, [FromBody]Model value)
        {

            var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            var responseContent = new ModelControllerResponseContent();
            responseContent.Method = HttpMethod.Post.ToString();
            responseContent.Models.Add(value);
            responseContent.RequestUri = Request.RequestUri.ToString();

            responseMessage.Content = new StringContent(JsonConvert.SerializeObject(responseContent));

            return Task.FromResult(responseMessage);

        }


        [HttpPut]
        public Task<HttpResponseMessage> PutWithQueryString([FromUri] long ModelId, [FromBody]Model value)
        {

            var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            var responseContent = new ModelControllerResponseContent();
            responseContent.Method = HttpMethod.Post.ToString();
            responseContent.Models.Add(value);
            responseContent.RequestUri = Request.RequestUri.ToString();

            responseMessage.Content = new StringContent(JsonConvert.SerializeObject(responseContent));

            return Task.FromResult(responseMessage);

        }

    }
}