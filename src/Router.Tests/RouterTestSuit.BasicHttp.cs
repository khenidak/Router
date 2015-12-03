using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RouterLib;
using HttpRouterLib;

using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;
using System.Linq;

using System.Net.Http;
using System.Collections.Generic;

namespace RouterTests
{
    public partial class RouterTestSuit
    {
       
        [TestMethod]
        [TestCategory("Basic-Http")]
        public async  Task BasicWiring()
        {
            var resolver = new HttpRouteResolver();


            // simple routing to ensure that resolver routing algorthim is working
            /*
            // it is a linked list, executed from last to end
                if source address is exactly matching http://www.bing.com 
                then
                    Set address list (with one address) to http://www.microsoft.com
                    set http method to httpGet;
            */
            var firstMatcher = new HttpSetMethodMatcher(HttpMethod.Get);
            firstMatcher.Chain(new SetAddressListMatcher("www.microsoft.com"),
                               new HostAddressMatcher("www.bing.com", StringMatchType.UriHostandPortMatch));


            await resolver.AddMatcherAsync(firstMatcher, 0);


            var router = new HttpRouter(resolver);

            
            var results = await router.RouteAsync("http://www.bing.com");


            var responseMessage = await results.ResultAsAsync<HttpResponseMessage>();
            Trace.WriteLine(await responseMessage.Content.ReadAsStringAsync());
        }

        [TestMethod]
        [TestCategory("Basic-Http")]

        public async Task FastestRoute()
        {
            var resolver = new HttpRouteResolver();


            // simple routing to ensure that resolver routing algorthim is working

            /*
                anything point to http://localhost:9001 
                will be routed as fastest route to
                http://locahost:9002 or http://locahost:9003

                With HttpGet + Same path

            */



            var path = "api/randomdelay/0";
            var trytimes = 5; 
            // this pluming is to extract which host are we going to use to route

            



            var firstMatcher = new HttpSetMethodMatcher(HttpMethod.Get);

            firstMatcher.Chain(new SetAddressListMatcher(new string[] { Srv01HostNamePort, Srv02HostNamePort, Srv03HostNamePort }, false, ContextRoutingType.FastestRoute),
                               new HostAddressMatcher(Srv01HostNamePort, StringMatchType.UriHostandPortMatch));


            await resolver.AddMatcherAsync(firstMatcher, 0);


            var router = new HttpRouter(resolver);
            var resultsList = new List<HttpRoutingResult>(trytimes);
            var responsesList = new HashSet<string>();

            // call as many as try times 
            for (var i = 1; i<= trytimes; i++)
                   resultsList.Add(await router.RouteAsync(string.Concat(srv01Address, path)));

            foreach (var result in resultsList)
            {
                var responseMessage = await result.ResultAsAsync<HttpResponseMessage>();
                var responseString = await responseMessage.Content.ReadAsStringAsync();

                responsesList.Add(responseString);
            }


            

            // server returns the address
            // in the response
            // any count > 1 means that router is routing to more than one end point
            Assert.AreEqual(true, responsesList.Count > 1);
        }


        [TestMethod]
        [TestCategory("Basic-Http")]
        public async Task AggregateMultipleEndPoints()
        {
            var resolver = new HttpRouteResolver();


            // simple routing to ensure that resolver routing algorthim is working

            /*
                anything point to http://localhost:9001 
                will be routed as fastest route to
                http://locahost:9002 and http://locahost:9003

                With HttpGet + Same path

            */



            var path = "api/randomdelay/0";
            var Hosts = 3;
            // this pluming is to extract which host are we going to use to route





            var firstMatcher = new HttpSetMethodMatcher(HttpMethod.Get);

            firstMatcher.Chain(new SetAddressListMatcher(new string[] { Srv01HostNamePort, Srv02HostNamePort, Srv03HostNamePort }, false, ContextRoutingType.ScatterGather),
                               new HostAddressMatcher(Srv01HostNamePort, StringMatchType.UriHostandPortMatch));


            await resolver.AddMatcherAsync(firstMatcher, 0);


            var router = new HttpRouter(resolver);

            

            var resultsList = new List<HttpRoutingResult>(Hosts);
            var responsesList = new HashSet<string>();

            // call as many as try times 
            var allResults = await router.RouteAsync(string.Concat(srv01Address, path));

            foreach (var result in await allResults.ResultAsAsync<List<HttpResponseMessage>>())
            {
                var responseString = await result.Content.ReadAsStringAsync();
                responsesList.Add(responseString);
            }
            
            Assert.IsTrue(responsesList.Count == 3); // all hosts in the system
        }





        [TestMethod]
        [TestCategory("Basic-Http")]

        public async Task SimpleLoadBalance()
        {
            var resolver = new HttpRouteResolver();


            // simple routing to ensure that resolver routing algorthim is working

            /*
                anything point to http://localhost:9001 
                will be routed R/R balanced to 
                http://locahost:9001    
                http://locahost:9002 
                http://locahost:9003

                With HttpGet + Same path
            */



            var path = "/api/values/0";
            var trytimes = 3;
            // this pluming is to extract which host are we going to use to route





            var firstMatcher = new HttpSetMethodMatcher(HttpMethod.Get);

            firstMatcher.Chain(new SetAddressListMatcher(new string[] { Srv01HostNamePort, Srv02HostNamePort, Srv03HostNamePort }, false, ContextRoutingType.RoundRobin),
                               new HostAddressMatcher(Srv01HostNamePort, StringMatchType.UriHostandPortMatch));


            await resolver.AddMatcherAsync(firstMatcher, 0);


            var router = new HttpRouter(resolver);

            

            var resultsList = new List<HttpRoutingResult>(trytimes);
            var responsesList = new HashSet<string>();

            // call as many as try times 
            for (var i = 1; i <= trytimes; i++)
                resultsList.Add(await router.RouteAsync(string.Concat(srv01Address, path)));

            foreach (var result in resultsList)
            {
                var responseMessage = await result.ResultAsAsync<HttpResponseMessage>();
                var responseString = await responseMessage.Content.ReadAsStringAsync();

                responsesList.Add(responseString);
            }


            // response list should contain 
            // http <server1> + path
            // http <server2> + path 
            // http <server3> + path 



            var expectedList = new List<string>()
            {
                string.Concat("http://", Srv01HostNamePort, path),
                string.Concat("http://", Srv02HostNamePort, path),
                string.Concat("http://", Srv03HostNamePort, path)
            };

            foreach (var response in responsesList)
            {
                if (expectedList.Contains(response))
                    expectedList.Remove(response);

            }

            Assert.IsTrue(expectedList.Count == 0); // all hosts in the system
        }




        [TestMethod]
        [TestCategory("Basic-Http")]

        public async Task LongLoadBalance()
        {
            var resolver = new HttpRouteResolver();


            // simple routing to ensure that resolver routing algorthim is working

            /*
                anything point to http://localhost:9001 
                will be routed R/R balanced to 
                http://locahost:9001    
                http://locahost:9002 
                http://locahost:9003

                With HttpGet + Same path
            */



            var path = "/api/values/0";
            var trytimes = 9;
            // this pluming is to extract which host are we going to use to route





            var firstMatcher = new HttpSetMethodMatcher(HttpMethod.Get);

            firstMatcher.Chain(new SetAddressListMatcher(new string[] { Srv01HostNamePort, Srv02HostNamePort, Srv03HostNamePort }, false, ContextRoutingType.RoundRobin),
                               new HostAddressMatcher(Srv01HostNamePort, StringMatchType.UriHostandPortMatch));


            await resolver.AddMatcherAsync(firstMatcher, 0);


            var router = new HttpRouter(resolver);

            

            var resultsList = new List<HttpRoutingResult>(trytimes);
            var responsesList = new Dictionary<string, int>();

            // call as many as try times 
            for (var i = 1; i <= trytimes; i++)
                resultsList.Add(await router.RouteAsync(string.Concat(srv01Address, path)));

            foreach (var result in resultsList)
            {
                var responseMessage = await result.ResultAsAsync<HttpResponseMessage>();
                var responseString = await responseMessage.Content.ReadAsStringAsync();

                if (!responsesList.ContainsKey(responseString))
                    responsesList.Add(responseString, 1);
                else
                    responsesList[responseString] = ++responsesList[responseString];
                

            }

            var bSucess = true;

            // validate that each is called exactly 3 times
            foreach (var key in responsesList.Keys)
            {
                if (responsesList[key] != 3)
                {
                    bSucess = false;
                    break;
                }
            }
            
            

            Assert.IsTrue(bSucess); // all hosts in the system
        }


    }
}
