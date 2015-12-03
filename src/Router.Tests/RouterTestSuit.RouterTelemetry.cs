
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        [TestCategory("RouterTelemetry")]
        public async Task BasicTelemetry()
        {
            var resolver = new HttpRouteResolver();


            // simple routing to ensure that resolver routing algorthim is working

            /*
                Route all requests from Server 1 to Server 2

            */



            var path = "api/values/0";
            // this pluming is to extract which host are we going to use to route


            var headMatcher = new HttpSetMethodMatcher(HttpMethod.Get);

            headMatcher.Chain(new SetAddressListMatcher(Srv02HostNamePort),
                              new HostAddressMatcher(Srv01HostNamePort, StringMatchType.UriHostandPortMatch));


            await resolver.AddMatcherAsync(headMatcher, 0);


            var router = new HttpRouter(resolver);

            

            var results = await router.RouteAsync(string.Concat(srv01Address, path));


            var responseMessage = await results.ResultAsAsync<HttpResponseMessage>();
            var responseString = await responseMessage.Content.ReadAsStringAsync();

            // server returns the address
            // in the response

            Assert.AreEqual(string.Concat(srv02Address, path), responseString);
            Assert.IsTrue(router.TotalExecutedLastMin > 0);
            Assert.IsTrue(router.TotalResolvedLastMin > 0);
        }


#if ENABLE_LONGRUNNING_TESTS

        [TestMethod]
        [TestCategory("RouterTelemetry")]
        
        public async Task AllTelemetry()
        {

            Trace.WriteLine("This test expects router's min clicker to trim every 5 sec");

            var resolver = new HttpRouteResolver();


            // simple routing to ensure that resolver routing algorthim is working

            /*
                Route all requests from Server 1 to Server 2

            */



            var path = "api/values/0";
            // this pluming is to extract which host are we going to use to route


            var headMatcher = new HttpSetMethodMatcher(HttpMethod.Get);

            headMatcher.Chain(new SetAddressListMatcher(Srv02HostNamePort),
                              new IsHostAddressMatcher(Srv01HostNamePort, StringMatchType.UriHostandPortMatch));


            await resolver.AddMatcherAsync(headMatcher, 0);


            var router = new HttpRouter.Core.HttpRouter(resolver);

            router.DefaultCallHandlingStratgy = new RetryStrategy(100);

            var results = await router.RouteAsync(string.Concat(srv01Address, path));


            var responseMessage = await results.ResultAsAsync<HttpResponseMessage>();
            var responseString = await responseMessage.Content.ReadAsStringAsync();

            

            Assert.AreEqual(string.Concat(srv02Address, path), responseString);

            var TotalExecutedLastMin = router.TotalExecutedLastMin;
            var TotalResolvedLastMin =  router.TotalResolvedLastMin;


            Assert.IsTrue(TotalExecutedLastMin > 0);
            Assert.IsTrue(TotalResolvedLastMin > 0);

            await Task.Delay(TimeSpan.FromSeconds(61)); // wait for at least one trim to happen (from min clicker to hour cliker, every min)

            var TotalExecutedLastHour = router.TotalExecutedLastHour ;
            var TotalResolvedLastHour = router.TotalResolvedLastHour ;

            var AvgExecuteTimePerMinLastHour  = router.AvgExecuteTimePerMinLastHour ;
            var AvgResolveTimePerMinLastHour  = router.AvgResolveTimePerMinLastHour ;


            Assert.IsTrue(TotalExecutedLastHour > 0);
            Assert.IsTrue(TotalResolvedLastHour > 0);
            

            Assert.IsTrue(AvgExecuteTimePerMinLastHour > 0);
            Assert.IsTrue(AvgResolveTimePerMinLastHour >0);

            // Write
            Trace.WriteLine(string.Concat("TotalExecutedLastMin: ", TotalExecutedLastMin));
            Trace.WriteLine(string.Concat("TotalResolvedLastMin: ", TotalResolvedLastMin));
            Trace.WriteLine(string.Concat("TotalExecutedLastHour: ", TotalExecutedLastHour));
            Trace.WriteLine(string.Concat("TotalResolvedLastHour: ", TotalResolvedLastHour));
            Trace.WriteLine(string.Concat("AvgExecuteTimePerMinLastHour: ", AvgExecuteTimePerMinLastHour));
            Trace.WriteLine(string.Concat("AvgResolveTimePerMinLastHour: ", AvgResolveTimePerMinLastHour));
        }

#endif
    }
}
