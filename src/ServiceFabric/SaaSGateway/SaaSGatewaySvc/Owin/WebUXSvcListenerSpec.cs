
namespace SaaSGatewaySvc
{

    using Owin;
    using System.Threading.Tasks;
    using System.IO;
    using HttpRouterLib;
    using RouterLib;
    using RouterLib.Owin;
    using System.Net.Http;
    using System.Collections.Generic;
    using System.Linq;

    internal class SaaSGatwaySvcListenerSpec : IOwinListenerSpec
    {
        private readonly SaaSGatewaySvc service;

        public SaaSGatwaySvcListenerSpec(SaaSGatewaySvc service)
        {
            this.service = service;
        }
        private async Task<HttpRouter> GetRouter()
        {
            var resolver = new HttpRouteResolver();


            var headMatcher = new HttpFromOwinReqCtxMatcher(); // copy methods and headers as is

            //using my custom SaaS tenant matcher address matcher
            headMatcher.Chain(new MatchHostAddressForTenant());

            await resolver.AddMatcherAsync(headMatcher, 0);
            return new HttpRouter(resolver);

        }


        public void CreateOwinPipeline(IAppBuilder app)
        {
            // The below is just boilerplate code just as OwinToHttp bridge in the Router.Tests project

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
                    // Microsoft.Owin.Hosting.HttpListener adds its own 
                    // headers, the idea here is to copy backend custom headers if any
                    if(!downstreamHeaders.ContainsKey(header.Key))
                        downstreamHeaders.Add(header.Key, header.Value.ToArray());
                }
                #endregion

            };


            app.UseOwinRouter(mwOptions);


        }
    }
}