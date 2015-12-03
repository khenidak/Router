
using Owin;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;

using RouterLib.Owin;
using System.IO;
using System.Collections.Generic;

using WebSocketServer; // get it from here https://github.com/khenidak/WebSocketsServer (compile as AnyCpU check ../ref/)
using HttpRouterLib;
using RouterLib;

namespace RouterTests

{
    public class OwinWebSocketStartup
    {

        private async Task<HttpRouter> GetRouter()
        {
            /*
            factory create a instance of SocketSession for incoming connection. 
            in this test we will create a router attached it to the factory (which is injected in every socket session)

        */

            // prepare the router
            var resolver = new HttpRouteResolver();


            // routing logic

            //-> If context contains GetModels (route to backend(s), fastest route, add scheme, add path, ignore body, set HttpMethod to Get)


            var getModelsMatchers = new HttpSetMethodMatcher(HttpMethod.Get);
            getModelsMatchers.Chain(new IgnoreSourceBodyStreamMatcher(), // ignore whatever message body provided by the socket.
                                    new SetAddressListMatcher(new string[] { RouterTestSuit.Srv01HostNamePort, RouterTestSuit.Srv02HostNamePort, RouterTestSuit.Srv03HostNamePort }, // address list
                                                              true, // clear whatever there
                                                              ContextRoutingType.FastestRoute), // fastest
                                    new HttpOverridePathMatcher("/api/models/"), // add path
                                    new HttpOverrideSchemeMatcher("http"), // add scheme
                                    new ContextValueStringMatcher("Message.Type", "GetModels"));



            //-> If context contains AddModels
            var addModelsMatcher = new HttpSetMethodMatcher(HttpMethod.Post);
            addModelsMatcher.Chain(
                                   new SetAddressListMatcher(RouterTestSuit.Srv01HostNamePort), // set address to Server1 
                                   new HttpAddSetHeaderMatcher("Content-Type", "application/json"), // my socket puts everything as json
                                   new HttpOverridePathMatcher("/api/models/"), // add path
                                   new HttpOverrideSchemeMatcher("http"), // add scheme
                                   new ContextValueStringMatcher("Message.Type", "AddModels")
                                  );



            // There are multiple ways i can do use the above matchers 

            /* method 1) use Or matcher
                var orMatcher = new OrMatcher(getModelsMatchers, addModelsMatcher);
                await resolver.AddMatcherAsync(orMatcher, 0);
            */

            /* method 2) use AnyMatcher
                var anyMatcher = new AnyMatcher(getModelsMatchers, addModelsMatcher);
                await resolver.AddMatcherAsync(anyMatcher, 0);
            */

            // method 3) use the  new ContextValueStringMatcher("Message.Type", "AddModels") in an IIF matcher (remember to remove them from Add/Get matcher variables above).
            

            // or just create differet matchers in resolvers. the advantages, if each tree wants to cache and have state on thier 
            // they will do that without conficlts (check how load balanacer is used in HttpRoutingContext).

            await resolver.AddMatcherAsync(getModelsMatchers, 0);
            await resolver.AddMatcherAsync(addModelsMatcher, 1);
     
            var router = new HttpRouter(resolver);

            return router;

        }

        public void Configuration(IAppBuilder appBuilder)
        {


            // prepare the web socket factory, again read here  https://github.com/khenidak/WebSocketsServer

            var router = GetRouter().Result;
            var factory = new WSocketSessionFactory(router);


            appBuilder.MapWebSocket<WSocketSessionFactory, WSocketSession>(factory);
        }
    }
}