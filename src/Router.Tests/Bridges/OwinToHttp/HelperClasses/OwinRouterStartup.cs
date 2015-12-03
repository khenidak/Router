
using Owin;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;

using RouterLib.Owin;
using System.IO;
using System.Collections.Generic;

using RouterLib;
using HttpRouterLib;


namespace RouterTests

{
    public class TestOwinRouterStartUp
    {
        // creates an OWIN pipeline that has the router embeded 
        // in it
        private async Task<HttpRouter> GetRouter()
        {
            var resolver = new HttpRouteResolver();
            
            
            var headMatcher = new HttpFromOwinReqCtxMatcher(); // copy methods and headers as is

            // route from Owin Server Listening Address to server 2 
            headMatcher.Chain(new SetAddressListMatcher(RouterTestSuit.Srv02HostNamePort),
                              new HostAddressMatcher(RouterTestSuit.OwinRouterSrvHostNamePort, StringMatchType.UriHostandPortMatch));

            await resolver.AddMatcherAsync(headMatcher, 0);
            return new HttpRouter(resolver);

        }
        public void Configuration(IAppBuilder appBuilder)
        {

            // this bridge, bridges Http (via Owin) to Http endpoints


            var router = GetRouter().Result;
            var mwOptions = new DefaultRouterMwOptions<HttpRouter, HttpRoutingResult, HttpRouteResolver>(router);

            // when we get a response from a router 
            // what should we do?
            mwOptions.OnProcessResponse = async (OwinCtx, result) =>
            {
                // because our router sets on top of http router 
                // we can use chain the backend request to the front end down stream

                // for simplicity we are copying the body and the response headers from the backend as is

                var httpRoutingResults = result as HttpRoutingResult;

                // we are not expecting any aggregates in our routing logic, we always route 1:1

                // get the response message
                var hrm = await httpRoutingResults.ResultAsAsync<HttpResponseMessage>();


                // set the response status code
                OwinCtx["owin.ResponseStatusCode"] = (int)hrm.StatusCode;


                #region Body Processing
                    // copy body from the backend request to the down stream

                    var backendstream = await hrm.Content.ReadAsStreamAsync();

                    if (null != backendstream && 0 != backendstream.Length)
                    {
                        var downstream = OwinCtx["owin.ResponseBody"] as Stream;
                    

                        await backendstream.CopyToAsync(downstream);
                    }
                #endregion

                #region Headers Processing
                // the below assumes your gateway is completely transparent (there is no specific response headers coming from
                // the backend point that intstruct the gateway to do stuff. 
                var downstreamHeaders = OwinCtx["owin.ResponseHeaders"] as IDictionary<string, string[]>;
                foreach (var header in hrm.Headers)
                {
                    downstreamHeaders.Add(header.Key, header.Value.ToArray());
                }   
                #endregion

            };

            
            appBuilder.UseOwinRouter(mwOptions);
            

        }
    }
}