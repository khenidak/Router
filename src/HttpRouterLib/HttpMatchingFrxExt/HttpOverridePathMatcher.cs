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
    /// this path is used as is by default by HttpRoutingContext
    /// this allows you to override this behaviour and provide your own
    /// </summary>
    public class HttpOverridePathMatcher : MatcherBase
    {
        public string Path { get; set; } 
        public HttpOverridePathMatcher()
        {

        }

        public HttpOverridePathMatcher(string path)
        {
            
            Path = path;
        }


        public override async Task<bool> MatchAsync(RoutingContextBase routingContext,
                                              string sAddress,
                                              IDictionary<string, object> Context,
                                              Stream Body)
        {
            if (null == Path)
                throw new ArgumentNullException("Path");


            if (false == await base.MatchAsync(routingContext, sAddress, Context, Body))
                return false;

            var httpCtx = routingContext as HttpRoutingContext;
            httpCtx.Path = Path;
            httpCtx.OverridePath = true;

            return true;
        }
    }
}
