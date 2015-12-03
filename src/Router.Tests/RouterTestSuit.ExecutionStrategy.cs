using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Diagnostics;
using System.Net;
using System.Net.Http;

using RouterLib;
using HttpRouterLib;

namespace RouterTests
{
    public partial class  RouterTestSuit
    {

        [TestMethod]
        [TestCategory("ExecutionStrategyFrx")]
        public async Task RetryThenRoute()
        {
            var resolver = new HttpRouteResolver();


            
            /*
                Attempt to an address (that will fail), then retry after 10 ms, if still failing
                it will route to a different server.
            */



            var path = "api/values/0";
            

            var headMatcher = new HttpSetMethodMatcher(HttpMethod.Get);

            headMatcher.Chain(new SetAddressListMatcher("fail"), // point to an address that does not exist (i really hope that you have a machine on your network named "fail")
                              new HostAddressMatcher(Srv01HostNamePort, StringMatchType.UriHostandPortMatch));


            await resolver.AddMatcherAsync(headMatcher, 0);


            var router = new HttpRouter(resolver);

            var headStrategy = new RetryAfterStrategy(10);
            // chain
                headStrategy.ThenRouteToHost(Srv02HostNamePort);

            router.DefaultContextExecuteStrategy = headStrategy;

            var results = await router.RouteAsync(string.Concat(srv01Address, path));


            var responseMessage = await results.ResultAsAsync<HttpResponseMessage>();
            var responseString = await responseMessage.Content.ReadAsStringAsync();

            // server returns the address
            // in the response

            Assert.AreEqual(string.Concat(srv02Address, path), responseString);
        }


    }
}
