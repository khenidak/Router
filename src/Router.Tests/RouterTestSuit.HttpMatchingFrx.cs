
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
        [TestCategory("HttpMatchingFrx")]

        public async Task AddCustomHeader()
        {
            var resolver = new HttpRouteResolver();


            // simple routing to ensure that resolver routing algorthim is working

            /*
                Route all requests from Server 1 to Server 2
                and add a custom header
            */



            var path = "api/Headers/0";
            var customHeaderName = "customHeader";
            var customHeaderValue = "customHeaderValue";


            var headMatcher = new HttpSetMethodMatcher(HttpMethod.Get);

            headMatcher.Chain(new SetAddressListMatcher(Srv02HostNamePort),
                              new HttpAddSetHeaderMatcher(customHeaderName, customHeaderValue),
                              new HostAddressMatcher(Srv01HostNamePort, StringMatchType.UriHostandPortMatch));


            await resolver.AddMatcherAsync(headMatcher, 0);


            var router = new HttpRouter(resolver);

            

            var results = await router.RouteAsync(string.Concat(srv01Address, path));


            var responseMessage = await results.ResultAsAsync<HttpResponseMessage>();
            var responseString = await responseMessage.Content.ReadAsStringAsync();
            

            var dict = JsonConvert.DeserializeObject<Dictionary<string, IEnumerable<string>>>(responseString);


            // server returns the headers in the response

            Assert.AreEqual(true, dict.ContainsKey(customHeaderName));
            Assert.AreEqual(true, dict[customHeaderName].Where(v => v == customHeaderValue).Count() != 0);

        }

        [TestMethod]
        [TestCategory("HttpMatchingFrx")]

        public async Task RemoveHeader()
        {
            var resolver = new HttpRouteResolver();


            // simple routing to ensure that resolver routing algorthim is working

            /*
                Route all requests from Server 1 to Server 2
                and add a custom header
            */



            var path = "api/Headers/0";
            var customHeaderName = "customHeader";
            var customHeaderValue = "customHeaderValue";


            var headMatcher = new HttpSetMethodMatcher(HttpMethod.Get);

            headMatcher.Chain(new SetAddressListMatcher(Srv02HostNamePort),
                              new HttpRemoveHeaderMatcher(customHeaderName),
                              new HttpAddSetHeaderMatcher(customHeaderName, customHeaderValue),
                              new HostAddressMatcher(Srv01HostNamePort, StringMatchType.UriHostandPortMatch));


            await resolver.AddMatcherAsync(headMatcher, 0);


            var router = new HttpRouter(resolver);

            

            var results = await router.RouteAsync(string.Concat(srv01Address, path));


            var responseMessage = await results.ResultAsAsync<HttpResponseMessage>();
            var responseString = await responseMessage.Content.ReadAsStringAsync();


            var dict = JsonConvert.DeserializeObject<Dictionary<string, IEnumerable<string>>>(responseString);


            // server returns the headers in the response

            Assert.AreEqual(false, dict.ContainsKey(customHeaderName));
         
        }
    }
}
