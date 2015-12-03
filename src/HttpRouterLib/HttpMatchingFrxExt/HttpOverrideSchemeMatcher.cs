using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using RouterLib;


namespace HttpRouterLib
{
    /// <summary>
    /// The address passed to router is the entire address which is scheme://host:port/path?query
    /// this scheme is used as is by default by HttpRoutingContext
    /// this allows you to override this behaviour and provide your own
    /// </summary>
    public class HttpOverrideSchemeMatcher: MatcherBase
    {
        public string Scheme { get; set; } 
        public HttpOverrideSchemeMatcher()
        {

        }

        public HttpOverrideSchemeMatcher(string scheme)
        {
            
            Scheme = scheme;
        }


        public override async Task<bool> MatchAsync(RoutingContextBase routingContext,
                                              string sAddress,
                                              IDictionary<string, object> Context,
                                              Stream Body)
        {
            if (null == Scheme)
                throw new ArgumentNullException("Scheme");


            if (false == await base.MatchAsync(routingContext, sAddress, Context, Body))
                return false;

            var httpCtx = routingContext as HttpRoutingContext;
            httpCtx.Scheme = Scheme;
            httpCtx.OverrideScheme = true;

            return true;
        }
    }
}
