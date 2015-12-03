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
        [TestCategory("CoreMatchingFrx")]
        public async Task MatchNamePortAndSetHostAddress()
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
        }



        [TestMethod]
        [TestCategory("CoreMatchingFrx")]
        public async Task MatchOnIIFMatcher()
        {
            var resolver = new HttpRouteResolver();



            var path = "api/values/0";
            var customKey = "MyCustomContextKey";
            // this pluming is to extract which host are we going to use to route


            var headMatcher = new HttpSetMethodMatcher(HttpMethod.Get);
            // Whenever a context contains a key we will route all the traffic to server02
            headMatcher.Chain(new SetAddressListMatcher(Srv02HostNamePort),
                              new ContextContainsKeyMatcher(customKey, StringMatchType.Exact));


            await resolver.AddMatcherAsync(headMatcher, 0);
            var router = new HttpRouter(resolver);
            


            var context = new Dictionary<string, object>();
            context.Add(customKey, "any value, the matcher expects a key");

            
            var results = await router.RouteAsync(string.Concat(srv01Address, path), context);


            var responseMessage = await results.ResultAsAsync<HttpResponseMessage>();
            var responseString = await responseMessage.Content.ReadAsStringAsync();

            // server returns the address
            // in the response

            Assert.AreEqual(string.Concat(srv02Address, path), responseString);
        }




        [TestMethod]
        [TestCategory("CoreMatchingFrx")]
        public async Task MatchOnContextContainsKey()
        {
            var resolver = new HttpRouteResolver();



            var path = "api/values/0";
            var customKey = "MyCustomContextKey";
            // this pluming is to extract which host are we going to use to route


            var headMatcher = new HttpSetMethodMatcher(HttpMethod.Get);
            // Whenever a context contains a key we will route all the traffic to server02
            headMatcher.Chain(

                        new IIFMatcher(
                                           new ContextContainsKeyMatcher(customKey, StringMatchType.Exact),
                                           new SetAddressListMatcher(Srv02HostNamePort),
                                           new SetAddressListMatcher(Srv03HostNamePort)
                                       )
                              );


            await resolver.AddMatcherAsync(headMatcher, 0);
            var router = new HttpRouter(resolver);
            

            

            var results = await router.RouteAsync(string.Concat(srv01Address, path));


            var responseMessage = await results.ResultAsAsync<HttpResponseMessage>();
            var responseString = await responseMessage.Content.ReadAsStringAsync();

            // server returns the address
            // in the response

            Assert.AreEqual(string.Concat(srv03Address, path), responseString);
        }





        [TestMethod]
        [TestCategory("CoreMatchingFrx")]
        public async Task MatchOnContextContainsKeyWithStringValue()
        {
            var resolver = new HttpRouteResolver();



            var path = "api/values/0";
            var customKey = "MyCustomContextKey";
            var customValue = "MyCustomValue";

            // this pluming is to extract which host are we going to use to route


            var headMatcher = new HttpSetMethodMatcher(HttpMethod.Get);
            // Whenever a context contains a key with expected value we will route all the traffic to server02
            headMatcher.Chain(new SetAddressListMatcher(Srv02HostNamePort),
                              new ContextValueStringMatcher(customKey, customValue, StringMatchType.Exact));


            await resolver.AddMatcherAsync(headMatcher, 0);
            var router = new HttpRouter(resolver);
            


            var context = new Dictionary<string, object>();
            context.Add(customKey, customValue);


            var results = await router.RouteAsync(string.Concat(srv01Address, path), context);


            var responseMessage = await results.ResultAsAsync<HttpResponseMessage>();
            var responseString = await responseMessage.Content.ReadAsStringAsync();

            // server returns the address
            // in the response

            Assert.AreEqual(string.Concat(srv02Address, path), responseString);
        }




        [TestMethod]
        [TestCategory("CoreMatchingFrx")]
        public async Task MatchOnContextUsingCustomValuePredicate()
        {
            var resolver = new HttpRouteResolver();



            var path = "api/values/0";
            var customKey = "MyCustomContextKey";
            var customValue = "MyCustomValue";

            // this pluming is to extract which host are we going to use to route


            var headMatcher = new HttpSetMethodMatcher(HttpMethod.Get);
            // whenever our custom predicate return true we will route the traffic to server 2
                headMatcher.Chain(new SetAddressListMatcher(Srv02HostNamePort),
                              new ContextCustomValueMatcher(customKey, (k,v) => 
                              {
                                  var ar = v as string[];
                                  if (null == ar)
                                      return false;

                                  if (ar.Length == 3)
                                      return true;

                                  return false;

                              }));


            await resolver.AddMatcherAsync(headMatcher, 0);
            var router = new HttpRouter(resolver);
            


            var context = new Dictionary<string, object>();
            // context contains a custom value that is a string array
            context.Add(customKey, new string[] { customValue,customValue,customValue});


            var results = await router.RouteAsync(string.Concat(srv01Address, path), context);


            var responseMessage = await results.ResultAsAsync<HttpResponseMessage>();
            var responseString = await responseMessage.Content.ReadAsStringAsync();

            // server returns the address
            // in the response

            Assert.AreEqual(string.Concat(srv02Address, path), responseString);
        }


        [TestMethod]
        [TestCategory("CoreMatchingFrx")]
        public async Task MatchUsingCustomPredicate()
        {
            var resolver = new HttpRouteResolver();



            var path = "api/values/0";

            


            var headMatcher = new HttpSetMethodMatcher(HttpMethod.Get);
            // whenever our custom predicate return true we will route the traffic to server 2

            headMatcher.Chain(new SetAddressListMatcher(Srv02HostNamePort),
                          new PredicateMatcher((routingctx,address, ctx, body) =>
                          {
                              return true;

                          }));


            await resolver.AddMatcherAsync(headMatcher, 0);
            var router = new HttpRouter(resolver);
            


            var context = new Dictionary<string, object>();
            // context contains a custom value that is a string array
            


            var results = await router.RouteAsync(string.Concat(srv01Address, path), context);


            var responseMessage = await results.ResultAsAsync<HttpResponseMessage>();
            var responseString = await responseMessage.Content.ReadAsStringAsync();

            // server returns the address
            // in the response

            Assert.AreEqual(string.Concat(srv02Address, path), responseString);
        }



        [TestMethod]
        [TestCategory("CoreMatchingFrx")]
        public async Task MatchUsingAndMatcher()
        {
            var resolver = new HttpRouteResolver();


            var path = "api/values/0";

            // if host name = srv1's name and port srv1's port then set the address to server 2 and http method to get.
            var headMatcher = new HttpSetMethodMatcher(HttpMethod.Get);


            headMatcher.Chain(new SetAddressListMatcher(Srv02HostNamePort),
                                new AndMatcher( 
                                            new HostAddressMatcher(srv01Uri.Port.ToString(), StringMatchType.UriHostPortMatch),
                                            new HostAddressMatcher(srv01Uri.Host, StringMatchType.UriHostNameMatch)
                                            )
                              );


            await resolver.AddMatcherAsync(headMatcher, 0);


            var router = new HttpRouter(resolver);

            

            var results = await router.RouteAsync(string.Concat(srv01Address, path));


            var responseMessage = await results.ResultAsAsync<HttpResponseMessage>();
            var responseString = await responseMessage.Content.ReadAsStringAsync();

            // server returns the address
            // in the response

            Assert.AreEqual(string.Concat(srv02Address, path), responseString);
        }




        [TestMethod]
        [TestCategory("CoreMatchingFrx")]
        public async Task MatchUsingOrMatcher()
        {
            var resolver = new HttpRouteResolver();





            var path = "api/values/0";

            // if true || False then set address to serve2 and method to get
            var headMatcher = new HttpSetMethodMatcher(HttpMethod.Get);


            headMatcher.Chain(new SetAddressListMatcher(Srv02HostNamePort),
                                new OrMatcher(
                                            new FalseMatcher(),
                                            new TrueMatcher()
                                            )
                              );


            await resolver.AddMatcherAsync(headMatcher, 0);


            var router = new HttpRouter(resolver);

            

            var results = await router.RouteAsync(string.Concat(srv01Address, path));


            var responseMessage = await results.ResultAsAsync<HttpResponseMessage>();
            var responseString = await responseMessage.Content.ReadAsStringAsync();

            // server returns the address
            // in the response

            Assert.AreEqual(string.Concat(srv02Address, path), responseString);
        }


        [TestMethod]
        [TestCategory("CoreMatchingFrx")]
        public async Task MatchUsingAnyMatcher()
        {
            var resolver = new HttpRouteResolver();





            var path = "api/values/0";

            var headMatcher = new HttpSetMethodMatcher(HttpMethod.Get);


            headMatcher.Chain(new SetAddressListMatcher(Srv02HostNamePort),
                                new AnyMatcher(
                                            new FalseMatcher(),
                                            new TrueMatcher()
                                            )
                              );


            await resolver.AddMatcherAsync(headMatcher, 0);


            var router = new HttpRouter(resolver);

            

            var results = await router.RouteAsync(string.Concat(srv01Address, path));


            var responseMessage = await results.ResultAsAsync<HttpResponseMessage>();
            var responseString = await responseMessage.Content.ReadAsStringAsync();

            // server returns the address
            // in the response

            Assert.AreEqual(string.Concat(srv02Address, path), responseString);
        }



        [TestMethod]
        [TestCategory("CoreMatchingFrx")]
        public async Task MatchUsingAllMatcher()
        {
            var resolver = new HttpRouteResolver();





            var path = "api/values/0";

            var headMatcher = new HttpSetMethodMatcher(HttpMethod.Get);


            headMatcher.Chain(new SetAddressListMatcher(Srv02HostNamePort),
                                new AnyMatcher(
                                            new TrueMatcher(),
                                            new TrueMatcher()
                                            )
                              );


            await resolver.AddMatcherAsync(headMatcher, 0);


            var router = new HttpRouter(resolver);

            

            var results = await router.RouteAsync(string.Concat(srv01Address, path));


            var responseMessage = await results.ResultAsAsync<HttpResponseMessage>();
            var responseString = await responseMessage.Content.ReadAsStringAsync();

            // server returns the address
            // in the response

            Assert.AreEqual(string.Concat(srv02Address, path), responseString);
        }

    }
}
