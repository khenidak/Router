using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Diagnostics;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;


using RouterLib;
using HttpRouterLib;
namespace RouterTests
{
    public partial class  RouterTestSuit
    {

        [TestMethod]
        [TestCategory("Bridges.OwinToHttp")]
        public async Task OwinToHttpRoutingGet()
        {
            // in order to review what are the routing rules
            // used by the router look at the Owin startup routine

            // start a web server that holds our owin pipeline (which has the router in it)
            var OwinWebServer = new TestWebServer();
            OwinWebServer.Start<TestOwinRouterStartUp>(OwinRouterSrvAddress);
            // run a test 

            var path = "api/values/0"; // this address doesn't exist  on 
                                       // the router but it will be routed anyway
                                       // because we have rules configured for that 

            var hrm = new HttpRequestMessage();
            // call the router endpoint
            hrm.RequestUri = new System.Uri(string.Concat(OwinRouterSrvAddress, path));
            hrm.Method = HttpMethod.Get;
            var httpClient = new HttpClient();
            var httpResponse = await httpClient.SendAsync(hrm);


            var responseString = await httpResponse.Content.ReadAsStringAsync();


            httpResponse.EnsureSuccessStatusCode();

            // stop the server
            OwinWebServer.Stop();
        }

        [TestMethod]
        [TestCategory("Bridges.OwinToHttp")]
        public async Task OwinToHttpRoutingPut()
        {
            // start a web server that holds our owin pipeline (which has the router in it)
            var OwinWebServer = new TestWebServer();
            OwinWebServer.Start<TestOwinRouterStartUp>(OwinRouterSrvAddress);


            var path = "api/models/0";
            var sentModel = Model.getRandomModel();


            var hrm = new HttpRequestMessage();
            // call the router endpoint
            hrm.RequestUri = new System.Uri(string.Concat(OwinRouterSrvAddress, path));
            hrm.Method = HttpMethod.Put;
            hrm.Content = new StreamContent(sentModel.asJsonStream());
            hrm.Content.Headers.Add("Content-Type", "application/json");

            var httpClient = new HttpClient();
            var httpResponse = await httpClient.SendAsync(hrm);



            httpResponse.EnsureSuccessStatusCode();

            // stop the server
            OwinWebServer.Stop();
        


            var responseString = await httpResponse.Content.ReadAsStringAsync();
            var modelReturn = JsonConvert.DeserializeObject<ModelControllerResponseContent>(responseString);


            Assert.AreEqual(sentModel.ModelId, modelReturn.Models[0].ModelId);
        }




    }
}
