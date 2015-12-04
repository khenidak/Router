
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


using RouterLib;
using HttpRouterLib;


namespace RouterTests
{
    public partial class RouterTestSuit
    {
        [TestMethod]
        [TestCategory("AdvancedHttp")]
        public async Task OverrideSchemeAndPath()
        {
            var resolver = new HttpRouteResolver();


            


            var path = "api/values/0";
            
            /*
                route to server 2 override the scheme to http & path to a static path
            */


            var headMatcher = new HttpSetMethodMatcher(HttpMethod.Get);

            headMatcher.Chain(new SetAddressListMatcher(Srv02HostNamePort),
                              new HttpOverridePathMatcher("/api/values/0"),
                              new HttpOverrideSchemeMatcher("http"),
                              new HostAddressMatcher(Srv01HostNamePort, StringMatchType.UriHostandPortMatch));


            await resolver.AddMatcherAsync(headMatcher, 0);
            var router = new HttpRouter(resolver);
            
            var results = await router.RouteAsync(string.Concat("xxx://", Srv01HostNamePort)); // bad scehme & no path

            var responseMessage = await results.ResultAsAsync<HttpResponseMessage>();
            var responseString = await responseMessage.Content.ReadAsStringAsync();

            // server returns the address
            // in the response

            Assert.AreEqual(string.Concat(srv02Address, path), responseString);
        }



        [TestMethod]
        [TestCategory("AdvancedHttp")]
        public async Task doHttpPost()
        {
            var resolver = new HttpRouteResolver();


            
            /*
                Route all requests from Server 1 to Server 2

            */



            var path = "api/models/";
            var sentModel = Model.getRandomModel();

            var headMatcher = new HttpSetMethodMatcher(HttpMethod.Post);

            headMatcher.Chain(new SetAddressListMatcher(Srv02HostNamePort),
                            new HttpAddSetHeaderMatcher("Content-Type", "application/json"),
                              new HostAddressMatcher(Srv01HostNamePort, StringMatchType.UriHostandPortMatch));


            await resolver.AddMatcherAsync(headMatcher, 0);

            var router = new HttpRouter(resolver);
            var results = await router.RouteAsync(string.Concat(srv01Address, path), sentModel.asJsonStream());

            var responseMessage = await results.ResultAsAsync<HttpResponseMessage>();
            var responseString = await responseMessage.Content.ReadAsStringAsync();
            var modelReturn = JsonConvert.DeserializeObject<ModelControllerResponseContent>(responseString);


            Assert.AreEqual(sentModel.ModelId, modelReturn.Models[0].ModelId);
            Assert.AreEqual(string.Concat(srv02Address, path), modelReturn.RequestUri);

        }



        [TestMethod]
        [TestCategory("AdvancedHttp")]
        public async Task doHttpPut()
        {
            var resolver = new HttpRouteResolver();

            var path = "api/models/";
            var sentModel = Model.getRandomModel();

           


            var headMatcher = new HttpSetMethodMatcher(HttpMethod.Put);

            headMatcher.Chain(new SetAddressListMatcher(Srv02HostNamePort),
                            new HttpAddSetHeaderMatcher("Content-Type", "application/json"),
                              new HostAddressMatcher(Srv01HostNamePort, StringMatchType.UriHostandPortMatch));


            await resolver.AddMatcherAsync(headMatcher, 0);


            var router = new HttpRouter(resolver);

            

            var results = await router.RouteAsync(string.Concat(srv01Address, path, sentModel.ModelId), sentModel.asJsonStream());


            var responseMessage = await results.ResultAsAsync<HttpResponseMessage>();
            var responseString = await responseMessage.Content.ReadAsStringAsync();
            var modelReturn = JsonConvert.DeserializeObject<ModelControllerResponseContent>(responseString);


            Assert.AreEqual(sentModel.ModelId, modelReturn.Models[0].ModelId);

            Assert.AreEqual(string.Concat(srv02Address, path, sentModel.ModelId), modelReturn.RequestUri);

        }

        [TestMethod]
        [TestCategory("AdvancedHttp")]
        public async Task doHttpPutWithQueryString()
        {
            var resolver = new HttpRouteResolver();

            var path = "api/models/";
            var sentModel = Model.getRandomModel();




            var headMatcher = new HttpSetMethodMatcher(HttpMethod.Put);

            headMatcher.Chain(new SetAddressListMatcher(Srv02HostNamePort),
                            new HttpAddSetHeaderMatcher("Content-Type", "application/json"),
                              new HostAddressMatcher(Srv01HostNamePort, StringMatchType.UriHostandPortMatch));


            await resolver.AddMatcherAsync(headMatcher, 0);


            var router = new HttpRouter(resolver);
            var results = await router.RouteAsync(string.Concat(srv01Address, path,"?ModelId=", sentModel.ModelId), sentModel.asJsonStream());


            var responseMessage = await results.ResultAsAsync<HttpResponseMessage>();
            var responseString = await responseMessage.Content.ReadAsStringAsync();
            var modelReturn = JsonConvert.DeserializeObject<ModelControllerResponseContent>(responseString);


            Assert.AreEqual(sentModel.ModelId, modelReturn.Models[0].ModelId);
            Assert.AreEqual(string.Concat(srv02Address, path, "?ModelId=", sentModel.ModelId), modelReturn.RequestUri);

        }

    }
}
